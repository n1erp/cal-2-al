using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalGroupActionListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _actionsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _actions;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => GroupActionListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ActionsKeyword => _actionsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalActionBaseSyntax> Actions => new InternalSyntaxList<InternalActionBaseSyntax>(_actions);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalGroupActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode actions, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalGroupActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode actions, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalGroupActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode actions, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
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
			0 => _actionsKeyword, 
			1 => _openBraceToken, 
			2 => _actions, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new GroupActionListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGroupActionList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitGroupActionList(this);
	}

	public InternalGroupActionListSyntax Update(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		if (actionsKeyword != ActionsKeyword || openBraceToken != OpenBraceToken || actions != Actions || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.GroupActionList(actionsKeyword, openBraceToken, actions, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalGroupActionListSyntax(Kind, _actionsKeyword, _openBraceToken, _actions, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalGroupActionListSyntax(Kind, _actionsKeyword, _openBraceToken, _actions, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
