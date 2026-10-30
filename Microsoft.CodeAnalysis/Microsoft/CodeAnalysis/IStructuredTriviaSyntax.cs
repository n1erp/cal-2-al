namespace Microsoft.CodeAnalysis;

public interface IStructuredTriviaSyntax
{
	SyntaxTrivia ParentTrivia { get; }
}
