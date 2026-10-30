using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit;

public sealed class EmitBaseline
{
	internal sealed class MetadataSymbols
	{
		public readonly IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> AnonymousTypes;

		public readonly ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> AssemblyReferenceIdentityMap;

		public readonly object MetadataDecoder;

		public MetadataSymbols(IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypes, object metadataDecoder, ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> assemblyReferenceIdentityMap)
		{
			AnonymousTypes = anonymousTypes;
			MetadataDecoder = metadataDecoder;
			AssemblyReferenceIdentityMap = assemblyReferenceIdentityMap;
		}
	}

	private static readonly ImmutableArray<int> s_emptyTableSizes = ImmutableArray.Create(new int[MetadataTokens.TableCount]);

	internal MetadataSymbols LazyMetadataSymbols;

	internal readonly Compilation Compilation;

	internal readonly CommonPEModuleBuilder PEModuleBuilder;

	internal readonly Guid ModuleVersionId;

	internal readonly bool HasPortablePdb;

	internal readonly int Ordinal;

	internal readonly Guid EncId;

	internal readonly IReadOnlyDictionary<ITypeDefinition, int> TypesAdded;

	internal readonly IReadOnlyDictionary<IEventDefinition, int> EventsAdded;

	internal readonly IReadOnlyDictionary<IFieldDefinition, int> FieldsAdded;

	internal readonly IReadOnlyDictionary<IMethodDefinition, int> MethodsAdded;

	internal readonly IReadOnlyDictionary<IPropertyDefinition, int> PropertiesAdded;

	internal readonly IReadOnlyDictionary<int, int> EventMapAdded;

	internal readonly IReadOnlyDictionary<int, int> PropertyMapAdded;

	internal readonly IReadOnlyDictionary<MethodImplKey, int> MethodImplsAdded;

	internal readonly ImmutableArray<int> TableEntriesAdded;

	internal readonly int BlobStreamLengthAdded;

	internal readonly int StringStreamLengthAdded;

	internal readonly int UserStringStreamLengthAdded;

	internal readonly int GuidStreamLengthAdded;

	internal readonly IReadOnlyDictionary<int, AddedOrChangedMethodInfo> AddedOrChangedMethods;

	internal readonly Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> DebugInformationProvider;

	internal readonly Func<MethodDefinitionHandle, StandaloneSignatureHandle> LocalSignatureProvider;

	internal readonly ImmutableArray<int> TableSizes;

	internal readonly IReadOnlyDictionary<int, int> TypeToEventMap;

	internal readonly IReadOnlyDictionary<int, int> TypeToPropertyMap;

	internal readonly IReadOnlyDictionary<MethodImplKey, int> MethodImpls;

	private readonly IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> _anonymousTypeMap;

	internal readonly ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> SynthesizedMembers;

	internal EmitBaseline InitialBaseline { get; }

	public ModuleMetadata OriginalMetadata { get; }

	internal IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> AnonymousTypeMap
	{
		get
		{
			if (Ordinal > 0)
			{
				return _anonymousTypeMap;
			}
			return LazyMetadataSymbols.AnonymousTypes;
		}
	}

	internal MetadataReader MetadataReader => OriginalMetadata.MetadataReader;

