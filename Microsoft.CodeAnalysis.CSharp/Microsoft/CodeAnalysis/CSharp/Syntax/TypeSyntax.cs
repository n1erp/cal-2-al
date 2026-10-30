using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class TypeSyntax : ExpressionSyntax
{
	public bool IsVar => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)Green).IsVar;

	public bool IsUnmanaged => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeSyntax)Green).IsUnmanaged;

	internal TypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
