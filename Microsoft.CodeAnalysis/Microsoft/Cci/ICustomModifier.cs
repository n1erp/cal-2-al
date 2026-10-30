using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface ICustomModifier
{
	bool IsOptional { get; }

	ITypeReference GetModifier(EmitContext context);
}
