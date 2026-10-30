using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalElseTableRelationSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalSyntaxToken _elseKeywordToken;

	internal readonly InternalTableRelationPropertyValueSyntax _elseTableRelationCondition;

	internal override SyntaxNodeDefinition Definition => ElseTableRelationSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ElseKeywordToken => _elseKeywordToken;

	public InternalTableRelationPropertyValueSyntax ElseTableRelationCondition => _elseTableRelationCondition;

	internal InternalElseTableRelationSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalTableRelationPropertyValueSyntax elseTableRelationCondition, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		AdjustFlagsAndWidth(elseTableRelationCondition);
		_elseTableRelationCondition = elseTableRelationCondition;
	}

	internal InternalElseTableRelationSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalTableRelationPropertyValueSyntax elseTableRelationCondition, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		AdjustFlagsAndWidth(elseTableRelationCondition);
		_elseTableRelationCondition = elseTableRelationCondition;
	}

	internal InternalElseTableRelationSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalTableRelationPropertyValueSyntax elseTableRelationCondition)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		AdjustFlagsAndWidth(elseTableRelationCondition);
		_elseTableRelationCondition = elseTableRelationCondition;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_elseKeywordToken, 
			1 => _elseTableRelationCondition, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ElseTableRelationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElseTableRelation(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitElseTableRelation(this);
	}

	public InternalElseTableRelationSyntax Update(InternalSyntaxToken elseKeywordToken, InternalTableRelationPropertyValueSyntax elseTableRelationCondition)
	{
		if (elseKeywordToken != ElseKeywordToken || elseTableRelationCondition != ElseTableRelationCondition)
		{
			return InternalSyntaxFactory.ElseTableRelation(elseKeywordToken, elseTableRelationCondition).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalElseTableRelationSyntax(Kind, _elseKeywordToken, _elseTableRelationCondition, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalElseTableRelationSyntax(Kind, _elseKeywordToken, _elseTableRelationCondition, GetDiagnostics(), annotations);
	}
}
