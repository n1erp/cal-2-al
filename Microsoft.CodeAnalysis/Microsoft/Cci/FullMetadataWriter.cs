using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal sealed class FullMetadataWriter : MetadataWriter
{
	private sealed class FullReferenceIndexer : ReferenceIndexer
	{
		internal FullReferenceIndexer(MetadataWriter metadataWriter)
			: base(metadataWriter)
		{
		}
	}

	private struct DefinitionIndex<T>(int capacity) where T : IReference
	{
		private readonly Dictionary<T, int> _index = new Dictionary<T, int>(capacity);

		private readonly List<T> _rows = new List<T>(capacity);

		public int this[T item] => _index[item];

		public T this[int rowId] => _rows[rowId - 1];

		public IReadOnlyList<T> Rows => _rows;

		public int NextRowId => _rows.Count + 1;

		public bool TryGetValue(T item, out int rowId)
		{
			return _index.TryGetValue(item, out rowId);
		}

		public void Add(T item)
		{
			_index.Add(item, NextRowId);
			_rows.Add(item);
		}
	}

	private readonly DefinitionIndex<ITypeDefinition> _typeDefs;

	private readonly DefinitionIndex<IEventDefinition> _eventDefs;

	private readonly DefinitionIndex<IFieldDefinition> _fieldDefs;

	private readonly DefinitionIndex<IMethodDefinition> _methodDefs;

	private readonly DefinitionIndex<IPropertyDefinition> _propertyDefs;

	private readonly DefinitionIndex<IParameterDefinition> _parameterDefs;

	private readonly DefinitionIndex<IGenericParameter> _genericParameters;

	private readonly Dictionary<ITypeDefinition, int> _fieldDefIndex;

	private readonly Dictionary<ITypeDefinition, int> _methodDefIndex;

	private readonly Dictionary<IMethodDefinition, int> _parameterListIndex;

	private readonly HeapOrReferenceIndex<AssemblyIdentity> _assemblyRefIndex;

	private readonly HeapOrReferenceIndex<string> _moduleRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<ITypeMemberReference> _memberRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference> _methodSpecIndex;

	private readonly HeapOrReferenceIndex<ITypeReference> _typeRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<ITypeReference> _typeSpecIndex;

	private readonly HeapOrReferenceIndex<BlobHandle> _standAloneSignatureIndex;

	protected override ushort Generation => 0;

	protected override Guid EncId => Guid.Empty;

	protected override Guid EncBaseId => Guid.Empty;

	protected override int GreatestMethodDefIndex => _methodDefs.NextRowId;

	public static MetadataWriter Create(EmitContext context, CommonMessageProvider messageProvider, bool metadataOnly, bool deterministic, bool emitTestCoverageData, bool hasPdbStream, CancellationToken cancellationToken)
	{
		MetadataBuilder builder = new MetadataBuilder();
		MetadataBuilder debugBuilderOpt = context.Module.DebugInformationFormat switch
		{
			DebugInformationFormat.PortablePdb => hasPdbStream ? new MetadataBuilder() : null, 
			DebugInformationFormat.Embedded => metadataOnly ? null : new MetadataBuilder(), 
			_ => null, 
		};
		DynamicAnalysisDataWriter dynamicAnalysisDataWriterOpt = (emitTestCoverageData ? new DynamicAnalysisDataWriter(context.Module.DebugDocumentCount, context.Module.HintNumberOfMethodDefinitions) : null);
		return new FullMetadataWriter(context, builder, debugBuilderOpt, dynamicAnalysisDataWriterOpt, messageProvider, metadataOnly, deterministic, emitTestCoverageData, cancellationToken);
	}

	private FullMetadataWriter(EmitContext context, MetadataBuilder builder, MetadataBuilder debugBuilderOpt, DynamicAnalysisDataWriter dynamicAnalysisDataWriterOpt, CommonMessageProvider messageProvider, bool metadataOnly, bool deterministic, bool emitTestCoverageData, CancellationToken cancellationToken)
		: base(builder, debugBuilderOpt, dynamicAnalysisDataWriterOpt, context, messageProvider, metadataOnly, deterministic, emitTestCoverageData, cancellationToken)
	{
		int hintNumberOfMethodDefinitions = module.HintNumberOfMethodDefinitions;
		int num = hintNumberOfMethodDefinitions / 6;
		int capacity = num * 4;
		int capacity2 = hintNumberOfMethodDefinitions / 4;
		_typeDefs = new DefinitionIndex<ITypeDefinition>(num);
		_eventDefs = new DefinitionIndex<IEventDefinition>(0);
		_fieldDefs = new DefinitionIndex<IFieldDefinition>(capacity);
		_methodDefs = new DefinitionIndex<IMethodDefinition>(hintNumberOfMethodDefinitions);
		_propertyDefs = new DefinitionIndex<IPropertyDefinition>(capacity2);
		_parameterDefs = new DefinitionIndex<IParameterDefinition>(hintNumberOfMethodDefinitions);
		_genericParameters = new DefinitionIndex<IGenericParameter>(0);
		_fieldDefIndex = new Dictionary<ITypeDefinition, int>(num);
		_methodDefIndex = new Dictionary<ITypeDefinition, int>(num);
		_parameterListIndex = new Dictionary<IMethodDefinition, int>(hintNumberOfMethodDefinitions);
		_assemblyRefIndex = new HeapOrReferenceIndex<AssemblyIdentity>(this);
		_moduleRefIndex = new HeapOrReferenceIndex<string>(this);
		_memberRefIndex = new InstanceAndStructuralReferenceIndex<ITypeMemberReference>(this, new MemberRefComparer(this));
		_methodSpecIndex = new InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference>(this, new MethodSpecComparer(this));
		_typeRefIndex = new HeapOrReferenceIndex<ITypeReference>(this);
		_typeSpecIndex = new InstanceAndStructuralReferenceIndex<ITypeReference>(this, new TypeSpecComparer(this));
		_standAloneSignatureIndex = new HeapOrReferenceIndex<BlobHandle>(this);
	}

	protected override bool TryGetTypeDefinitionHandle(ITypeDefinition def, out TypeDefinitionHandle handle)
	{
		bool result = _typeDefs.TryGetValue(def, out var rowId);
		handle = MetadataTokens.TypeDefinitionHandle(rowId);
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
		return _typeDefs.Rows;
	}

	protected override EventDefinitionHandle GetEventDefinitionHandle(IEventDefinition def)
	{
		return MetadataTokens.EventDefinitionHandle(_eventDefs[def]);
	}

	protected override IReadOnlyList<IEventDefinition> GetEventDefs()
	{
		return _eventDefs.Rows;
	}

	protected override FieldDefinitionHandle GetFieldDefinitionHandle(IFieldDefinition def)
	{
		return MetadataTokens.FieldDefinitionHandle(_fieldDefs[def]);
	}

	protected override IReadOnlyList<IFieldDefinition> GetFieldDefs()
	{
		return _fieldDefs.Rows;
	}

	protected override bool TryGetMethodDefinitionHandle(IMethodDefinition def, out MethodDefinitionHandle handle)
	{
		bool result = _methodDefs.TryGetValue(def, out var rowId);
		handle = MetadataTokens.MethodDefinitionHandle(rowId);
		return result;
	}

	protected override MethodDefinitionHandle GetMethodDefinitionHandle(IMethodDefinition def)
	{
		return MetadataTokens.MethodDefinitionHandle(_methodDefs[def]);
	}

	protected override IMethodDefinition GetMethodDef(MethodDefinitionHandle handle)
	{
		return _methodDefs[MetadataTokens.GetRowNumber(handle)];
	}

	protected override IReadOnlyList<IMethodDefinition> GetMethodDefs()
	{
		return _methodDefs.Rows;
	}

	protected override PropertyDefinitionHandle GetPropertyDefIndex(IPropertyDefinition def)
	{
		return MetadataTokens.PropertyDefinitionHandle(_propertyDefs[def]);
	}

	protected override IReadOnlyList<IPropertyDefinition> GetPropertyDefs()
	{
		return _propertyDefs.Rows;
	}

	protected override ParameterHandle GetParameterHandle(IParameterDefinition def)
	{
		return MetadataTokens.ParameterHandle(_parameterDefs[def]);
	}

	protected override IReadOnlyList<IParameterDefinition> GetParameterDefs()
	{
		return _parameterDefs.Rows;
	}

	protected override IReadOnlyList<IGenericParameter> GetGenericParameters()
	{
		return _genericParameters.Rows;
	}

	protected override FieldDefinitionHandle GetFirstFieldDefinitionHandle(INamedTypeDefinition typeDef)
	{
		return MetadataTokens.FieldDefinitionHandle(_fieldDefIndex[typeDef]);
	}

	protected override MethodDefinitionHandle GetFirstMethodDefinitionHandle(INamedTypeDefinition typeDef)
	{
		return MetadataTokens.MethodDefinitionHandle(_methodDefIndex[typeDef]);
	}

	protected override ParameterHandle GetFirstParameterHandle(IMethodDefinition methodDef)
	{
		return MetadataTokens.ParameterHandle(_parameterListIndex[methodDef]);
	}

	protected override AssemblyReferenceHandle GetOrAddAssemblyReferenceHandle(IAssemblyReference reference)
	{
		return MetadataTokens.AssemblyReferenceHandle(_assemblyRefIndex.GetOrAdd(reference.Identity));
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

	protected override ReferenceIndexer CreateReferenceVisitor()
	{
		return new FullReferenceIndexer(this);
	}

	protected override void ReportReferencesToAddedSymbols()
	{
	}

	protected override void PopulateEncLogTableRows(ImmutableArray<int> rowCounts)
	{
	}

	protected override void PopulateEncMapTableRows(ImmutableArray<int> rowCounts)
	{
	}

	protected override void PopulateEventMapTableRows()
	{
		ITypeDefinition typeDefinition = null;
		foreach (IEventDefinition eventDef in GetEventDefs())
		{
			if (eventDef.ContainingTypeDefinition != typeDefinition)
			{
				typeDefinition = eventDef.ContainingTypeDefinition;
				metadata.AddEventMap(GetTypeDefinitionHandle(typeDefinition), GetEventDefinitionHandle(eventDef));
			}
		}
	}

	protected override void PopulatePropertyMapTableRows()
	{
		ITypeDefinition typeDefinition = null;
		foreach (IPropertyDefinition propertyDef in GetPropertyDefs())
		{
			if (propertyDef.ContainingTypeDefinition != typeDefinition)
			{
				typeDefinition = propertyDef.ContainingTypeDefinition;
				metadata.AddPropertyMap(GetTypeDefinitionHandle(typeDefinition), GetPropertyDefIndex(propertyDef));
			}
		}
	}

	protected override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(CommonPEModuleBuilder module)
	{
		return module.GetTopLevelTypes(Context);
	}

	protected override void CreateIndicesForNonTypeMembers(ITypeDefinition typeDef)
	{
		_typeDefs.Add(typeDef);
		IEnumerable<IGenericTypeParameter> consolidatedTypeParameters = GetConsolidatedTypeParameters(typeDef);
		if (consolidatedTypeParameters != null)
		{
			foreach (IGenericTypeParameter item in consolidatedTypeParameters)
			{
				_genericParameters.Add(item);
			}
		}
		foreach (MethodImplementation explicitImplementationOverride in typeDef.GetExplicitImplementationOverrides(Context))
		{
			methodImplList.Add(explicitImplementationOverride);
		}
		foreach (IEventDefinition @event in typeDef.GetEvents(Context))
		{
			_eventDefs.Add(@event);
		}
		_fieldDefIndex.Add(typeDef, _fieldDefs.NextRowId);
		foreach (IFieldDefinition field in typeDef.GetFields(Context))
		{
			_fieldDefs.Add(field);
		}
		_methodDefIndex.Add(typeDef, _methodDefs.NextRowId);
		foreach (IMethodDefinition method in typeDef.GetMethods(Context))
		{
			CreateIndicesFor(method);
			_methodDefs.Add(method);
		}
		foreach (IPropertyDefinition property in typeDef.GetProperties(Context))
		{
			_propertyDefs.Add(property);
		}
	}

	private void CreateIndicesFor(IMethodDefinition methodDef)
	{
		_parameterListIndex.Add(methodDef, _parameterDefs.NextRowId);
		foreach (IParameterDefinition item in GetParametersToEmit(methodDef))
		{
			_parameterDefs.Add(item);
		}
		if (methodDef.GenericParameterCount <= 0)
		{
			return;
		}
		foreach (IGenericMethodParameter genericParameter in methodDef.GenericParameters)
		{
			_genericParameters.Add(genericParameter);
		}
	}
}
