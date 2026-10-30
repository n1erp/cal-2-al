using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInvocationExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalArgumentListSyntax _argumentList;

	internal override SyntaxNodeDefinition Definition => InvocationExpressionSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalArgumentListSyntax ArgumentList => _argumentList;

	internal InternalInvocationExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalArgumentListSyntax argumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(argumentList);
		_argumentList = argumentList;
	}

	internal InternalInvocationExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalArgumentListSyntax argumentList, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(argumentList);
		_argumentList = argumentList;
	}

	internal InternalInvocationExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalArgumentListSyntax argumentList)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(argumentList);
		_argumentList = argumentList;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_expression, 
			1 => _argumentList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new InvocationExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvocationExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInvocationExpression(this);
	}

	public InternalInvocationExpressionSyntax Update(InternalCodeExpressionSyntax expression, InternalArgumentListSyntax argumentList)
	{
		if (expression != Expression || argumentList != ArgumentList)
		{
			return InternalSyntaxFactory.InvocationExpression(expression, argumentList).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInvocationExpressionSyntax(Kind, _expression, _argumentList, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInvocationExpressionSyntax(Kind, _expression, _argumentList, GetDiagnostics(), annotations);
	}
}
