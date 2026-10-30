using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageControlMove : INavDesignerPageControlPositionalChange, INavDesignerPageControlChange
{
	private ControlMoveChangeSyntax controlMoveSyntax;

	public SyntaxKind SyntaxKind => controlMoveSyntax.Kind;

	public ControlChangeBaseSyntax Syntax => controlMoveSyntax;

	public string ParentName
	{
		get
		{
			if (controlMoveSyntax.ChangeKeyword.Kind != SyntaxKind.MoveFirstKeyword)
			{
				return null;
			}
			return controlMoveSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
		}
		set
		{
			if (value != null)
			{
				controlMoveSyntax = controlMoveSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
			}
		}
	}

	public string AnchorControlName
	{
		get
		{
			if (controlMoveSyntax.ChangeKeyword.Kind != SyntaxKind.MoveAfterKeyword)
			{
				return null;
			}
			return controlMoveSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
		}
		set
		{
			if (value != null)
			{
				controlMoveSyntax = controlMoveSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.MoveAfterKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
			}
		}
	}

	public string ControlName => controlMoveSyntax.Controls.FirstOrDefault()?.Identifier.ValueText.UnquoteIdentifier();

	internal NavDesignerPageControlMove(string controlName, ControlContainerType controlContainerType = ControlContainerType.ContentArea, string parentName = null, string previousControlName = null)
	{
		if (!string.IsNullOrEmpty(previousControlName))
		{
			controlMoveSyntax = SyntaxFactory.ControlMoveChange(SyntaxFactory.Token(SyntaxKind.MoveAfterKeyword), SyntaxFactory.IdentifierName(previousControlName), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(controlName)));
			return;
		}
		if (!string.IsNullOrEmpty(parentName))
		{
			controlMoveSyntax = SyntaxFactory.ControlMoveChange(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), SyntaxFactory.IdentifierName(parentName), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(controlName)));
			return;
		}
		AreaKind areaKind = NavDesignerUtil.ControlContainerTypeToAreaKind(controlContainerType);
		controlMoveSyntax = SyntaxFactory.ControlMoveChange(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), SyntaxFactory.IdentifierName(areaKind.ToString()), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(controlName)));
	}

	internal NavDesignerPageControlMove(ControlMoveChangeSyntax moveSyntax)
	{
		controlMoveSyntax = moveSyntax;
	}
}
