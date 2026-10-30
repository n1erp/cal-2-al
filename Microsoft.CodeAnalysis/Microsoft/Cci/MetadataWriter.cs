using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.DiaSymReader;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal abstract class MetadataWriter
{
	protected abstract class HeapOrReferenceIndexBase<T>
	{
		private readonly MetadataWriter _writer;

		private readonly List<T> _rows;

		private readonly int _firstRowId;

		public IReadOnlyList<T> Rows => _rows;

		protected HeapOrReferenceIndexBase(MetadataWriter writer, int lastRowId)
		{
			_writer = writer;
			_rows = new List<T>();
			_firstRowId = lastRowId + 1;
		}

		public abstract bool TryGetValue(T item, out int index);

		public int GetOrAdd(T item)
		{
			if (!TryGetValue(item, out var index))
			{
				return Add(item);
			}
			return index;
		}

		public int Add(T item)
		{
			int num = _firstRowId + _rows.Count;
			AddItem(item, num);
			_rows.Add(item);
			return num;
		}

		protected abstract void AddItem(T item, int index);
	}

	protected sealed class HeapOrReferenceIndex<T> : HeapOrReferenceIndexBase<T>
	{
		private readonly Dictionary<T, int> _index;

		public HeapOrReferenceIndex(MetadataWriter writer, int lastRowId = 0)
			: this(writer, new Dictionary<T, int>(), lastRowId)
		{
		}

		private HeapOrReferenceIndex(MetadataWriter writer, Dictionary<T, int> index, int lastRowId)
			: base(writer, lastRowId)
		{
			_index = index;
		}

		public override bool TryGetValue(T item, out int index)
		{
			return _index.TryGetValue(item, out index);
		}

		protected override void AddItem(T item, int index)
		{
			_index.Add(item, index);
		}
	}

	protected sealed class InstanceAndStructuralReferenceIndex<T> : HeapOrReferenceIndexBase<T> where T : IReference
	{
		private readonly Dictionary<T, int> _instanceIndex;

		private readonly Dictionary<T, int> _structuralIndex;

		public InstanceAndStructuralReferenceIndex(MetadataWriter writer, IEqualityComparer<T> structuralComparer, int lastRowId = 0)
			: base(writer, lastRowId)
		{
			_instanceIndex = new Dictionary<T, int>();
			_structuralIndex = new Dictionary<T, int>(structuralComparer);
		}

		public override bool TryGetValue(T item, out int index)
		{
			if (_instanceIndex.TryGetValue(item, out index))
			{
				return true;
			}
			if (_structuralIndex.TryGetValue(item, out index))
			{
				_instanceIndex.Add(item, index);
				return true;
			}
			return false;
		}

		protected override void AddItem(T item, int index)
		{
			_instanceIndex.Add(item, index);
			_structuralIndex.Add(item, index);
		}
	}

	internal sealed class ImportScopeEqualityComparer : IEqualityComparer<IImportScope>
	{
		public static readonly ImportScopeEqualityComparer Instance = new ImportScopeEqualityComparer();

		public bool Equals(IImportScope x, IImportScope y)
		{
			if (x != y)
			{
				if (x != null && y != null && Equals(x.Parent, y.Parent))
				{
					return x.GetUsedNamespaces().SequenceEqual(y.GetUsedNamespaces());
				}
				return false;
			}
			return true;
		}

		public int GetHashCode(IImportScope obj)
		{
			return Hash.Combine(Hash.CombineValues(obj.GetUsedNamespaces()), (obj.Parent != null) ? GetHashCode(obj.Parent) : 0);
		}
	}

	internal static readonly Encoding s_utf8Encoding = Encoding.UTF8;

	internal const int NameLengthLimit = 1023;

	internal const int PathLengthLimit = 259;

	internal const int PdbLengthLimit = 2046;

	private readonly int _numTypeDefsEstimate;

	private readonly bool _deterministic;

	internal readonly bool MetadataOnly;

	internal readonly bool EmitTestCoverageData;

	private readonly Dictionary<ImmutableArray<byte>, int> _smallMethodBodies;

	private const byte TinyFormat = 2;

	private const int ThrowNullCodeSize = 2;

	private static readonly ImmutableArray<byte> ThrowNullEncodedBody = ImmutableArray.Create((byte)10, (byte)20, (byte)122);

	private readonly CancellationToken _cancellationToken;

	protected readonly CommonPEModuleBuilder module;

	public readonly EmitContext Context;

	protected readonly CommonMessageProvider messageProvider;

	private bool _tableIndicesAreComplete;

	private EntityHandle[] _pseudoSymbolTokenToTokenMap;

	private IReference[] _pseudoSymbolTokenToReferenceMap;

	private UserStringHandle[] _pseudoStringTokenToTokenMap;

	private bool _userStringTokenOverflow;

	private List<string> _pseudoStringTokenToStringMap;

	private ReferenceIndexer _referenceVisitor;

	protected readonly MetadataBuilder metadata;

	protected readonly MetadataBuilder _debugMetadataOpt;

	private readonly DynamicAnalysisDataWriter _dynamicAnalysisDataWriterOpt;

	private readonly Dictionary<ICustomAttribute, BlobHandle> _customAttributeSignatureIndex = new Dictionary<ICustomAttribute, BlobHandle>();

	private readonly Dictionary<ITypeReference, BlobHandle> _typeSpecSignatureIndex = new Dictionary<ITypeReference, BlobHandle>();

	private readonly List<ITypeReference> _exportedTypeList;

	private readonly Dictionary<string, int> _fileRefIndex = new Dictionary<string, int>(32);

	private readonly List<IFileReference> _fileRefList = new List<IFileReference>(32);

	private readonly Dictionary<IFieldReference, BlobHandle> _fieldSignatureIndex = new Dictionary<IFieldReference, BlobHandle>();

	private readonly Dictionary<ISignature, KeyValuePair<BlobHandle, ImmutableArray<byte>>> _signatureIndex;

	private readonly Dictionary<IMarshallingInformation, BlobHandle> _marshallingDescriptorIndex = new Dictionary<IMarshallingInformation, BlobHandle>();

	protected readonly List<MethodImplementation> methodImplList = new List<MethodImplementation>();

	private readonly Dictionary<IGenericMethodInstanceReference, BlobHandle> _methodInstanceSignatureIndex = new Dictionary<IGenericMethodInstanceReference, BlobHandle>();

	internal const string dummyAssemblyAttributeParentNamespace = "System.Runtime.CompilerServices";

	internal const string dummyAssemblyAttributeParentName = "AssemblyAttributesGoHere";

	internal static readonly string[,] dummyAssemblyAttributeParentQualifier = new string[2, 2]
	{
		{ "", "M" },
		{ "S", "SM" }
	};

	private readonly TypeReferenceHandle[,] _dummyAssemblyAttributeParent = new TypeReferenceHandle[2, 2];

	internal const uint LiteralMethodDefinitionToken = 2147483648u;

	internal const uint LiteralGreatestMethodDefinitionToken = 1073741824u;

	internal const uint SourceDocumentIndex = 536870912u;

	internal const uint ModuleVersionIdStringToken = 2147483648u;

	private readonly Dictionary<DebugSourceDocument, DocumentHandle> _documentIndex = new Dictionary<DebugSourceDocument, DocumentHandle>();

	private readonly Dictionary<IImportScope, ImportScopeHandle> _scopeIndex = new Dictionary<IImportScope, ImportScopeHandle>(ImportScopeEqualityComparer.Instance);

	private static readonly ImportScopeHandle ModuleImportScopeHandle = MetadataTokens.ImportScopeHandle(1);

	private int NumberOfTypeDefsEstimate => _numTypeDefsEstimate;

	internal bool IsFullMetadata => Generation == 0;

	private bool IsMinimalDelta => !IsFullMetadata;

	private bool EmitAssemblyDefinition
	{
		get
		{
			if (module.OutputKind != OutputKind.NetModule)
			{
				return !IsMinimalDelta;
			}
			return false;
		}
	}

	protected abstract ushort Generation { get; }

	protected abstract Guid EncId { get; }

	protected abstract Guid EncBaseId { get; }

	protected abstract int GreatestMethodDefIndex { get; }

	internal bool EmitPortableDebugMetadata => _debugMetadataOpt != null;

	internal CommonPEModuleBuilder Module => module;

	protected MetadataWriter(MetadataBuilder metadata, MetadataBuilder debugMetadataOpt, DynamicAnalysisDataWriter dynamicAnalysisDataWriterOpt, EmitContext context, CommonMessageProvider messageProvider, bool metadataOnly, bool deterministic, bool emitTestCoverageData, CancellationToken cancellationToken)
	{
		module = context.Module;
		_deterministic = deterministic;
		MetadataOnly = metadataOnly;
		EmitTestCoverageData = emitTestCoverageData;
		_signatureIndex = new Dictionary<ISignature, KeyValuePair<BlobHandle, ImmutableArray<byte>>>(module.HintNumberOfMethodDefinitions);
		_numTypeDefsEstimate = module.HintNumberOfMethodDefinitions / 6;
		_exportedTypeList = new List<ITypeReference>(_numTypeDefsEstimate);
		Context = context;
		this.messageProvider = messageProvider;
		_cancellationToken = cancellationToken;
		this.metadata = metadata;
		_debugMetadataOpt = debugMetadataOpt;
		_dynamicAnalysisDataWriterOpt = dynamicAnalysisDataWriterOpt;
		_smallMethodBodies = new Dictionary<ImmutableArray<byte>, int>(ByteSequenceComparer.Instance);
	}

	protected abstract bool TryGetTypeDefinitionHandle(ITypeDefinition def, out TypeDefinitionHandle handle);

	protected abstract TypeDefinitionHandle GetTypeDefinitionHandle(ITypeDefinition def);

	protected abstract ITypeDefinition GetTypeDef(TypeDefinitionHandle handle);

	protected abstract IReadOnlyList<ITypeDefinition> GetTypeDefs();

	protected abstract EventDefinitionHandle GetEventDefinitionHandle(IEventDefinition def);

	protected abstract IReadOnlyList<IEventDefinition> GetEventDefs();

	protected abstract FieldDefinitionHandle GetFieldDefinitionHandle(IFieldDefinition def);

	protected abstract IReadOnlyList<IFieldDefinition> GetFieldDefs();

	protected abstract bool TryGetMethodDefinitionHandle(IMethodDefinition def, out MethodDefinitionHandle handle);

	protected abstract MethodDefinitionHandle GetMethodDefinitionHandle(IMethodDefinition def);

	protected abstract IMethodDefinition GetMethodDef(MethodDefinitionHandle handle);

	protected abstract IReadOnlyList<IMethodDefinition> GetMethodDefs();

	protected abstract PropertyDefinitionHandle GetPropertyDefIndex(IPropertyDefinition def);

	protected abstract IReadOnlyList<IPropertyDefinition> GetPropertyDefs();

	protected abstract ParameterHandle GetParameterHandle(IParameterDefinition def);

	protected abstract IReadOnlyList<IParameterDefinition> GetParameterDefs();

	protected abstract IReadOnlyList<IGenericParameter> GetGenericParameters();

	protected abstract FieldDefinitionHandle GetFirstFieldDefinitionHandle(INamedTypeDefinition typeDef);

	protected abstract MethodDefinitionHandle GetFirstMethodDefinitionHandle(INamedTypeDefinition typeDef);

	protected abstract ParameterHandle GetFirstParameterHandle(IMethodDefinition methodDef);

	protected abstract AssemblyReferenceHandle GetOrAddAssemblyReferenceHandle(IAssemblyReference reference);

	protected abstract IReadOnlyList<AssemblyIdentity> GetAssemblyRefs();

	protected abstract ModuleReferenceHandle GetOrAddModuleReferenceHandle(string reference);

	protected abstract IReadOnlyList<string> GetModuleRefs();

	protected abstract MemberReferenceHandle GetOrAddMemberReferenceHandle(ITypeMemberReference reference);

	protected abstract IReadOnlyList<ITypeMemberReference> GetMemberRefs();

	protected abstract MethodSpecificationHandle GetOrAddMethodSpecificationHandle(IGenericMethodInstanceReference reference);

	protected abstract IReadOnlyList<IGenericMethodInstanceReference> GetMethodSpecs();

	protected abstract bool TryGetTypeReferenceHandle(ITypeReference reference, out TypeReferenceHandle handle);

	protected abstract TypeReferenceHandle GetOrAddTypeReferenceHandle(ITypeReference reference);

	protected abstract IReadOnlyList<ITypeReference> GetTypeRefs();

	protected abstract TypeSpecificationHandle GetOrAddTypeSpecificationHandle(ITypeReference reference);

	protected abstract IReadOnlyList<ITypeReference> GetTypeSpecs();

	protected abstract StandaloneSignatureHandle GetOrAddStandaloneSignatureHandle(BlobHandle handle);

	protected abstract IReadOnlyList<BlobHandle> GetStandaloneSignatureBlobHandles();

	protected abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(CommonPEModuleBuilder module);

	protected abstract void CreateIndicesForNonTypeMembers(ITypeDefinition typeDef);

	protected abstract ReferenceIndexer CreateReferenceVisitor();

	protected abstract void PopulateEventMapTableRows();

	protected abstract void PopulatePropertyMapTableRows();

	protected abstract void PopulateEncLogTableRows(ImmutableArray<int> rowCounts);

	protected abstract void PopulateEncMapTableRows(ImmutableArray<int> rowCounts);

	protected abstract void ReportReferencesToAddedSymbols();

	private void CreateMethodBodyReferenceIndex()
	{
		IEnumerable<IReference> enumerable = module.ReferencesInIL(out var count);
		_pseudoSymbolTokenToTokenMap = new EntityHandle[count];
		_pseudoSymbolTokenToReferenceMap = new IReference[count];
		int num = 0;
		foreach (IReference item in enumerable)
		{
			_pseudoSymbolTokenToReferenceMap[num] = item;
			num++;
		}
	}

	private void CreateIndices()
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		CreateUserStringIndices();
		CreateInitialAssemblyRefIndex();
		CreateInitialFileRefIndex();
		CreateIndicesForModule();
		_referenceVisitor = CreateReferenceVisitor();
		_referenceVisitor.Visit(module);
		CreateMethodBodyReferenceIndex();
		OnIndicesCreated();
	}

	private void CreateUserStringIndices()
	{
		_pseudoStringTokenToStringMap = new List<string>();
		foreach (string @string in module.GetStrings())
		{
			_pseudoStringTokenToStringMap.Add(@string);
		}
		_pseudoStringTokenToTokenMap = new UserStringHandle[_pseudoStringTokenToStringMap.Count];
	}

	private void CreateIndicesForModule()
	{
		Queue<INestedTypeDefinition> queue = new Queue<INestedTypeDefinition>();
		foreach (INamespaceTypeDefinition topLevelType in GetTopLevelTypes(module))
		{
			CreateIndicesFor(topLevelType, queue);
		}
		while (queue.Count > 0)
		{
			INestedTypeDefinition typeDef = queue.Dequeue();
			CreateIndicesFor(typeDef, queue);
		}
	}

	protected virtual void OnIndicesCreated()
	{
	}

	private void CreateIndicesFor(ITypeDefinition typeDef, Queue<INestedTypeDefinition> nestedTypes)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		CreateIndicesForNonTypeMembers(typeDef);
		foreach (INestedTypeDefinition nestedType in typeDef.GetNestedTypes(Context))
		{
			nestedTypes.Enqueue(nestedType);
		}
	}

	protected IEnumerable<IGenericTypeParameter> GetConsolidatedTypeParameters(ITypeDefinition typeDef)
	{
		if (typeDef.AsNestedTypeDefinition(Context) == null)
		{
			if (typeDef.IsGeneric)
			{
				return typeDef.GenericParameters;
			}
			return null;
		}
		return GetConsolidatedTypeParameters(typeDef, typeDef);
	}

	private List<IGenericTypeParameter> GetConsolidatedTypeParameters(ITypeDefinition typeDef, ITypeDefinition owner)
	{
		List<IGenericTypeParameter> list = null;
		INestedTypeDefinition nestedTypeDefinition = typeDef.AsNestedTypeDefinition(Context);
		if (nestedTypeDefinition != null)
		{
			list = GetConsolidatedTypeParameters(nestedTypeDefinition.ContainingTypeDefinition, owner);
		}
		if (typeDef.GenericParameterCount > 0)
		{
			ushort num = 0;
			if (list == null)
			{
				list = new List<IGenericTypeParameter>();
			}
			else
			{
				num = (ushort)list.Count;
			}
			if (typeDef == owner && num == 0)
			{
				list.AddRange(typeDef.GenericParameters);
			}
			else
			{
				foreach (IGenericTypeParameter genericParameter in typeDef.GenericParameters)
				{
					list.Add(new InheritedTypeParameter(num++, owner, genericParameter));
				}
			}
		}
		return list;
	}

	protected ImmutableArray<IParameterDefinition> GetParametersToEmit(IMethodDefinition methodDef)
	{
		if (methodDef.ParameterCount == 0 && !methodDef.ReturnValueIsMarshalledExplicitly && !IteratorHelper.EnumerableIsNotEmpty(methodDef.GetReturnValueAttributes(Context)))
		{
			return ImmutableArray<IParameterDefinition>.Empty;
		}
		return GetParametersToEmitCore(methodDef);
	}

	private ImmutableArray<IParameterDefinition> GetParametersToEmitCore(IMethodDefinition methodDef)
	{
		ArrayBuilder<IParameterDefinition> arrayBuilder = null;
		ImmutableArray<IParameterDefinition> parameters = methodDef.Parameters;
		if (methodDef.ReturnValueIsMarshalledExplicitly || IteratorHelper.EnumerableIsNotEmpty(methodDef.GetReturnValueAttributes(Context)))
		{
			arrayBuilder = ArrayBuilder<IParameterDefinition>.GetInstance(parameters.Length + 1);
			arrayBuilder.Add(new ReturnValueParameter(methodDef));
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			IParameterDefinition parameterDefinition = parameters[i];
			if (parameterDefinition.Name != string.Empty || parameterDefinition.HasDefaultValue || parameterDefinition.IsOptional || parameterDefinition.IsOut || parameterDefinition.IsMarshalledExplicitly || IteratorHelper.EnumerableIsNotEmpty(parameterDefinition.GetAttributes(Context)))
			{
				arrayBuilder?.Add(parameterDefinition);
			}
			else if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<IParameterDefinition>.GetInstance(parameters.Length);
				arrayBuilder.AddRange(parameters, i);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? parameters;
	}

	public static IUnitReference GetDefiningUnitReference(ITypeReference typeReference, EmitContext context)
	{
		for (INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference; asNestedTypeReference != null; asNestedTypeReference = typeReference.AsNestedTypeReference)
		{
			if (asNestedTypeReference.AsGenericTypeInstanceReference != null)
			{
				return null;
			}
			typeReference = asNestedTypeReference.GetContainingType(context);
		}
		return typeReference.AsNamespaceTypeReference?.GetUnit(context);
	}

	private void CreateInitialAssemblyRefIndex()
	{
		foreach (IAssemblyReference assemblyReference in module.GetAssemblyReferences(Context))
		{
			GetOrAddAssemblyReferenceHandle(assemblyReference);
		}
	}

	private void CreateInitialFileRefIndex()
	{
		foreach (IFileReference file in module.GetFiles(Context))
		{
			string fileName = file.FileName;
			if (!_fileRefIndex.ContainsKey(fileName))
			{
				_fileRefList.Add(file);
				_fileRefIndex.Add(fileName, _fileRefList.Count);
			}
		}
	}

	internal AssemblyReferenceHandle GetAssemblyReferenceHandle(IAssemblyReference assemblyReference)
	{
		IAssemblyReference containingAssembly = module.GetContainingAssembly(Context);
		if (containingAssembly != null && assemblyReference == containingAssembly)
		{
			return default;
		}
		return GetOrAddAssemblyReferenceHandle(assemblyReference);
	}

	internal ModuleReferenceHandle GetModuleReferenceHandle(string moduleName)
	{
		return GetOrAddModuleReferenceHandle(moduleName);
	}

	private BlobHandle GetCustomAttributeSignatureIndex(ICustomAttribute customAttribute)
	{
		if (_customAttributeSignatureIndex.TryGetValue(customAttribute, out var value))
		{
			return value;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		SerializeCustomAttributeSignature(customAttribute, instance);
		value = metadata.GetOrAddBlob(instance);
		_customAttributeSignatureIndex.Add(customAttribute, value);
		instance.Free();
		return value;
	}

	private EntityHandle GetCustomAttributeTypeCodedIndex(IMethodReference methodReference)
	{
		IMethodDefinition methodDefinition = null;
		IUnitReference definingUnitReference = GetDefiningUnitReference(methodReference.GetContainingType(Context), Context);
		if (definingUnitReference != null && definingUnitReference == module)
		{
			methodDefinition = methodReference.GetResolvedMethod(Context);
		}
		if (methodDefinition == null)
		{
			return GetMemberReferenceHandle(methodReference);
		}
		return GetMethodDefinitionHandle(methodDefinition);
	}

	public static EventAttributes GetEventAttributes(IEventDefinition eventDef)
	{
		EventAttributes eventAttributes = EventAttributes.None;
		if (eventDef.IsSpecialName)
		{
			eventAttributes |= EventAttributes.SpecialName;
		}
		if (eventDef.IsRuntimeSpecial)
		{
			eventAttributes |= EventAttributes.RTSpecialName;
		}
		return eventAttributes;
	}

	public static FieldAttributes GetFieldAttributes(IFieldDefinition fieldDef)
	{
		FieldAttributes fieldAttributes = (FieldAttributes)fieldDef.Visibility;
		if (fieldDef.IsStatic)
		{
			fieldAttributes |= FieldAttributes.Static;
		}
		if (fieldDef.IsReadOnly)
		{
			fieldAttributes |= FieldAttributes.InitOnly;
		}
		if (fieldDef.IsCompileTimeConstant)
		{
			fieldAttributes |= FieldAttributes.Literal;
		}
		if (fieldDef.IsNotSerialized)
		{
			fieldAttributes |= FieldAttributes.NotSerialized;
		}
		if (!fieldDef.MappedData.IsDefault)
		{
			fieldAttributes |= FieldAttributes.HasFieldRVA;
		}
		if (fieldDef.IsSpecialName)
		{
			fieldAttributes |= FieldAttributes.SpecialName;
		}
		if (fieldDef.IsRuntimeSpecial)
		{
			fieldAttributes |= FieldAttributes.RTSpecialName;
		}
		if (fieldDef.IsMarshalledExplicitly)
		{
			fieldAttributes |= FieldAttributes.HasFieldMarshal;
		}
		if (fieldDef.IsCompileTimeConstant)
		{
			fieldAttributes |= FieldAttributes.HasDefault;
		}
		return fieldAttributes;
	}

	internal BlobHandle GetFieldSignatureIndex(IFieldReference fieldReference)
	{
		ISpecializedFieldReference asSpecializedFieldReference = fieldReference.AsSpecializedFieldReference;
		if (asSpecializedFieldReference != null)
		{
			fieldReference = asSpecializedFieldReference.UnspecializedVersion;
		}
		if (_fieldSignatureIndex.TryGetValue(fieldReference, out var value))
		{
			return value;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		SerializeFieldSignature(fieldReference, instance);
		value = metadata.GetOrAddBlob(instance);
		_fieldSignatureIndex.Add(fieldReference, value);
		instance.Free();
		return value;
	}

	internal EntityHandle GetFieldHandle(IFieldReference fieldReference)
	{
		IFieldDefinition fieldDefinition = null;
		IUnitReference definingUnitReference = GetDefiningUnitReference(fieldReference.GetContainingType(Context), Context);
		if (definingUnitReference != null && definingUnitReference == module)
		{
			fieldDefinition = fieldReference.GetResolvedField(Context);
		}
		if (fieldDefinition == null)
		{
			return GetMemberReferenceHandle(fieldReference);
		}
		return GetFieldDefinitionHandle(fieldDefinition);
	}

	internal AssemblyFileHandle GetAssemblyFileHandle(IFileReference fileReference)
	{
		string fileName = fileReference.FileName;
		if (!_fileRefIndex.TryGetValue(fileName, out var value))
		{
			_fileRefList.Add(fileReference);
			_fileRefIndex.Add(fileName, value = _fileRefList.Count);
		}
		return MetadataTokens.AssemblyFileHandle(value);
	}

	private AssemblyFileHandle GetAssemblyFileHandle(IModuleReference mref)
	{
		return MetadataTokens.AssemblyFileHandle(_fileRefIndex[mref.Name]);
	}

	private static GenericParameterAttributes GetGenericParameterAttributes(IGenericParameter genPar)
	{
		GenericParameterAttributes genericParameterAttributes = GenericParameterAttributes.None;
		switch (genPar.Variance)
		{
		case TypeParameterVariance.Covariant:
			genericParameterAttributes |= GenericParameterAttributes.Covariant;
			break;
		case TypeParameterVariance.Contravariant:
			genericParameterAttributes |= GenericParameterAttributes.Contravariant;
			break;
		}
		if (genPar.MustBeReferenceType)
		{
			genericParameterAttributes |= GenericParameterAttributes.ReferenceTypeConstraint;
		}
		if (genPar.MustBeValueType)
		{
			genericParameterAttributes |= GenericParameterAttributes.NotNullableValueTypeConstraint;
		}
		if (genPar.MustHaveDefaultConstructor)
		{
			genericParameterAttributes |= GenericParameterAttributes.DefaultConstructorConstraint;
		}
		return genericParameterAttributes;
	}

	private EntityHandle GetExportedTypeImplementation(INamespaceTypeReference namespaceRef)
	{
		IUnitReference unit = namespaceRef.GetUnit(Context);
		if (unit is IAssemblyReference assemblyReference)
		{
			return GetAssemblyReferenceHandle(assemblyReference);
		}
		IModuleReference moduleReference = (IModuleReference)unit;
		IAssemblyReference containingAssembly = moduleReference.GetContainingAssembly(Context);
		if (containingAssembly != null && containingAssembly != module.GetContainingAssembly(Context))
		{
			return GetAssemblyReferenceHandle(containingAssembly);
		}
		return GetAssemblyFileHandle(moduleReference);
	}

	private static uint GetManagedResourceOffset(ManagedResource resource, BlobBuilder resourceWriter)
	{
		if (resource.ExternalFile != null)
		{
			return resource.Offset;
		}
		int count = resourceWriter.Count;
		resource.WriteData(resourceWriter);
		return (uint)count;
	}

	private static uint GetManagedResourceOffset(BlobBuilder resource, BlobBuilder resourceWriter)
	{
		int count = resourceWriter.Count;
		resourceWriter.WriteInt32(resource.Count);
		resource.WriteContentTo(resourceWriter);
		resourceWriter.Align(8);
		return (uint)count;
	}

	public static string GetMangledName(INamedTypeReference namedType)
	{
		string name = namedType.Name;
		if (!namedType.MangleName)
		{
			return name;
		}
		return MetadataHelpers.ComposeAritySuffixedMetadataName(name, namedType.GenericParameterCount);
	}

	internal MemberReferenceHandle GetMemberReferenceHandle(ITypeMemberReference memberRef)
	{
		return GetOrAddMemberReferenceHandle(memberRef);
	}

	internal EntityHandle GetMemberReferenceParent(ITypeMemberReference memberRef)
	{
		ITypeDefinition typeDefinition = memberRef.GetContainingType(Context).AsTypeDefinition(Context);
		if (typeDefinition != null)
		{
			TryGetTypeDefinitionHandle(typeDefinition, out var handle);
			if (!handle.IsNil)
			{
				if (memberRef is IFieldReference)
				{
					return handle;
				}
				if (memberRef is IMethodReference methodReference)
				{
					if (methodReference.AcceptsExtraArguments && TryGetMethodDefinitionHandle(methodReference.GetResolvedMethod(Context), out var handle2))
					{
						return handle2;
					}
					return handle;
				}
			}
		}
		ITypeReference containingType = memberRef.GetContainingType(Context);
		if (!containingType.IsTypeSpecification())
		{
			return GetTypeReferenceHandle(containingType);
		}
		return GetTypeSpecificationHandle(containingType);
	}

	internal EntityHandle GetMethodDefinitionOrReferenceHandle(IMethodReference methodReference)
	{
		IMethodDefinition methodDefinition = null;
		IUnitReference definingUnitReference = GetDefiningUnitReference(methodReference.GetContainingType(Context), Context);
		if (definingUnitReference != null && definingUnitReference == module)
		{
			methodDefinition = methodReference.GetResolvedMethod(Context);
		}
		if (methodDefinition == null)
		{
			return GetMemberReferenceHandle(methodReference);
		}
		return GetMethodDefinitionHandle(methodDefinition);
	}

	public static MethodAttributes GetMethodAttributes(IMethodDefinition methodDef)
	{
		MethodAttributes methodAttributes = (MethodAttributes)methodDef.Visibility;
		if (methodDef.IsStatic)
		{
			methodAttributes |= MethodAttributes.Static;
		}
		if (methodDef.IsSealed)
		{
			methodAttributes |= MethodAttributes.Final;
		}
		if (methodDef.IsVirtual)
		{
			methodAttributes |= MethodAttributes.Virtual;
		}
		if (methodDef.IsHiddenBySignature)
		{
			methodAttributes |= MethodAttributes.HideBySig;
		}
		if (methodDef.IsNewSlot)
		{
			methodAttributes |= MethodAttributes.VtableLayoutMask;
		}
		if (methodDef.IsAccessCheckedOnOverride)
		{
			methodAttributes |= MethodAttributes.CheckAccessOnOverride;
		}
		if (methodDef.IsAbstract)
		{
			methodAttributes |= MethodAttributes.Abstract;
		}
		if (methodDef.IsSpecialName)
		{
			methodAttributes |= MethodAttributes.SpecialName;
		}
		if (methodDef.IsRuntimeSpecial)
		{
			methodAttributes |= MethodAttributes.RTSpecialName;
		}
		if (methodDef.IsPlatformInvoke)
		{
			methodAttributes |= MethodAttributes.PinvokeImpl;
		}
		if (methodDef.HasDeclarativeSecurity)
		{
			methodAttributes |= MethodAttributes.HasSecurity;
		}
		if (methodDef.RequiresSecurityObject)
		{
			methodAttributes |= MethodAttributes.RequireSecObject;
		}
		return methodAttributes;
	}

	internal BlobHandle GetMethodSpecificationSignatureHandle(IGenericMethodInstanceReference methodInstanceReference)
	{
		if (_methodInstanceSignatureIndex.TryGetValue(methodInstanceReference, out var value))
		{
			return value;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		GenericTypeArgumentsEncoder genericTypeArgumentsEncoder = new BlobEncoder(instance).MethodSpecificationSignature(methodInstanceReference.GetGenericMethod(Context).GenericParameterCount);
		foreach (ITypeReference genericArgument in methodInstanceReference.GetGenericArguments(Context))
		{
			SerializeTypeReference(genericTypeArgumentsEncoder.AddArgument(), genericArgument);
		}
		value = metadata.GetOrAddBlob(instance);
		_methodInstanceSignatureIndex.Add(methodInstanceReference, value);
		instance.Free();
		return value;
	}

	private BlobHandle GetMarshallingDescriptorHandle(IMarshallingInformation marshallingInformation)
	{
		if (_marshallingDescriptorIndex.TryGetValue(marshallingInformation, out var value))
		{
			return value;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		SerializeMarshallingDescriptor(marshallingInformation, instance);
		value = metadata.GetOrAddBlob(instance);
		_marshallingDescriptorIndex.Add(marshallingInformation, value);
		instance.Free();
		return value;
	}

	private BlobHandle GetMarshallingDescriptorHandle(ImmutableArray<byte> descriptor)
	{
		return metadata.GetOrAddBlob(descriptor);
	}

	private BlobHandle GetMemberReferenceSignatureHandle(ITypeMemberReference memberRef)
	{
		if (memberRef is IFieldReference fieldReference)
		{
			return GetFieldSignatureIndex(fieldReference);
		}
		if (memberRef is IMethodReference methodReference)
		{
			return GetMethodSignatureHandle(methodReference);
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal BlobHandle GetMethodSignatureHandle(IMethodReference methodReference)
	{
		ImmutableArray<byte> signatureBlob;
		return GetMethodSignatureHandleAndBlob(methodReference, out signatureBlob);
	}

	internal byte[] GetMethodSignature(IMethodReference methodReference)
	{
		GetMethodSignatureHandleAndBlob(methodReference, out var signatureBlob);
		return signatureBlob.ToArray();
	}

	private BlobHandle GetMethodSignatureHandleAndBlob(IMethodReference methodReference, out ImmutableArray<byte> signatureBlob)
	{
		ISpecializedMethodReference asSpecializedMethodReference = methodReference.AsSpecializedMethodReference;
		if (asSpecializedMethodReference != null)
		{
			methodReference = asSpecializedMethodReference.UnspecializedVersion;
		}
		if (_signatureIndex.TryGetValue(methodReference, out var value))
		{
			signatureBlob = value.Value;
			return value.Key;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		MethodSignatureEncoder encoder = new BlobEncoder(instance).MethodSignature(new SignatureHeader((byte)methodReference.CallingConvention).CallingConvention, methodReference.GenericParameterCount, (methodReference.CallingConvention & CallingConvention.HasThis) != 0);
		SerializeReturnValueAndParameters(encoder, methodReference, methodReference.ExtraParameters);
		signatureBlob = instance.ToImmutableArray();
		BlobHandle orAddBlob = metadata.GetOrAddBlob(signatureBlob);
		_signatureIndex.Add(methodReference, KeyValuePairUtil.Create(orAddBlob, signatureBlob));
		instance.Free();
		return orAddBlob;
	}

	private BlobHandle GetMethodSpecificationBlobHandle(IGenericMethodInstanceReference genericMethodInstanceReference)
	{
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		SerializeMethodSpecificationSignature(instance, genericMethodInstanceReference);
		BlobHandle orAddBlob = metadata.GetOrAddBlob(instance);
		instance.Free();
		return orAddBlob;
	}

	private MethodSpecificationHandle GetMethodSpecificationHandle(IGenericMethodInstanceReference methodSpec)
	{
		return GetOrAddMethodSpecificationHandle(methodSpec);
	}

	internal EntityHandle GetMethodHandle(IMethodReference methodReference)
	{
		IMethodDefinition methodDefinition = null;
		IUnitReference definingUnitReference = GetDefiningUnitReference(methodReference.GetContainingType(Context), Context);
		if (definingUnitReference != null && definingUnitReference == module)
		{
			methodDefinition = methodReference.GetResolvedMethod(Context);
		}
		if (methodDefinition != null && (methodReference == methodDefinition || !methodReference.AcceptsExtraArguments) && TryGetMethodDefinitionHandle(methodDefinition, out var handle))
		{
			return handle;
		}
		IGenericMethodInstanceReference asGenericMethodInstanceReference = methodReference.AsGenericMethodInstanceReference;
		if (asGenericMethodInstanceReference == null)
		{
			return GetMemberReferenceHandle(methodReference);
		}
		return GetMethodSpecificationHandle(asGenericMethodInstanceReference);
	}

	public static ParameterAttributes GetParameterAttributes(IParameterDefinition parDef)
	{
		ParameterAttributes parameterAttributes = ParameterAttributes.None;
		if (parDef.IsIn)
		{
			parameterAttributes |= ParameterAttributes.In;
		}
		if (parDef.IsOut)
		{
			parameterAttributes |= ParameterAttributes.Out;
		}
		if (parDef.IsOptional)
		{
			parameterAttributes |= ParameterAttributes.Optional;
		}
		if (parDef.HasDefaultValue)
		{
			parameterAttributes |= ParameterAttributes.HasDefault;
		}
		if (parDef.IsMarshalledExplicitly)
		{
			parameterAttributes |= ParameterAttributes.HasFieldMarshal;
		}
		return parameterAttributes;
	}

	private BlobHandle GetPermissionSetBlobHandle(ImmutableArray<ICustomAttribute> permissionSet)
	{
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		try
		{
			instance.WriteByte(46);
			instance.WriteCompressedInteger(permissionSet.Length);
			SerializePermissionSet(permissionSet, instance);
			return metadata.GetOrAddBlob(instance);
		}
		finally
		{
			instance.Free();
		}
	}

	public static PropertyAttributes GetPropertyAttributes(IPropertyDefinition propertyDef)
	{
		PropertyAttributes propertyAttributes = PropertyAttributes.None;
		if (propertyDef.IsSpecialName)
		{
			propertyAttributes |= PropertyAttributes.SpecialName;
		}
		if (propertyDef.IsRuntimeSpecial)
		{
			propertyAttributes |= PropertyAttributes.RTSpecialName;
		}
		if (propertyDef.HasDefaultValue)
		{
			propertyAttributes |= PropertyAttributes.HasDefault;
		}
		return propertyAttributes;
	}

	private BlobHandle GetPropertySignatureHandle(IPropertyDefinition propertyDef)
	{
		if (_signatureIndex.TryGetValue(propertyDef, out var value))
		{
			return value.Key;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		MethodSignatureEncoder encoder = new BlobEncoder(instance).PropertySignature((propertyDef.CallingConvention & CallingConvention.HasThis) != 0);
		SerializeReturnValueAndParameters(encoder, propertyDef, ImmutableArray<IParameterTypeInformation>.Empty);
		ImmutableArray<byte> value2 = instance.ToImmutableArray();
		BlobHandle orAddBlob = metadata.GetOrAddBlob(value2);
		_signatureIndex.Add(propertyDef, KeyValuePairUtil.Create(orAddBlob, value2));
		instance.Free();
		return orAddBlob;
	}

	private EntityHandle GetResolutionScopeHandle(IUnitReference unitReference)
	{
		if (unitReference is IAssemblyReference assemblyReference)
		{
			return GetAssemblyReferenceHandle(assemblyReference);
		}
		IModuleReference moduleReference = (IModuleReference)unitReference;
		IAssemblyReference containingAssembly = moduleReference.GetContainingAssembly(Context);
		if (containingAssembly != null && containingAssembly != module.GetContainingAssembly(Context))
		{
			return GetAssemblyReferenceHandle(containingAssembly);
		}
		return GetModuleReferenceHandle(moduleReference.Name);
	}

	private StringHandle GetStringHandleForPathAndCheckLength(string path, INamedEntity errorEntity = null)
	{
		CheckPathLength(path, errorEntity);
		return metadata.GetOrAddString(path);
	}

	private StringHandle GetStringHandleForNameAndCheckLength(string name, INamedEntity errorEntity = null)
	{
		CheckNameLength(name, errorEntity);
		return metadata.GetOrAddString(name);
	}

	private StringHandle GetStringHandleForNamespaceAndCheckLength(INamespaceTypeReference namespaceType, string mangledTypeName)
	{
		string namespaceName = namespaceType.NamespaceName;
		if (namespaceName.Length == 0)
		{
			return default;
		}
		CheckNamespaceLength(namespaceName, mangledTypeName, namespaceType);
		return metadata.GetOrAddString(namespaceName);
	}

	private void CheckNameLength(string name, INamedEntity errorEntity)
	{
		if (IsTooLongInternal(name, 1023))
		{
			Location namedEntityLocation = GetNamedEntityLocation(errorEntity);
			Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MetadataNameTooLong, namedEntityLocation, name));
		}
	}

	private void CheckPathLength(string path, INamedEntity errorEntity = null)
	{
		if (IsTooLongInternal(path, 259))
		{
			Location namedEntityLocation = GetNamedEntityLocation(errorEntity);
			Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MetadataNameTooLong, namedEntityLocation, path));
		}
	}

	private void CheckNamespaceLength(string namespaceName, string mangledTypeName, INamespaceTypeReference errorEntity)
	{
		if (namespaceName.Length + 1 + mangledTypeName.Length > 341 && s_utf8Encoding.GetByteCount(namespaceName) + 1 + s_utf8Encoding.GetByteCount(mangledTypeName) > 1023)
		{
			Location namedEntityLocation = GetNamedEntityLocation(errorEntity);
			Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MetadataNameTooLong, namedEntityLocation, namespaceName + "." + mangledTypeName));
		}
	}

	internal bool IsUsingStringTooLong(string usingString, INamedEntity errorEntity = null)
	{
		if (IsTooLongInternal(usingString, 2046))
		{
			Location namedEntityLocation = GetNamedEntityLocation(errorEntity);
			Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.WRN_PdbUsingNameTooLong, namedEntityLocation, usingString));
			return true;
		}
		return false;
	}

	internal bool IsLocalNameTooLong(ILocalDefinition localDefinition)
	{
		string name = localDefinition.Name;
		if (IsTooLongInternal(name, 2046))
		{
			Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.WRN_PdbLocalNameTooLong, localDefinition.Location, name));
			return true;
		}
		return false;
	}

	internal static bool IsTooLongInternal(string str, int maxLength)
	{
		if (str.Length < maxLength / 3)
		{
			return false;
		}
		return s_utf8Encoding.GetByteCount(str) > maxLength;
	}

	private static Location GetNamedEntityLocation(INamedEntity errorEntity)
	{
		return GetSymbolLocation(errorEntity as ISymbol);
	}

	protected static Location GetSymbolLocation(ISymbol symbolOpt)
	{
		if (symbolOpt == null || symbolOpt.Locations.IsDefaultOrEmpty)
		{
			return Location.None;
		}
		return symbolOpt.Locations[0];
	}

	internal TypeAttributes GetTypeAttributes(ITypeDefinition typeDef)
	{
		return GetTypeAttributes(typeDef, Context);
	}

	public static TypeAttributes GetTypeAttributes(ITypeDefinition typeDef, EmitContext context)
	{
		TypeAttributes typeAttributes = TypeAttributes.NotPublic;
		switch (typeDef.Layout)
		{
		case LayoutKind.Sequential:
			typeAttributes |= TypeAttributes.SequentialLayout;
			break;
		case LayoutKind.Explicit:
			typeAttributes |= TypeAttributes.ExplicitLayout;
			break;
		}
		if (typeDef.IsInterface)
		{
			typeAttributes |= TypeAttributes.ClassSemanticsMask;
		}
		if (typeDef.IsAbstract)
		{
			typeAttributes |= TypeAttributes.Abstract;
		}
		if (typeDef.IsSealed)
		{
			typeAttributes |= TypeAttributes.Sealed;
		}
		if (typeDef.IsSpecialName)
		{
			typeAttributes |= TypeAttributes.SpecialName;
		}
		if (typeDef.IsRuntimeSpecial)
		{
			typeAttributes |= TypeAttributes.RTSpecialName;
		}
		if (typeDef.IsComObject)
		{
			typeAttributes |= TypeAttributes.Import;
		}
		if (typeDef.IsSerializable)
		{
			typeAttributes |= TypeAttributes.Serializable;
		}
		if (typeDef.IsWindowsRuntimeImport)
		{
			typeAttributes |= TypeAttributes.WindowsRuntime;
		}
		switch (typeDef.StringFormat)
		{
		case CharSet.Unicode:
			typeAttributes |= TypeAttributes.UnicodeClass;
			break;
		case CharSet.Auto:
			typeAttributes |= TypeAttributes.AutoClass;
			break;
		}
		if (typeDef.HasDeclarativeSecurity)
		{
			typeAttributes |= TypeAttributes.HasSecurity;
		}
		if (typeDef.IsBeforeFieldInit)
		{
			typeAttributes |= TypeAttributes.BeforeFieldInit;
		}
		if (typeDef.AsNestedTypeDefinition(context) != null)
		{
			switch (((ITypeDefinitionMember)typeDef).Visibility)
			{
			case TypeMemberVisibility.Public:
				typeAttributes |= TypeAttributes.NestedPublic;
				break;
			case TypeMemberVisibility.Private:
				typeAttributes |= TypeAttributes.NestedPrivate;
				break;
			case TypeMemberVisibility.Family:
				typeAttributes |= TypeAttributes.NestedFamily;
				break;
			case TypeMemberVisibility.Assembly:
				typeAttributes |= TypeAttributes.NestedAssembly;
				break;
			case TypeMemberVisibility.FamilyAndAssembly:
				typeAttributes |= TypeAttributes.NestedFamANDAssem;
				break;
			case TypeMemberVisibility.FamilyOrAssembly:
				typeAttributes |= TypeAttributes.VisibilityMask;
				break;
			}
			return typeAttributes;
		}
		INamespaceTypeDefinition namespaceTypeDefinition = typeDef.AsNamespaceTypeDefinition(context);
		if (namespaceTypeDefinition != null && namespaceTypeDefinition.IsPublic)
		{
			typeAttributes |= TypeAttributes.Public;
		}
		return typeAttributes;
	}

	private EntityHandle GetDeclaringTypeOrMethodHandle(IGenericParameter genPar)
	{
		IGenericTypeParameter asGenericTypeParameter = genPar.AsGenericTypeParameter;
		if (asGenericTypeParameter != null)
		{
			return GetTypeDefinitionHandle(asGenericTypeParameter.DefiningType);
		}
		IGenericMethodParameter asGenericMethodParameter = genPar.AsGenericMethodParameter;
		if (asGenericMethodParameter != null)
		{
			return GetMethodDefinitionHandle(asGenericMethodParameter.DefiningMethod);
		}
		throw ExceptionUtilities.Unreachable;
	}

	private TypeReferenceHandle GetTypeReferenceHandle(ITypeReference typeReference)
	{
		if (TryGetTypeReferenceHandle(typeReference, out var handle))
		{
			return handle;
		}
		INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference;
		if (asNestedTypeReference != null)
		{
			GetTypeReferenceHandle(asNestedTypeReference.GetContainingType(Context));
		}
		return GetOrAddTypeReferenceHandle(typeReference);
	}

	private TypeSpecificationHandle GetTypeSpecificationHandle(ITypeReference typeReference)
	{
		return GetOrAddTypeSpecificationHandle(typeReference);
	}

	internal ITypeDefinition GetTypeDefinition(int token)
	{
		return GetTypeDef(MetadataTokens.TypeDefinitionHandle(token));
	}

	internal IMethodDefinition GetMethodDefinition(int token)
	{
		return GetMethodDef(MetadataTokens.MethodDefinitionHandle(token));
	}

	internal INestedTypeReference GetNestedTypeReference(int token)
	{
		return GetTypeDef(MetadataTokens.TypeDefinitionHandle(token)).AsNestedTypeReference;
	}

	internal BlobHandle GetTypeSpecSignatureIndex(ITypeReference typeReference)
	{
		if (_typeSpecSignatureIndex.TryGetValue(typeReference, out var value))
		{
			return value;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		SerializeTypeReference(new BlobEncoder(instance).TypeSpecificationSignature(), typeReference);
		value = metadata.GetOrAddBlob(instance);
		_typeSpecSignatureIndex.Add(typeReference, value);
		instance.Free();
		return value;
	}

	internal EntityHandle GetTypeHandle(ITypeReference typeReference, bool treatRefAsPotentialTypeSpec = true)
	{
		ITypeDefinition typeDefinition = typeReference.AsTypeDefinition(Context);
		if (typeDefinition != null && TryGetTypeDefinitionHandle(typeDefinition, out var handle))
		{
			return handle;
		}
		if (!treatRefAsPotentialTypeSpec || !typeReference.IsTypeSpecification())
		{
			return GetTypeReferenceHandle(typeReference);
		}
		return GetTypeSpecificationHandle(typeReference);
	}

	internal EntityHandle GetDefinitionHandle(IDefinition definition)
	{
		if (definition is ITypeDefinition def)
		{
			return GetTypeDefinitionHandle(def);
		}
		if (definition is IMethodDefinition def2)
		{
			return GetMethodDefinitionHandle(def2);
		}
		if (definition is IFieldDefinition def3)
		{
			return GetFieldDefinitionHandle(def3);
		}
		if (definition is IEventDefinition def4)
		{
			return GetEventDefinitionHandle(def4);
		}
		if (definition is IPropertyDefinition def5)
		{
			return GetPropertyDefIndex(def5);
		}
		throw ExceptionUtilities.Unreachable;
	}

	public void WriteMetadataAndIL(PdbWriter nativePdbWriterOpt, Stream metadataStream, Stream ilStream, Stream portablePdbStreamOpt, out MetadataSizes metadataSizes)
	{
		nativePdbWriterOpt?.SetMetadataEmitter(this);
		BlobBuilder blobBuilder = new BlobBuilder(1024);
		BlobBuilder blobBuilder2 = new BlobBuilder(4096);
		BlobBuilder mappedFieldDataBuilder = new BlobBuilder(0);
		BlobBuilder managedResourceDataBuilder = new BlobBuilder(0);
		blobBuilder.WriteUInt32(0u);
		BuildMetadataAndIL(nativePdbWriterOpt, blobBuilder, mappedFieldDataBuilder, managedResourceDataBuilder, out var _, out var _);
		ImmutableArray<int> rowCounts = metadata.GetRowCounts();
		PopulateEncTables(rowCounts);
		MetadataRootBuilder metadataRootBuilder = new MetadataRootBuilder(metadata, module.SerializationProperties.TargetRuntimeVersion, suppressValidation: true);
		metadataRootBuilder.Serialize(blobBuilder2, 0, 0);
		metadataSizes = metadataRootBuilder.Sizes;
		try
		{
			blobBuilder.WriteContentTo(ilStream);
			blobBuilder2.WriteContentTo(metadataStream);
		}
		catch (Exception ex) when (!(ex is OperationCanceledException))
		{
			throw new PeWritingException(ex);
		}
		if (portablePdbStreamOpt != null)
		{
			PortablePdbBuilder portablePdbBuilder = GetPortablePdbBuilder(rowCounts, default, null);
			BlobBuilder blobBuilder3 = new BlobBuilder();
			portablePdbBuilder.Serialize(blobBuilder3);
			try
			{
				blobBuilder3.WriteContentTo(portablePdbStreamOpt);
			}
			catch (Exception ex2) when (!(ex2 is OperationCanceledException))
			{
				throw new SymUnmanagedWriterException(ex2.Message, ex2);
			}
		}
	}

	public void BuildMetadataAndIL(PdbWriter nativePdbWriterOpt, BlobBuilder ilBuilder, BlobBuilder mappedFieldDataBuilder, BlobBuilder managedResourceDataBuilder, out Blob mvidFixup, out Blob mvidStringFixup)
	{
		CreateIndices();
		if (_debugMetadataOpt != null)
		{
			DefineModuleImportScope();
			if (module.SourceLinkStreamOpt != null)
			{
				EmbedSourceLink(module.SourceLinkStreamOpt);
			}
		}
		int[] methodBodyOffsets;
		if (MetadataOnly)
		{
			methodBodyOffsets = SerializeThrowNullMethodBodies(ilBuilder);
			mvidStringFixup = default;
		}
		else
		{
			methodBodyOffsets = SerializeMethodBodies(ilBuilder, nativePdbWriterOpt, out mvidStringFixup);
		}
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		_tableIndicesAreComplete = true;
		ReportReferencesToAddedSymbols();
		BlobBuilder blobBuilder = null;
		if (_dynamicAnalysisDataWriterOpt != null)
		{
			blobBuilder = new BlobBuilder();
			_dynamicAnalysisDataWriterOpt.SerializeMetadataTables(blobBuilder);
		}
		PopulateTypeSystemTables(methodBodyOffsets, mappedFieldDataBuilder, managedResourceDataBuilder, blobBuilder, out mvidFixup);
	}

	public void PopulateEncTables(ImmutableArray<int> typeSystemRowCounts)
	{
		PopulateEncLogTableRows(typeSystemRowCounts);
		PopulateEncMapTableRows(typeSystemRowCounts);
	}

	public MetadataRootBuilder GetRootBuilder()
	{
		return new MetadataRootBuilder(metadata, module.SerializationProperties.TargetRuntimeVersion, suppressValidation: true);
	}

	public PortablePdbBuilder GetPortablePdbBuilder(ImmutableArray<int> typeSystemRowCounts, MethodDefinitionHandle debugEntryPoint, Func<IEnumerable<Blob>, BlobContentId> deterministicIdProviderOpt)
	{
		return new PortablePdbBuilder(_debugMetadataOpt, typeSystemRowCounts, debugEntryPoint, deterministicIdProviderOpt);
	}

	internal void GetEntryPoints(out MethodDefinitionHandle entryPointHandle, out MethodDefinitionHandle debugEntryPointHandle)
	{
		if (IsFullMetadata && !MetadataOnly)
		{
			IMethodReference pEEntryPoint = module.PEEntryPoint;
			entryPointHandle = ((pEEntryPoint != null) ? ((MethodDefinitionHandle)GetMethodHandle((IMethodDefinition)pEEntryPoint.AsDefinition(Context))) : default(MethodDefinitionHandle));
			IMethodReference debugEntryPoint = module.DebugEntryPoint;
			if (debugEntryPoint != null && debugEntryPoint != pEEntryPoint)
			{
				debugEntryPointHandle = (MethodDefinitionHandle)GetMethodHandle((IMethodDefinition)debugEntryPoint.AsDefinition(Context));
			}
			else
			{
				debugEntryPointHandle = entryPointHandle;
			}
		}
		else
		{
			entryPointHandle = (debugEntryPointHandle = default);
		}
	}

	private ImmutableArray<IGenericParameter> GetSortedGenericParameters()
	{
		return GetGenericParameters().OrderBy((IGenericParameter x, IGenericParameter y) =>
		{
			int num = CodedIndex.TypeOrMethodDef(GetDeclaringTypeOrMethodHandle(x)) - CodedIndex.TypeOrMethodDef(GetDeclaringTypeOrMethodHandle(y));
			return (num != 0) ? num : (x.Index - y.Index);
		}).ToImmutableArray();
	}

	private void PopulateTypeSystemTables(int[] methodBodyOffsets, BlobBuilder mappedFieldDataWriter, BlobBuilder resourceWriter, BlobBuilder dynamicAnalysisDataOpt, out Blob mvidFixup)
	{
		ImmutableArray<IGenericParameter> sortedGenericParameters = GetSortedGenericParameters();
		PopulateAssemblyRefTableRows();
		PopulateAssemblyTableRows();
		PopulateClassLayoutTableRows();
		PopulateConstantTableRows();
		PopulateDeclSecurityTableRows();
		PopulateEventMapTableRows();
		PopulateEventTableRows();
		PopulateExportedTypeTableRows();
		PopulateFieldLayoutTableRows();
		PopulateFieldMarshalTableRows();
		PopulateFieldRvaTableRows(mappedFieldDataWriter);
		PopulateFieldTableRows();
		PopulateFileTableRows();
		PopulateGenericParameters(sortedGenericParameters);
		PopulateImplMapTableRows();
		PopulateInterfaceImplTableRows();
		PopulateManifestResourceTableRows(resourceWriter, dynamicAnalysisDataOpt);
		PopulateMemberRefTableRows();
		PopulateMethodImplTableRows();
		PopulateMethodTableRows(methodBodyOffsets);
		PopulateMethodSemanticsTableRows();
		PopulateMethodSpecTableRows();
		PopulateModuleRefTableRows();
		PopulateModuleTableRow(out mvidFixup);
		PopulateNestedClassTableRows();
		PopulateParamTableRows();
		PopulatePropertyMapTableRows();
		PopulatePropertyTableRows();
		PopulateTypeDefTableRows();
		PopulateTypeRefTableRows();
		PopulateTypeSpecTableRows();
		PopulateStandaloneSignatures();
		PopulateCustomAttributeTableRows(sortedGenericParameters);
	}

	private void PopulateAssemblyRefTableRows()
	{
		IReadOnlyList<AssemblyIdentity> assemblyRefs = GetAssemblyRefs();
		metadata.SetCapacity(TableIndex.AssemblyRef, assemblyRefs.Count);
		foreach (AssemblyIdentity item in assemblyRefs)
		{
			metadata.AddAssemblyReference(GetStringHandleForPathAndCheckLength(item.Name), item.Version, metadata.GetOrAddString(item.CultureName), metadata.GetOrAddBlob(item.PublicKeyToken), (AssemblyFlags)(((int)item.ContentType << 9) | (item.IsRetargetable ? 256 : 0)), default);
		}
	}

	private void PopulateAssemblyTableRows()
	{
		if (EmitAssemblyDefinition)
		{
			ISourceAssemblySymbolInternal sourceAssemblyOpt = module.SourceAssemblyOpt;
			AssemblyFlags assemblyFlags = sourceAssemblyOpt.AssemblyFlags & ~AssemblyFlags.PublicKey;
			if (!sourceAssemblyOpt.Identity.PublicKey.IsDefaultOrEmpty)
			{
				assemblyFlags |= AssemblyFlags.PublicKey;
			}
			metadata.AddAssembly(flags: assemblyFlags, hashAlgorithm: sourceAssemblyOpt.HashAlgorithm, version: sourceAssemblyOpt.Identity.Version, publicKey: metadata.GetOrAddBlob(sourceAssemblyOpt.Identity.PublicKey), name: GetStringHandleForPathAndCheckLength(module.Name, module), culture: metadata.GetOrAddString(sourceAssemblyOpt.Identity.CultureName));
		}
	}

	private void PopulateCustomAttributeTableRows(ImmutableArray<IGenericParameter> sortedGenericParameters)
	{
		if (IsFullMetadata)
		{
			AddAssemblyAttributesToTable();
		}
		AddCustomAttributesToTable(GetMethodDefs(), (IMethodDefinition def) => GetMethodDefinitionHandle(def));
		AddCustomAttributesToTable(GetFieldDefs(), (IFieldDefinition def) => GetFieldDefinitionHandle(def));
		IReadOnlyList<ITypeDefinition> typeDefs = GetTypeDefs();
		AddCustomAttributesToTable(typeDefs, (ITypeDefinition def) => GetTypeDefinitionHandle(def));
		AddCustomAttributesToTable(GetParameterDefs(), (IParameterDefinition def) => GetParameterHandle(def));
		if (IsFullMetadata)
		{
			AddModuleAttributesToTable(module);
		}
		AddCustomAttributesToTable(GetPropertyDefs(), (IPropertyDefinition def) => GetPropertyDefIndex(def));
		AddCustomAttributesToTable(GetEventDefs(), (IEventDefinition def) => GetEventDefinitionHandle(def));
		AddCustomAttributesToTable(sortedGenericParameters, TableIndex.GenericParam);
	}

	private void AddAssemblyAttributesToTable()
	{
		bool flag = module.OutputKind == OutputKind.NetModule;
		if (flag)
		{
			AddAssemblyAttributesToTable(from sa in module.GetSourceAssemblySecurityAttributes()
				select sa.Attribute, needsDummyParent: true, isSecurity: true);
		}
		AddAssemblyAttributesToTable(module.GetSourceAssemblyAttributes(Context.IsRefAssembly), flag, isSecurity: false);
	}

	private void AddAssemblyAttributesToTable(IEnumerable<ICustomAttribute> assemblyAttributes, bool needsDummyParent, bool isSecurity)
	{
		EntityHandle parentHandle = Handle.AssemblyDefinition;
		foreach (ICustomAttribute assemblyAttribute in assemblyAttributes)
		{
			if (needsDummyParent)
			{
				parentHandle = GetDummyAssemblyAttributeParent(isSecurity, assemblyAttribute.AllowMultiple);
			}
			AddCustomAttributeToTable(parentHandle, assemblyAttribute);
		}
	}

	private TypeReferenceHandle GetDummyAssemblyAttributeParent(bool isSecurity, bool allowMultiple)
	{
		int num = (isSecurity ? 1 : 0);
		int num2 = (allowMultiple ? 1 : 0);
		if (_dummyAssemblyAttributeParent[num, num2].IsNil)
		{
			_dummyAssemblyAttributeParent[num, num2] = metadata.AddTypeReference(GetResolutionScopeHandle(module.GetCorLibrary(Context)), metadata.GetOrAddString("System.Runtime.CompilerServices"), metadata.GetOrAddString("AssemblyAttributesGoHere" + dummyAssemblyAttributeParentQualifier[num, num2]));
		}
		return _dummyAssemblyAttributeParent[num, num2];
	}

	private void AddModuleAttributesToTable(CommonPEModuleBuilder module)
	{
		foreach (ICustomAttribute sourceModuleAttribute in module.GetSourceModuleAttributes())
		{
			AddCustomAttributeToTable(EntityHandle.ModuleDefinition, sourceModuleAttribute);
		}
	}

	private void AddCustomAttributesToTable<T>(IEnumerable<T> parentList, TableIndex tableIndex) where T : IReference
	{
		int num = 1;
		foreach (T parent in parentList)
		{
			EntityHandle parentHandle = MetadataTokens.Handle(tableIndex, num++);
			foreach (ICustomAttribute attribute in parent.GetAttributes(Context))
			{
				AddCustomAttributeToTable(parentHandle, attribute);
			}
		}
	}

	private void AddCustomAttributesToTable<T>(IEnumerable<T> parentList, Func<T, EntityHandle> getDefinitionHandle) where T : IReference
	{
		foreach (T parent in parentList)
		{
			EntityHandle parentHandle = getDefinitionHandle(parent);
			foreach (ICustomAttribute attribute in parent.GetAttributes(Context))
			{
				AddCustomAttributeToTable(parentHandle, attribute);
			}
		}
	}

	private void AddCustomAttributesToTable(EntityHandle handle, ImmutableArray<ICustomAttribute> attributes)
	{
		foreach (ICustomAttribute item in attributes)
		{
			AddCustomAttributeToTable(handle, item);
		}
	}

	private void AddCustomAttributesToTable(IEnumerable<TypeReferenceWithAttributes> typeRefsWithAttributes)
	{
		foreach (TypeReferenceWithAttributes typeRefsWithAttribute in typeRefsWithAttributes)
		{
			EntityHandle typeHandle = GetTypeHandle(typeRefsWithAttribute.TypeRef);
			foreach (ICustomAttribute attribute in typeRefsWithAttribute.Attributes)
			{
				AddCustomAttributeToTable(typeHandle, attribute);
			}
		}
	}

	private void AddCustomAttributeToTable(EntityHandle parentHandle, ICustomAttribute customAttribute)
	{
		IMethodReference methodReference = customAttribute.Constructor(Context, reportDiagnostics: true);
		if (methodReference != null)
		{
			metadata.AddCustomAttribute(parentHandle, GetCustomAttributeTypeCodedIndex(methodReference), GetCustomAttributeSignatureIndex(customAttribute));
		}
	}

	private void PopulateDeclSecurityTableRows()
	{
		if (module.OutputKind != OutputKind.NetModule)
		{
			PopulateDeclSecurityTableRowsFor(EntityHandle.AssemblyDefinition, module.GetSourceAssemblySecurityAttributes());
		}
		foreach (ITypeDefinition typeDef in GetTypeDefs())
		{
			if (typeDef.HasDeclarativeSecurity)
			{
				PopulateDeclSecurityTableRowsFor(GetTypeDefinitionHandle(typeDef), typeDef.SecurityAttributes);
			}
		}
		foreach (IMethodDefinition methodDef in GetMethodDefs())
		{
			if (methodDef.HasDeclarativeSecurity)
			{
				PopulateDeclSecurityTableRowsFor(GetMethodDefinitionHandle(methodDef), methodDef.SecurityAttributes);
			}
		}
	}

	private void PopulateDeclSecurityTableRowsFor(EntityHandle parentHandle, IEnumerable<SecurityAttribute> attributes)
	{
		OrderPreservingMultiDictionary<DeclarativeSecurityAction, ICustomAttribute> orderPreservingMultiDictionary = null;
		foreach (SecurityAttribute attribute in attributes)
		{
			orderPreservingMultiDictionary = orderPreservingMultiDictionary ?? OrderPreservingMultiDictionary<DeclarativeSecurityAction, ICustomAttribute>.GetInstance();
			orderPreservingMultiDictionary.Add(attribute.Action, attribute.Attribute);
		}
		if (orderPreservingMultiDictionary == null)
		{
			return;
		}
		foreach (DeclarativeSecurityAction key in orderPreservingMultiDictionary.Keys)
		{
			metadata.AddDeclarativeSecurityAttribute(parentHandle, key, GetPermissionSetBlobHandle(orderPreservingMultiDictionary[key]));
		}
		orderPreservingMultiDictionary.Free();
	}

	private void PopulateEventTableRows()
	{
		IReadOnlyList<IEventDefinition> eventDefs = GetEventDefs();
		metadata.SetCapacity(TableIndex.Event, eventDefs.Count);
		foreach (IEventDefinition item in eventDefs)
		{
			metadata.AddEvent(GetEventAttributes(item), GetStringHandleForNameAndCheckLength(item.Name, item), GetTypeHandle(item.GetType(Context)));
		}
	}

	private void PopulateExportedTypeTableRows()
	{
		if (!IsFullMetadata)
		{
			return;
		}
		ImmutableArray<ExportedType> exportedTypes = module.GetExportedTypes(Context.Diagnostics);
		if (exportedTypes.Length == 0)
		{
			return;
		}
		metadata.SetCapacity(TableIndex.ExportedType, exportedTypes.Length);
		foreach (ExportedType item in exportedTypes)
		{
			INamespaceTypeReference asNamespaceTypeReference;
			StringHandle stringHandleForNameAndCheckLength;
			StringHandle stringHandle;
			EntityHandle implementation;
			TypeAttributes attributes;
			if ((asNamespaceTypeReference = item.Type.AsNamespaceTypeReference) != null)
			{
				string mangledName = GetMangledName(asNamespaceTypeReference);
				stringHandleForNameAndCheckLength = GetStringHandleForNameAndCheckLength(mangledName, asNamespaceTypeReference);
				stringHandle = GetStringHandleForNamespaceAndCheckLength(asNamespaceTypeReference, mangledName);
				implementation = GetExportedTypeImplementation(asNamespaceTypeReference);
				attributes = ((!item.IsForwarder) ? TypeAttributes.Public : ((TypeAttributes)2097152));
			}
			else
			{
				INestedTypeReference asNestedTypeReference;
				if ((asNestedTypeReference = item.Type.AsNestedTypeReference) == null)
				{
					throw ExceptionUtilities.UnexpectedValue(item);
				}
				stringHandleForNameAndCheckLength = GetStringHandleForNameAndCheckLength(GetMangledName(asNestedTypeReference), asNestedTypeReference);
				stringHandle = default;
				implementation = MetadataTokens.ExportedTypeHandle(item.ParentIndex + 1);
				attributes = ((!item.IsForwarder) ? TypeAttributes.NestedPublic : TypeAttributes.NotPublic);
			}
			metadata.AddExportedType(attributes, stringHandle, stringHandleForNameAndCheckLength, implementation, (!item.IsForwarder) ? MetadataTokens.GetToken(item.Type.TypeDef) : 0);
		}
	}

	private void PopulateFieldLayoutTableRows()
	{
		foreach (IFieldDefinition fieldDef in GetFieldDefs())
		{
			if (fieldDef.ContainingTypeDefinition.Layout == LayoutKind.Explicit && !fieldDef.IsStatic)
			{
				metadata.AddFieldLayout(GetFieldDefinitionHandle(fieldDef), fieldDef.Offset);
			}
		}
	}

	private void PopulateFieldMarshalTableRows()
	{
		foreach (IFieldDefinition fieldDef in GetFieldDefs())
		{
			if (fieldDef.IsMarshalledExplicitly)
			{
				IMarshallingInformation marshallingInformation = fieldDef.MarshallingInformation;
				BlobHandle descriptor = ((marshallingInformation != null) ? GetMarshallingDescriptorHandle(marshallingInformation) : GetMarshallingDescriptorHandle(fieldDef.MarshallingDescriptor));
				metadata.AddMarshallingDescriptor(GetFieldDefinitionHandle(fieldDef), descriptor);
			}
		}
		foreach (IParameterDefinition parameterDef in GetParameterDefs())
		{
			if (parameterDef.IsMarshalledExplicitly)
			{
				IMarshallingInformation marshallingInformation2 = parameterDef.MarshallingInformation;
				BlobHandle descriptor2 = ((marshallingInformation2 != null) ? GetMarshallingDescriptorHandle(marshallingInformation2) : GetMarshallingDescriptorHandle(parameterDef.MarshallingDescriptor));
				metadata.AddMarshallingDescriptor(GetParameterHandle(parameterDef), descriptor2);
			}
		}
	}

	private void PopulateFieldRvaTableRows(BlobBuilder mappedFieldDataWriter)
	{
		foreach (IFieldDefinition fieldDef in GetFieldDefs())
		{
			if (!fieldDef.MappedData.IsDefault)
			{
				int count = mappedFieldDataWriter.Count;
				mappedFieldDataWriter.WriteBytes(fieldDef.MappedData);
				mappedFieldDataWriter.Align(8);
				metadata.AddFieldRelativeVirtualAddress(GetFieldDefinitionHandle(fieldDef), count);
			}
		}
	}

	private void PopulateFieldTableRows()
	{
		IReadOnlyList<IFieldDefinition> fieldDefs = GetFieldDefs();
		metadata.SetCapacity(TableIndex.Field, fieldDefs.Count);
		foreach (IFieldDefinition item in fieldDefs)
		{
			if (item.IsContextualNamedEntity)
			{
				((IContextualNamedEntity)item).AssociateWithMetadataWriter(this);
			}
			metadata.AddFieldDefinition(GetFieldAttributes(item), GetStringHandleForNameAndCheckLength(item.Name, item), GetFieldSignatureIndex(item));
		}
	}

	private void PopulateConstantTableRows()
	{
		foreach (IFieldDefinition fieldDef in GetFieldDefs())
		{
			MetadataConstant compileTimeValue = fieldDef.GetCompileTimeValue(Context);
			if (compileTimeValue != null)
			{
				metadata.AddConstant(GetFieldDefinitionHandle(fieldDef), compileTimeValue.Value);
			}
		}
		foreach (IParameterDefinition parameterDef in GetParameterDefs())
		{
			MetadataConstant defaultValue = parameterDef.GetDefaultValue(Context);
			if (defaultValue != null)
			{
				metadata.AddConstant(GetParameterHandle(parameterDef), defaultValue.Value);
			}
		}
		foreach (IPropertyDefinition propertyDef in GetPropertyDefs())
		{
			if (propertyDef.HasDefaultValue)
			{
				metadata.AddConstant(GetPropertyDefIndex(propertyDef), propertyDef.DefaultValue.Value);
			}
		}
	}

	private void PopulateFileTableRows()
	{
		ISourceAssemblySymbolInternal sourceAssemblyOpt = module.SourceAssemblyOpt;
		if (sourceAssemblyOpt == null)
		{
			return;
		}
		AssemblyHashAlgorithm hashAlgorithm = sourceAssemblyOpt.HashAlgorithm;
		metadata.SetCapacity(TableIndex.File, _fileRefList.Count);
		foreach (IFileReference fileRef in _fileRefList)
		{
			metadata.AddAssemblyFile(GetStringHandleForPathAndCheckLength(fileRef.FileName), metadata.GetOrAddBlob(fileRef.GetHashValue(hashAlgorithm)), fileRef.HasMetadata);
		}
	}

	private void PopulateGenericParameters(ImmutableArray<IGenericParameter> sortedGenericParameters)
	{
		foreach (IGenericParameter item in sortedGenericParameters)
		{
			GenericParameterHandle genericParameter = metadata.AddGenericParameter(GetDeclaringTypeOrMethodHandle(item), GetGenericParameterAttributes(item), GetStringHandleForNameAndCheckLength(item.Name, item), item.Index);
			foreach (TypeReferenceWithAttributes constraint in item.GetConstraints(Context))
			{
				GenericParameterConstraintHandle genericParameterConstraintHandle = metadata.AddGenericParameterConstraint(genericParameter, GetTypeHandle(constraint.TypeRef));
				AddCustomAttributesToTable(genericParameterConstraintHandle, constraint.Attributes);
			}
		}
	}

	private void PopulateImplMapTableRows()
	{
		foreach (IMethodDefinition methodDef in GetMethodDefs())
		{
			if (methodDef.IsPlatformInvoke)
			{
				IPlatformInvokeInformation platformInvokeData = methodDef.PlatformInvokeData;
				string entryPointName = platformInvokeData.EntryPointName;
				StringHandle name = ((entryPointName != null) ? GetStringHandleForNameAndCheckLength(entryPointName, methodDef) : metadata.GetOrAddString(methodDef.Name));
				metadata.AddMethodImport(GetMethodDefinitionHandle(methodDef), platformInvokeData.Flags, name, GetModuleReferenceHandle(platformInvokeData.ModuleName));
			}
		}
	}

	private void PopulateInterfaceImplTableRows()
	{
		foreach (ITypeDefinition typeDef in GetTypeDefs())
		{
			TypeDefinitionHandle typeDefinitionHandle = GetTypeDefinitionHandle(typeDef);
			foreach (TypeReferenceWithAttributes item in typeDef.Interfaces(Context))
			{
				InterfaceImplementationHandle interfaceImplementationHandle = metadata.AddInterfaceImplementation(typeDefinitionHandle, GetTypeHandle(item.TypeRef));
				AddCustomAttributesToTable(interfaceImplementationHandle, item.Attributes);
			}
		}
	}

	private void PopulateManifestResourceTableRows(BlobBuilder resourceDataWriter, BlobBuilder dynamicAnalysisDataOpt)
	{
		if (dynamicAnalysisDataOpt != null)
		{
			metadata.AddManifestResource(ManifestResourceAttributes.Private, metadata.GetOrAddString("<DynamicAnalysisData>"), default, GetManagedResourceOffset(dynamicAnalysisDataOpt, resourceDataWriter));
		}
		foreach (ManagedResource resource in module.GetResources(Context))
		{
			EntityHandle implementation = ((resource.ExternalFile == null) ? default(EntityHandle) : ((EntityHandle)GetAssemblyFileHandle(resource.ExternalFile)));
			metadata.AddManifestResource(resource.IsPublic ? ManifestResourceAttributes.Public : ManifestResourceAttributes.Private, GetStringHandleForNameAndCheckLength(resource.Name), implementation, GetManagedResourceOffset(resource, resourceDataWriter));
		}
	}

	private void PopulateMemberRefTableRows()
	{
		IReadOnlyList<ITypeMemberReference> memberRefs = GetMemberRefs();
		metadata.SetCapacity(TableIndex.MemberRef, memberRefs.Count);
		foreach (ITypeMemberReference item in memberRefs)
		{
			metadata.AddMemberReference(GetMemberReferenceParent(item), GetStringHandleForNameAndCheckLength(item.Name, item), GetMemberReferenceSignatureHandle(item));
		}
	}

	private void PopulateMethodImplTableRows()
	{
		metadata.SetCapacity(TableIndex.MethodImpl, methodImplList.Count);
		foreach (MethodImplementation methodImpl in methodImplList)
		{
			metadata.AddMethodImplementation(GetTypeDefinitionHandle(methodImpl.ContainingType), GetMethodDefinitionOrReferenceHandle(methodImpl.ImplementingMethod), GetMethodDefinitionOrReferenceHandle(methodImpl.ImplementedMethod));
		}
	}

	private void PopulateMethodSpecTableRows()
	{
		IReadOnlyList<IGenericMethodInstanceReference> methodSpecs = GetMethodSpecs();
		metadata.SetCapacity(TableIndex.MethodSpec, methodSpecs.Count);
		foreach (IGenericMethodInstanceReference item in methodSpecs)
		{
			metadata.AddMethodSpecification(GetMethodDefinitionOrReferenceHandle(item.GetGenericMethod(Context)), GetMethodSpecificationBlobHandle(item));
		}
	}

	private void PopulateMethodTableRows(int[] methodBodyOffsets)
	{
		IReadOnlyList<IMethodDefinition> methodDefs = GetMethodDefs();
		metadata.SetCapacity(TableIndex.MethodDef, methodDefs.Count);
		int num = 0;
		foreach (IMethodDefinition item in methodDefs)
		{
			metadata.AddMethodDefinition(GetMethodAttributes(item), item.GetImplementationAttributes(Context), GetStringHandleForNameAndCheckLength(item.Name, item), GetMethodSignatureHandle(item), methodBodyOffsets[num], GetFirstParameterHandle(item));
			num++;
		}
	}

	private void PopulateMethodSemanticsTableRows()
	{
		IReadOnlyList<IPropertyDefinition> propertyDefs = GetPropertyDefs();
		IReadOnlyList<IEventDefinition> eventDefs = GetEventDefs();
		metadata.SetCapacity(TableIndex.MethodSemantics, propertyDefs.Count * 2 + eventDefs.Count * 2);
		foreach (IPropertyDefinition propertyDef in GetPropertyDefs())
		{
			PropertyDefinitionHandle propertyDefIndex = GetPropertyDefIndex(propertyDef);
			foreach (IMethodReference accessor in propertyDef.GetAccessors(Context))
			{
				MethodSemanticsAttributes semantics = ((accessor == propertyDef.Setter) ? MethodSemanticsAttributes.Setter : ((accessor != propertyDef.Getter) ? MethodSemanticsAttributes.Other : MethodSemanticsAttributes.Getter));
				metadata.AddMethodSemantics(propertyDefIndex, semantics, GetMethodDefinitionHandle(accessor.GetResolvedMethod(Context)));
			}
		}
		foreach (IEventDefinition eventDef in GetEventDefs())
		{
			EventDefinitionHandle eventDefinitionHandle = GetEventDefinitionHandle(eventDef);
			foreach (IMethodReference accessor2 in eventDef.GetAccessors(Context))
			{
				MethodSemanticsAttributes semantics2;
				if (accessor2 != eventDef.Adder)
				{
					if (accessor2 != eventDef.Remover)
					{
						semantics2 = ((accessor2 != eventDef.Caller) ? MethodSemanticsAttributes.Other : MethodSemanticsAttributes.Raiser);
					}
					else
					{
						semantics2 = MethodSemanticsAttributes.Remover;
					}
				}
				else
				{
					semantics2 = MethodSemanticsAttributes.Adder;
				}
				metadata.AddMethodSemantics(eventDefinitionHandle, semantics2, GetMethodDefinitionHandle(accessor2.GetResolvedMethod(Context)));
			}
		}
	}

	private void PopulateModuleRefTableRows()
	{
		IReadOnlyList<string> moduleRefs = GetModuleRefs();
		metadata.SetCapacity(TableIndex.ModuleRef, moduleRefs.Count);
		foreach (string item in moduleRefs)
		{
			metadata.AddModuleReference(GetStringHandleForPathAndCheckLength(item));
		}
	}

	private void PopulateModuleTableRow(out Blob mvidFixup)
	{
		CheckPathLength(module.ModuleName);
		Guid persistentIdentifier = module.SerializationProperties.PersistentIdentifier;
		GuidHandle mvid;
		if (persistentIdentifier != default(Guid))
		{
			mvid = metadata.GetOrAddGuid(persistentIdentifier);
			mvidFixup = default;
		}
		else
		{
			ReservedBlob<GuidHandle> reservedBlob = metadata.ReserveGuid();
			mvidFixup = reservedBlob.Content;
			mvid = reservedBlob.Handle;
			reservedBlob.CreateWriter().WriteBytes(0, mvidFixup.Length);
		}
		metadata.AddModule(Generation, metadata.GetOrAddString(module.ModuleName), mvid, metadata.GetOrAddGuid(EncId), metadata.GetOrAddGuid(EncBaseId));
	}

	private void PopulateParamTableRows()
	{
		IReadOnlyList<IParameterDefinition> parameterDefs = GetParameterDefs();
		metadata.SetCapacity(TableIndex.Param, parameterDefs.Count);
		foreach (IParameterDefinition item in parameterDefs)
		{
			metadata.AddParameter(GetParameterAttributes(item), sequenceNumber: (!(item is ReturnValueParameter)) ? (item.Index + 1) : 0, name: GetStringHandleForNameAndCheckLength(item.Name, item));
		}
	}

	private void PopulatePropertyTableRows()
	{
		IReadOnlyList<IPropertyDefinition> propertyDefs = GetPropertyDefs();
		metadata.SetCapacity(TableIndex.Property, propertyDefs.Count);
		foreach (IPropertyDefinition item in propertyDefs)
		{
			metadata.AddProperty(GetPropertyAttributes(item), GetStringHandleForNameAndCheckLength(item.Name, item), GetPropertySignatureHandle(item));
		}
	}

	private void PopulateTypeDefTableRows()
	{
		IReadOnlyList<ITypeDefinition> typeDefs = GetTypeDefs();
		metadata.SetCapacity(TableIndex.TypeDef, typeDefs.Count);
		foreach (INamedTypeDefinition item in typeDefs)
		{
			INamespaceTypeDefinition namespaceTypeDefinition = item.AsNamespaceTypeDefinition(Context);
			string mangledName = GetMangledName(item);
			ITypeReference baseClass = item.GetBaseClass(Context);
			metadata.AddTypeDefinition(GetTypeAttributes(item), (namespaceTypeDefinition != null) ? GetStringHandleForNamespaceAndCheckLength(namespaceTypeDefinition, mangledName) : default(StringHandle), GetStringHandleForNameAndCheckLength(mangledName, item), (baseClass != null) ? GetTypeHandle(baseClass) : default(EntityHandle), GetFirstFieldDefinitionHandle(item), GetFirstMethodDefinitionHandle(item));
		}
	}

	private void PopulateNestedClassTableRows()
	{
		foreach (ITypeDefinition typeDef in GetTypeDefs())
		{
			INestedTypeDefinition nestedTypeDefinition = typeDef.AsNestedTypeDefinition(Context);
			if (nestedTypeDefinition != null)
			{
				metadata.AddNestedType(GetTypeDefinitionHandle(typeDef), GetTypeDefinitionHandle(nestedTypeDefinition.ContainingTypeDefinition));
			}
		}
	}

	private void PopulateClassLayoutTableRows()
	{
		foreach (ITypeDefinition typeDef in GetTypeDefs())
		{
			if (typeDef.Alignment != 0 || typeDef.SizeOf != 0)
			{
				metadata.AddTypeLayout(GetTypeDefinitionHandle(typeDef), typeDef.Alignment, typeDef.SizeOf);
			}
		}
	}

	private void PopulateTypeRefTableRows()
	{
		IReadOnlyList<ITypeReference> typeRefs = GetTypeRefs();
		metadata.SetCapacity(TableIndex.TypeRef, typeRefs.Count);
		foreach (ITypeReference item in typeRefs)
		{
			INestedTypeReference asNestedTypeReference = item.AsNestedTypeReference;
			EntityHandle resolutionScope;
			StringHandle stringHandleForNameAndCheckLength;
			StringHandle stringHandle;
			if (asNestedTypeReference != null)
			{
				ISpecializedNestedTypeReference asSpecializedNestedTypeReference = asNestedTypeReference.AsSpecializedNestedTypeReference;
				ITypeReference typeReference = ((asSpecializedNestedTypeReference == null) ? asNestedTypeReference.GetContainingType(Context) : asSpecializedNestedTypeReference.GetUnspecializedVersion(Context).GetContainingType(Context));
				resolutionScope = GetTypeReferenceHandle(typeReference);
				stringHandleForNameAndCheckLength = GetStringHandleForNameAndCheckLength(GetMangledName(asNestedTypeReference), asNestedTypeReference);
				stringHandle = default;
			}
			else
			{
				INamespaceTypeReference asNamespaceTypeReference = item.AsNamespaceTypeReference;
				if (asNamespaceTypeReference == null)
				{
					throw ExceptionUtilities.UnexpectedValue(item);
				}
				resolutionScope = GetResolutionScopeHandle(asNamespaceTypeReference.GetUnit(Context));
				string mangledName = GetMangledName(asNamespaceTypeReference);
				stringHandleForNameAndCheckLength = GetStringHandleForNameAndCheckLength(mangledName, asNamespaceTypeReference);
				stringHandle = GetStringHandleForNamespaceAndCheckLength(asNamespaceTypeReference, mangledName);
			}
			metadata.AddTypeReference(resolutionScope, stringHandle, stringHandleForNameAndCheckLength);
		}
	}

	private void PopulateTypeSpecTableRows()
	{
		IReadOnlyList<ITypeReference> typeSpecs = GetTypeSpecs();
		metadata.SetCapacity(TableIndex.TypeSpec, typeSpecs.Count);
		foreach (ITypeReference item in typeSpecs)
		{
			metadata.AddTypeSpecification(GetTypeSpecSignatureIndex(item));
		}
	}

	private void PopulateStandaloneSignatures()
	{
		foreach (BlobHandle standaloneSignatureBlobHandle in GetStandaloneSignatureBlobHandles())
		{
			metadata.AddStandaloneSignature(standaloneSignatureBlobHandle);
		}
	}

	private int[] SerializeThrowNullMethodBodies(BlobBuilder ilBuilder)
	{
		IReadOnlyList<IMethodDefinition> methodDefs = GetMethodDefs();
		int[] array = new int[methodDefs.Count];
		int num = -1;
		int num2 = 0;
		foreach (IMethodDefinition item in methodDefs)
		{
			if (item.HasBody())
			{
				if (num == -1)
				{
					num = ilBuilder.Count;
					ilBuilder.WriteBytes(ThrowNullEncodedBody);
				}
				array[num2] = num;
			}
			else
			{
				array[num2] = -1;
			}
			num2++;
		}
		return array;
	}

	private int[] SerializeMethodBodies(BlobBuilder ilBuilder, PdbWriter nativePdbWriterOpt, out Blob mvidStringFixup)
	{
		CustomDebugInfoWriter customDebugInfoWriter = ((nativePdbWriterOpt != null) ? new CustomDebugInfoWriter(nativePdbWriterOpt) : null);
		IReadOnlyList<IMethodDefinition> methodDefs = GetMethodDefs();
		int[] array = new int[methodDefs.Count];
		LocalVariableHandle lastLocalVariableHandle = default;
		LocalConstantHandle lastLocalConstantHandle = default;
		MethodBodyStreamEncoder encoder = new MethodBodyStreamEncoder(ilBuilder);
		UserStringHandle mvidStringHandle = default;
		mvidStringFixup = default;
		int num = 1;
		foreach (IMethodDefinition item in methodDefs)
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			IMethodBody methodBody;
			StandaloneSignatureHandle localSignatureHandleOpt;
			int num2;
			if (item.HasBody())
			{
				methodBody = item.GetBody(Context);
				if (methodBody != null)
				{
					localSignatureHandleOpt = SerializeLocalVariablesSignature(methodBody);
					num2 = SerializeMethodBody(encoder, methodBody, localSignatureHandleOpt, ref mvidStringHandle, ref mvidStringFixup);
					nativePdbWriterOpt?.SerializeDebugInfo(methodBody, localSignatureHandleOpt, customDebugInfoWriter);
				}
				else
				{
					num2 = 0;
					localSignatureHandleOpt = default;
				}
			}
			else
			{
				num2 = -1;
				methodBody = null;
				localSignatureHandleOpt = default;
			}
			if (_debugMetadataOpt != null)
			{
				SerializeMethodDebugInfo(methodBody, num, localSignatureHandleOpt, ref lastLocalVariableHandle, ref lastLocalConstantHandle);
			}
			_dynamicAnalysisDataWriterOpt?.SerializeMethodDynamicAnalysisData(methodBody);
			array[num - 1] = num2;
			num++;
		}
		return array;
	}

	private int SerializeMethodBody(MethodBodyStreamEncoder encoder, IMethodBody methodBody, StandaloneSignatureHandle localSignatureHandleOpt, ref UserStringHandle mvidStringHandle, ref Blob mvidStringFixup)
	{
		int length = methodBody.IL.Length;
		ImmutableArray<ExceptionHandlerRegion> exceptionRegions = methodBody.ExceptionRegions;
		bool flag = length < 64 && methodBody.MaxStack <= 8 && localSignatureHandleOpt.IsNil && exceptionRegions.Length == 0;
		if ((!_deterministic & flag) && _smallMethodBodies.TryGetValue(methodBody.IL, out var value))
		{
			return value;
		}
		MethodBodyStreamEncoder.MethodBody methodBody2 = encoder.AddMethodBody(methodBody.IL.Length, methodBody.MaxStack, exceptionRegions.Length, MayUseSmallExceptionHeaders(exceptionRegions), localSignatureHandleOpt, methodBody.LocalsAreZeroed ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None);
		if (flag && !_deterministic)
		{
			_smallMethodBodies.Add(methodBody.IL, methodBody2.Offset);
		}
		WriteInstructions(methodBody2.Instructions, methodBody.IL, ref mvidStringHandle, ref mvidStringFixup);
		SerializeMethodBodyExceptionHandlerTable(methodBody2.ExceptionRegions, exceptionRegions);
		return methodBody2.Offset;
	}

	protected virtual StandaloneSignatureHandle SerializeLocalVariablesSignature(IMethodBody body)
	{
		ImmutableArray<ILocalDefinition> localVariables = body.LocalVariables;
		if (localVariables.Length == 0)
		{
			return default;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		LocalVariablesEncoder localVariablesEncoder = new BlobEncoder(instance).LocalVariableSignature(localVariables.Length);
		foreach (ILocalDefinition item in localVariables)
		{
			SerializeLocalVariableType(localVariablesEncoder.AddVariable(), item);
		}
		BlobHandle orAddBlob = metadata.GetOrAddBlob(instance);
		StandaloneSignatureHandle orAddStandaloneSignatureHandle = GetOrAddStandaloneSignatureHandle(orAddBlob);
		instance.Free();
		return orAddStandaloneSignatureHandle;
	}

	protected void SerializeLocalVariableType(LocalVariableTypeEncoder encoder, ILocalDefinition local)
	{
		if (local.CustomModifiers.Length > 0)
		{
			SerializeCustomModifiers(encoder.CustomModifiers(), local.CustomModifiers);
		}
		if (module.IsPlatformType(local.Type, PlatformType.SystemTypedReference))
		{
			encoder.TypedReference();
		}
		else
		{
			SerializeTypeReference(encoder.Type(local.IsReference, local.IsPinned), local.Type);
		}
	}

	internal StandaloneSignatureHandle SerializeLocalConstantStandAloneSignature(ILocalDefinition localConstant)
	{
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		SignatureTypeEncoder encoder = new BlobEncoder(instance).FieldSignature();
		if (localConstant.CustomModifiers.Length > 0)
		{
			SerializeCustomModifiers(encoder.CustomModifiers(), localConstant.CustomModifiers);
		}
		SerializeTypeReference(encoder, localConstant.Type);
		BlobHandle orAddBlob = metadata.GetOrAddBlob(instance);
		StandaloneSignatureHandle orAddStandaloneSignatureHandle = GetOrAddStandaloneSignatureHandle(orAddBlob);
		instance.Free();
		return orAddStandaloneSignatureHandle;
	}

	private static byte ReadByte(ImmutableArray<byte> buffer, int pos)
	{
		return buffer[pos];
	}

	private static int ReadInt32(ImmutableArray<byte> buffer, int pos)
	{
		return buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16) | (buffer[pos + 3] << 24);
	}

	private EntityHandle GetHandle(IReference reference)
	{
		if (reference is ITypeReference typeReference)
		{
			return GetTypeHandle(typeReference);
		}
		if (reference is IFieldReference fieldReference)
		{
			return GetFieldHandle(fieldReference);
		}
		if (reference is IMethodReference methodReference)
		{
			return GetMethodHandle(methodReference);
		}
		throw ExceptionUtilities.UnexpectedValue(reference);
	}

	private EntityHandle ResolveEntityHandleFromPseudoToken(int pseudoSymbolToken)
	{
		IReference reference = _pseudoSymbolTokenToReferenceMap[pseudoSymbolToken];
		if (reference != null)
		{
			_referenceVisitor.VisitMethodBodyReference(reference);
			EntityHandle handle = GetHandle(reference);
			_pseudoSymbolTokenToTokenMap[pseudoSymbolToken] = handle;
			_pseudoSymbolTokenToReferenceMap[pseudoSymbolToken] = null;
			return handle;
		}
		return _pseudoSymbolTokenToTokenMap[pseudoSymbolToken];
	}

	private UserStringHandle ResolveUserStringHandleFromPseudoToken(int pseudoStringToken)
	{
		string text = _pseudoStringTokenToStringMap[pseudoStringToken];
		if (text != null)
		{
			UserStringHandle orAddUserString = GetOrAddUserString(text);
			_pseudoStringTokenToTokenMap[pseudoStringToken] = orAddUserString;
			_pseudoStringTokenToStringMap[pseudoStringToken] = null;
			return orAddUserString;
		}
		return _pseudoStringTokenToTokenMap[pseudoStringToken];
	}

	private UserStringHandle GetOrAddUserString(string str)
	{
		if (!_userStringTokenOverflow)
		{
			try
			{
				return metadata.GetOrAddUserString(str);
			}
			catch (ImageFormatLimitationException)
			{
				Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_TooManyUserStrings, NoLocation.Singleton));
				_userStringTokenOverflow = true;
			}
		}
		return default;
	}

	private ReservedBlob<UserStringHandle> ReserveUserString(int length)
	{
		if (!_userStringTokenOverflow)
		{
			try
			{
				return metadata.ReserveUserString(length);
			}
			catch (ImageFormatLimitationException)
			{
				Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_TooManyUserStrings, NoLocation.Singleton));
				_userStringTokenOverflow = true;
			}
		}
		return default;
	}

	private void WriteInstructions(Blob finalIL, ImmutableArray<byte> generatedIL, ref UserStringHandle mvidStringHandle, ref Blob mvidStringFixup)
	{
		BlobWriter blobWriter = new BlobWriter(finalIL);
		blobWriter.WriteBytes(generatedIL);
		blobWriter.Offset = 0;
		int position = 0;
		while (position < generatedIL.Length)
		{
			OperandType operandType = InstructionOperandTypes.ReadOperandType(generatedIL, ref position);
			switch (operandType)
			{
			case OperandType.InlineField:
			case OperandType.InlineMethod:
			case OperandType.InlineTok:
			case OperandType.InlineType:
			{
				int num3 = ReadInt32(generatedIL, position);
				int num4 = 0;
				if (operandType == OperandType.InlineTok)
				{
					int num5 = num3 & -16777216;
					if (num5 != 0 && num3 != -1)
					{
						blobWriter.Offset = position - 1;
						blobWriter.WriteByte(32);
						num4 = (uint)num5 switch
						{
							2147483648u => MetadataTokens.GetToken((MethodDefinitionHandle)ResolveEntityHandleFromPseudoToken(num3 & 0xFFFFFF)) & 0xFFFFFF, 
							1073741824u => GreatestMethodDefIndex, 
							536870912u => _dynamicAnalysisDataWriterOpt.GetOrAddDocument(module.GetSourceDocumentFromIndex((uint)(num3 & 0xFFFFFF))), 
							_ => throw ExceptionUtilities.UnexpectedValue(num5), 
						};
					}
				}
				blobWriter.Offset = position;
				blobWriter.WriteInt32((num4 == 0) ? MetadataTokens.GetToken(ResolveEntityHandleFromPseudoToken(num3)) : num4);
				position += 4;
				break;
			}
			case OperandType.InlineString:
			{
				blobWriter.Offset = position;
				int num2 = ReadInt32(generatedIL, position);
				UserStringHandle userStringHandle;
				if (num2 == int.MinValue)
				{
					if (mvidStringHandle.IsNil)
					{
						ReservedBlob<UserStringHandle> reservedBlob = ReserveUserString(36);
						mvidStringHandle = reservedBlob.Handle;
						mvidStringFixup = reservedBlob.Content;
					}
					userStringHandle = mvidStringHandle;
				}
				else
				{
					userStringHandle = ResolveUserStringHandleFromPseudoToken(num2);
				}
				blobWriter.WriteInt32(MetadataTokens.GetToken(userStringHandle));
				position += 4;
				break;
			}
			case OperandType.InlineBrTarget:
			case OperandType.InlineI:
			case OperandType.InlineSig:
			case OperandType.ShortInlineR:
				position += 4;
				break;
			case OperandType.InlineSwitch:
			{
				int num = ReadInt32(generatedIL, position);
				position += (num + 1) * 4;
				break;
			}
			case OperandType.InlineI8:
			case OperandType.InlineR:
				position += 8;
				break;
			case OperandType.InlineVar:
				position += 2;
				break;
			case OperandType.ShortInlineBrTarget:
			case OperandType.ShortInlineI:
			case OperandType.ShortInlineVar:
				position++;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(operandType);
			case OperandType.InlineNone:
				break;
			}
		}
	}

	private void SerializeMethodBodyExceptionHandlerTable(ExceptionRegionEncoder encoder, ImmutableArray<ExceptionHandlerRegion> regions)
	{
		foreach (ExceptionHandlerRegion item in regions)
		{
			ITypeReference exceptionType = item.ExceptionType;
			encoder.Add(item.HandlerKind, item.TryStartOffset, item.TryLength, item.HandlerStartOffset, item.HandlerLength, (exceptionType != null) ? GetTypeHandle(exceptionType) : default(EntityHandle), item.FilterDecisionStartOffset);
		}
	}

	private static bool MayUseSmallExceptionHeaders(ImmutableArray<ExceptionHandlerRegion> exceptionRegions)
	{
		if (!ExceptionRegionEncoder.IsSmallRegionCount(exceptionRegions.Length))
		{
			return false;
		}
		foreach (ExceptionHandlerRegion item in exceptionRegions)
		{
			if (!ExceptionRegionEncoder.IsSmallExceptionRegion(item.TryStartOffset, item.TryLength) || !ExceptionRegionEncoder.IsSmallExceptionRegion(item.HandlerStartOffset, item.HandlerLength))
			{
				return false;
			}
		}
		return true;
	}

	private void SerializeParameterInformation(ParameterTypeEncoder encoder, IParameterTypeInformation parameterTypeInformation)
	{
		ITypeReference type = parameterTypeInformation.GetType(Context);
		if (module.IsPlatformType(type, PlatformType.SystemTypedReference))
		{
			SerializeCustomModifiers(encoder.CustomModifiers(), parameterTypeInformation.CustomModifiers);
			encoder.TypedReference();
			return;
		}
		SerializeCustomModifiers(encoder.CustomModifiers(), parameterTypeInformation.RefCustomModifiers);
		SignatureTypeEncoder encoder2 = encoder.Type(parameterTypeInformation.IsByReference);
		SerializeCustomModifiers(encoder2.CustomModifiers(), parameterTypeInformation.CustomModifiers);
		SerializeTypeReference(encoder2, type);
	}

	private void SerializeFieldSignature(IFieldReference fieldReference, BlobBuilder builder)
	{
		SignatureTypeEncoder encoder = new BlobEncoder(builder).FieldSignature();
		SerializeTypeReference(encoder, fieldReference.GetType(Context));
	}

	private void SerializeMethodSpecificationSignature(BlobBuilder builder, IGenericMethodInstanceReference genericMethodInstanceReference)
	{
		GenericTypeArgumentsEncoder genericTypeArgumentsEncoder = new BlobEncoder(builder).MethodSpecificationSignature(genericMethodInstanceReference.GetGenericMethod(Context).GenericParameterCount);
		foreach (ITypeReference genericArgument in genericMethodInstanceReference.GetGenericArguments(Context))
		{
			SerializeTypeReference(genericTypeArgumentsEncoder.AddArgument(), genericArgument);
		}
	}

	private void SerializeCustomAttributeSignature(ICustomAttribute customAttribute, BlobBuilder builder)
	{
		ImmutableArray<IParameterTypeInformation> parameters = customAttribute.Constructor(Context, reportDiagnostics: false).GetParameters(Context);
		ImmutableArray<IMetadataExpression> arguments = customAttribute.GetArguments(Context);
		new BlobEncoder(builder).CustomAttributeSignature(out var fixedArguments, out var namedArguments);
		for (int i = 0; i < parameters.Length; i++)
		{
			SerializeMetadataExpression(fixedArguments.AddArgument(), arguments[i], parameters[i].GetType(Context));
		}
		SerializeCustomAttributeNamedArguments(namedArguments.Count(customAttribute.NamedArgumentCount), customAttribute);
	}

	private void SerializeCustomAttributeNamedArguments(NamedArgumentsEncoder encoder, ICustomAttribute customAttribute)
	{
		foreach (IMetadataNamedArgument namedArgument in customAttribute.GetNamedArguments(Context))
		{
			encoder.AddArgument(namedArgument.IsField, out var type, out var name, out var literal);
			SerializeNamedArgumentType(type, namedArgument.Type);
			name.Name(namedArgument.ArgumentName);
			SerializeMetadataExpression(literal, namedArgument.ArgumentValue, namedArgument.Type);
		}
	}

	private void SerializeNamedArgumentType(NamedArgumentTypeEncoder encoder, ITypeReference type)
	{
		if (type is IArrayTypeReference arrayTypeReference)
		{
			SerializeCustomAttributeArrayType(encoder.SZArray(), arrayTypeReference);
		}
		else if (module.IsPlatformType(type, PlatformType.SystemObject))
		{
			encoder.Object();
		}
		else
		{
			SerializeCustomAttributeElementType(encoder.ScalarType(), type);
		}
	}

	private void SerializeMetadataExpression(LiteralEncoder encoder, IMetadataExpression expression, ITypeReference targetType)
	{
		if (expression is MetadataCreateArray metadataCreateArray)
		{
			VectorEncoder vector;
			ITypeReference elementType;
			if (!(targetType is IArrayTypeReference arrayTypeReference))
			{
				encoder.TaggedVector(out var arrayType, out vector);
				SerializeCustomAttributeArrayType(arrayType, metadataCreateArray.ArrayType);
				elementType = metadataCreateArray.ElementType;
			}
			else
			{
				vector = encoder.Vector();
				elementType = arrayTypeReference.GetElementType(Context);
			}
			LiteralsEncoder literalsEncoder = vector.Count(metadataCreateArray.Elements.Length);
			foreach (IMetadataExpression element in metadataCreateArray.Elements)
			{
				SerializeMetadataExpression(literalsEncoder.AddLiteral(), element, elementType);
			}
			return;
		}
		MetadataConstant metadataConstant = expression as MetadataConstant;
		ScalarEncoder scalar;
		if (module.IsPlatformType(targetType, PlatformType.SystemObject))
		{
			encoder.TaggedScalar(out var type, out scalar);
			if (metadataConstant != null && metadataConstant.Value == null && module.IsPlatformType(metadataConstant.Type, PlatformType.SystemObject))
			{
				type.String();
			}
			else
			{
				SerializeCustomAttributeElementType(type, expression.Type);
			}
		}
		else
		{
			scalar = encoder.Scalar();
		}
		if (metadataConstant != null)
		{
			if (metadataConstant.Type is IArrayTypeReference)
			{
				scalar.NullArray();
			}
			else
			{
				scalar.Constant(metadataConstant.Value);
			}
		}
		else
		{
			scalar.SystemType(((MetadataTypeOf)expression).TypeToGet.GetSerializedTypeName(Context));
		}
	}

	private void SerializeMarshallingDescriptor(IMarshallingInformation marshallingInformation, BlobBuilder writer)
	{
		writer.WriteCompressedInteger((int)marshallingInformation.UnmanagedType);
		switch (marshallingInformation.UnmanagedType)
		{
		case UnmanagedType.ByValArray:
			writer.WriteCompressedInteger(marshallingInformation.NumberOfElements);
			if (marshallingInformation.ElementType >= (UnmanagedType)0)
			{
				writer.WriteCompressedInteger((int)marshallingInformation.ElementType);
			}
			break;
		case UnmanagedType.CustomMarshaler:
		{
			writer.WriteUInt16(0);
			object customMarshaller = marshallingInformation.GetCustomMarshaller(Context);
			if (customMarshaller is ITypeReference typeReference)
			{
				SerializeTypeName(typeReference, writer);
			}
			else if (customMarshaller != null)
			{
				writer.WriteSerializedString((string)customMarshaller);
			}
			else
			{
				writer.WriteByte(0);
			}
			string customMarshallerRuntimeArgument = marshallingInformation.CustomMarshallerRuntimeArgument;
			if (customMarshallerRuntimeArgument != null)
			{
				writer.WriteSerializedString(customMarshallerRuntimeArgument);
			}
			else
			{
				writer.WriteByte(0);
			}
			break;
		}
		case UnmanagedType.LPArray:
			writer.WriteCompressedInteger((int)marshallingInformation.ElementType);
			if (marshallingInformation.ParamIndex >= 0)
			{
				writer.WriteCompressedInteger(marshallingInformation.ParamIndex);
				if (marshallingInformation.NumberOfElements >= 0)
				{
					writer.WriteCompressedInteger(marshallingInformation.NumberOfElements);
					writer.WriteByte(1);
				}
			}
			else if (marshallingInformation.NumberOfElements >= 0)
			{
				writer.WriteByte(0);
				writer.WriteCompressedInteger(marshallingInformation.NumberOfElements);
				writer.WriteByte(0);
			}
			break;
		case UnmanagedType.SafeArray:
			if (marshallingInformation.SafeArrayElementSubtype >= VarEnum.VT_EMPTY)
			{
				writer.WriteCompressedInteger((int)marshallingInformation.SafeArrayElementSubtype);
				ITypeReference safeArrayElementUserDefinedSubtype = marshallingInformation.GetSafeArrayElementUserDefinedSubtype(Context);
				if (safeArrayElementUserDefinedSubtype != null)
				{
					SerializeTypeName(safeArrayElementUserDefinedSubtype, writer);
				}
			}
			break;
		case UnmanagedType.ByValTStr:
			writer.WriteCompressedInteger(marshallingInformation.NumberOfElements);
			break;
		case UnmanagedType.IUnknown:
		case UnmanagedType.IDispatch:
		case UnmanagedType.Interface:
			if (marshallingInformation.IidParameterIndex >= 0)
			{
				writer.WriteCompressedInteger(marshallingInformation.IidParameterIndex);
			}
			break;
		}
	}

	private void SerializeTypeName(ITypeReference typeReference, BlobBuilder writer)
	{
		writer.WriteSerializedString(typeReference.GetSerializedTypeName(Context));
	}

	internal static string StrongName(IAssemblyReference assemblyReference)
	{
		AssemblyIdentity identity = assemblyReference.Identity;
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append(identity.Name);
		builder.AppendFormat(CultureInfo.InvariantCulture, ", Version={0}.{1}.{2}.{3}", identity.Version.Major, identity.Version.Minor, identity.Version.Build, identity.Version.Revision);
		if (!string.IsNullOrEmpty(identity.CultureName))
		{
			builder.AppendFormat(CultureInfo.InvariantCulture, ", Culture={0}", identity.CultureName);
		}
		else
		{
			builder.Append(", Culture=neutral");
		}
		builder.Append(", PublicKeyToken=");
		if (identity.PublicKeyToken.Length > 0)
		{
			foreach (byte item in identity.PublicKeyToken)
			{
				builder.Append(item.ToString("x2"));
			}
		}
		else
		{
			builder.Append("null");
		}
		if (identity.IsRetargetable)
		{
			builder.Append(", Retargetable=Yes");
		}
		if (identity.ContentType == AssemblyContentType.WindowsRuntime)
		{
			builder.Append(", ContentType=WindowsRuntime");
		}
		return instance.ToStringAndFree();
	}

	private void SerializePermissionSet(ImmutableArray<ICustomAttribute> permissionSet, BlobBuilder writer)
	{
		EmitContext context = Context;
		foreach (ICustomAttribute item in permissionSet)
		{
			bool isAssemblyQualified = true;
			string text = item.GetType(context).GetSerializedTypeName(context, ref isAssemblyQualified);
			if (!isAssemblyQualified && item.GetType(context).AsNamespaceTypeReference?.GetUnit(context) is IAssemblyReference assemblyReference)
			{
				text = text + ", " + StrongName(assemblyReference);
			}
			writer.WriteSerializedString(text);
			PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
			NamedArgumentsEncoder encoder = new BlobEncoder(instance).PermissionSetArguments(item.NamedArgumentCount);
			SerializeCustomAttributeNamedArguments(encoder, item);
			writer.WriteCompressedInteger(instance.Count);
			instance.WriteContentTo(writer);
			instance.Free();
		}
	}

	private void SerializeReturnValueAndParameters(MethodSignatureEncoder encoder, ISignature signature, ImmutableArray<IParameterTypeInformation> varargParameters)
	{
		ImmutableArray<IParameterTypeInformation> parameters = signature.GetParameters(Context);
		ITypeReference type = signature.GetType(Context);
		encoder.Parameters(parameters.Length + varargParameters.Length, out var returnType, out var parameters2);
		if (module.IsPlatformType(type, PlatformType.SystemTypedReference))
		{
			SerializeCustomModifiers(returnType.CustomModifiers(), signature.ReturnValueCustomModifiers);
			returnType.TypedReference();
		}
		else if (module.IsPlatformType(type, PlatformType.SystemVoid))
		{
			SerializeCustomModifiers(returnType.CustomModifiers(), signature.ReturnValueCustomModifiers);
			returnType.Void();
		}
		else
		{
			SerializeCustomModifiers(returnType.CustomModifiers(), signature.RefCustomModifiers);
			SignatureTypeEncoder encoder2 = returnType.Type(signature.ReturnValueIsByRef);
			SerializeCustomModifiers(encoder2.CustomModifiers(), signature.ReturnValueCustomModifiers);
			SerializeTypeReference(encoder2, type);
		}
		foreach (IParameterTypeInformation item in parameters)
		{
			SerializeParameterInformation(parameters2.AddParameter(), item);
		}
		if (varargParameters.Length > 0)
		{
			parameters2 = parameters2.StartVarArgs();
			foreach (IParameterTypeInformation item2 in varargParameters)
			{
				SerializeParameterInformation(parameters2.AddParameter(), item2);
			}
		}
	}

	private void SerializeTypeReference(SignatureTypeEncoder encoder, ITypeReference typeReference)
	{
		while (true)
		{
			if (typeReference is IModifiedTypeReference modifiedTypeReference)
			{
				SerializeCustomModifiers(encoder.CustomModifiers(), modifiedTypeReference.CustomModifiers);
				typeReference = modifiedTypeReference.UnmodifiedType;
				continue;
			}
			PrimitiveTypeCode typeCode = typeReference.TypeCode;
			if (typeCode != PrimitiveTypeCode.Pointer && typeCode != PrimitiveTypeCode.NotPrimitive)
			{
				SerializePrimitiveType(encoder, typeCode);
				return;
			}
			if (typeReference is IPointerTypeReference pointerTypeReference)
			{
				typeReference = pointerTypeReference.GetTargetType(Context);
				encoder = encoder.Pointer();
				continue;
			}
			IGenericTypeParameterReference asGenericTypeParameterReference = typeReference.AsGenericTypeParameterReference;
			if (asGenericTypeParameterReference != null)
			{
				encoder.GenericTypeParameter(GetNumberOfInheritedTypeParameters(asGenericTypeParameterReference.DefiningType) + asGenericTypeParameterReference.Index);
				return;
			}
			if (!(typeReference is IArrayTypeReference arrayTypeReference))
			{
				break;
			}
			typeReference = arrayTypeReference.GetElementType(Context);
			if (arrayTypeReference.IsSZArray)
			{
				encoder = encoder.SZArray();
				continue;
			}
			encoder.Array(out var elementType, out var arrayShape);
			SerializeTypeReference(elementType, typeReference);
			arrayShape.Shape(arrayTypeReference.Rank, arrayTypeReference.Sizes, arrayTypeReference.LowerBounds);
			return;
		}
		if (module.IsPlatformType(typeReference, PlatformType.SystemObject))
		{
			encoder.Object();
			return;
		}
		IGenericMethodParameterReference asGenericMethodParameterReference = typeReference.AsGenericMethodParameterReference;
		if (asGenericMethodParameterReference != null)
		{
			encoder.GenericMethodTypeParameter(asGenericMethodParameterReference.Index);
		}
		else if (typeReference.IsTypeSpecification())
		{
			ITypeReference uninstantiatedGenericType = typeReference.GetUninstantiatedGenericType(Context);
			ArrayBuilder<ITypeReference> instance = ArrayBuilder<ITypeReference>.GetInstance();
			typeReference.GetConsolidatedTypeArguments(instance, Context);
			GenericTypeArgumentsEncoder genericTypeArgumentsEncoder = encoder.GenericInstantiation(GetTypeHandle(uninstantiatedGenericType, treatRefAsPotentialTypeSpec: false), instance.Count, typeReference.IsValueType);
			foreach (ITypeReference item in instance)
			{
				SerializeTypeReference(genericTypeArgumentsEncoder.AddArgument(), item);
			}
			instance.Free();
		}
		else
		{
			encoder.Type(GetTypeHandle(typeReference), typeReference.IsValueType);
		}
	}

	private static void SerializePrimitiveType(SignatureTypeEncoder encoder, PrimitiveTypeCode primitiveType)
	{
		switch (primitiveType)
		{
		case PrimitiveTypeCode.Boolean:
			encoder.Boolean();
			break;
		case PrimitiveTypeCode.UInt8:
			encoder.Byte();
			break;
		case PrimitiveTypeCode.Int8:
			encoder.SByte();
			break;
		case PrimitiveTypeCode.Char:
			encoder.Char();
			break;
		case PrimitiveTypeCode.Int16:
			encoder.Int16();
			break;
		case PrimitiveTypeCode.UInt16:
			encoder.UInt16();
			break;
		case PrimitiveTypeCode.Int32:
			encoder.Int32();
			break;
		case PrimitiveTypeCode.UInt32:
			encoder.UInt32();
			break;
		case PrimitiveTypeCode.Int64:
			encoder.Int64();
			break;
		case PrimitiveTypeCode.UInt64:
			encoder.UInt64();
			break;
		case PrimitiveTypeCode.Float32:
			encoder.Single();
			break;
		case PrimitiveTypeCode.Float64:
			encoder.Double();
			break;
		case PrimitiveTypeCode.IntPtr:
			encoder.IntPtr();
			break;
		case PrimitiveTypeCode.UIntPtr:
			encoder.UIntPtr();
			break;
		case PrimitiveTypeCode.String:
			encoder.String();
			break;
		case PrimitiveTypeCode.Void:
			encoder.Builder.WriteByte(1);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(primitiveType);
		}
	}

	private void SerializeCustomAttributeArrayType(CustomAttributeArrayTypeEncoder encoder, IArrayTypeReference arrayTypeReference)
	{
		ITypeReference elementType = arrayTypeReference.GetElementType(Context);
		if (module.IsPlatformType(elementType, PlatformType.SystemObject))
		{
			encoder.ObjectArray();
		}
		else
		{
			SerializeCustomAttributeElementType(encoder.ElementType(), elementType);
		}
	}

	private void SerializeCustomAttributeElementType(CustomAttributeElementTypeEncoder encoder, ITypeReference typeReference)
	{
		PrimitiveTypeCode typeCode = typeReference.TypeCode;
		if (typeCode != PrimitiveTypeCode.NotPrimitive)
		{
			SerializePrimitiveType(encoder, typeCode);
		}
		else if (module.IsPlatformType(typeReference, PlatformType.SystemType))
		{
			encoder.SystemType();
		}
		else
		{
			encoder.Enum(typeReference.GetSerializedTypeName(Context));
		}
	}

	private static void SerializePrimitiveType(CustomAttributeElementTypeEncoder encoder, PrimitiveTypeCode primitiveType)
	{
		switch (primitiveType)
		{
		case PrimitiveTypeCode.Boolean:
			encoder.Boolean();
			break;
		case PrimitiveTypeCode.UInt8:
			encoder.Byte();
			break;
		case PrimitiveTypeCode.Int8:
			encoder.SByte();
			break;
		case PrimitiveTypeCode.Char:
			encoder.Char();
			break;
		case PrimitiveTypeCode.Int16:
			encoder.Int16();
			break;
		case PrimitiveTypeCode.UInt16:
			encoder.UInt16();
			break;
		case PrimitiveTypeCode.Int32:
			encoder.Int32();
			break;
		case PrimitiveTypeCode.UInt32:
			encoder.UInt32();
			break;
		case PrimitiveTypeCode.Int64:
			encoder.Int64();
			break;
		case PrimitiveTypeCode.UInt64:
			encoder.UInt64();
			break;
		case PrimitiveTypeCode.Float32:
			encoder.Single();
			break;
		case PrimitiveTypeCode.Float64:
			encoder.Double();
			break;
		case PrimitiveTypeCode.String:
			encoder.String();
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(primitiveType);
		}
	}

	private void SerializeCustomModifiers(CustomModifiersEncoder encoder, ImmutableArray<ICustomModifier> modifiers)
	{
		foreach (ICustomModifier item in modifiers)
		{
			encoder = encoder.AddModifier(GetTypeHandle(item.GetModifier(Context)), item.IsOptional);
		}
	}

	private int GetNumberOfInheritedTypeParameters(ITypeReference type)
	{
		INestedTypeReference nestedTypeReference = type.AsNestedTypeReference;
		if (nestedTypeReference == null)
		{
			return 0;
		}
		ISpecializedNestedTypeReference asSpecializedNestedTypeReference = nestedTypeReference.AsSpecializedNestedTypeReference;
		if (asSpecializedNestedTypeReference != null)
		{
			nestedTypeReference = asSpecializedNestedTypeReference.GetUnspecializedVersion(Context);
		}
		int num = 0;
		type = nestedTypeReference.GetContainingType(Context);
		for (nestedTypeReference = type.AsNestedTypeReference; nestedTypeReference != null; nestedTypeReference = type.AsNestedTypeReference)
		{
			num += nestedTypeReference.GenericParameterCount;
			type = nestedTypeReference.GetContainingType(Context);
		}
		return num + type.AsNamespaceTypeReference.GenericParameterCount;
	}

	internal static EditAndContinueMethodDebugInformation GetEncMethodDebugInfo(IMethodBody methodBody)
	{
		ImmutableArray<EncHoistedLocalInfo> stateMachineHoistedLocalSlots = methodBody.StateMachineHoistedLocalSlots;
		return new EditAndContinueMethodDebugInformation(localSlots: (!stateMachineHoistedLocalSlots.IsDefault) ? GetLocalSlotDebugInfos(stateMachineHoistedLocalSlots) : GetLocalSlotDebugInfos(methodBody.LocalVariables), methodOrdinal: methodBody.MethodId.Ordinal, closures: methodBody.ClosureDebugInfo, lambdas: methodBody.LambdaDebugInfo);
	}

	internal static ImmutableArray<LocalSlotDebugInfo> GetLocalSlotDebugInfos(ImmutableArray<ILocalDefinition> locals)
	{
		if (!locals.Any((ILocalDefinition variable) => !variable.SlotInfo.Id.IsNone))
		{
			return ImmutableArray<LocalSlotDebugInfo>.Empty;
		}
		return locals.SelectAsArray((ILocalDefinition variable) => variable.SlotInfo);
	}

	internal static ImmutableArray<LocalSlotDebugInfo> GetLocalSlotDebugInfos(ImmutableArray<EncHoistedLocalInfo> locals)
	{
		if (!locals.Any((EncHoistedLocalInfo variable) => !variable.SlotInfo.Id.IsNone))
		{
			return ImmutableArray<LocalSlotDebugInfo>.Empty;
		}
		return locals.SelectAsArray((EncHoistedLocalInfo variable) => variable.SlotInfo);
	}

	private void SerializeMethodDebugInfo(IMethodBody bodyOpt, int methodRid, StandaloneSignatureHandle localSignatureHandleOpt, ref LocalVariableHandle lastLocalVariableHandle, ref LocalConstantHandle lastLocalConstantHandle)
	{
		if (bodyOpt == null)
		{
			_debugMetadataOpt.AddMethodDebugInformation(default, default);
			return;
		}
		if (bodyOpt.StateMachineTypeName == null && bodyOpt.SequencePoints.IsEmpty)
		{
			_debugMetadataOpt.AddMethodDebugInformation(default, default);
			return;
		}
		MethodDefinitionHandle methodDefinitionHandle = MetadataTokens.MethodDefinitionHandle(methodRid);
		IImportScope importScope = bodyOpt.ImportScope;
		ImportScopeHandle importScope2 = ((importScope != null) ? GetImportScopeIndex(importScope, _scopeIndex) : default(ImportScopeHandle));
		BlobHandle sequencePoints = SerializeSequencePoints(localSignatureHandleOpt, bodyOpt.SequencePoints, _documentIndex, out var singleDocumentHandle);
		_debugMetadataOpt.AddMethodDebugInformation(singleDocumentHandle, sequencePoints);
		if (bodyOpt.LocalScopes.Length == 0)
		{
			_debugMetadataOpt.AddLocalScope(methodDefinitionHandle, importScope2, NextHandle(lastLocalVariableHandle), NextHandle(lastLocalConstantHandle), 0, bodyOpt.IL.Length);
		}
		else
		{
			foreach (LocalScope localScope in bodyOpt.LocalScopes)
			{
				_debugMetadataOpt.AddLocalScope(methodDefinitionHandle, importScope2, NextHandle(lastLocalVariableHandle), NextHandle(lastLocalConstantHandle), localScope.StartOffset, localScope.Length);
				foreach (ILocalDefinition variable in localScope.Variables)
				{
					lastLocalVariableHandle = _debugMetadataOpt.AddLocalVariable(variable.PdbAttributes, variable.SlotIndex, _debugMetadataOpt.GetOrAddString(variable.Name));
					SerializeLocalInfo(variable, lastLocalVariableHandle);
				}
				foreach (ILocalDefinition constant in localScope.Constants)
				{
					_ = constant.CompileTimeValue;
					lastLocalConstantHandle = _debugMetadataOpt.AddLocalConstant(_debugMetadataOpt.GetOrAddString(constant.Name), SerializeLocalConstantSignature(constant));
					SerializeLocalInfo(constant, lastLocalConstantHandle);
				}
			}
		}
		StateMachineMoveNextBodyDebugInfo moveNextBodyInfo = bodyOpt.MoveNextBodyInfo;
		if (moveNextBodyInfo != null)
		{
			_debugMetadataOpt.AddStateMachineMethod(methodDefinitionHandle, GetMethodDefinitionHandle(moveNextBodyInfo.KickoffMethod));
			if (moveNextBodyInfo is AsyncMoveNextBodyDebugInfo asyncInfo)
			{
				SerializeAsyncMethodSteppingInfo(asyncInfo, methodDefinitionHandle);
			}
		}
		SerializeStateMachineLocalScopes(bodyOpt, methodDefinitionHandle);
		if (Context.Module.CommonCompilation.Options.EnableEditAndContinue && IsFullMetadata)
		{
			SerializeEncMethodDebugInformation(bodyOpt, methodDefinitionHandle);
		}
	}

	private static LocalVariableHandle NextHandle(LocalVariableHandle handle)
	{
		return MetadataTokens.LocalVariableHandle(MetadataTokens.GetRowNumber(handle) + 1);
	}

	private static LocalConstantHandle NextHandle(LocalConstantHandle handle)
	{
		return MetadataTokens.LocalConstantHandle(MetadataTokens.GetRowNumber(handle) + 1);
	}

	private BlobHandle SerializeLocalConstantSignature(ILocalDefinition localConstant)
	{
		BlobBuilder blobBuilder = new BlobBuilder();
		CustomModifiersEncoder encoder = new CustomModifiersEncoder(blobBuilder);
		SerializeCustomModifiers(encoder, localConstant.CustomModifiers);
		ITypeReference type = localConstant.Type;
		PrimitiveTypeCode typeCode = type.TypeCode;
		object value = localConstant.CompileTimeValue.Value;
		if (value is decimal)
		{
			blobBuilder.WriteByte(17);
			blobBuilder.WriteCompressedInteger(CodedIndex.TypeDefOrRefOrSpec(GetTypeHandle(type)));
			blobBuilder.WriteDecimal((decimal)value);
		}
		else if (value is DateTime)
		{
			blobBuilder.WriteByte(17);
			blobBuilder.WriteCompressedInteger(CodedIndex.TypeDefOrRefOrSpec(GetTypeHandle(type)));
			blobBuilder.WriteDateTime((DateTime)value);
		}
		else if (typeCode == PrimitiveTypeCode.String)
		{
			blobBuilder.WriteByte(14);
			if (value == null)
			{
				blobBuilder.WriteByte(byte.MaxValue);
			}
			else
			{
				blobBuilder.WriteUTF16((string)value);
			}
		}
		else if (value != null)
		{
			blobBuilder.WriteByte((byte)GetConstantTypeCode(value));
			blobBuilder.WriteConstant(value);
			if (type.IsEnum)
			{
				blobBuilder.WriteCompressedInteger(CodedIndex.TypeDefOrRefOrSpec(GetTypeHandle(type)));
			}
		}
		else if (module.IsPlatformType(type, PlatformType.SystemObject))
		{
			blobBuilder.WriteByte(28);
		}
		else
		{
			blobBuilder.WriteByte((byte)(type.IsValueType ? 17 : 18));
			blobBuilder.WriteCompressedInteger(CodedIndex.TypeDefOrRefOrSpec(GetTypeHandle(type)));
		}
		return _debugMetadataOpt.GetOrAddBlob(blobBuilder);
	}

	private static SignatureTypeCode GetConstantTypeCode(object value)
	{
		if (value == null)
		{
			return (SignatureTypeCode)18;
		}
		if ((object)value.GetType() == typeof(int))
		{
			return SignatureTypeCode.Int32;
		}
		if ((object)value.GetType() == typeof(string))
		{
			return SignatureTypeCode.String;
		}
		if ((object)value.GetType() == typeof(bool))
		{
			return SignatureTypeCode.Boolean;
		}
		if ((object)value.GetType() == typeof(char))
		{
			return SignatureTypeCode.Char;
		}
		if ((object)value.GetType() == typeof(byte))
		{
			return SignatureTypeCode.Byte;
		}
		if ((object)value.GetType() == typeof(long))
		{
			return SignatureTypeCode.Int64;
		}
		if ((object)value.GetType() == typeof(double))
		{
			return SignatureTypeCode.Double;
		}
		if ((object)value.GetType() == typeof(short))
		{
			return SignatureTypeCode.Int16;
		}
		if ((object)value.GetType() == typeof(ushort))
		{
			return SignatureTypeCode.UInt16;
		}
		if ((object)value.GetType() == typeof(uint))
		{
			return SignatureTypeCode.UInt32;
		}
		if ((object)value.GetType() == typeof(sbyte))
		{
			return SignatureTypeCode.SByte;
		}
		if ((object)value.GetType() == typeof(ulong))
		{
			return SignatureTypeCode.UInt64;
		}
		if ((object)value.GetType() == typeof(float))
		{
			return SignatureTypeCode.Single;
		}
		throw ExceptionUtilities.Unreachable;
	}

	private void SerializeImport(BlobBuilder writer, AssemblyReferenceAlias alias)
	{
		writer.WriteByte(6);
		writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(alias.Name)));
		writer.WriteCompressedInteger(MetadataTokens.GetRowNumber(GetOrAddAssemblyReferenceHandle(alias.Assembly)));
	}

	private void SerializeImport(BlobBuilder writer, UsedNamespaceOrType import)
	{
		if (import.TargetXmlNamespaceOpt != null)
		{
			writer.WriteByte(4);
			writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(import.AliasOpt)));
			writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(import.TargetXmlNamespaceOpt)));
		}
		else if (import.TargetTypeOpt != null)
		{
			if (import.AliasOpt != null)
			{
				writer.WriteByte(9);
				writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(import.AliasOpt)));
			}
			else
			{
				writer.WriteByte(3);
			}
			writer.WriteCompressedInteger(CodedIndex.TypeDefOrRefOrSpec(GetTypeHandle(import.TargetTypeOpt)));
		}
		else if (import.TargetNamespaceOpt != null)
		{
			if (import.TargetAssemblyOpt != null)
			{
				if (import.AliasOpt != null)
				{
					writer.WriteByte(8);
					writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(import.AliasOpt)));
				}
				else
				{
					writer.WriteByte(2);
				}
				writer.WriteCompressedInteger(MetadataTokens.GetRowNumber(GetAssemblyReferenceHandle(import.TargetAssemblyOpt)));
			}
			else if (import.AliasOpt != null)
			{
				writer.WriteByte(7);
				writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(import.AliasOpt)));
			}
			else
			{
				writer.WriteByte(1);
			}
			string value = TypeNameSerializer.BuildQualifiedNamespaceName(import.TargetNamespaceOpt);
			writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(value)));
		}
		else
		{
			writer.WriteByte(5);
			writer.WriteCompressedInteger(MetadataTokens.GetHeapOffset(_debugMetadataOpt.GetOrAddBlobUTF8(import.AliasOpt)));
		}
	}

	private void DefineModuleImportScope()
	{
		BlobBuilder blobBuilder = new BlobBuilder();
		SerializeModuleDefaultNamespace();
		foreach (AssemblyReferenceAlias assemblyReferenceAlias in module.GetAssemblyReferenceAliases(Context))
		{
			SerializeImport(blobBuilder, assemblyReferenceAlias);
		}
		foreach (UsedNamespaceOrType import in module.GetImports())
		{
			SerializeImport(blobBuilder, import);
		}
		_debugMetadataOpt.AddImportScope(default, _debugMetadataOpt.GetOrAddBlob(blobBuilder));
	}

	private ImportScopeHandle GetImportScopeIndex(IImportScope scope, Dictionary<IImportScope, ImportScopeHandle> scopeIndex)
	{
		if (scopeIndex.TryGetValue(scope, out var value))
		{
			return value;
		}
		ImportScopeHandle parentScope = ((scope.Parent != null) ? GetImportScopeIndex(scope.Parent, scopeIndex) : ModuleImportScopeHandle);
		ImportScopeHandle importScopeHandle = _debugMetadataOpt.AddImportScope(parentScope, SerializeImportsBlob(scope));
		scopeIndex.Add(scope, importScopeHandle);
		return importScopeHandle;
	}

	private BlobHandle SerializeImportsBlob(IImportScope scope)
	{
		BlobBuilder blobBuilder = new BlobBuilder();
		foreach (UsedNamespaceOrType usedNamespace in scope.GetUsedNamespaces())
		{
			SerializeImport(blobBuilder, usedNamespace);
		}
		return _debugMetadataOpt.GetOrAddBlob(blobBuilder);
	}

	private void SerializeModuleDefaultNamespace()
	{
		if (module.DefaultNamespace != null)
		{
			_debugMetadataOpt.AddCustomDebugInformation(EntityHandle.ModuleDefinition, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.DefaultNamespace), _debugMetadataOpt.GetOrAddBlobUTF8(module.DefaultNamespace));
		}
	}

	private void SerializeLocalInfo(ILocalDefinition local, EntityHandle parent)
	{
		ImmutableArray<bool> dynamicTransformFlags = local.DynamicTransformFlags;
		if (!dynamicTransformFlags.IsEmpty)
		{
			ImmutableArray<byte> value = SerializeBitVector(dynamicTransformFlags);
			_debugMetadataOpt.AddCustomDebugInformation(parent, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.DynamicLocalVariables), _debugMetadataOpt.GetOrAddBlob(value));
		}
		ImmutableArray<string> tupleElementNames = local.TupleElementNames;
		if (!tupleElementNames.IsEmpty)
		{
			BlobBuilder blobBuilder = new BlobBuilder();
			SerializeTupleElementNames(blobBuilder, tupleElementNames);
			_debugMetadataOpt.AddCustomDebugInformation(parent, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.TupleElementNames), _debugMetadataOpt.GetOrAddBlob(blobBuilder));
		}
	}

	private static ImmutableArray<byte> SerializeBitVector(ImmutableArray<bool> vector)
	{
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < vector.Length; i++)
		{
			if (vector[i])
			{
				num |= 1 << num2;
			}
			if (num2 == 7)
			{
				instance.Add((byte)num);
				num = 0;
				num2 = 0;
			}
			else
			{
				num2++;
			}
		}
		if (num != 0)
		{
			instance.Add((byte)num);
		}
		else
		{
			int num3 = instance.Count - 1;
			while (instance[num3] == 0)
			{
				num3--;
			}
			instance.Clip(num3 + 1);
		}
		return instance.ToImmutableAndFree();
	}

	private static void SerializeTupleElementNames(BlobBuilder builder, ImmutableArray<string> names)
	{
		foreach (string item in names)
		{
			WriteUtf8String(builder, item ?? string.Empty);
		}
	}

	private static void WriteUtf8String(BlobBuilder builder, string str)
	{
		builder.WriteUTF8(str);
		builder.WriteByte(0);
	}

	private void SerializeAsyncMethodSteppingInfo(AsyncMoveNextBodyDebugInfo asyncInfo, MethodDefinitionHandle moveNextMethod)
	{
		BlobBuilder blobBuilder = new BlobBuilder();
		blobBuilder.WriteUInt32((uint)((ulong)asyncInfo.CatchHandlerOffset + 1uL));
		for (int i = 0; i < asyncInfo.ResumeOffsets.Length; i++)
		{
			blobBuilder.WriteUInt32((uint)asyncInfo.YieldOffsets[i]);
			blobBuilder.WriteUInt32((uint)asyncInfo.ResumeOffsets[i]);
			blobBuilder.WriteCompressedInteger(MetadataTokens.GetRowNumber(moveNextMethod));
		}
		_debugMetadataOpt.AddCustomDebugInformation(moveNextMethod, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.AsyncMethodSteppingInformationBlob), _debugMetadataOpt.GetOrAddBlob(blobBuilder));
	}

	private void SerializeStateMachineLocalScopes(IMethodBody methodBody, MethodDefinitionHandle method)
	{
		ImmutableArray<StateMachineHoistedLocalScope> stateMachineHoistedLocalScopes = methodBody.StateMachineHoistedLocalScopes;
		if (!stateMachineHoistedLocalScopes.IsDefaultOrEmpty)
		{
			BlobBuilder blobBuilder = new BlobBuilder();
			foreach (StateMachineHoistedLocalScope item in stateMachineHoistedLocalScopes)
			{
				blobBuilder.WriteUInt32((uint)item.StartOffset);
				blobBuilder.WriteUInt32((uint)item.Length);
			}
			_debugMetadataOpt.AddCustomDebugInformation(method, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.StateMachineHoistedLocalScopes), _debugMetadataOpt.GetOrAddBlob(blobBuilder));
		}
	}

	private BlobHandle SerializeSequencePoints(StandaloneSignatureHandle localSignatureHandleOpt, ImmutableArray<SequencePoint> sequencePoints, Dictionary<DebugSourceDocument, DocumentHandle> documentIndex, out DocumentHandle singleDocumentHandle)
	{
		if (sequencePoints.Length == 0)
		{
			singleDocumentHandle = default;
			return default;
		}
		BlobBuilder blobBuilder = new BlobBuilder();
		int num = -1;
		int num2 = -1;
		blobBuilder.WriteCompressedInteger(MetadataTokens.GetRowNumber(localSignatureHandleOpt));
		DebugSourceDocument debugSourceDocument = TryGetSingleDocument(sequencePoints);
		singleDocumentHandle = ((debugSourceDocument != null) ? GetOrAddDocument(debugSourceDocument, documentIndex) : default(DocumentHandle));
		for (int i = 0; i < sequencePoints.Length; i++)
		{
			DebugSourceDocument document = sequencePoints[i].Document;
			if (debugSourceDocument != document)
			{
				DocumentHandle orAddDocument = GetOrAddDocument(document, documentIndex);
				if (debugSourceDocument != null)
				{
					blobBuilder.WriteCompressedInteger(0);
				}
				blobBuilder.WriteCompressedInteger(MetadataTokens.GetRowNumber(orAddDocument));
				debugSourceDocument = document;
			}
			if (i > 0)
			{
				blobBuilder.WriteCompressedInteger(sequencePoints[i].Offset - sequencePoints[i - 1].Offset);
			}
			else
			{
				blobBuilder.WriteCompressedInteger(sequencePoints[i].Offset);
			}
			if (sequencePoints[i].IsHidden)
			{
				blobBuilder.WriteInt16(0);
				continue;
			}
			SerializeDeltaLinesAndColumns(blobBuilder, sequencePoints[i]);
			if (num < 0)
			{
				blobBuilder.WriteCompressedInteger(sequencePoints[i].StartLine);
				blobBuilder.WriteCompressedInteger(sequencePoints[i].StartColumn);
			}
			else
			{
				blobBuilder.WriteCompressedSignedInteger(sequencePoints[i].StartLine - num);
				blobBuilder.WriteCompressedSignedInteger(sequencePoints[i].StartColumn - num2);
			}
			num = sequencePoints[i].StartLine;
			num2 = sequencePoints[i].StartColumn;
		}
		return _debugMetadataOpt.GetOrAddBlob(blobBuilder);
	}

	private static DebugSourceDocument TryGetSingleDocument(ImmutableArray<SequencePoint> sequencePoints)
	{
		DebugSourceDocument document = sequencePoints[0].Document;
		for (int i = 1; i < sequencePoints.Length; i++)
		{
			if (sequencePoints[i].Document != document)
			{
				return null;
			}
		}
		return document;
	}

	private void SerializeDeltaLinesAndColumns(BlobBuilder writer, SequencePoint sequencePoint)
	{
		int num = sequencePoint.EndLine - sequencePoint.StartLine;
		int value = sequencePoint.EndColumn - sequencePoint.StartColumn;
		writer.WriteCompressedInteger(num);
		if (num == 0)
		{
			writer.WriteCompressedInteger(value);
		}
		else
		{
			writer.WriteCompressedSignedInteger(value);
		}
	}

	private DocumentHandle GetOrAddDocument(DebugSourceDocument document, Dictionary<DebugSourceDocument, DocumentHandle> index)
	{
		if (!index.TryGetValue(document, out var value))
		{
			DebugSourceInfo sourceInfo = document.GetSourceInfo();
			value = _debugMetadataOpt.AddDocument(_debugMetadataOpt.GetOrAddDocumentName(document.Location), sourceInfo.Checksum.IsDefault ? default(GuidHandle) : _debugMetadataOpt.GetOrAddGuid(sourceInfo.ChecksumAlgorithmId), sourceInfo.Checksum.IsDefault ? default(BlobHandle) : _debugMetadataOpt.GetOrAddBlob(sourceInfo.Checksum), _debugMetadataOpt.GetOrAddGuid(document.Language));
			index.Add(document, value);
			if (sourceInfo.EmbeddedTextBlob != null)
			{
				_debugMetadataOpt.AddCustomDebugInformation(value, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.EmbeddedSource), _debugMetadataOpt.GetOrAddBlob(sourceInfo.EmbeddedTextBlob));
			}
		}
		return value;
	}

	public void AddRemainingEmbeddedDocuments(IEnumerable<DebugSourceDocument> documents)
	{
		foreach (DebugSourceDocument document in documents)
		{
			GetOrAddDocument(document, _documentIndex);
		}
	}

	private void SerializeEncMethodDebugInformation(IMethodBody methodBody, MethodDefinitionHandle method)
	{
		EditAndContinueMethodDebugInformation encMethodDebugInfo = GetEncMethodDebugInfo(methodBody);
		if (!encMethodDebugInfo.LocalSlots.IsDefaultOrEmpty)
		{
			BlobBuilder blobBuilder = new BlobBuilder();
			encMethodDebugInfo.SerializeLocalSlots(blobBuilder);
			_debugMetadataOpt.AddCustomDebugInformation(method, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.EncLocalSlotMap), _debugMetadataOpt.GetOrAddBlob(blobBuilder));
		}
		if (!encMethodDebugInfo.Lambdas.IsDefaultOrEmpty)
		{
			BlobBuilder blobBuilder2 = new BlobBuilder();
			encMethodDebugInfo.SerializeLambdaMap(blobBuilder2);
			_debugMetadataOpt.AddCustomDebugInformation(method, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.EncLambdaAndClosureMap), _debugMetadataOpt.GetOrAddBlob(blobBuilder2));
		}
	}

	private void EmbedSourceLink(Stream stream)
	{
		byte[] value;
		try
		{
			value = stream.ReadAllBytes();
		}
		catch (Exception ex) when (!(ex is OperationCanceledException))
		{
			throw new SymUnmanagedWriterException(ex.Message, ex);
		}
		_debugMetadataOpt.AddCustomDebugInformation(EntityHandle.ModuleDefinition, _debugMetadataOpt.GetOrAddGuid(PortableCustomDebugInfoKinds.SourceLink), _debugMetadataOpt.GetOrAddBlob(value));
	}
}
