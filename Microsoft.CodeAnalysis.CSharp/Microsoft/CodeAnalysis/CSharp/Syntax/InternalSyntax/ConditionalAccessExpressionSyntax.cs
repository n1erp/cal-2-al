using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConditionalAccessExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken operatorToken;

	internal readonly ExpressionSyntax whenNotNull;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken OperatorToken => operatorToken;

	public ExpressionSyntax WhenNotNull => whenNotNull;

	internal ConditionalAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, ExpressionSyntax whenNotNull, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(whenNotNull);
		this.whenNotNull = whenNotNull;
	}

	internal ConditionalAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, ExpressionSyntax whenNotNull, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(whenNotNull);
		this.whenNotNull = whenNotNull;
	}

	internal ConditionalAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, ExpressionSyntax whenNotNull)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(whenNotNull);
		this.whenNotNull = whenNotNull;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)expression, 
			1 => operatorToken, 
			2 => whenNotNull, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConditionalAccessExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConditionalAccessExpression(this);
	}

	public ConditionalAccessExpressionSyntax Update(ExpressionSyntax expression, SyntaxToken operatorToken, ExpressionSyntax whenNotNull)
	{
		if (expression != Expression || operatorToken != OperatorToken || whenNotNull != WhenNotNull)
		{
			ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax = SyntaxFactory.ConditionalAccessExpression(expression, operatorToken, whenNotNull);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				conditionalAccessExpressionSyntax = conditionalAccessExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				conditionalAccessExpressionSyntax = conditionalAccessExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return conditionalAccessExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ConditionalAccessExpressionSyntax(Kind, expression, operatorToken, whenNotNull, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ConditionalAccessExpressionSyntax(Kind, expression, operatorToken, whenNotNull, GetDiagnostics(), annotations);
	}

	internal ConditionalAccessExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			operatorToken = syntaxToken;
		}
		ExpressionSyntax expressionSyntax2 = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax2 != null)
		{
			AdjustFlagsAndWidth(expressionSyntax2);
			whenNotNull = expressionSyntax2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(expression);
		writer.WriteValue(operatorToken);
		writer.WriteValue(whenNotNull);
	}

	static ConditionalAccessExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ConditionalAccessExpressionSyntax), (ObjectReader r) => new ConditionalAccessExpressionSyntax(r));
	}
}
