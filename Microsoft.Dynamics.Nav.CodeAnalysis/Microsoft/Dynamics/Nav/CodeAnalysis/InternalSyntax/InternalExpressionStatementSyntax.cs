using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalExpressionStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => ExpressionStatementSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalExpressionStatementSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalExpressionStatementSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalExpressionStatementSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_expression, 
			1 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ExpressionStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExpressionStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitExpressionStatement(this);
	}

	public InternalExpressionStatementSyntax Update(InternalCodeExpressionSyntax expression, InternalSyntaxToken semicolonToken)
	{
		if (expression != Expression || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.ExpressionStatement(expression, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalExpressionStatementSyntax(Kind, _expression, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalExpressionStatementSyntax(Kind, _expression, _semicolonToken, GetDiagnostics(), annotations);
	}
}
