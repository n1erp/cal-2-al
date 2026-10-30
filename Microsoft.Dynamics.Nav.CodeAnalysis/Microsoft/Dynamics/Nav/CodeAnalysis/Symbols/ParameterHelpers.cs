using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class ParameterHelpers
{
	public static ImmutableArray<ParameterSymbol> MakeParameters(Binder binder, Symbol owner, SeparatedSyntaxList<ParameterSyntax> syntax, DiagnosticBag diagnostics)
	{
		int num = 0;
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
		foreach (ParameterSyntax item2 in syntax)
		{
			bool isVar = item2.VarKeyword.Kind == SyntaxKind.VarKeyword;
			DebugAssertHelper.Assert(item2.Type != null);
			TypeSymbol parameterType = binder.BindParameterType(owner, item2.Type, isVar, diagnostics);
			SourceParameterSymbol item = SourceParameterSymbol.Create(owner, parameterType, item2, num, isVar, item2.Name.Identifier);
			instance.Add(item);
			num++;
		}
		return instance.ToImmutableAndFree();
	}
}
