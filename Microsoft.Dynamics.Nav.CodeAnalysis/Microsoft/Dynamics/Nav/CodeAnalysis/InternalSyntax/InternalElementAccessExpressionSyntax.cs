using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalElementAccessExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalBracketedArgumentListSyntax _argumentList;

	internal override SyntaxNodeDefinition Definition => ElementAccessExpressionSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalBracketedArgumentListSyntax ArgumentList => _argumentList;

	internal InternalElementAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalBracketedArgumentListSyntax argumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(argumentList);
		_argumentList = argumentList;
	}

	internal InternalElementAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalBracketedArgumentListSyntax argumentList, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(argumentList);
		_argumentList = argumentList;
	}

	internal InternalElementAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalBracketedArgumentListSyntax argumentList)
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
		return new ElementAccessExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElementAccessExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitElementAccessExpression(this);
	}

	public InternalElementAccessExpressionSyntax Update(InternalCodeExpressionSyntax expression, InternalBracketedArgumentListSyntax argumentList)
	{
		if (expression != Expression || argumentList != ArgumentList)
		{
			return InternalSyntaxFactory.ElementAccessExpression(expression, argumentList).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalElementAccessExpressionSyntax(Kind, _expression, _argumentList, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalElementAccessExpressionSyntax(Kind, _expression, _argumentList, GetDiagnostics(), annotations);
	}
}
