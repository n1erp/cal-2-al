using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ElementAccessExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly BracketedArgumentListSyntax argumentList;

	public ExpressionSyntax Expression => expression;

	public BracketedArgumentListSyntax ArgumentList => argumentList;

	internal ElementAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, BracketedArgumentListSyntax argumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ElementAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, BracketedArgumentListSyntax argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ElementAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, BracketedArgumentListSyntax argumentList)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElementAccessExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElementAccessExpression(this);
	}

	public ElementAccessExpressionSyntax Update(ExpressionSyntax expression, BracketedArgumentListSyntax argumentList)
	{
		if (expression != Expression || argumentList != ArgumentList)
		{
			ElementAccessExpressionSyntax elementAccessExpressionSyntax = SyntaxFactory.ElementAccessExpression(expression, argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				elementAccessExpressionSyntax = elementAccessExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				elementAccessExpressionSyntax = elementAccessExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return elementAccessExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ElementAccessExpressionSyntax(Kind, expression, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ElementAccessExpressionSyntax(Kind, expression, argumentList, GetDiagnostics(), annotations);
	}

	internal ElementAccessExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
		BracketedArgumentListSyntax bracketedArgumentListSyntax = (BracketedArgumentListSyntax)reader.ReadValue();
		if (bracketedArgumentListSyntax != null)
		{
			AdjustFlagsAndWidth(bracketedArgumentListSyntax);
			argumentList = bracketedArgumentListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(expression);
		writer.WriteValue(argumentList);
	}

	static ElementAccessExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ElementAccessExpressionSyntax), (ObjectReader r) => new ElementAccessExpressionSyntax(r));
	}
}
