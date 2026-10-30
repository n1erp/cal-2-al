using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InvocationExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly ArgumentListSyntax argumentList;

	public ExpressionSyntax Expression => expression;

	public ArgumentListSyntax ArgumentList => argumentList;

	internal InvocationExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, ArgumentListSyntax argumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal InvocationExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, ArgumentListSyntax argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal InvocationExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, ArgumentListSyntax argumentList)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)expression, 
			1 => argumentList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvocationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInvocationExpression(this);
	}

	public InvocationExpressionSyntax Update(ExpressionSyntax expression, ArgumentListSyntax argumentList)
	{
		if (expression != Expression || argumentList != ArgumentList)
		{
			InvocationExpressionSyntax invocationExpressionSyntax = SyntaxFactory.InvocationExpression(expression, argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return invocationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InvocationExpressionSyntax(Kind, expression, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InvocationExpressionSyntax(Kind, expression, argumentList, GetDiagnostics(), annotations);
	}

	internal InvocationExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
		ArgumentListSyntax argumentListSyntax = (ArgumentListSyntax)reader.ReadValue();
		if (argumentListSyntax != null)
		{
			AdjustFlagsAndWidth(argumentListSyntax);
			argumentList = argumentListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(expression);
		writer.WriteValue(argumentList);
	}

	static InvocationExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(InvocationExpressionSyntax), (ObjectReader r) => new InvocationExpressionSyntax(r));
	}
}
