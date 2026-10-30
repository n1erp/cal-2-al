using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class BaseAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new BaseAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
	}
}
