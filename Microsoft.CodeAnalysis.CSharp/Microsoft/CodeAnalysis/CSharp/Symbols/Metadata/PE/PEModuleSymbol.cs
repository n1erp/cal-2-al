using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PEModuleSymbol : NonMissingModuleSymbol
{
	private readonly AssemblySymbol _assemblySymbol;

	private readonly int _ordinal;

	private readonly PEModule _module;

	private readonly PENamespaceSymbol _globalNamespace;

	private NamedTypeSymbol _lazySystemTypeSymbol;

	private NamedTypeSymbol _lazyEventRegistrationTokenSymbol;

	private NamedTypeSymbol _lazyEventRegistrationTokenTableSymbol;

	private const int DefaultTypeMapCapacity = 31;

	internal readonly ConcurrentDictionary<TypeDefinitionHandle, TypeSymbol> TypeHandleToTypeMap = new ConcurrentDictionary<TypeDefinitionHandle, TypeSymbol>(2, 31);

	internal readonly ConcurrentDictionary<TypeReferenceHandle, TypeSymbol> TypeRefHandleToTypeMap = new ConcurrentDictionary<TypeReferenceHandle, TypeSymbol>(2, 31);

	internal readonly ImmutableArray<MetadataLocation> MetadataLocation;

	internal readonly MetadataImportOptions ImportOptions;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private ImmutableArray<CSharpAttributeData> _lazyAssemblyAttributes;

	private ICollection<string> _lazyTypeNames;

	private ICollection<string> _lazyNamespaceNames;

	internal override int Ordinal => _ordinal;

	internal override Machine Machine => _module.Machine;

	internal override bool Bit32Required => _module.Bit32Required;

	internal PEModule Module => _module;

	public override NamespaceSymbol GlobalNamespace => _globalNamespace;

	public override string Name => _module.Name;

	private static EntityHandle Token => EntityHandle.ModuleDefinition;

	public override Symbol ContainingSymbol => _assemblySymbol;

	public override AssemblySymbol ContainingAssembly => _assemblySymbol;

	public override ImmutableArray<Location> Locations => MetadataLocation.Cast<MetadataLocation, Location>();

	internal override ICollection<string> TypeNames
	{
		get
		{
			if (_lazyTypeNames == null)
			{
				Interlocked.CompareExchange(ref _lazyTypeNames, _module.TypeNames.AsCaseSensitiveCollection(), null);
			}
			return _lazyTypeNames;
		}
	}

	internal override ICollection<string> NamespaceNames
	{
		get
		{
			if (_lazyNamespaceNames == null)
			{
				Interlocked.CompareExchange(ref _lazyNamespaceNames, _module.NamespaceNames.AsCaseSensitiveCollection(), null);
			}
			return _lazyNamespaceNames;
		}
	}

	internal DocumentationProvider DocumentationProvider
	{
		get
		{
			if (_assemblySymbol is PEAssemblySymbol pEAssemblySymbol)
			{
				return pEAssemblySymbol.DocumentationProvider;
			}
			return DocumentationProvider.Default;
		}
	}

	internal NamedTypeSymbol EventRegistrationToken
	{
		get
		{
			if ((object)_lazyEventRegistrationTokenSymbol == null)
			{
				Interlocked.CompareExchange(ref _lazyEventRegistrationTokenSymbol, GetTypeSymbolForWellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationToken), null);
			}
			return _lazyEventRegistrationTokenSymbol;
		}
	}

	internal NamedTypeSymbol EventRegistrationTokenTable_T
	{
		get
		{
			if ((object)_lazyEventRegistrationTokenTableSymbol == null)
			{
				Interlocked.CompareExchange(ref _lazyEventRegistrationTokenTableSymbol, GetTypeSymbolForWellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T), null);
			}
			return _lazyEventRegistrationTokenTableSymbol;
		}
	}

	internal NamedTypeSymbol SystemTypeSymbol
	{
		get
		{
			if ((object)_lazySystemTypeSymbol == null)
			{
				Interlocked.CompareExchange(ref _lazySystemTypeSymbol, GetTypeSymbolForWellKnownType(WellKnownType.System_Type), null);
			}
			return _lazySystemTypeSymbol;
		}
	}

	internal override bool HasAssemblyCompilationRelaxationsAttribute => GetAssemblyAttributes().IndexOfAttribute(this, AttributeDescription.CompilationRelaxationsAttribute) >= 0;

	internal override bool HasAssemblyRuntimeCompatibilityAttribute => GetAssemblyAttributes().IndexOfAttribute(this, AttributeDescription.RuntimeCompatibilityAttribute) >= 0;

	internal override CharSet? DefaultMarshallingCharSet
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal PEModuleSymbol(PEAssemblySymbol assemblySymbol, PEModule module, MetadataImportOptions importOptions, int ordinal)
		: this((AssemblySymbol)assemblySymbol, module, importOptions, ordinal)
	{
	}

	internal PEModuleSymbol(SourceAssemblySymbol assemblySymbol, PEModule module, MetadataImportOptions importOptions, int ordinal)
		: this((AssemblySymbol)assemblySymbol, module, importOptions, ordinal)
	{
	}

	internal PEModuleSymbol(RetargetingAssemblySymbol assemblySymbol, PEModule module, MetadataImportOptions importOptions, int ordinal)
		: this((AssemblySymbol)assemblySymbol, module, importOptions, ordinal)
	{
	}

	private PEModuleSymbol(AssemblySymbol assemblySymbol, PEModule module, MetadataImportOptions importOptions, int ordinal)
	{
		_assemblySymbol = assemblySymbol;
		_ordinal = ordinal;
		_module = module;
		ImportOptions = importOptions;
		_globalNamespace = new PEGlobalNamespaceSymbol(this);
		MetadataLocation = ImmutableArray.Create(new MetadataLocation(this));
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_lazyCustomAttributes.IsDefault)
		{
			LoadCustomAttributes(Token, ref _lazyCustomAttributes);
		}
		return _lazyCustomAttributes;
	}

	internal ImmutableArray<CSharpAttributeData> GetAssemblyAttributes()
	{
		if (_lazyAssemblyAttributes.IsDefault)
		{
			ArrayBuilder<CSharpAttributeData> arrayBuilder = null;
			string name = ContainingAssembly.CorLibrary.Name;
			EntityHandle resolutionScope = Module.GetAssemblyRef(name);
			if (!resolutionScope.IsNil)
			{
				string[,] dummyAssemblyAttributeParentQualifier = MetadataWriter.dummyAssemblyAttributeParentQualifier;
				foreach (string text in dummyAssemblyAttributeParentQualifier)
				{
					EntityHandle typeRef = Module.GetTypeRef(resolutionScope, "System.Runtime.CompilerServices", "AssemblyAttributesGoHere" + text);
					if (typeRef.IsNil)
					{
						continue;
					}
					try
					{
						foreach (CustomAttributeHandle item in Module.GetCustomAttributesOrThrow(typeRef))
						{
							if (arrayBuilder == null)
							{
								arrayBuilder = new ArrayBuilder<CSharpAttributeData>();
							}
							arrayBuilder.Add(new PEAttributeData(this, item));
						}
					}
					catch (BadImageFormatException)
					{
					}
				}
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyAssemblyAttributes, arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<CSharpAttributeData>.Empty, default);
		}
		return _lazyAssemblyAttributes;
	}

	internal void LoadCustomAttributes(EntityHandle token, ref ImmutableArray<CSharpAttributeData> customAttributes)
	{
		ImmutableArray<CSharpAttributeData> customAttributesForToken = GetCustomAttributesForToken(token);
		ImmutableInterlocked.InterlockedInitialize(ref customAttributes, customAttributesForToken);
	}

	internal void LoadCustomAttributesFilterExtensions(EntityHandle token, ref ImmutableArray<CSharpAttributeData> customAttributes, out bool foundExtension)
	{
		ImmutableArray<CSharpAttributeData> customAttributesFilterExtensions = GetCustomAttributesFilterExtensions(token, out foundExtension);
		ImmutableInterlocked.InterlockedInitialize(ref customAttributes, customAttributesFilterExtensions);
	}

	internal void LoadCustomAttributesFilterExtensions(EntityHandle token, ref ImmutableArray<CSharpAttributeData> customAttributes)
	{
		ImmutableArray<CSharpAttributeData> customAttributesFilterExtensions = GetCustomAttributesFilterExtensions(token, out var _);
		ImmutableInterlocked.InterlockedInitialize(ref customAttributes, customAttributesFilterExtensions);
	}

	internal ImmutableArray<CSharpAttributeData> GetCustomAttributesForToken(EntityHandle token, out CustomAttributeHandle filteredOutAttribute1, AttributeDescription filterOut1)
	{
		CustomAttributeHandle filteredOutAttribute2;
		CustomAttributeHandle filteredOutAttribute3;
		CustomAttributeHandle filteredOutAttribute4;
		return GetCustomAttributesForToken(token, out filteredOutAttribute1, filterOut1, out filteredOutAttribute2, default, out filteredOutAttribute3, default, out filteredOutAttribute4, default);
	}

	internal ImmutableArray<CSharpAttributeData> GetCustomAttributesForToken(EntityHandle token, out CustomAttributeHandle filteredOutAttribute1, AttributeDescription filterOut1, out CustomAttributeHandle filteredOutAttribute2, AttributeDescription filterOut2, out CustomAttributeHandle filteredOutAttribute3, AttributeDescription filterOut3, out CustomAttributeHandle filteredOutAttribute4, AttributeDescription filterOut4)
	{
		filteredOutAttribute1 = default;
		filteredOutAttribute2 = default;
		filteredOutAttribute3 = default;
		filteredOutAttribute4 = default;
		ArrayBuilder<CSharpAttributeData> arrayBuilder = null;
		try
		{
			foreach (CustomAttributeHandle item in _module.GetCustomAttributesOrThrow(token))
			{
				if (matchesFilter(item, filterOut1))
				{
					filteredOutAttribute1 = item;
					continue;
				}
				if (matchesFilter(item, filterOut2))
				{
					filteredOutAttribute2 = item;
					continue;
				}
				if (matchesFilter(item, filterOut3))
				{
					filteredOutAttribute3 = item;
					continue;
				}
				if (matchesFilter(item, filterOut4))
				{
					filteredOutAttribute4 = item;
					continue;
				}
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<CSharpAttributeData>.GetInstance();
				}
				arrayBuilder.Add(new PEAttributeData(this, item));
			}
		}
		catch (BadImageFormatException)
		{
		}
		return arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<CSharpAttributeData>.Empty;
		bool matchesFilter(CustomAttributeHandle handle, AttributeDescription filter)
		{
			if (filter.Signatures != null)
			{
				return Module.GetTargetAttributeSignatureIndex(handle, filter) != -1;
			}
			return false;
		}
	}

	internal ImmutableArray<CSharpAttributeData> GetCustomAttributesForToken(EntityHandle token)
	{
		CustomAttributeHandle filteredOutAttribute;
		return GetCustomAttributesForToken(token, out filteredOutAttribute, default);
	}

	internal ImmutableArray<CSharpAttributeData> GetCustomAttributesForToken(EntityHandle token, out CustomAttributeHandle paramArrayAttribute)
	{
		return GetCustomAttributesForToken(token, out paramArrayAttribute, AttributeDescription.ParamArrayAttribute);
	}

	internal bool HasAnyCustomAttributes(EntityHandle token)
	{
		try
		{
			using CustomAttributeHandleCollection.Enumerator enumerator = _module.GetCustomAttributesOrThrow(token).GetEnumerator();
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		catch (BadImageFormatException)
		{
		}
		return false;
	}

	internal TypeSymbol TryDecodeAttributeWithTypeArgument(EntityHandle handle, AttributeDescription attributeDescription)
	{
		if (_module.HasStringValuedAttribute(handle, attributeDescription, out var value))
		{
			return new MetadataDecoder(this).GetTypeSymbolForSerializedType(value);
		}
		return null;
	}

	internal ImmutableArray<CSharpAttributeData> GetCustomAttributesFilterExtensions(EntityHandle token, out bool foundExtension)
	{
		ImmutableArray<CSharpAttributeData> customAttributesForToken = GetCustomAttributesForToken(token, out var filteredOutAttribute, AttributeDescription.CaseSensitiveExtensionAttribute);
		foundExtension = !filteredOutAttribute.IsNil;
		return customAttributesForToken;
	}

	internal void OnNewTypeDeclarationsLoaded(Dictionary<string, ImmutableArray<PENamedTypeSymbol>> typesDict)
	{
		bool flag = _ordinal == 0 && _assemblySymbol.KeepLookingForDeclaredSpecialTypes;
		foreach (ImmutableArray<PENamedTypeSymbol> value in typesDict.Values)
		{
			foreach (PENamedTypeSymbol item in value)
			{
				TypeHandleToTypeMap.TryAdd(item.Handle, item);
				if (flag && item.SpecialType != SpecialType.None)
				{
					_assemblySymbol.RegisterDeclaredSpecialType(item);
					flag = _assemblySymbol.KeepLookingForDeclaredSpecialTypes;
				}
			}
		}
	}

	internal override ImmutableArray<byte> GetHash(AssemblyHashAlgorithm algorithmId)
	{
		return _module.GetHash(algorithmId);
	}

	private NamedTypeSymbol GetTypeSymbolForWellKnownType(WellKnownType type)
	{
		MetadataTypeName emittedName = MetadataTypeName.FromFullName(type.GetMetadataName(), useCLSCompliantNameArityEncoding: true);
		NamedTypeSymbol namedTypeSymbol = LookupTopLevelMetadataType(ref emittedName);
		if (IsAcceptableSystemTypeSymbol(namedTypeSymbol))
		{
			return namedTypeSymbol;
		}
		NamedTypeSymbol namedTypeSymbol2 = null;
		foreach (AssemblySymbol referencedAssemblySymbol in GetReferencedAssemblySymbols())
		{
			NamedTypeSymbol namedTypeSymbol3 = referencedAssemblySymbol.LookupTopLevelMetadataType(ref emittedName, digThroughForwardedTypes: true);
			if (!IsAcceptableSystemTypeSymbol(namedTypeSymbol3))
			{
				continue;
			}
			if ((object)namedTypeSymbol2 == null)
			{
				namedTypeSymbol2 = namedTypeSymbol3;
				continue;
			}
			if (namedTypeSymbol2 != namedTypeSymbol3)
			{
				namedTypeSymbol2 = null;
			}
			break;
		}
		if ((object)namedTypeSymbol2 != null)
		{
			return namedTypeSymbol2;
		}
		return namedTypeSymbol;
	}

	private static bool IsAcceptableSystemTypeSymbol(NamedTypeSymbol candidate)
	{
		if (candidate.Kind == SymbolKind.ErrorType)
		{
			return !(candidate is MissingMetadataTypeSymbol);
		}
		return true;
	}

	internal NamedTypeSymbol LookupTopLevelMetadataType(ref MetadataTypeName emittedName, out bool isNoPiaLocalType)
	{
		PENamespaceSymbol pENamespaceSymbol = (PENamespaceSymbol)GlobalNamespace.LookupNestedNamespace(emittedName.NamespaceSegments);
		if ((object)pENamespaceSymbol == null)
		{
			isNoPiaLocalType = false;
			return new MissingMetadataTypeSymbol.TopLevel(this, ref emittedName);
		}
		return pENamespaceSymbol.LookupMetadataType(ref emittedName, out isNoPiaLocalType);
	}

	internal (AssemblySymbol FirstSymbol, AssemblySymbol SecondSymbol) GetAssembliesForForwardedType(ref MetadataTypeName fullName)
	{
		var (num, num2) = Module.GetAssemblyRefsForForwardedType(fullName.FullName, ignoreCase: false, out var _);
		if (num < 0)
		{
			return (FirstSymbol: null, SecondSymbol: null);
		}
		AssemblySymbol referencedAssemblySymbol = GetReferencedAssemblySymbol(num);
		if (num2 < 0)
		{
			return (FirstSymbol: referencedAssemblySymbol, SecondSymbol: null);
		}
		AssemblySymbol referencedAssemblySymbol2 = GetReferencedAssemblySymbol(num2);
		return (FirstSymbol: referencedAssemblySymbol, SecondSymbol: referencedAssemblySymbol2);
	}

	internal IEnumerable<NamedTypeSymbol> GetForwardedTypes()
	{
		foreach (KeyValuePair<string, (int, int)> forwardedType in Module.GetForwardedTypes())
		{
			MetadataTypeName emittedName = MetadataTypeName.FromFullName(forwardedType.Key);
			AssemblySymbol referencedAssemblySymbol = GetReferencedAssemblySymbol(forwardedType.Value.Item1);
			if (forwardedType.Value.Item2 >= 0)
			{
				AssemblySymbol referencedAssemblySymbol2 = GetReferencedAssemblySymbol(forwardedType.Value.Item2);
				yield return ContainingAssembly.CreateMultipleForwardingErrorTypeSymbol(ref emittedName, this, referencedAssemblySymbol, referencedAssemblySymbol2);
			}
			else
			{
				yield return referencedAssemblySymbol.LookupTopLevelMetadataType(ref emittedName, digThroughForwardedTypes: true);
			}
		}
	}

	public override ModuleMetadata GetMetadata()
	{
		return _module.GetNonDisposableMetadata();
	}
}
