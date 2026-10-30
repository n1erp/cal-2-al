using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageViewMove : INavDesignerPageViewPositionalChange, INavDesignerPageViewChange
{
	private ViewMoveChangeSyntax viewMoveSyntax;

	public SyntaxKind SyntaxKind => viewMoveSyntax.Kind;

	public ViewChangeBaseSyntax Syntax => viewMoveSyntax;

	public string AnchorViewName
	{
		get
		{
			if (viewMoveSyntax.ChangeKeyword.Kind != SyntaxKind.MoveAfterKeyword)
			{
				return null;
			}
			return viewMoveSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
		}
		set
		{
			if (value != null)
			{
				viewMoveSyntax = viewMoveSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.MoveAfterKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
			}
		}
	}

	public string ViewName => viewMoveSyntax.Views.FirstOrDefault()?.Identifier.ValueText.UnquoteIdentifier();

	internal NavDesignerPageViewMove(string viewName, string previousViewName = null)
	{
		if (!string.IsNullOrEmpty(previousViewName))
		{
			viewMoveSyntax = SyntaxFactory.ViewMoveChange(SyntaxFactory.Token(SyntaxKind.MoveAfterKeyword), SyntaxFactory.Token(SyntaxKind.OpenParenToken), SyntaxFactory.IdentifierName(previousViewName), SyntaxFactory.Token(SyntaxKind.SemicolonToken), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(viewName)), SyntaxFactory.Token(SyntaxKind.CloseParenToken));
		}
		else
		{
			viewMoveSyntax = SyntaxFactory.ViewMoveChange(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), null, default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(viewName)));
		}
	}

	internal NavDesignerPageViewMove(ViewMoveChangeSyntax moveSyntax)
	{
		viewMoveSyntax = moveSyntax;
	}
}
