using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class RunOnClientAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new RunOnClientAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.HasErrors())
		{
			symbol.IsAttributeOnVariableOfCompatibleType(diagnostics, NavTypeKind.DotNet);
		}
	}
}
