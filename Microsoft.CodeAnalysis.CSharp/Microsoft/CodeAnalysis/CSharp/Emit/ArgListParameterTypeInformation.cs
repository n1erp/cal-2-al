using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class ArgListParameterTypeInformation : IParameterTypeInformation, IParameterListEntry
{
	private readonly ushort _ordinal;

	private readonly bool _isByRef;

	private readonly ITypeReference _type;

	ImmutableArray<ICustomModifier> IParameterTypeInformation.CustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	bool IParameterTypeInformation.IsByReference => _isByRef;

	ImmutableArray<ICustomModifier> IParameterTypeInformation.RefCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	ushort IParameterListEntry.Index => _ordinal;

	public ArgListParameterTypeInformation(int ordinal, bool isByRef, ITypeReference type)
	{
		_ordinal = (ushort)ordinal;
		_isByRef = isByRef;
		_type = type;
	}

	ITypeReference IParameterTypeInformation.GetType(EmitContext context)
	{
		return _type;
	}
}
