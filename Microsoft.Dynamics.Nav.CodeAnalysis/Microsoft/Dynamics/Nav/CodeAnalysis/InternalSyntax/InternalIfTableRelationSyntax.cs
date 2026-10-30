using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIfTableRelationSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalSyntaxToken _ifKeywordToken;

	internal readonly InternalTableFilterExpressionSyntax _ifTableRelationCondition;

	internal override SyntaxNodeDefinition Definition => IfTableRelationSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken IfKeywordToken => _ifKeywordToken;

	public InternalTableFilterExpressionSyntax IfTableRelationCondition => _ifTableRelationCondition;

	internal InternalIfTableRelationSyntax(SyntaxKind kind, InternalSyntaxToken ifKeywordToken, InternalTableFilterExpressionSyntax ifTableRelationCondition, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(ifKeywordToken);
		_ifKeywordToken = ifKeywordToken;
		AdjustFlagsAndWidth(ifTableRelationCondition);
		_ifTableRelationCondition = ifTableRelationCondition;
	}

	internal InternalIfTableRelationSyntax(SyntaxKind kind, InternalSyntaxToken ifKeywordToken, InternalTableFilterExpressionSyntax ifTableRelationCondition, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(ifKeywordToken);
		_ifKeywordToken = ifKeywordToken;
		AdjustFlagsAndWidth(ifTableRelationCondition);
		_ifTableRelationCondition = ifTableRelationCondition;
	}

	internal InternalIfTableRelationSyntax(SyntaxKind kind, InternalSyntaxToken ifKeywordToken, InternalTableFilterExpressionSyntax ifTableRelationCondition)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(ifKeywordToken);
		_ifKeywordToken = ifKeywordToken;
		AdjustFlagsAndWidth(ifTableRelationCondition);
		_ifTableRelationCondition = ifTableRelationCondition;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_ifKeywordToken, 
			1 => _ifTableRelationCondition, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new IfTableRelationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfTableRelation(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIfTableRelation(this);
	}

	public InternalIfTableRelationSyntax Update(InternalSyntaxToken ifKeywordToken, InternalTableFilterExpressionSyntax ifTableRelationCondition)
	{
		if (ifKeywordToken != IfKeywordToken || ifTableRelationCondition != IfTableRelationCondition)
		{
			return InternalSyntaxFactory.IfTableRelation(ifKeywordToken, ifTableRelationCondition).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIfTableRelationSyntax(Kind, _ifKeywordToken, _ifTableRelationCondition, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIfTableRelationSyntax(Kind, _ifKeywordToken, _ifTableRelationCondition, GetDiagnostics(), annotations);
	}
}
