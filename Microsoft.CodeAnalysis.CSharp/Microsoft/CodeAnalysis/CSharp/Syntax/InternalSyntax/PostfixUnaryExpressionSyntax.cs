using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PostfixUnaryExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax operand;

	internal readonly SyntaxToken operatorToken;

	public ExpressionSyntax Operand => operand;

	public SyntaxToken OperatorToken => operatorToken;

	internal PostfixUnaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(operand);
		this.operand = operand;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
	}

	internal PostfixUnaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(operand);
		this.operand = operand;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
	}

	internal PostfixUnaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(operand);
		this.operand = operand;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)operand, 
			1 => operatorToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPostfixUnaryExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPostfixUnaryExpression(this);
	}

	public PostfixUnaryExpressionSyntax Update(ExpressionSyntax operand, SyntaxToken operatorToken)
	{
		if (operand != Operand || operatorToken != OperatorToken)
		{
			PostfixUnaryExpressionSyntax postfixUnaryExpressionSyntax = SyntaxFactory.PostfixUnaryExpression(Kind, operand, operatorToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				postfixUnaryExpressionSyntax = postfixUnaryExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				postfixUnaryExpressionSyntax = postfixUnaryExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return postfixUnaryExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new PostfixUnaryExpressionSyntax(Kind, operand, operatorToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new PostfixUnaryExpressionSyntax(Kind, operand, operatorToken, GetDiagnostics(), annotations);
	}

	internal PostfixUnaryExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			operand = expressionSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			operatorToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(operand);
		writer.WriteValue(operatorToken);
	}

	static PostfixUnaryExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(PostfixUnaryExpressionSyntax), (ObjectReader r) => new PostfixUnaryExpressionSyntax(r));
	}
}
