using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal class ReturnValueParameter : IParameterDefinition, IDefinition, IReference, INamedEntity, IParameterTypeInformation, IParameterListEntry
{
	private readonly IMethodDefinition _containingMethod;

	public ISignature ContainingSignature => _containingMethod;

	public MetadataConstant Constant => null;

	public ImmutableArray<ICustomModifier> RefCustomModifiers => _containingMethod.RefCustomModifiers;

	public ImmutableArray<ICustomModifier> CustomModifiers => _containingMethod.ReturnValueCustomModifiers;

	public bool HasDefaultValue => false;

	public ushort Index => 0;

	public bool IsIn => false;

	public bool IsByReference => _containingMethod.ReturnValueIsByRef;

	public bool IsMarshalledExplicitly => _containingMethod.ReturnValueIsMarshalledExplicitly;

	public bool IsOptional => false;

	public bool IsOut => false;

	public IMarshallingInformation MarshallingInformation => _containingMethod.ReturnValueMarshallingInformation;

	public ImmutableArray<byte> MarshallingDescriptor => _containingMethod.ReturnValueMarshallingDescriptor;

	public string Name => string.Empty;

	internal ReturnValueParameter(IMethodDefinition containingMethod)
	{
		_containingMethod = containingMethod;
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return _containingMethod.GetReturnValueAttributes(context);
	}

	public MetadataConstant GetDefaultValue(EmitContext context)
	{
		return null;
	}

	public void Dispatch(MetadataVisitor visitor)
	{
	}

	public ITypeReference GetType(EmitContext context)
	{
		return _containingMethod.GetType(context);
	}

	public IDefinition AsDefinition(EmitContext context)
	{
		return this;
	}
}
