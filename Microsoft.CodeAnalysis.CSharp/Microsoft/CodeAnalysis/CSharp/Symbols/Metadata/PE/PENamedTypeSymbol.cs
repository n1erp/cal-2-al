using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.DocumentationComments;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal abstract class PENamedTypeSymbol : NamedTypeSymbol
{
	private class UncommonProperties
	{
		internal ImmutableArray<PEFieldSymbol> lazyInstanceEnumFields;

		internal NamedTypeSymbol lazyEnumUnderlyingType;

		internal ImmutableArray<CSharpAttributeData> lazyCustomAttributes;

		internal ImmutableArray<string> lazyConditionalAttributeSymbols;

		internal ObsoleteAttributeData lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

		internal AttributeUsageInfo lazyAttributeUsageInfo = AttributeUsageInfo.Null;

		internal ThreeState lazyContainsExtensionMethods;

		internal ThreeState lazyIsByRefLike;

		internal ThreeState lazyIsReadOnly;

		internal string lazyDefaultMemberName;

		internal NamedTypeSymbol lazyComImportCoClassType = ErrorTypeSymbol.UnknownResultType;

		internal ThreeState lazyHasEmbeddedAttribute;

		internal bool IsDefaultValue()
		{
			if (lazyInstanceEnumFields.IsDefault && (object)lazyEnumUnderlyingType == null && lazyCustomAttributes.IsDefault && lazyConditionalAttributeSymbols.IsDefault && lazyObsoleteAttributeData == ObsoleteAttributeData.Uninitialized && lazyAttributeUsageInfo.IsNull && !lazyContainsExtensionMethods.HasValue() && lazyDefaultMemberName == null && (object)lazyComImportCoClassType == ErrorTypeSymbol.UnknownResultType)
			{
				return !lazyHasEmbeddedAttribute.HasValue();
			}
			return false;
		}
	}

	private class DeclarationOrderTypeSymbolComparer : IComparer<Symbol>
	{
		public static readonly DeclarationOrderTypeSymbolComparer Instance = new DeclarationOrderTypeSymbolComparer();

		private DeclarationOrderTypeSymbolComparer()
		{
		}

		public int Compare(Symbol x, Symbol y)
		{
			return HandleComparer.Default.Compare(((PENamedTypeSymbol)x).Handle, ((PENamedTypeSymbol)y).Handle);
		}
	}

	private sealed class PENamedTypeSymbolNonGeneric : PENamedTypeSymbol
	{
		public override int Arity => 0;

		internal override bool MangleName => false;

		internal override int MetadataArity
		{
			get
			{
				if (_container is PENamedTypeSymbol pENamedTypeSymbol)
				{
					return pENamedTypeSymbol.MetadataArity;
				}
				return 0;
			}
		}

		internal PENamedTypeSymbolNonGeneric(PEModuleSymbol moduleSymbol, NamespaceOrTypeSymbol container, TypeDefinitionHandle handle, string emittedNamespaceName, out bool mangleName)
			: base(moduleSymbol, container, handle, emittedNamespaceName, 0, out mangleName)
		{
		}
	}

	private sealed class PENamedTypeSymbolGeneric : PENamedTypeSymbol
	{
		private readonly GenericParameterHandleCollection _genericParameterHandles;

		private readonly ushort _arity;

		private readonly bool _mangleName;

		private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

		public override int Arity => _arity;

		internal override bool MangleName => _mangleName;

		internal override int MetadataArity => _genericParameterHandles.Count;

		internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

		public override ImmutableArray<TypeParameterSymbol> TypeParameters
		{
			get
			{
				EnsureTypeParametersAreLoaded();
				return _lazyTypeParameters;
			}
		}

		internal PENamedTypeSymbolGeneric(PEModuleSymbol moduleSymbol, NamespaceOrTypeSymbol container, TypeDefinitionHandle handle, string emittedNamespaceName, GenericParameterHandleCollection genericParameterHandles, ushort arity, out bool mangleName)
			: base(moduleSymbol, container, handle, emittedNamespaceName, arity, out mangleName)
		{
			_arity = arity;
			_genericParameterHandles = genericParameterHandles;
			_mangleName = mangleName;
		}

		private void EnsureTypeParametersAreLoaded()
		{
			if (_lazyTypeParameters.IsDefault)
			{
				PEModuleSymbol containingPEModule = ContainingPEModule;
				int num = _genericParameterHandles.Count - _arity;
				TypeParameterSymbol[] array = new TypeParameterSymbol[_arity];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new PETypeParameterSymbol(containingPEModule, this, (ushort)i, _genericParameterHandles[num + i]);
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameters, ImmutableArray.Create(array));
			}
		}

		protected override DiagnosticInfo GetUseSiteDiagnosticImpl()
		{
			DiagnosticInfo result = null;
			if (!MergeUseSiteDiagnostics(ref result, base.GetUseSiteDiagnosticImpl()) && !MatchesContainingTypeParameters())
			{
				result = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, this);
			}
			return result;
		}

		private bool MatchesContainingTypeParameters()
		{
			NamedTypeSymbol containingType = ContainingType;
			if ((object)containingType == null)
			{
				return true;
			}
			ImmutableArray<TypeParameterSymbol> allTypeParameters = containingType.GetAllTypeParameters();
			int length = allTypeParameters.Length;
			if (length == 0)
			{
				return true;
			}
			ImmutableArray<TypeParameterSymbol> typeParameters = Create(ContainingPEModule, (PENamespaceSymbol)ContainingNamespace, _handle, null).TypeParameters;
			TypeMap typeMap = new TypeMap(allTypeParameters, IndexedTypeParameterSymbol.Take(length));
			TypeMap typeMap2 = new TypeMap(typeParameters, IndexedTypeParameterSymbol.Take(typeParameters.Length));
			for (int i = 0; i < length; i++)
			{
				TypeParameterSymbol typeParameter = allTypeParameters[i];
				TypeParameterSymbol typeParameter2 = typeParameters[i];
				if (!MemberSignatureComparer.HaveSameConstraints(typeParameter, typeMap, typeParameter2, typeMap2))
				{
					return false;
				}
			}
			return true;
		}
	}

	private static readonly Dictionary<string, ImmutableArray<PENamedTypeSymbol>> s_emptyNestedTypes = new Dictionary<string, ImmutableArray<PENamedTypeSymbol>>(EmptyComparer.Instance);

	private readonly NamespaceOrTypeSymbol _container;

	private readonly TypeDefinitionHandle _handle;

	private readonly string _name;

	private readonly TypeAttributes _flags;

	private readonly SpecialType _corTypeId;

	private ICollection<string> _lazyMemberNames;

	private ImmutableArray<Symbol> _lazyMembersInDeclarationOrder;

	private Dictionary<string, ImmutableArray<Symbol>> _lazyMembersByName;

	private Dictionary<string, ImmutableArray<PENamedTypeSymbol>> _lazyNestedTypes;

	private TypeKind _lazyKind;

	private NamedTypeSymbol _lazyBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyInterfaces;

	private NamedTypeSymbol _lazyDeclaredBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyDeclaredInterfaces;

	private Tuple<CultureInfo, string> _lazyDocComment;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private static readonly UncommonProperties s_noUncommonProperties = new UncommonProperties();

	private UncommonProperties _lazyUncommonProperties;

	public override SpecialType SpecialType => _corTypeId;

	internal PEModuleSymbol ContainingPEModule
	{
		get
		{
			Symbol symbol = _container;
			while (symbol.Kind != SymbolKind.Namespace)
			{
				symbol = symbol.ContainingSymbol;
			}
			return ((PENamespaceSymbol)symbol).ContainingPEModule;
		}
	}

	internal override ModuleSymbol ContainingModule => ContainingPEModule;

	public abstract override int Arity { get; }

	internal abstract override bool MangleName { get; }

	internal abstract int MetadataArity { get; }

	internal TypeDefinitionHandle Handle => _handle;

	internal override bool HasCodeAnalysisEmbeddedAttribute
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyHasEmbeddedAttribute.HasValue())
			{
				uncommonProperties.lazyHasEmbeddedAttribute = ContainingPEModule.Module.HasCodeAnalysisEmbeddedAttribute(_handle).ToThreeState();
			}
			return uncommonProperties.lazyHasEmbeddedAttribute.Value();
		}
	}

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if ((object)_lazyBaseType == ErrorTypeSymbol.UnknownResultType)
			{
				Interlocked.CompareExchange(ref _lazyBaseType, MakeAcyclicBaseType(), ErrorTypeSymbol.UnknownResultType);
			}
			return _lazyBaseType;
		}
	}

	public override NamedTypeSymbol ConstructedFrom => this;

	public override Symbol ContainingSymbol => _container;

	public override NamedTypeSymbol ContainingType => _container as NamedTypeSymbol;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			switch (_flags & TypeAttributes.VisibilityMask)
			{
			case TypeAttributes.NestedAssembly:
				return Accessibility.Internal;
			case TypeAttributes.VisibilityMask:
				return Accessibility.ProtectedOrInternal;
			case TypeAttributes.NestedFamANDAssem:
				return Accessibility.ProtectedAndInternal;
			case TypeAttributes.NestedPrivate:
				return Accessibility.Private;
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				return Accessibility.Public;
			case TypeAttributes.NestedFamily:
				return Accessibility.Protected;
			case TypeAttributes.NotPublic:
				return Accessibility.Internal;
			default:
				throw ExceptionUtilities.UnexpectedValue(_flags & TypeAttributes.VisibilityMask);
			}
		}
	}

	public override NamedTypeSymbol EnumUnderlyingType
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return null;
			}
			EnsureEnumUnderlyingTypeIsLoaded(uncommonProperties);
			return uncommonProperties.lazyEnumUnderlyingType;
		}
	}

	public override IEnumerable<string> MemberNames
	{
		get
		{
			EnsureNonTypeMemberNamesAreLoaded();
			return _lazyMemberNames;
		}
	}

	internal override FieldSymbol FixedElementField
	{
		get
		{
			FieldSymbol result = null;
			ImmutableArray<Symbol> members = GetMembers("FixedElementField");
			if (!members.IsDefault && members.Length == 1)
			{
				result = members[0] as FieldSymbol;
			}
			return result;
		}
	}

	public override ImmutableArray<Location> Locations => ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override string Name => _name;

	internal override bool HasSpecialName => (_flags & TypeAttributes.SpecialName) != 0;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => ImmutableArray<TypeSymbol>.Empty;

	internal sealed override bool HasTypeArgumentsCustomModifiers => false;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override bool IsStatic
	{
		get
		{
			if ((_flags & TypeAttributes.Sealed) != 0)
			{
				return (_flags & TypeAttributes.Abstract) != 0;
			}
			return false;
		}
	}

	public override bool IsAbstract
	{
		get
		{
			if ((_flags & TypeAttributes.Abstract) != 0)
			{
				return (_flags & TypeAttributes.Sealed) == 0;
			}
			return false;
		}
	}

	internal override bool IsMetadataAbstract => (_flags & TypeAttributes.Abstract) != 0;

	public override bool IsSealed
	{
		get
		{
			if ((_flags & TypeAttributes.Sealed) != 0)
			{
				return (_flags & TypeAttributes.Abstract) == 0;
			}
			return false;
		}
	}

	internal override bool IsMetadataSealed => (_flags & TypeAttributes.Sealed) != 0;

	internal TypeAttributes Flags => _flags;

	public override bool MightContainExtensionMethods
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyContainsExtensionMethods.HasValue())
			{
				ThreeState lazyContainsExtensionMethods = ThreeState.False;
				TypeKind typeKind = TypeKind;
				if (typeKind - 2 <= TypeKind.Array || typeKind == TypeKind.Struct)
				{
					bool flag = ContainingPEModule.Module.HasExtensionAttribute(_handle, ignoreCase: false);
					lazyContainsExtensionMethods = ((!(ContainingAssembly is PEAssemblySymbol pEAssemblySymbol)) ? flag.ToThreeState() : (flag && pEAssemblySymbol.MightContainExtensionMethods).ToThreeState());
				}
				uncommonProperties.lazyContainsExtensionMethods = lazyContainsExtensionMethods;
			}
			return uncommonProperties.lazyContainsExtensionMethods.Value();
		}
	}

	public override TypeKind TypeKind
	{
		get
		{
			TypeKind typeKind = _lazyKind;
			if (typeKind == TypeKind.Unknown)
			{
				if (_flags.IsInterface())
				{
					typeKind = TypeKind.Interface;
				}
				else
				{
					TypeSymbol declaredBaseType = GetDeclaredBaseType(null);
					typeKind = TypeKind.Class;
					if ((object)declaredBaseType != null)
					{
						switch (declaredBaseType.SpecialType)
						{
						case SpecialType.System_Enum:
							typeKind = TypeKind.Enum;
							break;
						case SpecialType.System_MulticastDelegate:
							typeKind = TypeKind.Delegate;
							break;
						case SpecialType.System_ValueType:
							if (SpecialType != SpecialType.System_Enum)
							{
								typeKind = TypeKind.Struct;
							}
							break;
						}
					}
				}
				_lazyKind = typeKind;
			}
			return typeKind;
		}
	}

	internal sealed override bool IsInterface => _flags.IsInterface();

	internal string DefaultMemberName
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return "";
			}
			if (uncommonProperties.lazyDefaultMemberName == null)
			{
				ContainingPEModule.Module.HasDefaultMemberAttribute(_handle, out var memberName);
				Interlocked.CompareExchange(ref uncommonProperties.lazyDefaultMemberName, memberName ?? "", null);
			}
			return uncommonProperties.lazyDefaultMemberName;
		}
	}

	internal override bool IsComImport => (_flags & TypeAttributes.Import) != 0;

	internal override bool ShouldAddWinRTMembers => IsWindowsRuntimeImport;

	internal override bool IsWindowsRuntimeImport => (_flags & TypeAttributes.WindowsRuntime) != 0;

	internal override TypeLayout Layout => ContainingPEModule.Module.GetTypeLayout(_handle);

	internal override CharSet MarshallingCharSet
	{
		get
		{
			CharSet charSet = _flags.ToCharSet();
			if (charSet == 0)
			{
				return CharSet.Ansi;
			}
			return charSet;
		}
	}

	public override bool IsSerializable => (_flags & TypeAttributes.Serializable) != 0;

	internal override bool IsByRefLikeType
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyIsByRefLike.HasValue())
			{
				ThreeState lazyIsByRefLike = ThreeState.False;
				if (TypeKind == TypeKind.Struct)
				{
					lazyIsByRefLike = ContainingPEModule.Module.HasIsByRefLikeAttribute(_handle).ToThreeState();
				}
				uncommonProperties.lazyIsByRefLike = lazyIsByRefLike;
			}
			return uncommonProperties.lazyIsByRefLike.Value();
		}
	}

	internal override bool IsReadOnly
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyIsReadOnly.HasValue())
			{
				ThreeState lazyIsReadOnly = ThreeState.False;
				if (TypeKind == TypeKind.Struct)
				{
					lazyIsReadOnly = ContainingPEModule.Module.HasIsReadOnlyAttribute(_handle).ToThreeState();
				}
				uncommonProperties.lazyIsReadOnly = lazyIsReadOnly;
			}
			return uncommonProperties.lazyIsReadOnly.Value();
		}
	}

	internal override bool HasDeclarativeSecurity => (_flags & TypeAttributes.HasSecurity) != 0;

	internal override NamedTypeSymbol ComImportCoClass
	{
		get
		{
			if (!this.IsInterfaceType())
			{
				return null;
			}
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return null;
			}
			if ((object)uncommonProperties.lazyComImportCoClassType == ErrorTypeSymbol.UnknownResultType)
			{
				TypeSymbol typeSymbol = ContainingPEModule.TryDecodeAttributeWithTypeArgument(Handle, AttributeDescription.CoClassAttribute);
				NamedTypeSymbol value = (((object)typeSymbol != null && (typeSymbol.TypeKind == TypeKind.Class || typeSymbol.IsErrorType())) ? ((NamedTypeSymbol)typeSymbol) : null);
				Interlocked.CompareExchange(ref uncommonProperties.lazyComImportCoClassType, value, ErrorTypeSymbol.UnknownResultType);
			}
			return uncommonProperties.lazyComImportCoClassType;
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return null;
			}
			bool isByRefLikeType = IsByRefLikeType;
			ObsoleteAttributeHelpers.InitializeObsoleteDataFromMetadata(ref uncommonProperties.lazyObsoleteAttributeData, _handle, ContainingPEModule, isByRefLikeType);
			return uncommonProperties.lazyObsoleteAttributeData;
		}
	}

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public IEnumerable<object> fieldDefs { get; set; }

	private UncommonProperties GetUncommonProperties()
	{
		UncommonProperties lazyUncommonProperties = _lazyUncommonProperties;
		if (lazyUncommonProperties != null)
		{
			return lazyUncommonProperties;
		}
		if (IsUncommon())
		{
			lazyUncommonProperties = new UncommonProperties();
			return Interlocked.CompareExchange(ref _lazyUncommonProperties, lazyUncommonProperties, null) ?? lazyUncommonProperties;
		}
		return _lazyUncommonProperties = s_noUncommonProperties;
	}

	private bool IsUncommon()
	{
		if (ContainingPEModule.HasAnyCustomAttributes(_handle))
		{
			return true;
		}
		if (TypeKind == TypeKind.Enum)
		{
			return true;
		}
		return false;
	}

	internal static PENamedTypeSymbol Create(PEModuleSymbol moduleSymbol, PENamespaceSymbol containingNamespace, TypeDefinitionHandle handle, string emittedNamespaceName)
	{
		BadImageFormatException mrEx = null;
		GetGenericInfo(moduleSymbol, handle, out var genericParameterHandles, out var arity, out mrEx);
		PENamedTypeSymbol pENamedTypeSymbol = ((arity != 0) ? ((PENamedTypeSymbol)new PENamedTypeSymbolGeneric(moduleSymbol, containingNamespace, handle, emittedNamespaceName, genericParameterHandles, arity, out var mangleName)) : ((PENamedTypeSymbol)new PENamedTypeSymbolNonGeneric(moduleSymbol, containingNamespace, handle, emittedNamespaceName, out mangleName)));
		if (mrEx != null)
		{
			pENamedTypeSymbol._lazyUseSiteDiagnostic = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, pENamedTypeSymbol);
		}
		return pENamedTypeSymbol;
	}

	private static void GetGenericInfo(PEModuleSymbol moduleSymbol, TypeDefinitionHandle handle, out GenericParameterHandleCollection genericParameterHandles, out ushort arity, out BadImageFormatException mrEx)
	{
		try
		{
			genericParameterHandles = moduleSymbol.Module.GetTypeDefGenericParamsOrThrow(handle);
			arity = (ushort)genericParameterHandles.Count;
			mrEx = null;
		}
		catch (BadImageFormatException ex)
		{
			arity = 0;
			genericParameterHandles = default;
			mrEx = ex;
		}
	}

	internal static PENamedTypeSymbol Create(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, TypeDefinitionHandle handle)
	{
		BadImageFormatException mrEx = null;
		GetGenericInfo(moduleSymbol, handle, out var genericParameterHandles, out var arity, out mrEx);
		ushort arity2 = 0;
		int metadataArity = containingType.MetadataArity;
		if (arity > metadataArity)
		{
			arity2 = (ushort)(arity - metadataArity);
		}
		PENamedTypeSymbol pENamedTypeSymbol = ((arity != 0) ? ((PENamedTypeSymbol)new PENamedTypeSymbolGeneric(moduleSymbol, containingType, handle, null, genericParameterHandles, arity2, out var mangleName)) : ((PENamedTypeSymbol)new PENamedTypeSymbolNonGeneric(moduleSymbol, containingType, handle, null, out mangleName)));
		if (mrEx != null || arity < metadataArity)
		{
			pENamedTypeSymbol._lazyUseSiteDiagnostic = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, pENamedTypeSymbol);
		}
		return pENamedTypeSymbol;
	}

	private PENamedTypeSymbol(PEModuleSymbol moduleSymbol, NamespaceOrTypeSymbol container, TypeDefinitionHandle handle, string emittedNamespaceName, ushort arity, out bool mangleName)
	{
		bool flag = false;
		string text;
		try
		{
			text = moduleSymbol.Module.GetTypeDefNameOrThrow(handle);
		}
		catch (BadImageFormatException)
		{
			text = string.Empty;
			flag = true;
		}
		_handle = handle;
		_container = container;
		try
		{
			_flags = moduleSymbol.Module.GetTypeDefFlagsOrThrow(handle);
		}
		catch (BadImageFormatException)
		{
			flag = true;
		}
		if (arity == 0)
		{
			_name = text;
			mangleName = false;
		}
		else
		{
			_name = MetadataHelpers.UnmangleMetadataNameForArity(text, arity);
			mangleName = (object)_name != text;
		}
		if (emittedNamespaceName != null && moduleSymbol.ContainingAssembly.KeepLookingForDeclaredSpecialTypes && DeclaredAccessibility == Accessibility.Public)
		{
			_corTypeId = SpecialTypes.GetTypeFromMetadataName(MetadataHelpers.BuildQualifiedName(emittedNamespaceName, text));
		}
		else
		{
			_corTypeId = SpecialType.None;
		}
		if (flag)
		{
			_lazyUseSiteDiagnostic = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, this);
		}
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved = null)
	{
		if (_lazyInterfaces.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyInterfaces, MakeAcyclicInterfaces(), default);
		}
		return _lazyInterfaces;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return InterfacesNoUseSiteDiagnostics();
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		if ((object)_lazyDeclaredBaseType == ErrorTypeSymbol.UnknownResultType)
		{
			Interlocked.CompareExchange(ref _lazyDeclaredBaseType, MakeDeclaredBaseType(), ErrorTypeSymbol.UnknownResultType);
		}
		return _lazyDeclaredBaseType;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		if (_lazyDeclaredInterfaces.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyDeclaredInterfaces, MakeDeclaredInterfaces(), default);
		}
		return _lazyDeclaredInterfaces;
	}

	private NamedTypeSymbol MakeDeclaredBaseType()
	{
		if (!_flags.IsInterface())
		{
			try
			{
				PEModuleSymbol containingPEModule = ContainingPEModule;
				EntityHandle baseTypeOfTypeOrThrow = containingPEModule.Module.GetBaseTypeOfTypeOrThrow(_handle);
				if (!baseTypeOfTypeOrThrow.IsNil)
				{
					return (NamedTypeSymbol)TupleTypeDecoder.DecodeTupleTypesIfApplicable(DynamicTypeDecoder.TransformType(new MetadataDecoder(containingPEModule, this).GetTypeOfToken(baseTypeOfTypeOrThrow), 0, _handle, containingPEModule), _handle, containingPEModule);
				}
			}
			catch (BadImageFormatException mrEx)
			{
				return new UnsupportedMetadataTypeSymbol(mrEx);
			}
		}
		return null;
	}

	private ImmutableArray<NamedTypeSymbol> MakeDeclaredInterfaces()
	{
		try
		{
			PEModuleSymbol containingPEModule = ContainingPEModule;
			InterfaceImplementationHandleCollection interfaceImplementationsOrThrow = containingPEModule.Module.GetInterfaceImplementationsOrThrow(_handle);
			if (interfaceImplementationsOrThrow.Count > 0)
			{
				NamedTypeSymbol[] array = new NamedTypeSymbol[interfaceImplementationsOrThrow.Count];
				MetadataDecoder metadataDecoder = new MetadataDecoder(containingPEModule, this);
				int num = 0;
				foreach (InterfaceImplementationHandle item in interfaceImplementationsOrThrow)
				{
					EntityHandle token = containingPEModule.Module.MetadataReader.GetInterfaceImplementation(item).Interface;
					NamedTypeSymbol namedTypeSymbol = TupleTypeDecoder.DecodeTupleTypesIfApplicable(metadataDecoder.GetTypeOfToken(token), item, containingPEModule) as NamedTypeSymbol;
					array[num++] = (((object)namedTypeSymbol != null) ? namedTypeSymbol : new UnsupportedMetadataTypeSymbol());
				}
				return array.AsImmutableOrNull();
			}
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		catch (BadImageFormatException mrEx)
		{
			return ImmutableArray.Create((NamedTypeSymbol)new UnsupportedMetadataTypeSymbol(mrEx));
		}
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		UncommonProperties uncommonProperties = GetUncommonProperties();
		if (uncommonProperties == s_noUncommonProperties)
		{
			return ImmutableArray<CSharpAttributeData>.Empty;
		}
		if (uncommonProperties.lazyCustomAttributes.IsDefault)
		{
			ImmutableArray<CSharpAttributeData> customAttributesForToken = ContainingPEModule.GetCustomAttributesForToken(Handle, out var _, MightContainExtensionMethods ? AttributeDescription.CaseSensitiveExtensionAttribute : default(AttributeDescription), out var _, (IsByRefLikeType && ObsoleteAttributeData == null) ? AttributeDescription.ObsoleteAttribute : default(AttributeDescription), out var _, IsReadOnly ? AttributeDescription.IsReadOnlyAttribute : default(AttributeDescription), out var _, IsByRefLikeType ? AttributeDescription.IsByRefLikeAttribute : default(AttributeDescription));
			ImmutableInterlocked.InterlockedInitialize(ref uncommonProperties.lazyCustomAttributes, customAttributesForToken);
		}
		return uncommonProperties.lazyCustomAttributes;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return GetAttributes();
	}

	private void EnsureNonTypeMemberNamesAreLoaded()
	{
		if (_lazyMemberNames != null)
		{
			return;
		}
		PEModule module = ContainingPEModule.Module;
		HashSet<string> hashSet = new HashSet<string>();
		try
		{
			foreach (MethodDefinitionHandle item in module.GetMethodsOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetMethodDefNameOrThrow(item));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		try
		{
			foreach (PropertyDefinitionHandle item2 in module.GetPropertiesOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetPropertyDefNameOrThrow(item2));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		try
		{
			foreach (EventDefinitionHandle item3 in module.GetEventsOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetEventDefNameOrThrow(item3));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		try
		{
			foreach (FieldDefinitionHandle item4 in module.GetFieldsOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetFieldDefNameOrThrow(item4));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		if (IsValueType)
		{
			hashSet.Add(".ctor");
		}
		Interlocked.CompareExchange(ref _lazyMemberNames, CreateReadOnlyMemberNames(hashSet), null);
	}

	private static ICollection<string> CreateReadOnlyMemberNames(HashSet<string> names)
	{
		switch (names.Count)
		{
		case 0:
			return SpecializedCollections.EmptySet<string>();
		case 1:
			return SpecializedCollections.SingletonCollection(names.First());
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
			return ImmutableArray.CreateRange(names);
		default:
			return SpecializedCollections.ReadOnlySet(names);
		}
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		return GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		EnsureAllMembersAreLoaded();
		return _lazyMembersInDeclarationOrder;
	}

	private IEnumerable<FieldSymbol> GetEnumFieldsToEmit()
	{
		UncommonProperties uncommon = GetUncommonProperties();
		if (uncommon == s_noUncommonProperties)
		{
			yield break;
		}
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		ArrayBuilder<FieldDefinitionHandle> fieldDefs = ArrayBuilder<FieldDefinitionHandle>.GetInstance();
		try
		{
			foreach (FieldDefinitionHandle item in module.GetFieldsOfTypeOrThrow(_handle))
			{
				fieldDefs.Add(item);
			}
		}
		catch (BadImageFormatException)
		{
		}
		if (uncommon.lazyInstanceEnumFields.IsDefault)
		{
			ArrayBuilder<PEFieldSymbol> instance = ArrayBuilder<PEFieldSymbol>.GetInstance();
			foreach (FieldDefinitionHandle item2 in fieldDefs)
			{
				try
				{
					FieldAttributes fieldDefFlagsOrThrow = module.GetFieldDefFlagsOrThrow(item2);
					if ((fieldDefFlagsOrThrow & FieldAttributes.Static) == 0 && ModuleExtensions.ShouldImportField(fieldDefFlagsOrThrow, containingPEModule.ImportOptions))
					{
						instance.Add(new PEFieldSymbol(containingPEModule, this, item2));
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
			ImmutableInterlocked.InterlockedInitialize(ref uncommon.lazyInstanceEnumFields, instance.ToImmutableAndFree());
		}
		int staticIndex = 0;
		ImmutableArray<Symbol> staticFields = GetMembers();
		int instanceIndex = 0;
		foreach (FieldDefinitionHandle item3 in fieldDefs)
		{
			if (instanceIndex < uncommon.lazyInstanceEnumFields.Length && uncommon.lazyInstanceEnumFields[instanceIndex].Handle == item3)
			{
				yield return uncommon.lazyInstanceEnumFields[instanceIndex];
				instanceIndex++;
			}
			else if (staticIndex < staticFields.Length && staticFields[staticIndex].Kind == SymbolKind.Field)
			{
				PEFieldSymbol pEFieldSymbol = (PEFieldSymbol)staticFields[staticIndex];
				if (pEFieldSymbol.Handle == item3)
				{
					yield return pEFieldSymbol;
					staticIndex++;
				}
			}
		}
		fieldDefs.Free();
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		if (TypeKind == TypeKind.Enum)
		{
			return GetEnumFieldsToEmit();
		}
		IEnumerable<FieldSymbol> members = GetMembers<FieldSymbol>(GetMembers(), SymbolKind.Field, 0);
		ArrayBuilder<FieldSymbol> arrayBuilder = null;
		foreach (EventSymbol item in GetEventsToEmit())
		{
			FieldSymbol associatedField = item.AssociatedField;
			if ((object)associatedField != null)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<FieldSymbol>.GetInstance();
				}
				arrayBuilder.Add(associatedField);
			}
		}
		if (arrayBuilder == null)
		{
			return members;
		}
		SmallDictionary<FieldDefinitionHandle, FieldSymbol> smallDictionary = new SmallDictionary<FieldDefinitionHandle, FieldSymbol>();
		int num = 0;
		foreach (PEFieldSymbol item2 in members)
		{
			smallDictionary.Add(item2.Handle, item2);
			num++;
		}
		foreach (PEFieldSymbol item3 in arrayBuilder)
		{
			smallDictionary.Add(item3.Handle, item3);
		}
		num += arrayBuilder.Count;
		arrayBuilder.Free();
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(num);
		try
		{
			foreach (FieldDefinitionHandle item4 in ContainingPEModule.Module.GetFieldsOfTypeOrThrow(_handle))
			{
				if (smallDictionary.TryGetValue(item4, out var value))
				{
					instance.Add(value);
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return instance.ToImmutableAndFree();
	}

	internal override IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		ImmutableArray<Symbol> members = GetMembers();
		int index = GetIndexOfFirstMember(members, SymbolKind.Method);
		if (!this.IsInterfaceType())
		{
			for (; index < members.Length && members[index].Kind == SymbolKind.Method; index++)
			{
				MethodSymbol methodSymbol = (MethodSymbol)members[index];
				if (!methodSymbol.IsDefaultValueTypeConstructor())
				{
					yield return methodSymbol;
				}
			}
		}
		else
		{
			if (index >= members.Length || members[index].Kind != SymbolKind.Method)
			{
				yield break;
			}
			PEMethodSymbol method = (PEMethodSymbol)members[index];
			PEModule module = ContainingPEModule.Module;
			ArrayBuilder<MethodDefinitionHandle> methodDefs = ArrayBuilder<MethodDefinitionHandle>.GetInstance();
			try
			{
				foreach (MethodDefinitionHandle item in module.GetMethodsOfTypeOrThrow(_handle))
				{
					methodDefs.Add(item);
				}
			}
			catch (BadImageFormatException)
			{
			}
			foreach (MethodDefinitionHandle item2 in methodDefs)
			{
				if (method.Handle == item2)
				{
					yield return method;
					index++;
					if (index == members.Length || members[index].Kind != SymbolKind.Method)
					{
						methodDefs.Free();
						yield break;
					}
					method = (PEMethodSymbol)members[index];
				}
				else
				{
					int gapSize;
					try
					{
						gapSize = ModuleExtensions.GetVTableGapSize(module.GetMethodDefNameOrThrow(item2));
					}
					catch (BadImageFormatException)
					{
						gapSize = 1;
					}
					do
					{
						yield return null;
						gapSize--;
					}
					while (gapSize > 0);
				}
			}
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		return GetMembers<PropertySymbol>(GetMembers(), SymbolKind.Property);
	}

	internal override IEnumerable<EventSymbol> GetEventsToEmit()
	{
		return GetMembers<EventSymbol>(GetMembers(), SymbolKind.Event);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetMembersUnordered();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return GetMembers(name);
	}

	private void EnsureEnumUnderlyingTypeIsLoaded(UncommonProperties uncommon)
	{
		if ((object)uncommon.lazyEnumUnderlyingType != null || TypeKind != TypeKind.Enum)
		{
			return;
		}
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		MetadataDecoder metadataDecoder = new MetadataDecoder(containingPEModule, this);
		NamedTypeSymbol namedTypeSymbol = null;
		try
		{
			foreach (FieldDefinitionHandle item in module.GetFieldsOfTypeOrThrow(_handle))
			{
				FieldAttributes fieldDefFlagsOrThrow;
				try
				{
					fieldDefFlagsOrThrow = module.GetFieldDefFlagsOrThrow(item);
				}
				catch (BadImageFormatException)
				{
					continue;
				}
				if ((fieldDefFlagsOrThrow & FieldAttributes.Static) == 0)
				{
					TypeSymbol typeSymbol = metadataDecoder.DecodeFieldSignature(item, out var _, out var _);
					if (typeSymbol.SpecialType.IsValidEnumUnderlyingType())
					{
						namedTypeSymbol = (((object)namedTypeSymbol != null) ? new UnsupportedMetadataTypeSymbol() : ((NamedTypeSymbol)typeSymbol));
					}
				}
			}
			if ((object)namedTypeSymbol == null)
			{
				namedTypeSymbol = new UnsupportedMetadataTypeSymbol();
			}
		}
		catch (BadImageFormatException mrEx)
		{
			if ((object)namedTypeSymbol == null)
			{
				namedTypeSymbol = new UnsupportedMetadataTypeSymbol(mrEx);
			}
		}
		Interlocked.CompareExchange(ref uncommon.lazyEnumUnderlyingType, namedTypeSymbol, null);
	}

	private void EnsureAllMembersAreLoaded()
	{
		if (_lazyMembersByName == null)
		{
			LoadMembers();
		}
	}

	private void LoadMembers()
	{
		ArrayBuilder<Symbol> arrayBuilder = null;
		if (_lazyMembersInDeclarationOrder.IsDefault)
		{
			EnsureNestedTypesAreLoaded();
			arrayBuilder = ArrayBuilder<Symbol>.GetInstance();
			if (TypeKind == TypeKind.Enum)
			{
				EnsureEnumUnderlyingTypeIsLoaded(GetUncommonProperties());
				PEModuleSymbol containingPEModule = ContainingPEModule;
				PEModule module = containingPEModule.Module;
				try
				{
					foreach (FieldDefinitionHandle item3 in module.GetFieldsOfTypeOrThrow(_handle))
					{
						FieldAttributes fieldAttributes;
						try
						{
							fieldAttributes = module.GetFieldDefFlagsOrThrow(item3);
							if ((fieldAttributes & FieldAttributes.Static) == 0)
							{
								continue;
							}
						}
						catch (BadImageFormatException)
						{
							fieldAttributes = FieldAttributes.PrivateScope;
						}
						if (ModuleExtensions.ShouldImportField(fieldAttributes, containingPEModule.ImportOptions))
						{
							PEFieldSymbol item = new PEFieldSymbol(containingPEModule, this, item3);
							arrayBuilder.Add(item);
						}
					}
				}
				catch (BadImageFormatException)
				{
				}
				SynthesizedInstanceConstructor item2 = new SynthesizedInstanceConstructor(this);
				arrayBuilder.Add(item2);
			}
			else
			{
				ArrayBuilder<PEFieldSymbol> instance = ArrayBuilder<PEFieldSymbol>.GetInstance();
				ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
				MultiDictionary<string, PEFieldSymbol> privateFieldNameToSymbols = CreateFields(instance);
				PooledDictionary<MethodDefinitionHandle, PEMethodSymbol> pooledDictionary = CreateMethods(instance2);
				if (TypeKind == TypeKind.Struct)
				{
					bool flag = false;
					foreach (MethodSymbol item4 in instance2)
					{
						if (item4.IsParameterlessConstructor())
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						instance2.Insert(0, new SynthesizedInstanceConstructor(this));
					}
				}
				CreateProperties(pooledDictionary, instance2);
				CreateEvents(privateFieldNameToSymbols, pooledDictionary, instance2);
				foreach (PEFieldSymbol item5 in instance)
				{
					if ((object)item5.AssociatedSymbol == null)
					{
						arrayBuilder.Add(item5);
					}
				}
				arrayBuilder.AddRange(instance2);
				instance2.Free();
				instance.Free();
				pooledDictionary.Free();
			}
			int count = arrayBuilder.Count;
			foreach (ImmutableArray<PENamedTypeSymbol> value3 in _lazyNestedTypes.Values)
			{
				arrayBuilder.AddRange(value3);
			}
			arrayBuilder.Sort(count, DeclarationOrderTypeSymbolComparer.Instance);
			ImmutableArray<Symbol> value = arrayBuilder.ToImmutable();
			if (!ImmutableInterlocked.InterlockedInitialize(ref _lazyMembersInDeclarationOrder, value))
			{
				arrayBuilder.Free();
				arrayBuilder = null;
			}
			else
			{
				arrayBuilder.Clip(count);
			}
		}
		if (_lazyMembersByName == null)
		{
			if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<Symbol>.GetInstance();
				foreach (Symbol item6 in _lazyMembersInDeclarationOrder)
				{
					if (item6.Kind == SymbolKind.NamedType)
					{
						break;
					}
					arrayBuilder.Add(item6);
				}
			}
			Dictionary<string, ImmutableArray<Symbol>> dictionary = GroupByName(arrayBuilder);
			if (Interlocked.CompareExchange(ref _lazyMembersByName, dictionary, null) == null)
			{
				ICollection<string> value2 = SpecializedCollections.ReadOnlyCollection(dictionary.Keys);
				Interlocked.Exchange(ref _lazyMemberNames, value2);
			}
		}
		arrayBuilder?.Free();
	}

	internal override ImmutableArray<Symbol> GetSimpleNonTypeMembers(string name)
	{
		EnsureAllMembersAreLoaded();
		if (!_lazyMembersByName.TryGetValue(name, out var value))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return value;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		EnsureAllMembersAreLoaded();
		if (!_lazyMembersByName.TryGetValue(name, out var value))
		{
			value = ImmutableArray<Symbol>.Empty;
		}
		if (_lazyNestedTypes.TryGetValue(name, out var value2))
		{
			value = value.Concat(StaticCast<Symbol>.From(value2));
		}
		return value;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return GetTypeMembers();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		EnsureNestedTypesAreLoaded();
		return GetMemberTypesPrivate();
	}

	private ImmutableArray<NamedTypeSymbol> GetMemberTypesPrivate()
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		foreach (ImmutableArray<PENamedTypeSymbol> value in _lazyNestedTypes.Values)
		{
			instance.AddRange(value);
		}
		return instance.ToImmutableAndFree();
	}

	private void EnsureNestedTypesAreLoaded()
	{
		if (_lazyNestedTypes == null)
		{
			ArrayBuilder<PENamedTypeSymbol> instance = ArrayBuilder<PENamedTypeSymbol>.GetInstance();
			instance.AddRange(CreateNestedTypes());
			Dictionary<string, ImmutableArray<PENamedTypeSymbol>> dictionary = GroupByName(instance);
			if (Interlocked.CompareExchange(ref _lazyNestedTypes, dictionary, null) == null)
			{
				ContainingPEModule.OnNewTypeDeclarationsLoaded(dictionary);
			}
			instance.Free();
		}
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		EnsureNestedTypesAreLoaded();
		if (_lazyNestedTypes.TryGetValue(name, out var value))
		{
			return StaticCast<NamedTypeSymbol>.From(value);
		}
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol type) => type.Arity == arity);
	}

	public sealed override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}

	private static ExtendedErrorTypeSymbol CyclicInheritanceError(PENamedTypeSymbol type, TypeSymbol declaredBase)
	{
		CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_ImportedCircularBase, declaredBase, type);
		return new ExtendedErrorTypeSymbol(declaredBase, LookupResultKind.NotReferencable, errorInfo, unreported: true);
	}

	private NamedTypeSymbol MakeAcyclicBaseType()
	{
		NamedTypeSymbol declaredBaseType = GetDeclaredBaseType(null);
		if ((object)declaredBaseType == null)
		{
			return null;
		}
		if (BaseTypeAnalysis.ClassDependsOn(declaredBaseType, this))
		{
			return CyclicInheritanceError(this, declaredBaseType);
		}
		SetKnownToHaveNoDeclaredBaseCycles();
		return declaredBaseType;
	}

	private ImmutableArray<NamedTypeSymbol> MakeAcyclicInterfaces()
	{
		ImmutableArray<NamedTypeSymbol> declaredInterfaces = GetDeclaredInterfaces(null);
		if (!IsInterface)
		{
			return declaredInterfaces;
		}
		return declaredInterfaces.SelectAsArray((NamedTypeSymbol t) => (!BaseTypeAnalysis.InterfaceDependsOn(t, this)) ? t : CyclicInheritanceError(this, t));
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return PEDocumentationCommentUtils.GetDocumentationComment(this, ContainingPEModule, preferredCulture, cancellationToken, ref _lazyDocComment);
	}

	private IEnumerable<PENamedTypeSymbol> CreateNestedTypes()
	{
		PEModuleSymbol moduleSymbol = ContainingPEModule;
		PEModule module = moduleSymbol.Module;
		ImmutableArray<TypeDefinitionHandle> nestedTypeDefsOrThrow;
		try
		{
			nestedTypeDefsOrThrow = module.GetNestedTypeDefsOrThrow(_handle);
		}
		catch (BadImageFormatException)
		{
			yield break;
		}
		foreach (TypeDefinitionHandle item in nestedTypeDefsOrThrow)
		{
			if (module.ShouldImportNestedType(item))
			{
				yield return Create(moduleSymbol, this, item);
			}
		}
	}

	private MultiDictionary<string, PEFieldSymbol> CreateFields(ArrayBuilder<PEFieldSymbol> fieldMembers)
	{
		MultiDictionary<string, PEFieldSymbol> multiDictionary = new MultiDictionary<string, PEFieldSymbol>();
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		bool flag = false;
		bool flag2 = false;
		if (TypeKind == TypeKind.Struct)
		{
			if (SpecialType == SpecialType.None)
			{
				flag = true;
				flag2 = ContainingAssembly.IsLinked;
			}
			else
			{
				flag = SpecialType == SpecialType.System_Nullable_T;
			}
		}
		try
		{
			foreach (FieldDefinitionHandle item in module.GetFieldsOfTypeOrThrow(_handle))
			{
				try
				{
					if (!flag2 && (!flag || (module.GetFieldDefFlagsOrThrow(item) & FieldAttributes.Static) != 0) && !module.ShouldImportField(item, containingPEModule.ImportOptions))
					{
						continue;
					}
				}
				catch (BadImageFormatException)
				{
				}
				PEFieldSymbol pEFieldSymbol = new PEFieldSymbol(containingPEModule, this, item);
				fieldMembers.Add(pEFieldSymbol);
				if (pEFieldSymbol.DeclaredAccessibility == Accessibility.Private)
				{
					string name = pEFieldSymbol.Name;
					if (name.Length > 0)
					{
						multiDictionary.Add(name, pEFieldSymbol);
					}
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return multiDictionary;
	}

	private PooledDictionary<MethodDefinitionHandle, PEMethodSymbol> CreateMethods(ArrayBuilder<Symbol> members)
	{
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		PooledDictionary<MethodDefinitionHandle, PEMethodSymbol> instance = PooledDictionary<MethodDefinitionHandle, PEMethodSymbol>.GetInstance();
		bool flag = TypeKind == TypeKind.Struct && SpecialType == SpecialType.None && ContainingAssembly.IsLinked;
		try
		{
			foreach (MethodDefinitionHandle item in module.GetMethodsOfTypeOrThrow(_handle))
			{
				if (flag || module.ShouldImportMethod(item, containingPEModule.ImportOptions))
				{
					PEMethodSymbol pEMethodSymbol = new PEMethodSymbol(containingPEModule, this, item);
					members.Add(pEMethodSymbol);
					instance.Add(item, pEMethodSymbol);
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return instance;
	}

	private void CreateProperties(Dictionary<MethodDefinitionHandle, PEMethodSymbol> methodHandleToSymbol, ArrayBuilder<Symbol> members)
	{
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		try
		{
			foreach (PropertyDefinitionHandle item in module.GetPropertiesOfTypeOrThrow(_handle))
			{
				try
				{
					PropertyAccessors propertyMethodsOrThrow = module.GetPropertyMethodsOrThrow(item);
					PEMethodSymbol accessorMethod = GetAccessorMethod(module, methodHandleToSymbol, propertyMethodsOrThrow.Getter);
					PEMethodSymbol accessorMethod2 = GetAccessorMethod(module, methodHandleToSymbol, propertyMethodsOrThrow.Setter);
					if ((object)accessorMethod != null || (object)accessorMethod2 != null)
					{
						members.Add(PEPropertySymbol.Create(containingPEModule, this, item, accessorMethod, accessorMethod2));
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
	}

	private void CreateEvents(MultiDictionary<string, PEFieldSymbol> privateFieldNameToSymbols, Dictionary<MethodDefinitionHandle, PEMethodSymbol> methodHandleToSymbol, ArrayBuilder<Symbol> members)
	{
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		try
		{
			foreach (EventDefinitionHandle item in module.GetEventsOfTypeOrThrow(_handle))
			{
				try
				{
					EventAccessors eventMethodsOrThrow = module.GetEventMethodsOrThrow(item);
					PEMethodSymbol accessorMethod = GetAccessorMethod(module, methodHandleToSymbol, eventMethodsOrThrow.Adder);
					PEMethodSymbol accessorMethod2 = GetAccessorMethod(module, methodHandleToSymbol, eventMethodsOrThrow.Remover);
					if ((object)accessorMethod != null || (object)accessorMethod2 != null)
					{
						members.Add(new PEEventSymbol(containingPEModule, this, item, accessorMethod, accessorMethod2, privateFieldNameToSymbols));
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
	}

	private PEMethodSymbol GetAccessorMethod(PEModule module, Dictionary<MethodDefinitionHandle, PEMethodSymbol> methodHandleToSymbol, MethodDefinitionHandle methodDef)
	{
		if (methodDef.IsNil)
		{
			return null;
		}
		methodHandleToSymbol.TryGetValue(methodDef, out var value);
		return value;
	}

	private static Dictionary<string, ImmutableArray<Symbol>> GroupByName(ArrayBuilder<Symbol> symbols)
	{
		return symbols.ToDictionary((Symbol s) => s.Name, StringOrdinalComparer.Instance);
	}

	private static Dictionary<string, ImmutableArray<PENamedTypeSymbol>> GroupByName(ArrayBuilder<PENamedTypeSymbol> symbols)
	{
		if (symbols.Count == 0)
		{
			return s_emptyNestedTypes;
		}
		return symbols.ToDictionary((PENamedTypeSymbol s) => s.Name, StringOrdinalComparer.Instance);
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (_lazyUseSiteDiagnostic == CSDiagnosticInfo.EmptyErrorInfo)
		{
			_lazyUseSiteDiagnostic = GetUseSiteDiagnosticImpl();
		}
		return _lazyUseSiteDiagnostic;
	}

	protected virtual DiagnosticInfo GetUseSiteDiagnosticImpl()
	{
		DiagnosticInfo result = null;
		if (!MergeUseSiteDiagnostics(ref result, CalculateUseSiteDiagnostic()))
		{
			if (ContainingPEModule.Module.HasRequiredAttributeAttribute(_handle))
			{
				result = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, this);
			}
			else if (TypeKind == TypeKind.Class && SpecialType != SpecialType.System_Enum)
			{
				TypeSymbol declaredBaseType = GetDeclaredBaseType(null);
				if ((object)declaredBaseType != null && declaredBaseType.SpecialType == SpecialType.None)
				{
					AssemblySymbol containingAssembly = declaredBaseType.ContainingAssembly;
					if ((object)containingAssembly != null && containingAssembly.IsMissing && declaredBaseType is MissingMetadataTypeSymbol.TopLevel { Arity: 0 } topLevel)
					{
						SpecialType typeFromMetadataName = SpecialTypes.GetTypeFromMetadataName(MetadataHelpers.BuildQualifiedName(topLevel.NamespaceName, topLevel.MetadataName));
						if ((uint)(typeFromMetadataName - 2) <= 1u || typeFromMetadataName == SpecialType.System_ValueType)
						{
							result = topLevel.GetUseSiteDiagnostic();
						}
					}
				}
			}
		}
		return result;
	}

	internal override bool GetGuidString(out string guidString)
	{
		return ContainingPEModule.Module.HasGuidAttribute(_handle, out guidString);
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		UncommonProperties uncommonProperties = GetUncommonProperties();
		if (uncommonProperties == s_noUncommonProperties)
		{
			return ImmutableArray<string>.Empty;
		}
		if (uncommonProperties.lazyConditionalAttributeSymbols.IsDefault)
		{
			ImmutableArray<string> conditionalAttributeValues = ContainingPEModule.Module.GetConditionalAttributeValues(_handle);
			ImmutableInterlocked.InterlockedCompareExchange(ref uncommonProperties.lazyConditionalAttributeSymbols, conditionalAttributeValues, default);
		}
		return uncommonProperties.lazyConditionalAttributeSymbols;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		UncommonProperties uncommonProperties = GetUncommonProperties();
		if (uncommonProperties == s_noUncommonProperties)
		{
			if ((object)BaseTypeNoUseSiteDiagnostics == null)
			{
				return AttributeUsageInfo.Default;
			}
			return BaseTypeNoUseSiteDiagnostics.GetAttributeUsageInfo();
		}
		if (uncommonProperties.lazyAttributeUsageInfo.IsNull)
		{
			uncommonProperties.lazyAttributeUsageInfo = DecodeAttributeUsageInfo();
		}
		return uncommonProperties.lazyAttributeUsageInfo;
	}

	private AttributeUsageInfo DecodeAttributeUsageInfo()
	{
		CustomAttributeHandle attributeUsageAttributeHandle = ContainingPEModule.Module.GetAttributeUsageAttributeHandle(_handle);
		if (!attributeUsageAttributeHandle.IsNil && new MetadataDecoder(ContainingPEModule).GetCustomAttribute(attributeUsageAttributeHandle, out TypedConstant[] positionalArgs, out KeyValuePair<string, TypedConstant>[] namedArgs))
		{
			AttributeUsageInfo result = AttributeData.DecodeAttributeUsageAttribute(positionalArgs[0], namedArgs.AsImmutableOrNull());
			if (!result.HasValidAttributeTargets)
			{
				return AttributeUsageInfo.Default;
			}
			return result;
		}
		if ((object)BaseTypeNoUseSiteDiagnostics == null)
		{
			return AttributeUsageInfo.Default;
		}
		return BaseTypeNoUseSiteDiagnostics.GetAttributeUsageInfo();
	}

	private static int GetIndexOfFirstMember(ImmutableArray<Symbol> members, SymbolKind kind)
	{
		int length = members.Length;
		for (int i = 0; i < length; i++)
		{
			if (members[i].Kind == kind)
			{
				return i;
			}
		}
		return length;
	}

	private static IEnumerable<TSymbol> GetMembers<TSymbol>(ImmutableArray<Symbol> members, SymbolKind kind, int offset = -1) where TSymbol : Symbol
	{
		if (offset < 0)
		{
			offset = GetIndexOfFirstMember(members, kind);
		}
		int n = members.Length;
		for (int i = offset; i < n; i++)
		{
			Symbol symbol = members[i];
			if (symbol.Kind != kind)
			{
				break;
			}
			yield return (TSymbol)symbol;
		}
	}
}
