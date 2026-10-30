using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIntegerExpressionPropertyValueSyntax : InternalExpressionPropertyValueSyntax
{
	internal readonly InternalCodeExpressionSyntax _expression;

	internal override SyntaxNodeDefinition Definition => IntegerExpressionPropertyValueSyntaxDefinition.Instance.Value;

	public override InternalCodeExpressionSyntax Expression => _expression;

	internal InternalIntegerExpressionPropertyValueSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
	}

	internal InternalIntegerExpressionPropertyValueSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
	}

	internal InternalIntegerExpressionPropertyValueSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _expression;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new IntegerExpressionPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIntegerExpressionPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIntegerExpressionPropertyValue(this);
	}

	public InternalIntegerExpressionPropertyValueSyntax Update(InternalCodeExpressionSyntax expression)
	{
		if (expression != Expression)
		{
			return InternalSyntaxFactory.IntegerExpressionPropertyValue(expression).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIntegerExpressionPropertyValueSyntax(Kind, _expression, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIntegerExpressionPropertyValueSyntax(Kind, _expression, GetDiagnostics(), annotations);
	}
}