	internal int BlobStreamLength => BlobStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.Blob);

	internal int StringStreamLength => StringStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.String);

	internal int UserStringStreamLength => UserStringStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.UserString);

	internal int GuidStreamLength => GuidStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.Guid);

	public static EmitBaseline CreateInitialBaseline(ModuleMetadata module, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		if (!module.Module.HasIL)
		{
			throw new ArgumentException(CodeAnalysisResources.PEImageNotAvailable, "module");
		}
		return CreateInitialBaseline(hasPortableDebugInformation: module.Module.PEReaderOpt.ReadDebugDirectory().Any((DebugDirectoryEntry entry) => entry.IsPortableCodeView), localSignatureProvider: (MethodDefinitionHandle methodHandle) =>
		{
			try
			{
				return module.Module.GetMethodBodyOrThrow(methodHandle)?.LocalSignature ?? default(StandaloneSignatureHandle);
			}
			catch (Exception ex) when (ex is BadImageFormatException || ex is IOException)
			{
				throw new InvalidDataException(ex.Message, ex);
			}
		}, module: module, debugInformationProvider: debugInformationProvider);
	}

	public static EmitBaseline CreateInitialBaseline(ModuleMetadata module, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider, Func<MethodDefinitionHandle, StandaloneSignatureHandle> localSignatureProvider, bool hasPortableDebugInformation)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		if (debugInformationProvider == null)
		{
			throw new ArgumentNullException("debugInformationProvider");
		}
		if (localSignatureProvider == null)
		{
			throw new ArgumentNullException("localSignatureProvider");
		}
		MetadataReader metadataReader = module.MetadataReader;
		return new EmitBaseline(null, module, null, null, module.GetModuleVersionId(), 0, default, hasPortableDebugInformation, new Dictionary<ITypeDefinition, int>(), new Dictionary<IEventDefinition, int>(), new Dictionary<IFieldDefinition, int>(), new Dictionary<IMethodDefinition, int>(), new Dictionary<IPropertyDefinition, int>(), new Dictionary<int, int>(), new Dictionary<int, int>(), new Dictionary<MethodImplKey, int>(), s_emptyTableSizes, 0, 0, 0, 0, null, ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>>.Empty, new Dictionary<int, AddedOrChangedMethodInfo>(), debugInformationProvider, localSignatureProvider, CalculateTypeEventMap(metadataReader), CalculateTypePropertyMap(metadataReader), CalculateMethodImpls(metadataReader));
	}

	private EmitBaseline(EmitBaseline initialBaseline, ModuleMetadata module, Compilation compilation, CommonPEModuleBuilder moduleBuilder, Guid moduleVersionId, int ordinal, Guid encId, bool hasPortablePdb, IReadOnlyDictionary<ITypeDefinition, int> typesAdded, IReadOnlyDictionary<IEventDefinition, int> eventsAdded, IReadOnlyDictionary<IFieldDefinition, int> fieldsAdded, IReadOnlyDictionary<IMethodDefinition, int> methodsAdded, IReadOnlyDictionary<IPropertyDefinition, int> propertiesAdded, IReadOnlyDictionary<int, int> eventMapAdded, IReadOnlyDictionary<int, int> propertyMapAdded, IReadOnlyDictionary<MethodImplKey, int> methodImplsAdded, ImmutableArray<int> tableEntriesAdded, int blobStreamLengthAdded, int stringStreamLengthAdded, int userStringStreamLengthAdded, int guidStreamLengthAdded, IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypeMap, ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> synthesizedMembers, IReadOnlyDictionary<int, AddedOrChangedMethodInfo> methodsAddedOrChanged, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider, Func<MethodDefinitionHandle, StandaloneSignatureHandle> localSignatureProvider, IReadOnlyDictionary<int, int> typeToEventMap, IReadOnlyDictionary<int, int> typeToPropertyMap, IReadOnlyDictionary<MethodImplKey, int> methodImpls)
	{
		MetadataReader metadataReader = module.Module.MetadataReader;
		InitialBaseline = initialBaseline ?? this;
		OriginalMetadata = module;
		Compilation = compilation;
		PEModuleBuilder = moduleBuilder;
		ModuleVersionId = moduleVersionId;
		Ordinal = ordinal;
		EncId = encId;
		HasPortablePdb = hasPortablePdb;
		TypesAdded = typesAdded;
		EventsAdded = eventsAdded;
		FieldsAdded = fieldsAdded;
		MethodsAdded = methodsAdded;
		PropertiesAdded = propertiesAdded;
		EventMapAdded = eventMapAdded;
		PropertyMapAdded = propertyMapAdded;
		MethodImplsAdded = methodImplsAdded;
		TableEntriesAdded = tableEntriesAdded;
		BlobStreamLengthAdded = blobStreamLengthAdded;
		StringStreamLengthAdded = stringStreamLengthAdded;
		UserStringStreamLengthAdded = userStringStreamLengthAdded;
		GuidStreamLengthAdded = guidStreamLengthAdded;
		_anonymousTypeMap = anonymousTypeMap;
		SynthesizedMembers = synthesizedMembers;
		AddedOrChangedMethods = methodsAddedOrChanged;
		DebugInformationProvider = debugInformationProvider;
		LocalSignatureProvider = localSignatureProvider;
		TableSizes = CalculateTableSizes(metadataReader, TableEntriesAdded);
		TypeToEventMap = typeToEventMap;
		TypeToPropertyMap = typeToPropertyMap;
		MethodImpls = methodImpls;
	}

	internal EmitBaseline With(Compilation compilation, CommonPEModuleBuilder moduleBuilder, int ordinal, Guid encId, IReadOnlyDictionary<ITypeDefinition, int> typesAdded, IReadOnlyDictionary<IEventDefinition, int> eventsAdded, IReadOnlyDictionary<IFieldDefinition, int> fieldsAdded, IReadOnlyDictionary<IMethodDefinition, int> methodsAdded, IReadOnlyDictionary<IPropertyDefinition, int> propertiesAdded, IReadOnlyDictionary<int, int> eventMapAdded, IReadOnlyDictionary<int, int> propertyMapAdded, IReadOnlyDictionary<MethodImplKey, int> methodImplsAdded, ImmutableArray<int> tableEntriesAdded, int blobStreamLengthAdded, int stringStreamLengthAdded, int userStringStreamLengthAdded, int guidStreamLengthAdded, IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypeMap, ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> synthesizedMembers, IReadOnlyDictionary<int, AddedOrChangedMethodInfo> addedOrChangedMethods, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider, Func<MethodDefinitionHandle, StandaloneSignatureHandle> localSignatureProvider)
	{
		return new EmitBaseline(InitialBaseline, OriginalMetadata, compilation, moduleBuilder, ModuleVersionId, ordinal, encId, HasPortablePdb, typesAdded, eventsAdded, fieldsAdded, methodsAdded, propertiesAdded, eventMapAdded, propertyMapAdded, methodImplsAdded, tableEntriesAdded, blobStreamLengthAdded, stringStreamLengthAdded, userStringStreamLengthAdded, guidStreamLengthAdded, anonymousTypeMap, synthesizedMembers, addedOrChangedMethods, debugInformationProvider, localSignatureProvider, TypeToEventMap, TypeToPropertyMap, MethodImpls);
	}

	private static ImmutableArray<int> CalculateTableSizes(MetadataReader reader, ImmutableArray<int> delta)
	{
		int[] array = new int[MetadataTokens.TableCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = reader.GetTableRowCount((TableIndex)i) + delta[i];
		}
		return ImmutableArray.Create(array);
	}

	private static Dictionary<int, int> CalculateTypePropertyMap(MetadataReader reader)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 1;
		foreach (TypeDefinitionHandle typesWithProperty in reader.GetTypesWithProperties())
		{
			dictionary.Add(reader.GetRowNumber(typesWithProperty), num);
			num++;
		}
		return dictionary;
	}

	private static Dictionary<int, int> CalculateTypeEventMap(MetadataReader reader)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 1;
		foreach (TypeDefinitionHandle typesWithEvent in reader.GetTypesWithEvents())
		{
			dictionary.Add(reader.GetRowNumber(typesWithEvent), num);
			num++;
		}
		return dictionary;
	}

	private static Dictionary<MethodImplKey, int> CalculateMethodImpls(MetadataReader reader)
	{
		Dictionary<MethodImplKey, int> dictionary = new Dictionary<MethodImplKey, int>();
		int tableRowCount = reader.GetTableRowCount(TableIndex.MethodImpl);
		for (int i = 1; i <= tableRowCount; i++)
		{
			int rowNumber = MetadataTokens.GetRowNumber(reader.GetMethodImplementation(MetadataTokens.MethodImplementationHandle(i)).MethodBody);
			int num = 1;
			MethodImplKey key;
			while (true)
			{
				key = new MethodImplKey(rowNumber, num);
				if (!dictionary.ContainsKey(key))
				{
					break;
				}
				num++;
			}
			dictionary.Add(key, i);
		}
		return dictionary;
	}

	internal int GetNextAnonymousTypeIndex(bool fromDelegates = false)
	{
		int num = 0;
		foreach (KeyValuePair<AnonymousTypeKey, AnonymousTypeValue> item in AnonymousTypeMap)
		{
			if (fromDelegates == item.Key.IsDelegate)
			{
				int uniqueIndex = item.Value.UniqueIndex;
				if (uniqueIndex >= num)
				{
					num = uniqueIndex + 1;
				}
			}
		}
		return num;
	}
}
