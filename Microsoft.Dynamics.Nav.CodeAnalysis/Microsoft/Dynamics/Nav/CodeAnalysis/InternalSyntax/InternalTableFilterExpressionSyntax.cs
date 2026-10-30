using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTableFilterExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxNode _conditions;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => TableFilterExpressionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> Conditions => new InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_conditions));

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalTableFilterExpressionSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode conditions, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (conditions != null)
		{
			AdjustFlagsAndWidth(conditions);
			_conditions = conditions;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalTableFilterExpressionSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode conditions, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (conditions != null)
		{
			AdjustFlagsAndWidth(conditions);
			_conditions = conditions;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalTableFilterExpressionSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode conditions, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (conditions != null)
		{
			AdjustFlagsAndWidth(conditions);
			_conditions = conditions;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _openParenthesisToken, 
			1 => _conditions, 
			2 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TableFilterExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableFilterExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTableFilterExpression(this);
	}

	public InternalTableFilterExpressionSyntax Update(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions, InternalSyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || conditions != Conditions || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.TableFilterExpression(openParenthesisToken, conditions, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTableFilterExpressionSyntax(Kind, _openParenthesisToken, _conditions, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTableFilterExpressionSyntax(Kind, _openParenthesisToken, _conditions, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
