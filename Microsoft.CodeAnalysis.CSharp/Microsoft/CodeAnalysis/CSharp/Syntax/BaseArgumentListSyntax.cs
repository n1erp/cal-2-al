using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseArgumentListSyntax : CSharpSyntaxNode
{
	public abstract SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	internal BaseArgumentListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseArgumentListSyntax WithArguments(SeparatedSyntaxList<ArgumentSyntax> arguments)
	{
		return WithArgumentsCore(arguments);
	}

	internal abstract BaseArgumentListSyntax WithArgumentsCore(SeparatedSyntaxList<ArgumentSyntax> arguments);

	public BaseArgumentListSyntax AddArguments(params ArgumentSyntax[] items)
	{
		return AddArgumentsCore(items);
	}

	internal abstract BaseArgumentListSyntax AddArgumentsCore(params ArgumentSyntax[] items);
}
