using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageActionAreaSyntax : InternalActionGroupBaseSyntax
{
	internal readonly InternalSyntaxToken _actionKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _actions;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageActionAreaSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken ActionKeyword => _actionKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxList<InternalActionBaseSyntax> Actions => new InternalSyntaxList<InternalActionBaseSyntax>(_actions);

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageActionAreaSyntax(SyntaxKind kind, InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode actions, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(actionKeyword);
		_actionKeyword = actionKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageActionAreaSyntax(SyntaxKind kind, InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode actions, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(actionKeyword);
		_actionKeyword = actionKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageActionAreaSyntax(SyntaxKind kind, InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode actions, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(actionKeyword);
		_actionKeyword = actionKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _actionKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _closeParenthesisToken, 
			4 => _openBraceToken, 
			5 => _propertyList, 
			6 => _actions, 
			7 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageActionAreaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageActionArea(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageActionArea(this);
	}

	public InternalPageActionAreaSyntax Update(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		if (actionKeyword != ActionKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || actions != Actions || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageActionArea(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageActionAreaSyntax(Kind, _actionKeyword, _openParenthesisToken, _name, _closeParenthesisToken, _openBraceToken, _propertyList, _actions, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageActionAreaSyntax(Kind, _actionKeyword, _openParenthesisToken, _name, _closeParenthesisToken, _openBraceToken, _propertyList, _actions, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
