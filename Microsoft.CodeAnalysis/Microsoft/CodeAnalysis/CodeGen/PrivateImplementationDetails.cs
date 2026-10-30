using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class PrivateImplementationDetails : DefaultTypeDef, INamespaceTypeDefinition, INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, INamespaceTypeReference
{
	private sealed class FieldComparer : IComparer<SynthesizedStaticField>
	{
		public static readonly FieldComparer Instance = new FieldComparer();

		private FieldComparer()
		{
		}

		public int Compare(SynthesizedStaticField x, SynthesizedStaticField y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	internal const string SynthesizedStringHashFunctionName = "ComputeStringHash";

	private readonly CommonPEModuleBuilder _moduleBuilder;

	private readonly ITypeReference _systemObject;

	private readonly ITypeReference _systemValueType;

	private readonly ITypeReference _systemInt8Type;

	private readonly ITypeReference _systemInt16Type;

	private readonly ITypeReference _systemInt32Type;

	private readonly ITypeReference _systemInt64Type;

	private readonly ICustomAttribute _compilerGeneratedAttribute;

	private readonly string _name;

	private int _frozen;

	private ImmutableArray<SynthesizedStaticField> _orderedSynthesizedFields;

	private readonly ConcurrentDictionary<ImmutableArray<byte>, MappedField> _mappedFields = new ConcurrentDictionary<ImmutableArray<byte>, MappedField>(ByteSequenceComparer.Instance);

	private ModuleVersionIdField _mvidField;

	private readonly ConcurrentDictionary<int, InstrumentationPayloadRootField> _instrumentationPayloadRootFields = new ConcurrentDictionary<int, InstrumentationPayloadRootField>();

	private ImmutableArray<IMethodDefinition> _orderedSynthesizedMethods;

	private readonly ConcurrentDictionary<string, IMethodDefinition> _synthesizedMethods = new ConcurrentDictionary<string, IMethodDefinition>();

	private ImmutableArray<ITypeReference> _orderedProxyTypes;

	private readonly ConcurrentDictionary<uint, ITypeReference> _proxyTypes = new ConcurrentDictionary<uint, ITypeReference>();

	private bool IsFrozen => _frozen != 0;

	public override INamespaceTypeReference AsNamespaceTypeReference => this;

	public string Name => _name;

	public bool IsPublic => false;

	public string NamespaceName => string.Empty;

	internal PrivateImplementationDetails(CommonPEModuleBuilder moduleBuilder, string moduleName, int submissionSlotIndex, ITypeReference systemObject, ITypeReference systemValueType, ITypeReference systemInt8Type, ITypeReference systemInt16Type, ITypeReference systemInt32Type, ITypeReference systemInt64Type, ICustomAttribute compilerGeneratedAttribute)
	{
		_moduleBuilder = moduleBuilder;
		_systemObject = systemObject;
		_systemValueType = systemValueType;
		_systemInt8Type = systemInt8Type;
		_systemInt16Type = systemInt16Type;
		_systemInt32Type = systemInt32Type;
		_systemInt64Type = systemInt64Type;
		_compilerGeneratedAttribute = compilerGeneratedAttribute;
		bool isNetModule = moduleBuilder.OutputKind == OutputKind.NetModule;
		_name = GetClassName(moduleName, submissionSlotIndex, isNetModule);
	}

	private static string GetClassName(string moduleName, int submissionSlotIndex, bool isNetModule)
	{
		string text = (isNetModule ? ("<PrivateImplementationDetails><" + MetadataHelpers.MangleForTypeNameIfNeeded(moduleName) + ">") : "<PrivateImplementationDetails>");
		if (submissionSlotIndex >= 0)
		{
			text += submissionSlotIndex;
		}
		return text;
	}

	internal void Freeze()
	{
		if (Interlocked.Exchange(ref _frozen, 1) != 0)
		{
			throw new InvalidOperationException();
		}
		ArrayBuilder<SynthesizedStaticField> instance = ArrayBuilder<SynthesizedStaticField>.GetInstance(_mappedFields.Count + ((_mvidField != null) ? 1 : 0));
		instance.AddRange(_mappedFields.Values);
		if (_mvidField != null)
		{
			instance.Add(_mvidField);
		}
		instance.AddRange(_instrumentationPayloadRootFields.Values);
		instance.Sort(FieldComparer.Instance);
		_orderedSynthesizedFields = instance.ToImmutableAndFree();
		_orderedSynthesizedMethods = (from kvp in _synthesizedMethods
			orderby kvp.Key
			select kvp.Value).AsImmutable();
		_orderedProxyTypes = (from kvp in _proxyTypes
			orderby kvp.Key
			select kvp.Value).AsImmutable();
	}

	internal IFieldReference CreateDataField(ImmutableArray<byte> data)
	{
		ITypeReference type = _proxyTypes.GetOrAdd((uint)data.Length, GetStorageStruct);
		return _mappedFields.GetOrAdd(data, (ImmutableArray<byte> immutableArray) => new MappedField(GenerateDataFieldName(immutableArray), this, type, immutableArray));
	}

	private ITypeReference GetStorageStruct(uint size)
	{
		return size switch
		{
			1u => _systemInt8Type ?? new ExplicitSizeStruct(1u, this, _systemValueType), 
			2u => _systemInt16Type ?? new ExplicitSizeStruct(2u, this, _systemValueType), 
			4u => _systemInt32Type ?? new ExplicitSizeStruct(4u, this, _systemValueType), 
			8u => _systemInt64Type ?? new ExplicitSizeStruct(8u, this, _systemValueType), 
			_ => new ExplicitSizeStruct(size, this, _systemValueType), 
		};
	}

	internal IFieldReference GetModuleVersionId(ITypeReference mvidType)
	{
		if (_mvidField == null)
		{
			Interlocked.CompareExchange(ref _mvidField, new ModuleVersionIdField(this, mvidType), null);
		}
		return _mvidField;
	}

	internal IFieldReference GetOrAddInstrumentationPayloadRoot(int analysisKind, ITypeReference payloadRootType)
	{
		if (!_instrumentationPayloadRootFields.TryGetValue(analysisKind, out var value))
		{
			return _instrumentationPayloadRootFields.GetOrAdd(analysisKind, (int kind) => new InstrumentationPayloadRootField(this, kind, payloadRootType));
		}
		return value;
	}

	internal IOrderedEnumerable<KeyValuePair<int, InstrumentationPayloadRootField>> GetInstrumentationPayloadRoots()
	{
		return _instrumentationPayloadRootFields.OrderBy((KeyValuePair<int, InstrumentationPayloadRootField> analysis) => analysis.Key);
	}

	internal bool TryAddSynthesizedMethod(IMethodDefinition method)
	{
		return _synthesizedMethods.TryAdd(method.Name, method);
	}

	public override IEnumerable<IFieldDefinition> GetFields(EmitContext context)
	{
		return _orderedSynthesizedFields;
	}

	public override IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
	{
		return _orderedSynthesizedMethods;
	}

	internal IMethodDefinition GetMethod(string name)
	{
		_synthesizedMethods.TryGetValue(name, out var value);
		return value;
	}

	public override IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
	{
		return _orderedProxyTypes.OfType<ExplicitSizeStruct>();
	}

	public override string ToString()
	{
		return Name;
	}

	public override ITypeReference GetBaseClass(EmitContext context)
	{
		return _systemObject;
	}

	public override IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		if (_compilerGeneratedAttribute != null)
		{
			return SpecializedCollections.SingletonEnumerable(_compilerGeneratedAttribute);
		}
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override INamespaceTypeDefinition AsNamespaceTypeDefinition(EmitContext context)
	{
		return this;
	}

	public IUnitReference GetUnit(EmitContext context)
	{
		return _moduleBuilder;
	}

	internal static string GenerateDataFieldName(ImmutableArray<byte> data)
	{
		ImmutableArray<byte> immutableArray = CryptographicHashProvider.ComputeSha1(data);
		char[] array = new char[immutableArray.Length * 2];
		int num = 0;
		foreach (byte item in immutableArray)
		{
			array[num++] = Hexchar(item >> 4);
			array[num++] = Hexchar(item & 0xF);
		}
		return new string(array);
	}

	private static char Hexchar(int x)
	{
		return (char)((x <= 9) ? (x + 48) : (x + 55));
	}
}
