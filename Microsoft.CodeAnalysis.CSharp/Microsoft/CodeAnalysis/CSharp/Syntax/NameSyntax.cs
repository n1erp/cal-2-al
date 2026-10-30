using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class NameSyntax : TypeSyntax
{
	public int Arity
	{
		get
		{
			if (!(this is GenericNameSyntax))
			{
				return 0;
			}
			return ((GenericNameSyntax)this).TypeArgumentList.Arguments.Count;
		}
	}

	internal NameSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal abstract SimpleNameSyntax GetUnqualifiedName();

	internal abstract string ErrorDisplayName();

	internal string GetAliasQualifierOpt()
	{
		NameSyntax nameSyntax = this;
		while (true)
		{
			switch (nameSyntax.Kind())
			{
			case SyntaxKind.QualifiedName:
				break;
			case SyntaxKind.AliasQualifiedName:
				return ((AliasQualifiedNameSyntax)nameSyntax).Alias.Identifier.ValueText;
			default:
				return null;
			}
			nameSyntax = ((QualifiedNameSyntax)nameSyntax).Left;
		}
	}
}
