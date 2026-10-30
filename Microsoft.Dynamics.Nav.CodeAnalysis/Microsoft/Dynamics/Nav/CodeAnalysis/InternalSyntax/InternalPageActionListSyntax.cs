using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageActionListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _actionsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _areas;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageActionListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ActionsKeyword => _actionsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalPageActionAreaSyntax> Areas => new InternalSyntaxList<InternalPageActionAreaSyntax>(_areas);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode areas, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (areas != null)
		{
			AdjustFlagsAndWidth(areas);
			_areas = areas;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode areas, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (areas != null)
		{
			AdjustFlagsAndWidth(areas);
			_areas = areas;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageActionListSyntax(SyntaxKind kind, InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode areas, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(actionsKeyword);
		_actionsKeyword = actionsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (areas != null)
		{
			AdjustFlagsAndWidth(areas);
			_areas = areas;
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
			2 => _areas, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageActionListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageActionList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageActionList(this);
	}

	public InternalPageActionListSyntax Update(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageActionAreaSyntax> areas, InternalSyntaxToken closeBraceToken)
	{
		if (actionsKeyword != ActionsKeyword || openBraceToken != OpenBraceToken || areas != Areas || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageActionList(actionsKeyword, openBraceToken, areas, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageActionListSyntax(Kind, _actionsKeyword, _openBraceToken, _areas, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageActionListSyntax(Kind, _actionsKeyword, _openBraceToken, _areas, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
