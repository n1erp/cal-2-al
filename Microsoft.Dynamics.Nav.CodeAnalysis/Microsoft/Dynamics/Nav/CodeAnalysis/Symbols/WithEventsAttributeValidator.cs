using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class WithEventsAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new WithEventsAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.HasErrors())
		{
			if (!symbol.ContainingSymbol.IsKind(SymbolKind.GlobalVariable))
			{
				diagnostics.Add(ErrorCode.ERR_AttributeCanOnlyBeSpecifiedOnGlobalVariables, symbol.GetLocation(), symbol.AttributeInfo.Kind.ToString());
			}
			else if (symbol.IsAttributeOnVariableOfCompatibleType(diagnostics, NavTypeKind.DotNet) && ((VariableSymbol)symbol.ContainingSymbol).HasRunOnClientAttribute() && symbol.ContainingType.Kind != SymbolKind.Page)
			{
				diagnostics.Add(ErrorCode.ERR_ClientSideEventsAreSupportedOnlyOnPages, symbol.GetLocation());
			}
		}
	}
}
