using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceUserDefinedConversionSymbol : SourceUserDefinedOperatorSymbolBase
{
	protected override ParameterListSyntax ParameterListSyntax => GetSyntax().ParameterList;

	protected override TypeSyntax ReturnTypeSyntax => GetSyntax().Type;

	internal override bool GenerateDebugInfo => true;

	public static SourceUserDefinedConversionSymbol CreateUserDefinedConversionSymbol(SourceMemberContainerTypeSymbol containingType, ConversionOperatorDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		Location location = syntax.Type.Location;
		string name = (syntax.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword) ? "op_Implicit" : "op_Explicit");
		return new SourceUserDefinedConversionSymbol(containingType, name, location, syntax, diagnostics);
	}

	private SourceUserDefinedConversionSymbol(SourceMemberContainerTypeSymbol containingType, string name, Location location, ConversionOperatorDeclarationSyntax syntax, DiagnosticBag diagnostics)
		: base(MethodKind.Conversion, name, containingType, location, syntax, diagnostics)
	{
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
		if (syntax.ParameterList.Parameters.Count != 1)
		{
			diagnostics.Add(ErrorCode.ERR_OvlUnaryOperatorExpected, syntax.ParameterList.GetLocation());
		}
	}

	internal new ConversionOperatorDeclarationSyntax GetSyntax()
	{
		return (ConversionOperatorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}
}
