using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageExtensionActionListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _actionsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _changes;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageExtensionActionListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ActionsKeyword => _actionsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalActionChangeBaseSyntax> Changes => new InternalSyntaxList<InternalActionChangeBaseSyntax>(_changes);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageExtensionActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageExtensionActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageExtensionActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
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
			2 => _changes, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageExtensionActionListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtensionActionList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageExtensionActionList(this);
	}

	public InternalPageExtensionActionListSyntax Update(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		if (actionsKeyword != ActionsKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageExtensionActionList(actionsKeyword, openBraceToken, changes, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageExtensionActionListSyntax(Kind, _actionsKeyword, _openBraceToken, _changes, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageExtensionActionListSyntax(Kind, _actionsKeyword, _openBraceToken, _changes, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
