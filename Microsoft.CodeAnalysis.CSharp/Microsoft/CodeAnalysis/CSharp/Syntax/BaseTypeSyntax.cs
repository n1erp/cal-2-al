using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseTypeSyntax : CSharpSyntaxNode
{
	public abstract TypeSyntax Type { get; }

	internal BaseTypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseTypeSyntax WithType(TypeSyntax type)
	{
		return WithTypeCore(type);
	}

	internal abstract BaseTypeSyntax WithTypeCore(TypeSyntax type);
}
