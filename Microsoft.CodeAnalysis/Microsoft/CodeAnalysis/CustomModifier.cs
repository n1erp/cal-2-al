using System;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis;

public abstract class CustomModifier : ICustomModifier
{
	public abstract bool IsOptional { get; }

	public abstract INamedTypeSymbol Modifier { get; }

	bool ICustomModifier.IsOptional => IsOptional;

	ITypeReference ICustomModifier.GetModifier(EmitContext context)
	{
		throw new NotImplementedException();
	}
}
