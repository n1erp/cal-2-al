using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IParameterTypeInformation : IParameterListEntry
{
	ImmutableArray<ICustomModifier> CustomModifiers { get; }

	ImmutableArray<ICustomModifier> RefCustomModifiers { get; }

	bool IsByReference { get; }

	ITypeReference GetType(EmitContext context);
}
