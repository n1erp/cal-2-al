using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal abstract class ArrayMethod : IMethodReference, ISignature, ITypeMemberReference, IReference, INamedEntity
{
	private readonly ImmutableArray<ArrayMethodParameterInfo> _parameters;

	protected readonly IArrayTypeReference arrayType;

	public abstract string Name { get; }

	public virtual bool ReturnValueIsByRef => false;

	public bool AcceptsExtraArguments => false;

	public ushort GenericParameterCount => 0;

	public bool IsGeneric => false;

	public ImmutableArray<IParameterTypeInformation> ExtraParameters => ImmutableArray<IParameterTypeInformation>.Empty;

	public IGenericMethodInstanceReference AsGenericMethodInstanceReference => null;

	public ISpecializedMethodReference AsSpecializedMethodReference => null;

	public CallingConvention CallingConvention => CallingConvention.HasThis;

	public ushort ParameterCount => (ushort)_parameters.Length;

	public ImmutableArray<ICustomModifier> RefCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public ImmutableArray<ICustomModifier> ReturnValueCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	protected ArrayMethod(IArrayTypeReference arrayType)
	{
		this.arrayType = arrayType;
		_parameters = MakeParameters();
	}

	public abstract ITypeReference GetType(EmitContext context);

	protected virtual ImmutableArray<ArrayMethodParameterInfo> MakeParameters()
	{
		int rank = arrayType.Rank;
		ArrayBuilder<ArrayMethodParameterInfo> instance = ArrayBuilder<ArrayMethodParameterInfo>.GetInstance(rank);
		for (int i = 0; i < rank; i++)
		{
			instance.Add(ArrayMethodParameterInfo.GetIndexParameter((ushort)i));
		}
		return instance.ToImmutableAndFree();
	}

	public ImmutableArray<IParameterTypeInformation> GetParameters(EmitContext context)
	{
		return StaticCast<IParameterTypeInformation>.From(_parameters);
	}

	public IMethodDefinition GetResolvedMethod(EmitContext context)
	{
		return null;
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		return arrayType;
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
		return null;
	}

	public override string ToString()
	{
		return arrayType.ToString() + "." + Name;
	}
}
