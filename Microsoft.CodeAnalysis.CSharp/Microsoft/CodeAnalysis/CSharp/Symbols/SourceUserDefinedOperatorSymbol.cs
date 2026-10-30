using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceUserDefinedOperatorSymbol : SourceUserDefinedOperatorSymbolBase
{
	protected override ParameterListSyntax ParameterListSyntax => GetSyntax().ParameterList;

	protected override TypeSyntax ReturnTypeSyntax => GetSyntax().ReturnType;

	internal override bool GenerateDebugInfo => true;

	public static SourceUserDefinedOperatorSymbol CreateUserDefinedOperatorSymbol(SourceMemberContainerTypeSymbol containingType, OperatorDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		Location location = syntax.OperatorToken.GetLocation();
		string name = OperatorFacts.OperatorNameFromDeclaration(syntax);
		return new SourceUserDefinedOperatorSymbol(containingType, name, location, syntax, diagnostics, syntax.Body == null && syntax.ExpressionBody != null);
	}

	private SourceUserDefinedOperatorSymbol(SourceMemberContainerTypeSymbol containingType, string name, Location location, OperatorDeclarationSyntax syntax, DiagnosticBag diagnostics, bool isExpressionBodied)
		: base(MethodKind.UserDefinedOperator, name, containingType, location, syntax, diagnostics)
	{
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	internal new OperatorDeclarationSyntax GetSyntax()
	{
		return (OperatorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}
}
