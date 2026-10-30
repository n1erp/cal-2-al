using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal interface IAttributeValidator
{
	void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics);
}
