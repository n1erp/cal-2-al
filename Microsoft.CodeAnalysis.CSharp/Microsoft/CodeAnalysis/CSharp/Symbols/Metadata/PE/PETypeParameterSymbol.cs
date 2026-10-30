using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PETypeParameterSymbol : TypeParameterSymbol
{
	private readonly Symbol _containingSymbol;

	private readonly GenericParameterHandle _handle;

	private readonly string _name;

	private readonly ushort _ordinal;

	private DiagnosticInfo _lazyConstraintsUseSiteErrorInfo = CSDiagnosticInfo.EmptyErrorInfo;

	private readonly GenericParameterAttributes _flags;

	private ThreeState _lazyHasIsUnmanagedConstraint;

	private TypeParameterBounds _lazyBounds = TypeParameterBounds.Unset;

	private ImmutableArray<TypeSymbol> _lazyDeclaredConstraintTypes;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	public override TypeParameterKind TypeParameterKind
	{
		get
		{
			if (ContainingSymbol.Kind != SymbolKind.Method)
			{
				return TypeParameterKind.Type;
			}
			return TypeParameterKind.Method;
		}
	}

	public override int Ordinal => _ordinal;

	public override string Name => _name;

	internal GenericParameterHandle Handle => _handle;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override AssemblySymbol ContainingAssembly => _containingSymbol.ContainingAssembly;

	public override ImmutableArray<Location> Locations => _containingSymbol.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool HasConstructorConstraint => (_flags & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

	public override bool HasReferenceTypeConstraint => (_flags & GenericParameterAttributes.ReferenceTypeConstraint) != 0;

	public override bool HasValueTypeConstraint => (_flags & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

	public override bool HasUnmanagedTypeConstraint
	{
		get
		{
			GetDeclaredConstraintTypes();
			return _lazyHasIsUnmanagedConstraint.Value();
		}
	}

	public override VarianceKind Variance => (VarianceKind)(_flags & GenericParameterAttributes.VarianceMask);

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal PETypeParameterSymbol(PEModuleSymbol moduleSymbol, PENamedTypeSymbol definingNamedType, ushort ordinal, GenericParameterHandle handle)
		: this(moduleSymbol, (Symbol)definingNamedType, ordinal, handle)
	{
	}

	internal PETypeParameterSymbol(PEModuleSymbol moduleSymbol, PEMethodSymbol definingMethod, ushort ordinal, GenericParameterHandle handle)
		: this(moduleSymbol, (Symbol)definingMethod, ordinal, handle)
	{
	}

	private PETypeParameterSymbol(PEModuleSymbol moduleSymbol, Symbol definingSymbol, ushort ordinal, GenericParameterHandle handle)
	{
		_containingSymbol = definingSymbol;
		GenericParameterAttributes flags = GenericParameterAttributes.None;
		try
		{
			moduleSymbol.Module.GetGenericParamPropsOrThrow(handle, out _name, out flags);
		}
		catch (BadImageFormatException)
		{
			if (_name == null)
			{
				_name = string.Empty;
			}
			_lazyConstraintsUseSiteErrorInfo = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this);
		}
		_flags = (((flags & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0) ? flags : (flags & ~GenericParameterAttributes.DefaultConstructorConstraint));
		_ordinal = ordinal;
		_handle = handle;
	}

	private ImmutableArray<TypeSymbol> GetDeclaredConstraintTypes()
	{
		if (_lazyDeclaredConstraintTypes.IsDefault)
		{
			PEMethodSymbol pEMethodSymbol = null;
			PENamedTypeSymbol pENamedTypeSymbol;
			if (_containingSymbol.Kind == SymbolKind.Method)
			{
				pEMethodSymbol = (PEMethodSymbol)_containingSymbol;
				pENamedTypeSymbol = (PENamedTypeSymbol)pEMethodSymbol.ContainingSymbol;
			}
			else
			{
				pENamedTypeSymbol = (PENamedTypeSymbol)_containingSymbol;
			}
			PEModuleSymbol containingPEModule = pENamedTypeSymbol.ContainingPEModule;
			MetadataReader metadataReader = containingPEModule.Module.MetadataReader;
			GenericParameterConstraintHandleCollection genericParameterConstraintHandleCollection;
			try
			{
				genericParameterConstraintHandleCollection = metadataReader.GetGenericParameter(_handle).GetConstraints();
			}
			catch (BadImageFormatException)
			{
				genericParameterConstraintHandleCollection = default;
				Interlocked.CompareExchange(ref _lazyConstraintsUseSiteErrorInfo, new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this), CSDiagnosticInfo.EmptyErrorInfo);
			}
			bool flag = false;
			ImmutableArray<TypeSymbol> value;
			if (genericParameterConstraintHandleCollection.Count > 0)
			{
				ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
				MetadataDecoder metadataDecoder = (((object)pEMethodSymbol == null) ? new MetadataDecoder(containingPEModule, pENamedTypeSymbol) : new MetadataDecoder(containingPEModule, pEMethodSymbol));
				foreach (GenericParameterConstraintHandle item in genericParameterConstraintHandleCollection)
				{
					TypeSymbol typeSymbol = metadataDecoder.DecodeGenericParameterConstraint(metadataReader.GetGenericParameterConstraint(item).Type, out var isUnmanagedConstraint);
					if (typeSymbol.SpecialType == SpecialType.System_ValueType)
					{
						if (isUnmanagedConstraint)
						{
							flag = true;
						}
						if ((_flags & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None)
						{
							continue;
						}
					}
					if (typeSymbol.SpecialType != SpecialType.System_Object)
					{
						typeSymbol = TupleTypeDecoder.DecodeTupleTypesIfApplicable(typeSymbol, item, containingPEModule);
						instance.Add(typeSymbol);
					}
				}
				value = instance.ToImmutableAndFree();
			}
			else
			{
				value = ImmutableArray<TypeSymbol>.Empty;
			}
			if ((flag && (_flags & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0) || flag != containingPEModule.Module.HasIsUnmanagedAttribute(_handle))
			{
				flag = false;
				Interlocked.CompareExchange(ref _lazyConstraintsUseSiteErrorInfo, new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this), CSDiagnosticInfo.EmptyErrorInfo);
			}
			_lazyHasIsUnmanagedConstraint = flag.ToThreeState();
			ImmutableInterlocked.InterlockedInitialize(ref _lazyDeclaredConstraintTypes, value);
		}
		return _lazyDeclaredConstraintTypes;
	}

	internal override void EnsureAllConstraintsAreResolved()
	{
		if (_lazyBounds == TypeParameterBounds.Unset)
		{
			TypeParameterSymbol.EnsureAllConstraintsAreResolved((_containingSymbol.Kind == SymbolKind.Method) ? ((PEMethodSymbol)_containingSymbol).TypeParameters : ((PENamedTypeSymbol)_containingSymbol).TypeParameters);
		}
	}

	internal override ImmutableArray<TypeSymbol> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
	{
		return GetBounds(inProgress)?.ConstraintTypes ?? ImmutableArray<TypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
	{
		return GetBounds(inProgress)?.Interfaces ?? ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
	{
		TypeParameterBounds bounds = GetBounds(inProgress);
		if (bounds == null)
		{
			return GetDefaultBaseType();
		}
		return bounds.EffectiveBaseClass;
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		TypeParameterBounds bounds = GetBounds(inProgress);
		if (bounds == null)
		{
			return GetDefaultBaseType();
		}
		return bounds.DeducedBaseType;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_lazyCustomAttributes.IsDefault)
		{
			ImmutableArray<CSharpAttributeData> customAttributesForToken = ((PEModuleSymbol)ContainingModule).GetCustomAttributesForToken(Handle, out var _, HasUnmanagedTypeConstraint ? AttributeDescription.IsUnmanagedAttribute : default(AttributeDescription));
			ImmutableInterlocked.InterlockedInitialize(ref _lazyCustomAttributes, customAttributesForToken);
		}
		return _lazyCustomAttributes;
	}

	private TypeParameterBounds GetBounds(ConsList<TypeParameterSymbol> inProgress)
	{
		if (_lazyBounds == TypeParameterBounds.Unset)
		{
			ImmutableArray<TypeSymbol> declaredConstraintTypes = GetDeclaredConstraintTypes();
			ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
			ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
			bool inherited = _containingSymbol.Kind == SymbolKind.Method && ((MethodSymbol)_containingSymbol).IsOverride;
			TypeParameterBounds value = this.ResolveBounds(ContainingAssembly.CorLibrary, inProgress.Prepend(this), declaredConstraintTypes, inherited, null, instance, ref useSiteDiagnosticsBuilder);
			DiagnosticInfo diagnosticInfo = null;
			if (instance.Count > 0)
			{
				diagnosticInfo = instance[0].DiagnosticInfo;
			}
			else if (useSiteDiagnosticsBuilder != null && useSiteDiagnosticsBuilder.Count > 0)
			{
				foreach (TypeParameterDiagnosticInfo item in useSiteDiagnosticsBuilder)
				{
					if (item.DiagnosticInfo.Severity == DiagnosticSeverity.Error)
					{
						diagnosticInfo = item.DiagnosticInfo;
						break;
					}
					if (diagnosticInfo == null)
					{
						diagnosticInfo = item.DiagnosticInfo;
					}
				}
			}
			instance.Free();
			Interlocked.CompareExchange(ref _lazyConstraintsUseSiteErrorInfo, diagnosticInfo, CSDiagnosticInfo.EmptyErrorInfo);
			Interlocked.CompareExchange(ref _lazyBounds, value, TypeParameterBounds.Unset);
		}
		return _lazyBounds;
	}

	internal override DiagnosticInfo GetConstraintsUseSiteErrorInfo()
	{
		EnsureAllConstraintsAreResolved();
		return _lazyConstraintsUseSiteErrorInfo;
	}

	private NamedTypeSymbol GetDefaultBaseType()
	{
		return ContainingAssembly.GetSpecialType(SpecialType.System_Object);
	}
}
