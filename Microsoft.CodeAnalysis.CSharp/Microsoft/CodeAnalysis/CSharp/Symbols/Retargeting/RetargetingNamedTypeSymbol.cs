using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingNamedTypeSymbol : WrappedNamedTypeSymbol
{
	private readonly RetargetingModuleSymbol _retargetingModule;

	private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

	private NamedTypeSymbol _lazyBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyInterfaces;

	private NamedTypeSymbol _lazyDeclaredBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyDeclaredInterfaces;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			if (_lazyTypeParameters.IsDefault)
			{
				if (Arity == 0)
				{
					_lazyTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
				}
				else
				{
					ImmutableInterlocked.InterlockedCompareExchange(ref _lazyTypeParameters, RetargetingTranslator.Retarget(_underlyingType.TypeParameters), default);
				}
			}
			return _lazyTypeParameters;
		}
	}

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics
	{
		get
		{
			if (Arity > 0)
			{
				return StaticCast<TypeSymbol>.From(TypeParameters);
			}
			return ImmutableArray<TypeSymbol>.Empty;
		}
	}

	internal override bool HasTypeArgumentsCustomModifiers => false;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override NamedTypeSymbol EnumUnderlyingType
	{
		get
		{
			NamedTypeSymbol enumUnderlyingType = _underlyingType.EnumUnderlyingType;
			if ((object)enumUnderlyingType != null)
			{
				return RetargetingTranslator.Retarget(enumUnderlyingType, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
			}
			return null;
		}
	}

	public override IEnumerable<string> MemberNames => _underlyingType.MemberNames;

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingType.ContainingSymbol);

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if ((object)_lazyBaseType == ErrorTypeSymbol.UnknownResultType)
			{
				NamedTypeSymbol namedTypeSymbol = GetDeclaredBaseType(null);
				if ((object)namedTypeSymbol == null)
				{
					NamedTypeSymbol baseTypeNoUseSiteDiagnostics = _underlyingType.BaseTypeNoUseSiteDiagnostics;
					if ((object)baseTypeNoUseSiteDiagnostics != null)
					{
						namedTypeSymbol = RetargetingTranslator.Retarget(baseTypeNoUseSiteDiagnostics, RetargetOptions.RetargetPrimitiveTypesByName);
					}
				}
				if ((object)namedTypeSymbol != null && BaseTypeAnalysis.ClassDependsOn(namedTypeSymbol, this))
				{
					return CyclicInheritanceError(this, namedTypeSymbol);
				}
				Interlocked.CompareExchange(ref _lazyBaseType, namedTypeSymbol, ErrorTypeSymbol.UnknownResultType);
			}
			return _lazyBaseType;
		}
	}

	internal override NamedTypeSymbol ComImportCoClass
	{
		get
		{
			NamedTypeSymbol comImportCoClass = _underlyingType.ComImportCoClass;
			if ((object)comImportCoClass != null)
			{
				return RetargetingTranslator.Retarget(comImportCoClass, RetargetOptions.RetargetPrimitiveTypesByName);
			}
			return null;
		}
	}

	internal override bool IsComImport => _underlyingType.IsComImport;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingNamedTypeSymbol(RetargetingModuleSymbol retargetingModule, NamedTypeSymbol underlyingType)
		: base(underlyingType)
	{
		_retargetingModule = retargetingModule;
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetMembers());
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetMembersUnordered());
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetMembers(name));
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		foreach (FieldSymbol item in _underlyingType.GetFieldsToEmit())
		{
			yield return RetargetingTranslator.Retarget(item);
		}
	}

	internal override IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		bool isInterface = _underlyingType.IsInterfaceType();
		foreach (MethodSymbol item in _underlyingType.GetMethodsToEmit())
		{
			int gapSize = (isInterface ? ModuleExtensions.GetVTableGapSize(item.MetadataName) : 0);
			if (gapSize > 0)
			{
				do
				{
					yield return null;
					gapSize--;
				}
				while (gapSize > 0);
			}
			else
			{
				yield return RetargetingTranslator.Retarget(item);
			}
		}
	}

	internal override IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		foreach (PropertySymbol item in _underlyingType.GetPropertiesToEmit())
		{
			yield return RetargetingTranslator.Retarget(item);
		}
	}

	internal override IEnumerable<EventSymbol> GetEventsToEmit()
	{
		foreach (EventSymbol item in _underlyingType.GetEventsToEmit())
		{
			yield return RetargetingTranslator.Retarget(item);
		}
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetEarlyAttributeDecodingMembers());
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetEarlyAttributeDecodingMembers(name));
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembersUnordered());
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembers());
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembers(name));
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembers(name, arity));
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingType.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return RetargetingTranslator.RetargetAttributes(_underlyingType.GetCustomAttributesToEmit(moduleBuilder));
	}

	internal override NamedTypeSymbol LookupMetadataType(ref MetadataTypeName typeName)
	{
		return RetargetingTranslator.Retarget(_underlyingType.LookupMetadataType(ref typeName), RetargetOptions.RetargetPrimitiveTypesByName);
	}

	private static ExtendedErrorTypeSymbol CyclicInheritanceError(RetargetingNamedTypeSymbol type, TypeSymbol declaredBase)
	{
		CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_ImportedCircularBase, declaredBase, type);
		return new ExtendedErrorTypeSymbol(declaredBase, LookupResultKind.NotReferencable, errorInfo, unreported: true);
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		if (_lazyInterfaces.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> declaredInterfaces = GetDeclaredInterfaces(basesBeingResolved);
			if (!IsInterface)
			{
				return declaredInterfaces;
			}
			ImmutableArray<NamedTypeSymbol> value = declaredInterfaces.SelectAsArray((NamedTypeSymbol t) => (!BaseTypeAnalysis.InterfaceDependsOn(t, this)) ? t : CyclicInheritanceError(this, t));
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyInterfaces, value, default);
		}
		return _lazyInterfaces;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetInterfacesToEmit());
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		if ((object)_lazyDeclaredBaseType == ErrorTypeSymbol.UnknownResultType)
		{
			NamedTypeSymbol declaredBaseType = _underlyingType.GetDeclaredBaseType(basesBeingResolved);
			NamedTypeSymbol value = (((object)declaredBaseType != null) ? RetargetingTranslator.Retarget(declaredBaseType, RetargetOptions.RetargetPrimitiveTypesByName) : null);
			Interlocked.CompareExchange(ref _lazyDeclaredBaseType, value, ErrorTypeSymbol.UnknownResultType);
		}
		return _lazyDeclaredBaseType;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		if (_lazyDeclaredInterfaces.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> declaredInterfaces = _underlyingType.GetDeclaredInterfaces(basesBeingResolved);
			ImmutableArray<NamedTypeSymbol> value = RetargetingTranslator.Retarget(declaredInterfaces);
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyDeclaredInterfaces, value, default);
		}
		return _lazyDeclaredInterfaces;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (_lazyUseSiteDiagnostic == CSDiagnosticInfo.EmptyErrorInfo)
		{
			_lazyUseSiteDiagnostic = CalculateUseSiteDiagnostic();
		}
		return _lazyUseSiteDiagnostic;
	}
}
