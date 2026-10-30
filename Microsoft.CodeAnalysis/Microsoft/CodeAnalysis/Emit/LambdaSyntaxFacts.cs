namespace Microsoft.CodeAnalysis.Emit;

internal abstract class LambdaSyntaxFacts
{
	public abstract SyntaxNode GetLambda(SyntaxNode lambdaOrLambdaBodySyntax);

	public abstract SyntaxNode TryGetCorrespondingLambdaBody(SyntaxNode previousLambdaSyntax, SyntaxNode lambdaOrLambdaBodySyntax);
}
