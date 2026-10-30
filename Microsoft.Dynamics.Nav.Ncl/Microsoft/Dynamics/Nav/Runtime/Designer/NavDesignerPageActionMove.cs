using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageActionMove : INavDesignerPageActionPositionalChange, INavDesignerPageActionChange
{
	private ActionMoveChangeSyntax actionMoveSyntax;

	public SyntaxKind SyntaxKind => actionMoveSyntax.Kind;

	public ActionChangeBaseSyntax Syntax => actionMoveSyntax;

	public string ParentName
	{
		get
		{
			if (actionMoveSyntax.ChangeKeyword.Kind != SyntaxKind.MoveFirstKeyword)
			{
				return null;
			}
			return actionMoveSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
		}
		set
		{
			if (value != null)
			{
				actionMoveSyntax = actionMoveSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
			}
		}
	}

	public string AnchorActionName
	{
		get
		{
			if (actionMoveSyntax.ChangeKeyword.Kind != SyntaxKind.MoveAfterKeyword)
			{
				return null;
			}
			return actionMoveSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
		}
		set
		{
			if (value != null)
			{
				actionMoveSyntax = actionMoveSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.MoveAfterKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
			}
		}
	}

	public string ActionName => actionMoveSyntax.Actions.FirstOrDefault()?.Identifier.ValueText.UnquoteIdentifier();

	internal NavDesignerPageActionMove(string actionName, ActionAreaKind actionAreaKind, string parentName = null, string previousActionName = null)
	{
		if (!string.IsNullOrEmpty(previousActionName))
		{
			actionMoveSyntax = SyntaxFactory.ActionMoveChange(SyntaxFactory.Token(SyntaxKind.MoveAfterKeyword), SyntaxFactory.IdentifierName(previousActionName), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(actionName)));
		}
		else if (!string.IsNullOrEmpty(parentName))
		{
			actionMoveSyntax = SyntaxFactory.ActionMoveChange(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), SyntaxFactory.IdentifierName(parentName), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(actionName)));
		}
		else
		{
			actionMoveSyntax = SyntaxFactory.ActionMoveChange(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), SyntaxFactory.IdentifierName(actionAreaKind.ToString()), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(actionName)));
		}
	}

	internal NavDesignerPageActionMove(ActionMoveChangeSyntax moveSyntax)
	{
		actionMoveSyntax = moveSyntax;
	}
}
