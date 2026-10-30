using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalWhereExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalSyntaxToken _whereKeywordToken;

	internal readonly InternalTableFilterExpressionSyntax _filter;

	internal override SyntaxNodeDefinition Definition => WhereExpressionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken WhereKeywordToken => _whereKeywordToken;

	public InternalTableFilterExpressionSyntax Filter => _filter;

	internal InternalWhereExpressionSyntax(SyntaxKind kind, InternalSyntaxToken whereKeywordToken, InternalTableFilterExpressionSyntax filter, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(whereKeywordToken);
		_whereKeywordToken = whereKeywordToken;
		AdjustFlagsAndWidth(filter);
		_filter = filter;
	}

	internal InternalWhereExpressionSyntax(SyntaxKind kind, InternalSyntaxToken whereKeywordToken, InternalTableFilterExpressionSyntax filter, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(whereKeywordToken);
		_whereKeywordToken = whereKeywordToken;
		AdjustFlagsAndWidth(filter);
		_filter = filter;
	}

	internal InternalWhereExpressionSyntax(SyntaxKind kind, InternalSyntaxToken whereKeywordToken, InternalTableFilterExpressionSyntax filter)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(whereKeywordToken);
		_whereKeywordToken = whereKeywordToken;
		AdjustFlagsAndWidth(filter);
		_filter = filter;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_whereKeywordToken, 
			1 => _filter, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new WhereExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhereExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitWhereExpression(this);
	}

	public InternalWhereExpressionSyntax Update(InternalSyntaxToken whereKeywordToken, InternalTableFilterExpressionSyntax filter)
	{
		if (whereKeywordToken != WhereKeywordToken || filter != Filter)
		{
			return InternalSyntaxFactory.WhereExpression(whereKeywordToken, filter).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalWhereExpressionSyntax(Kind, _whereKeywordToken, _filter, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalWhereExpressionSyntax(Kind, _whereKeywordToken, _filter, GetDiagnostics(), annotations);
	}
}
