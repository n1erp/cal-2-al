using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SuppressDisposeAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new SuppressDisposeAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.HasErrors())
		{
			if (!symbol.ContainingSymbol.IsKind(SymbolKind.LocalVariable))
			{
				diagnostics.Add(ErrorCode.ERR_AttributeCanOnlyBeSpecifiedOnLocalVariables, symbol.GetLocation(), symbol.AttributeInfo.Kind.ToString());
			}
			else if (((VariableSymbol)symbol.ContainingSymbol).Type.IsArray())
			{
				diagnostics.Add(ErrorCode.ERR_AttributeCannotBeUsedOnVariablesOfArrayType, symbol.GetLocation(), symbol.AttributeInfo.Kind.ToString());
			}
			else
			{
				symbol.IsAttributeOnVariableOfCompatibleType(diagnostics, NavTypeKind.DotNet);
			}
		}
	}
}
