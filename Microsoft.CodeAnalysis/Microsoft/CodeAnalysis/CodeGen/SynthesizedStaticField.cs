using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal abstract class SynthesizedStaticField : IFieldDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IFieldReference
{
	private readonly INamedTypeDefinition _containingType;

	private readonly ITypeReference _type;

	private readonly string _name;

	public abstract ImmutableArray<byte> MappedData { get; }

	public bool IsCompileTimeConstant => false;

	public bool IsNotSerialized => false;

	public bool IsReadOnly => true;

	public bool IsRuntimeSpecial => false;

	public bool IsSpecialName => false;

	public bool IsStatic => true;

	public bool IsMarshalledExplicitly => false;

	public IMarshallingInformation MarshallingInformation => null;

	public ImmutableArray<byte> MarshallingDescriptor => default;

	public int Offset
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public ITypeDefinition ContainingTypeDefinition => _containingType;

	public TypeMemberVisibility Visibility => TypeMemberVisibility.Assembly;

	public string Name => _name;

	public bool IsContextualNamedEntity => false;

	internal ITypeReference Type => _type;

	public ISpecializedFieldReference AsSpecializedFieldReference => null;

	public MetadataConstant Constant
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal SynthesizedStaticField(string name, INamedTypeDefinition containingType, ITypeReference type)
	{
		_containingType = containingType;
		_type = type;
		_name = name;
	}

	public override string ToString()
	{
		return $"{_type} {_containingType}.{Name}";
	}

	public MetadataConstant GetCompileTimeValue(EmitContext context)
	{
		return null;
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		return _containingType;
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public IDefinition AsDefinition(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public ITypeReference GetType(EmitContext context)
	{
		return _type;
	}

	public IFieldDefinition GetResolvedField(EmitContext context)
	{
		return this;
	}
}
