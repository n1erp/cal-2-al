using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal sealed class SyntaxNavigator : AbstractSyntaxNavigator
{
	public static readonly AbstractSyntaxNavigator Instance = new SyntaxNavigator();

	protected override Func<SyntaxTrivia, bool> GetStepIntoFunction(bool skipped)
	{
		if (skipped)
		{
			return (SyntaxTrivia t) => t.Kind == SyntaxKind.SkippedTokensTrivia;
		}
		return null;
	}
}
