using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class DeltaMetadataWriter : MetadataWriter
{
	private abstract class DefinitionIndexBase<T>
	{
		protected readonly Dictionary<T, int> added;

		protected readonly List<T> rows;

		private readonly int _firstRowId;

		private bool _frozen;

		public int this[T item]
		{
			get
			{
				TryGetValue(item, out var index);
				return index;
			}
		}

		public int FirstRowId => _firstRowId;

		public int NextRowId => added.Count + _firstRowId;

		public bool IsFrozen => _frozen;

		public DefinitionIndexBase(int lastRowId)
		{
			added = new Dictionary<T, int>();
			rows = new List<T>();
			_firstRowId = lastRowId + 1;
		}

		public abstract bool TryGetValue(T item, out int index);

		public IReadOnlyDictionary<T, int> GetAdded()
		{
			Freeze();
			return added;
		}

		public IReadOnlyList<T> GetRows()
		{
			Freeze();
			return rows;
		}

		protected virtual void OnFrozen()
		{
		}

		private void Freeze()
		{
			if (!_frozen)
			{
				_frozen = true;
				OnFrozen();
			}
		}
	}

	private sealed class DefinitionIndex<T> : DefinitionIndexBase<T> where T : IDefinition
	{
		public delegate bool TryGetExistingIndex(T item, out int index);

		private readonly TryGetExistingIndex _tryGetExistingIndex;

		private readonly Dictionary<int, T> _map;

		public T this[int rowId] => _map[rowId];

		public DefinitionIndex(TryGetExistingIndex tryGetExistingIndex, int lastRowId)
			: base(lastRowId)
		{
			_tryGetExistingIndex = tryGetExistingIndex;
			_map = new Dictionary<int, T>();
		}

		public override bool TryGetValue(T item, out int index)
		{
			if (added.TryGetValue(item, out index))
			{
				return true;
			}
			if (_tryGetExistingIndex(item, out index))
			{
				_map[index] = item;
				return true;
			}
			return false;
		}

		public void Add(T item)
		{
			int nextRowId = NextRowId;
			added.Add(item, nextRowId);
			_map[nextRowId] = item;
			rows.Add(item);
		}

		public void AddUpdated(T item)
		{
			rows.Add(item);
		}

		public bool IsAddedNotChanged(T item)
		{
			return added.ContainsKey(item);
		}

		protected override void OnFrozen()
		{
			rows.Sort(CompareRows);
		}

		private int CompareRows(T x, T y)
		{
			return base[x] - base[y];
		}
	}

	private sealed class ParameterDefinitionIndex : DefinitionIndexBase<IParameterDefinition>
	{
		public ParameterDefinitionIndex(int lastRowId)
			: base(lastRowId)
		{
		}

		public override bool TryGetValue(IParameterDefinition item, out int index)
		{
			return added.TryGetValue(item, out index);
		}

		public void Add(IParameterDefinition item)
		{
			int nextRowId = NextRowId;
			added.Add(item, nextRowId);
			rows.Add(item);
		}
	}

	private sealed class GenericParameterIndex : DefinitionIndexBase<IGenericParameter>
	{
		public GenericParameterIndex(int lastRowId)
			: base(lastRowId)
		{
		}

		public override bool TryGetValue(IGenericParameter item, out int index)
		{
			return added.TryGetValue(item, out index);
		}

		public void Add(IGenericParameter item)
		{
			int nextRowId = NextRowId;
			added.Add(item, nextRowId);
			rows.Add(item);
		}
	}

	private sealed class EventOrPropertyMapIndex : DefinitionIndexBase<int>
	{
		public delegate bool TryGetExistingIndex(int item, out int index);

		private readonly TryGetExistingIndex _tryGetExistingIndex;

		public EventOrPropertyMapIndex(TryGetExistingIndex tryGetExistingIndex, int lastRowId)
			: base(lastRowId)
		{
			_tryGetExistingIndex = tryGetExistingIndex;
		}

		public override bool TryGetValue(int item, out int index)
		{
			if (added.TryGetValue(item, out index))
			{
				return true;
			}
			if (_tryGetExistingIndex(item, out index))
			{
				return true;
			}
			index = 0;
			return false;
		}

		public void Add(int item)
		{
			int nextRowId = NextRowId;
			added.Add(item, nextRowId);
			rows.Add(item);
		}
	}

	private sealed class MethodImplIndex : DefinitionIndexBase<MethodImplKey>
	{
		private readonly DeltaMetadataWriter _writer;

		public MethodImplIndex(DeltaMetadataWriter writer, int lastRowId)
			: base(lastRowId)
		{
			_writer = writer;
		}

		public override bool TryGetValue(MethodImplKey item, out int index)
		{
			if (added.TryGetValue(item, out index))
			{
				return true;
			}
			if (_writer.TryGetExistingMethodImplIndex(item, out index))
			{
				return true;
			}
			index = 0;
			return false;
		}

		public void Add(MethodImplKey item)
		{
			int nextRowId = NextRowId;
			added.Add(item, nextRowId);
			rows.Add(item);
		}
	}

	private sealed class DeltaReferenceIndexer : ReferenceIndexer
	{
		private readonly SymbolChanges _changes;

		public DeltaReferenceIndexer(DeltaMetadataWriter writer)
			: base(writer)
		{
			_changes = writer._changes;
		}

		public override void Visit(CommonPEModuleBuilder module)
		{
			Visit(((DeltaMetadataWriter)metadataWriter).GetTopLevelTypes(module));
		}

		public override void Visit(IEventDefinition eventDefinition)
		{
			base.Visit(eventDefinition);
		}

		public override void Visit(IFieldDefinition fieldDefinition)
		{
			base.Visit(fieldDefinition);
		}

		public override void Visit(ILocalDefinition localDefinition)
		{
			if (localDefinition.Signature == null)
			{
				base.Visit(localDefinition);
			}
		}

		public override void Visit(IMethodDefinition method)
		{
			base.Visit(method);
		}

		public override void Visit(Microsoft.Cci.MethodImplementation methodImplementation)
		{
			IMethodDefinition def = (IMethodDefinition)methodImplementation.ImplementingMethod.AsDefinition(Context);
			if (_changes.GetChange(def) == SymbolChange.Added)
			{
				base.Visit(methodImplementation);
			}
		}

		public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition)
		{
			base.Visit(namespaceTypeDefinition);
		}

		public override void Visit(INestedTypeDefinition nestedTypeDefinition)
		{
			base.Visit(nestedTypeDefinition);
		}

		public override void Visit(IPropertyDefinition propertyDefinition)
		{
			base.Visit(propertyDefinition);
		}

		public override void Visit(ITypeDefinition typeDefinition)
		{
			if (ShouldVisit(typeDefinition))
			{
				base.Visit(typeDefinition);
			}
		}

		public override void Visit(ITypeDefinitionMember typeMember)
		{
			if (ShouldVisit(typeMember))
			{
				base.Visit(typeMember);
			}
		}

		private bool ShouldVisit(IDefinition def)
		{
			return _changes.GetChange(def) != SymbolChange.None;
		}
	}

	private readonly EmitBaseline _previousGeneration;

	private readonly Guid _encId;

	private readonly DefinitionMap _definitionMap;

	private readonly SymbolChanges _changes;

	private readonly DefinitionIndex<ITypeDefinition> _typeDefs;

	private readonly DefinitionIndex<IEventDefinition> _eventDefs;

	private readonly DefinitionIndex<IFieldDefinition> _fieldDefs;

	private readonly DefinitionIndex<IMethodDefinition> _methodDefs;

	private readonly DefinitionIndex<IPropertyDefinition> _propertyDefs;

	private readonly ParameterDefinitionIndex _parameterDefs;

	private readonly List<KeyValuePair<IMethodDefinition, IParameterDefinition>> _parameterDefList;

	private readonly GenericParameterIndex _genericParameters;

	private readonly EventOrPropertyMapIndex _eventMap;

	private readonly EventOrPropertyMapIndex _propertyMap;

	private readonly MethodImplIndex _methodImpls;

	private readonly HeapOrReferenceIndex<AssemblyIdentity> _assemblyRefIndex;

	private readonly HeapOrReferenceIndex<string> _moduleRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<ITypeMemberReference> _memberRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference> _methodSpecIndex;

	private readonly HeapOrReferenceIndex<ITypeReference> _typeRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<ITypeReference> _typeSpecIndex;

	private readonly HeapOrReferenceIndex<BlobHandle> _standAloneSignatureIndex;

	private readonly Dictionary<IMethodDefinition, AddedOrChangedMethodInfo> _addedOrChangedMethods;

	protected override ushort Generation => (ushort)(_previousGeneration.Ordinal + 1);

	protected override Guid EncId => _encId;

	protected override Guid EncBaseId => _previousGeneration.EncId;

	protected override int GreatestMethodDefIndex => _methodDefs.NextRowId;

	public DeltaMetadataWriter(EmitContext context, CommonMessageProvider messageProvider, EmitBaseline previousGeneration, Guid encId, DefinitionMap definitionMap, SymbolChanges changes, CancellationToken cancellationToken)
		: base(MakeTablesBuilder(previousGeneration), (context.Module.DebugInformationFormat == DebugInformationFormat.PortablePdb) ? new MetadataBuilder() : null, null, context, messageProvider, metadataOnly: false, deterministic: false, emitTestCoverageData: false, cancellationToken)
	{
		_previousGeneration = previousGeneration;
		_encId = encId;
		_definitionMap = definitionMap;
		_changes = changes;
		ImmutableArray<int> tableSizes = previousGeneration.TableSizes;
		_typeDefs = new DefinitionIndex<ITypeDefinition>(TryGetExistingTypeDefIndex, tableSizes[2]);
		_eventDefs = new DefinitionIndex<IEventDefinition>(TryGetExistingEventDefIndex, tableSizes[20]);
		_fieldDefs = new DefinitionIndex<IFieldDefinition>(TryGetExistingFieldDefIndex, tableSizes[4]);
		_methodDefs = new DefinitionIndex<IMethodDefinition>(TryGetExistingMethodDefIndex, tableSizes[6]);
		_propertyDefs = new DefinitionIndex<IPropertyDefinition>(TryGetExistingPropertyDefIndex, tableSizes[23]);
		_parameterDefs = new ParameterDefinitionIndex(tableSizes[8]);
		_parameterDefList = new List<KeyValuePair<IMethodDefinition, IParameterDefinition>>();
		_genericParameters = new GenericParameterIndex(tableSizes[42]);
		_eventMap = new EventOrPropertyMapIndex(TryGetExistingEventMapIndex, tableSizes[18]);
		_propertyMap = new EventOrPropertyMapIndex(TryGetExistingPropertyMapIndex, tableSizes[21]);
		_methodImpls = new MethodImplIndex(this, tableSizes[25]);
		_assemblyRefIndex = new HeapOrReferenceIndex<AssemblyIdentity>(this, tableSizes[35]);
		_moduleRefIndex = new HeapOrReferenceIndex<string>(this, tableSizes[26]);
		_memberRefIndex = new InstanceAndStructuralReferenceIndex<ITypeMemberReference>(this, new MemberRefComparer(this), tableSizes[10]);
		_methodSpecIndex = new InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference>(this, new MethodSpecComparer(this), tableSizes[43]);
		_typeRefIndex = new HeapOrReferenceIndex<ITypeReference>(this, tableSizes[1]);
		_typeSpecIndex = new InstanceAndStructuralReferenceIndex<ITypeReference>(this, new TypeSpecComparer(this), tableSizes[27]);
		_standAloneSignatureIndex = new HeapOrReferenceIndex<BlobHandle>(this, tableSizes[17]);
		_addedOrChangedMethods = new Dictionary<IMethodDefinition, AddedOrChangedMethodInfo>();
	}

	private static MetadataBuilder MakeTablesBuilder(EmitBaseline previousGeneration)
	{
		return new MetadataBuilder(previousGeneration.UserStringStreamLength, previousGeneration.StringStreamLength, previousGeneration.BlobStreamLength, previousGeneration.GuidStreamLength);
	}

	private ImmutableArray<int> GetDeltaTableSizes(ImmutableArray<int> rowCounts)
	{
		int[] array = new int[MetadataTokens.TableCount];
		rowCounts.CopyTo(array);
		array[1] = _typeRefIndex.Rows.Count;
		array[2] = _typeDefs.GetAdded().Count;
		array[4] = _fieldDefs.GetAdded().Count;
		array[6] = _methodDefs.GetAdded().Count;
		array[8] = _parameterDefs.GetAdded().Count;
		array[10] = _memberRefIndex.Rows.Count;
		array[17] = _standAloneSignatureIndex.Rows.Count;
		array[18] = _eventMap.GetAdded().Count;
		array[20] = _eventDefs.GetAdded().Count;
		array[21] = _propertyMap.GetAdded().Count;
		array[23] = _propertyDefs.GetAdded().Count;
		array[25] = _methodImpls.GetAdded().Count;
		array[26] = _moduleRefIndex.Rows.Count;
		array[27] = _typeSpecIndex.Rows.Count;
		array[35] = _assemblyRefIndex.Rows.Count;
		array[42] = _genericParameters.GetAdded().Count;
		array[43] = _methodSpecIndex.Rows.Count;
		return ImmutableArray.Create(array);
	}

	internal EmitBaseline GetDelta(EmitBaseline baseline, Compilation compilation, Guid encId, MetadataSizes metadataSizes)
	{
		Dictionary<int, AddedOrChangedMethodInfo> dictionary = new Dictionary<int, AddedOrChangedMethodInfo>();
		foreach (KeyValuePair<IMethodDefinition, AddedOrChangedMethodInfo> addedOrChangedMethod in _addedOrChangedMethods)
		{
			dictionary.Add(MetadataTokens.GetRowNumber(GetMethodDefinitionHandle(addedOrChangedMethod.Key)), addedOrChangedMethod.Value);
		}
		ImmutableArray<int> tableEntriesAdded = _previousGeneration.TableEntriesAdded;
		ImmutableArray<int> deltaTableSizes = GetDeltaTableSizes(metadataSizes.RowCounts);
		int[] array = new int[MetadataTokens.TableCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = tableEntriesAdded[i] + deltaTableSizes[i];
		}
		ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> synthesizedMembers = ((baseline.Ordinal == 0) ? module.GetSynthesizedMembers() : baseline.SynthesizedMembers);
		return baseline.With(compilation, module, baseline.Ordinal + 1, encId, AddRange(_previousGeneration.TypesAdded, _typeDefs.GetAdded()), AddRange(_previousGeneration.EventsAdded, _eventDefs.GetAdded()), AddRange(_previousGeneration.FieldsAdded, _fieldDefs.GetAdded()), AddRange(_previousGeneration.MethodsAdded, _methodDefs.GetAdded()), AddRange(_previousGeneration.PropertiesAdded, _propertyDefs.GetAdded()), AddRange(_previousGeneration.EventMapAdded, _eventMap.GetAdded()), AddRange(_previousGeneration.PropertyMapAdded, _propertyMap.GetAdded()), AddRange(_previousGeneration.MethodImplsAdded, _methodImpls.GetAdded()), ImmutableArray.Create(array), metadataSizes.GetAlignedHeapSize(HeapIndex.Blob) + _previousGeneration.BlobStreamLengthAdded, metadataSizes.HeapSizes[1] + _previousGeneration.StringStreamLengthAdded, metadataSizes.GetAlignedHeapSize(HeapIndex.UserString) + _previousGeneration.UserStringStreamLengthAdded, metadataSizes.HeapSizes[3], ((IPEDeltaAssemblyBuilder)module).GetAnonymousTypeMap(), synthesizedMembers, AddRange(_previousGeneration.AddedOrChangedMethods, dictionary, replace: true), baseline.DebugInformationProvider, baseline.LocalSignatureProvider);
	}

	private static IReadOnlyDictionary<K, V> AddRange<K, V>(IReadOnlyDictionary<K, V> previous, IReadOnlyDictionary<K, V> current, bool replace = false)
	{
		if (previous.Count == 0)
		{
			return current;
		}
		if (current.Count == 0)
		{
			return previous;
		}
		Dictionary<K, V> dictionary = new Dictionary<K, V>();
		foreach (KeyValuePair<K, V> previou in previous)
		{
			dictionary.Add(previou.Key, previou.Value);
		}
		foreach (KeyValuePair<K, V> item in current)
		{
			dictionary[item.Key] = item.Value;
		}
		return dictionary;
	}

	public void GetMethodTokens(ICollection<MethodDefinitionHandle> methods)
	{
		foreach (IMethodDefinition row in _methodDefs.GetRows())
		{
			if (!_methodDefs.IsAddedNotChanged(row))
			{
				IMethodBody body = row.GetBody(Context);
				if (body != null && body.SequencePoints.Length > 0)
				{
					methods.Add(MetadataTokens.MethodDefinitionHandle(_methodDefs[row]));
				}
			}
		}
	}

	protected override EventDefinitionHandle GetEventDefinitionHandle(IEventDefinition def)
	{
		return MetadataTokens.EventDefinitionHandle(_eventDefs[def]);
	}

	protected override IReadOnlyList<IEventDefinition> GetEventDefs()
	{
		return _eventDefs.GetRows();
	}

	protected override FieldDefinitionHandle GetFieldDefinitionHandle(IFieldDefinition def)
	{
		return MetadataTokens.FieldDefinitionHandle(_fieldDefs[def]);
	}

	protected override IReadOnlyList<IFieldDefinition> GetFieldDefs()
	{
		return _fieldDefs.GetRows();
	}

	protected override bool TryGetTypeDefinitionHandle(ITypeDefinition def, out TypeDefinitionHandle handle)
	{
		bool result = _typeDefs.TryGetValue(def, out var index);
		handle = MetadataTokens.TypeDefinitionHandle(index);
		return result;
	}

	protected override TypeDefinitionHandle GetTypeDefinitionHandle(ITypeDefinition def)
	{
		return MetadataTokens.TypeDefinitionHandle(_typeDefs[def]);
	}

	protected override ITypeDefinition GetTypeDef(TypeDefinitionHandle handle)
	{
		return _typeDefs[MetadataTokens.GetRowNumber(handle)];
	}

	protected override IReadOnlyList<ITypeDefinition> GetTypeDefs()
	{
		return _typeDefs.GetRows();
	}

	protected override bool TryGetMethodDefinitionHandle(IMethodDefinition def, out MethodDefinitionHandle handle)
	{
		bool result = _methodDefs.TryGetValue(def, out var index);
		handle = MetadataTokens.MethodDefinitionHandle(index);
		return result;
	}

	protected override MethodDefinitionHandle GetMethodDefinitionHandle(IMethodDefinition def)
	{
		return MetadataTokens.MethodDefinitionHandle(_methodDefs[def]);
	}

	protected override IMethodDefinition GetMethodDef(MethodDefinitionHandle index)
	{
		return _methodDefs[MetadataTokens.GetRowNumber(index)];
	}

	protected override IReadOnlyList<IMethodDefinition> GetMethodDefs()
	{
		return _methodDefs.GetRows();
	}

	protected override PropertyDefinitionHandle GetPropertyDefIndex(IPropertyDefinition def)
	{
		return MetadataTokens.PropertyDefinitionHandle(_propertyDefs[def]);
	}

	protected override IReadOnlyList<IPropertyDefinition> GetPropertyDefs()
	{
		return _propertyDefs.GetRows();
	}

	protected override ParameterHandle GetParameterHandle(IParameterDefinition def)
	{
		return MetadataTokens.ParameterHandle(_parameterDefs[def]);
	}

	protected override IReadOnlyList<IParameterDefinition> GetParameterDefs()
	{
		return _parameterDefs.GetRows();
	}

	protected override IReadOnlyList<IGenericParameter> GetGenericParameters()
	{
		return _genericParameters.GetRows();
	}

	protected override FieldDefinitionHandle GetFirstFieldDefinitionHandle(INamedTypeDefinition typeDef)
	{
		return default;
	}

	protected override MethodDefinitionHandle GetFirstMethodDefinitionHandle(INamedTypeDefinition typeDef)
	{
		return default;
	}

	protected override ParameterHandle GetFirstParameterHandle(IMethodDefinition methodDef)
	{
		return default;
	}

	protected override AssemblyReferenceHandle GetOrAddAssemblyReferenceHandle(IAssemblyReference reference)
	{
		AssemblyIdentity assemblyIdentity = reference.Identity;
		Version assemblyVersionPattern = reference.AssemblyVersionPattern;
		if ((object)assemblyVersionPattern != null)
		{
			assemblyIdentity = _previousGeneration.InitialBaseline.LazyMetadataSymbols.AssemblyReferenceIdentityMap[assemblyIdentity.WithVersion(assemblyVersionPattern)];
		}
		return MetadataTokens.AssemblyReferenceHandle(_assemblyRefIndex.GetOrAdd(assemblyIdentity));
	}

	protected override IReadOnlyList<AssemblyIdentity> GetAssemblyRefs()
	{
		return _assemblyRefIndex.Rows;
	}

	protected override ModuleReferenceHandle GetOrAddModuleReferenceHandle(string reference)
	{
		return MetadataTokens.ModuleReferenceHandle(_moduleRefIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<string> GetModuleRefs()
	{
		return _moduleRefIndex.Rows;
	}

	protected override MemberReferenceHandle GetOrAddMemberReferenceHandle(ITypeMemberReference reference)
	{
		return MetadataTokens.MemberReferenceHandle(_memberRefIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<ITypeMemberReference> GetMemberRefs()
	{
		return _memberRefIndex.Rows;
	}

	protected override MethodSpecificationHandle GetOrAddMethodSpecificationHandle(IGenericMethodInstanceReference reference)
	{
		return MetadataTokens.MethodSpecificationHandle(_methodSpecIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<IGenericMethodInstanceReference> GetMethodSpecs()
	{
		return _methodSpecIndex.Rows;
	}

	protected override bool TryGetTypeReferenceHandle(ITypeReference reference, out TypeReferenceHandle handle)
	{
		bool result = _typeRefIndex.TryGetValue(reference, out var index);
		handle = MetadataTokens.TypeReferenceHandle(index);
		return result;
	}

	protected override TypeReferenceHandle GetOrAddTypeReferenceHandle(ITypeReference reference)
	{
		return MetadataTokens.TypeReferenceHandle(_typeRefIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<ITypeReference> GetTypeRefs()
	{
		return _typeRefIndex.Rows;
	}

	protected override TypeSpecificationHandle GetOrAddTypeSpecificationHandle(ITypeReference reference)
	{
		return MetadataTokens.TypeSpecificationHandle(_typeSpecIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<ITypeReference> GetTypeSpecs()
	{
		return _typeSpecIndex.Rows;
	}

	protected override StandaloneSignatureHandle GetOrAddStandaloneSignatureHandle(BlobHandle blobIndex)
	{
		return MetadataTokens.StandaloneSignatureHandle(_standAloneSignatureIndex.GetOrAdd(blobIndex));
	}

	protected override IReadOnlyList<BlobHandle> GetStandaloneSignatureBlobHandles()
	{
		return _standAloneSignatureIndex.Rows;
	}

	protected override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(CommonPEModuleBuilder module)
	{
		return _changes.GetTopLevelTypes(Context);
	}

	protected override void OnIndicesCreated()
	{
		((IPEDeltaAssemblyBuilder)module).OnCreatedIndices(Context.Diagnostics);
	}

	protected override void CreateIndicesForNonTypeMembers(ITypeDefinition typeDef)
	{
		SymbolChange change = _changes.GetChange(typeDef);
		switch (change)
		{
		case SymbolChange.Added:
		{
			_typeDefs.Add(typeDef);
			IEnumerable<IGenericTypeParameter> consolidatedTypeParameters = GetConsolidatedTypeParameters(typeDef);
			if (consolidatedTypeParameters == null)
			{
				break;
			}
			foreach (IGenericTypeParameter item4 in consolidatedTypeParameters)
			{
				_genericParameters.Add(item4);
			}
			break;
		}
		case SymbolChange.Updated:
			_typeDefs.AddUpdated(typeDef);
			break;
		case SymbolChange.None:
			return;
		default:
			throw ExceptionUtilities.UnexpectedValue(change);
		case SymbolChange.ContainsChanges:
			break;
		}
		_typeDefs.TryGetValue(typeDef, out var index);
		foreach (IEventDefinition @event in typeDef.GetEvents(Context))
		{
			if (!_eventMap.TryGetValue(index, out var _))
			{
				_eventMap.Add(index);
			}
			AddDefIfNecessary(_eventDefs, @event);
		}
		foreach (IFieldDefinition field in typeDef.GetFields(Context))
		{
			AddDefIfNecessary(_fieldDefs, field);
		}
		foreach (IMethodDefinition method in typeDef.GetMethods(Context))
		{
			if (!AddDefIfNecessary(_methodDefs, method))
			{
				continue;
			}
			foreach (IParameterDefinition item5 in GetParametersToEmit(method))
			{
				_parameterDefs.Add(item5);
				_parameterDefList.Add(KeyValuePairUtil.Create(method, item5));
			}
			if (method.GenericParameterCount <= 0)
			{
				continue;
			}
			foreach (IGenericMethodParameter genericParameter in method.GenericParameters)
			{
				_genericParameters.Add(genericParameter);
			}
		}
		foreach (IPropertyDefinition property in typeDef.GetProperties(Context))
		{
			if (!_propertyMap.TryGetValue(index, out var _))
			{
				_propertyMap.Add(index);
			}
			AddDefIfNecessary(_propertyDefs, property);
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		foreach (Microsoft.Cci.MethodImplementation explicitImplementationOverride in typeDef.GetExplicitImplementationOverrides(Context))
		{
			IMethodDefinition item = (IMethodDefinition)explicitImplementationOverride.ImplementingMethod.AsDefinition(Context);
			_methodDefs.TryGetValue(item, out var index4);
			MethodImplKey item2 = new MethodImplKey(index4, 1);
			if (!_methodImpls.TryGetValue(item2, out var _))
			{
				instance.Add(index4);
				methodImplList.Add(explicitImplementationOverride);
			}
		}
		foreach (int item6 in instance)
		{
			int num = 1;
			MethodImplKey item3;
			while (true)
			{
				item3 = new MethodImplKey(item6, num);
				if (!_methodImpls.TryGetValue(item3, out var _))
				{
					break;
				}
				num++;
			}
			_methodImpls.Add(item3);
		}
		instance.Free();
	}

	private bool AddDefIfNecessary<T>(DefinitionIndex<T> defIndex, T def) where T : IDefinition
	{
		switch (_changes.GetChange(def))
		{
		case SymbolChange.Added:
			defIndex.Add(def);
			return true;
		case SymbolChange.Updated:
			defIndex.AddUpdated(def);
			return false;
		case SymbolChange.ContainsChanges:
			return false;
		default:
			return false;
		}
	}

	protected override ReferenceIndexer CreateReferenceVisitor()
	{
		return new DeltaReferenceIndexer(this);
	}

	protected override void ReportReferencesToAddedSymbols()
	{
		foreach (ITypeReference typeRef in GetTypeRefs())
		{
			ReportReferencesToAddedSymbol(typeRef as ISymbol);
		}
		foreach (ITypeMemberReference memberRef in GetMemberRefs())
		{
			ReportReferencesToAddedSymbol(memberRef as ISymbol);
		}
	}

	private void ReportReferencesToAddedSymbol(ISymbol symbolOpt)
	{
		if (symbolOpt != null && _changes.IsAdded(symbolOpt))
		{
			Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_EncReferenceToAddedMember, MetadataWriter.GetSymbolLocation(symbolOpt), symbolOpt.Name, symbolOpt.ContainingAssembly.Name));
		}
	}

	protected override StandaloneSignatureHandle SerializeLocalVariablesSignature(IMethodBody body)
	{
		ImmutableArray<ILocalDefinition> localVariables = body.LocalVariables;
		ArrayBuilder<EncLocalInfo> instance = ArrayBuilder<EncLocalInfo>.GetInstance();
		StandaloneSignatureHandle result;
		if (localVariables.Length > 0)
		{
			PooledBlobBuilder instance2 = PooledBlobBuilder.GetInstance();
			LocalVariablesEncoder localVariablesEncoder = new BlobEncoder(instance2).LocalVariableSignature(localVariables.Length);
			foreach (ILocalDefinition item in localVariables)
			{
				byte[] array = item.Signature;
				if (array == null)
				{
					int count = instance2.Count;
					SerializeLocalVariableType(localVariablesEncoder.AddVariable(), item);
					array = instance2.ToArray(count, instance2.Count - count);
				}
				else
				{
					instance2.WriteBytes(array);
				}
				instance.Add(CreateEncLocalInfo(item, array));
			}
			BlobHandle orAddBlob = metadata.GetOrAddBlob(instance2);
			result = GetOrAddStandaloneSignatureHandle(orAddBlob);
			instance2.Free();
		}
		else
		{
			result = default;
		}
		AddedOrChangedMethodInfo value = new AddedOrChangedMethodInfo(body.MethodId, instance.ToImmutable(), body.LambdaDebugInfo, body.ClosureDebugInfo, body.StateMachineTypeName, body.StateMachineHoistedLocalSlots, body.StateMachineAwaiterSlots);
		_addedOrChangedMethods.Add(body.MethodDefinition, value);
		instance.Free();
		return result;
	}

	private EncLocalInfo CreateEncLocalInfo(ILocalDefinition localDef, byte[] signature)
	{
		if (localDef.SlotInfo.Id.IsNone)
		{
			return new EncLocalInfo(signature);
		}
		ITypeReference typeReference = localDef.Type;
		if (typeReference is ITypeSymbol type)
		{
			typeReference = Context.Module.EncTranslateType(type, Context.Diagnostics);
		}
		return new EncLocalInfo(localDef.SlotInfo, typeReference, localDef.Constraints, signature);
	}

	protected override void PopulateEncLogTableRows(ImmutableArray<int> rowCounts)
	{
		ImmutableArray<int> tableSizes = _previousGeneration.TableSizes;
		ImmutableArray<int> deltaTableSizes = GetDeltaTableSizes(rowCounts);
		PopulateEncLogTableRows(TableIndex.AssemblyRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.ModuleRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MemberRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MethodSpec, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.TypeRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.TypeSpec, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.StandAloneSig, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(_typeDefs, TableIndex.TypeDef);
		PopulateEncLogTableRows(TableIndex.EventMap, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.PropertyMap, tableSizes, deltaTableSizes);
		PopulateEncLogTableEventsOrProperties(_eventDefs, TableIndex.Event, EditAndContinueOperation.AddEvent, _eventMap, TableIndex.EventMap);
		PopulateEncLogTableFieldsOrMethods(_fieldDefs, TableIndex.Field, EditAndContinueOperation.AddField);
		PopulateEncLogTableFieldsOrMethods(_methodDefs, TableIndex.MethodDef, EditAndContinueOperation.AddMethod);
		PopulateEncLogTableEventsOrProperties(_propertyDefs, TableIndex.Property, EditAndContinueOperation.AddProperty, _propertyMap, TableIndex.PropertyMap);
		PopulateEncLogTableParameters();
		PopulateEncLogTableRows(TableIndex.Constant, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.CustomAttribute, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.DeclSecurity, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.ClassLayout, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.FieldLayout, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MethodSemantics, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MethodImpl, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.ImplMap, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.FieldRva, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.NestedClass, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.GenericParam, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.InterfaceImpl, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.GenericParamConstraint, tableSizes, deltaTableSizes);
	}

	private void PopulateEncLogTableEventsOrProperties<T>(DefinitionIndex<T> index, TableIndex table, EditAndContinueOperation addCode, EventOrPropertyMapIndex map, TableIndex mapTable) where T : ITypeDefinitionMember
	{
		foreach (T row in index.GetRows())
		{
			if (index.IsAddedNotChanged(row))
			{
				int item = _typeDefs[row.ContainingTypeDefinition];
				map.TryGetValue(item, out var index2);
				metadata.AddEncLogEntry(MetadataTokens.Handle(mapTable, index2), addCode);
			}
			metadata.AddEncLogEntry(MetadataTokens.Handle(table, index[row]), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncLogTableFieldsOrMethods<T>(DefinitionIndex<T> index, TableIndex tableIndex, EditAndContinueOperation addCode) where T : ITypeDefinitionMember
	{
		foreach (T row in index.GetRows())
		{
			if (index.IsAddedNotChanged(row))
			{
				metadata.AddEncLogEntry(MetadataTokens.TypeDefinitionHandle(_typeDefs[(INamedTypeDefinition)row.ContainingTypeDefinition]), addCode);
			}
			metadata.AddEncLogEntry(MetadataTokens.Handle(tableIndex, index[row]), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncLogTableParameters()
	{
		int firstRowId = _parameterDefs.FirstRowId;
		for (int i = 0; i < _parameterDefList.Count; i++)
		{
			IMethodDefinition key = _parameterDefList[i].Key;
			metadata.AddEncLogEntry(MetadataTokens.MethodDefinitionHandle(_methodDefs[key]), EditAndContinueOperation.AddParameter);
			metadata.AddEncLogEntry(MetadataTokens.ParameterHandle(firstRowId + i), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncLogTableRows<T>(DefinitionIndex<T> index, TableIndex tableIndex) where T : IDefinition
	{
		foreach (T row in index.GetRows())
		{
			metadata.AddEncLogEntry(MetadataTokens.Handle(tableIndex, index[row]), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncLogTableRows(TableIndex tableIndex, ImmutableArray<int> previousSizes, ImmutableArray<int> deltaSizes)
	{
		PopulateEncLogTableRows(tableIndex, previousSizes[(int)tableIndex] + 1, deltaSizes[(int)tableIndex]);
	}

	private void PopulateEncLogTableRows(TableIndex tableIndex, int firstRowId, int tokenCount)
	{
		for (int i = 0; i < tokenCount; i++)
		{
			metadata.AddEncLogEntry(MetadataTokens.Handle(tableIndex, firstRowId + i), EditAndContinueOperation.Default);
		}
	}

	protected override void PopulateEncMapTableRows(ImmutableArray<int> rowCounts)
	{
		ArrayBuilder<EntityHandle> instance = ArrayBuilder<EntityHandle>.GetInstance();
		ImmutableArray<int> tableSizes = _previousGeneration.TableSizes;
		ImmutableArray<int> deltaTableSizes = GetDeltaTableSizes(rowCounts);
		AddReferencedTokens(instance, TableIndex.AssemblyRef, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.ModuleRef, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.MemberRef, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.MethodSpec, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.TypeRef, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.TypeSpec, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.StandAloneSig, tableSizes, deltaTableSizes);
		AddDefinitionTokens(instance, _typeDefs, TableIndex.TypeDef);
		AddDefinitionTokens(instance, _eventDefs, TableIndex.Event);
		AddDefinitionTokens(instance, _fieldDefs, TableIndex.Field);
		AddDefinitionTokens(instance, _methodDefs, TableIndex.MethodDef);
		AddDefinitionTokens(instance, _propertyDefs, TableIndex.Property);
		AddReferencedTokens(instance, TableIndex.Param, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.Constant, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.CustomAttribute, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.DeclSecurity, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.ClassLayout, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.FieldLayout, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.EventMap, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.PropertyMap, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.MethodSemantics, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.MethodImpl, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.ImplMap, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.FieldRva, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.NestedClass, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.GenericParam, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.InterfaceImpl, tableSizes, deltaTableSizes);
		AddReferencedTokens(instance, TableIndex.GenericParamConstraint, tableSizes, deltaTableSizes);
		instance.Sort(HandleComparer.Default);
		foreach (EntityHandle item in instance)
		{
			metadata.AddEncMapEntry(item);
		}
		instance.Free();
		if (_debugMetadataOpt == null)
		{
			return;
		}
		ArrayBuilder<EntityHandle> instance2 = ArrayBuilder<EntityHandle>.GetInstance();
		AddDefinitionTokens(instance2, _methodDefs, TableIndex.MethodDebugInformation);
		instance2.Sort(HandleComparer.Default);
		foreach (EntityHandle item2 in instance2)
		{
			_debugMetadataOpt.AddEncMapEntry(item2);
		}
		instance2.Free();
	}

	private static void AddReferencedTokens(ArrayBuilder<EntityHandle> builder, TableIndex tableIndex, ImmutableArray<int> previousSizes, ImmutableArray<int> deltaSizes)
	{
		AddReferencedTokens(builder, tableIndex, previousSizes[(int)tableIndex] + 1, deltaSizes[(int)tableIndex]);
	}

	private static void AddReferencedTokens(ArrayBuilder<EntityHandle> builder, TableIndex tableIndex, int firstRowId, int nTokens)
	{
		for (int i = 0; i < nTokens; i++)
		{
			builder.Add(MetadataTokens.Handle(tableIndex, firstRowId + i));
		}
	}

	private static void AddDefinitionTokens<T>(ArrayBuilder<EntityHandle> tokens, DefinitionIndex<T> index, TableIndex tableIndex) where T : IDefinition
	{
		foreach (T row in index.GetRows())
		{
			tokens.Add(MetadataTokens.Handle(tableIndex, index[row]));
		}
	}

	protected override void PopulateEventMapTableRows()
	{
		foreach (int row in _eventMap.GetRows())
		{
			metadata.AddEventMap(MetadataTokens.TypeDefinitionHandle(row), MetadataTokens.EventDefinitionHandle(_eventMap[row]));
		}
	}

	protected override void PopulatePropertyMapTableRows()
	{
		foreach (int row in _propertyMap.GetRows())
		{
			metadata.AddPropertyMap(MetadataTokens.TypeDefinitionHandle(row), MetadataTokens.PropertyDefinitionHandle(_propertyMap[row]));
		}
	}

	private bool TryGetExistingTypeDefIndex(ITypeDefinition item, out int index)
	{
		if (_previousGeneration.TypesAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_definitionMap.TryGetTypeHandle(item, out var handle))
		{
			index = MetadataTokens.GetRowNumber(handle);
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingEventDefIndex(IEventDefinition item, out int index)
	{
		if (_previousGeneration.EventsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_definitionMap.TryGetEventHandle(item, out var handle))
		{
			index = MetadataTokens.GetRowNumber(handle);
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingFieldDefIndex(IFieldDefinition item, out int index)
	{
		if (_previousGeneration.FieldsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_definitionMap.TryGetFieldHandle(item, out var handle))
		{
			index = MetadataTokens.GetRowNumber(handle);
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingMethodDefIndex(IMethodDefinition item, out int index)
	{
		if (_previousGeneration.MethodsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_definitionMap.TryGetMethodHandle(item, out var handle))
		{
			index = MetadataTokens.GetRowNumber(handle);
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingPropertyDefIndex(IPropertyDefinition item, out int index)
	{
		if (_previousGeneration.PropertiesAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_definitionMap.TryGetPropertyHandle(item, out var handle))
		{
			index = MetadataTokens.GetRowNumber(handle);
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingEventMapIndex(int item, out int index)
	{
		if (_previousGeneration.EventMapAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_previousGeneration.TypeToEventMap.TryGetValue(item, out index))
		{
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingPropertyMapIndex(int item, out int index)
	{
		if (_previousGeneration.PropertyMapAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_previousGeneration.TypeToPropertyMap.TryGetValue(item, out index))
		{
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingMethodImplIndex(MethodImplKey item, out int index)
	{
		if (_previousGeneration.MethodImplsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_previousGeneration.MethodImpls.TryGetValue(item, out index))
		{
			return true;
		}
		index = 0;
		return false;
	}
}
