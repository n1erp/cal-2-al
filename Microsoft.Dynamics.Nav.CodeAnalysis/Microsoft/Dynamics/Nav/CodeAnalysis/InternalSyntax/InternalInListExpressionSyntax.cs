using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInListExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalSyntaxToken _inKeywordToken;

	internal readonly InternalBracketedArgumentListSyntax _listExpression;

	internal override SyntaxNodeDefinition Definition => InListExpressionSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalSyntaxToken InKeywordToken => _inKeywordToken;

	public InternalBracketedArgumentListSyntax ListExpression => _listExpression;

	internal InternalInListExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken inKeywordToken, InternalBracketedArgumentListSyntax listExpression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(inKeywordToken);
		_inKeywordToken = inKeywordToken;
		AdjustFlagsAndWidth(listExpression);
		_listExpression = listExpression;
	}

	internal InternalInListExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken inKeywordToken, InternalBracketedArgumentListSyntax listExpression, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(inKeywordToken);
		_inKeywordToken = inKeywordToken;
		AdjustFlagsAndWidth(listExpression);
		_listExpression = listExpression;
	}

	internal InternalInListExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken inKeywordToken, InternalBracketedArgumentListSyntax listExpression)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(inKeywordToken);
		_inKeywordToken = inKeywordToken;
		AdjustFlagsAndWidth(listExpression);
		_listExpression = listExpression;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_expression, 
			1 => _inKeywordToken, 
			2 => _listExpression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new InListExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInListExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInListExpression(this);
	}

	public InternalInListExpressionSyntax Update(InternalCodeExpressionSyntax expression, InternalSyntaxToken inKeywordToken, InternalBracketedArgumentListSyntax listExpression)
	{
		if (expression != Expression || inKeywordToken != InKeywordToken || listExpression != ListExpression)
		{
			return InternalSyntaxFactory.InListExpression(expression, inKeywordToken, listExpression).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInListExpressionSyntax(Kind, _expression, _inKeywordToken, _listExpression, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInListExpressionSyntax(Kind, _expression, _inKeywordToken, _listExpression, GetDiagnostics(), annotations);
	}
}
