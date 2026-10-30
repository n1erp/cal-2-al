using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PrefixUnaryExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken operatorToken;

	internal readonly ExpressionSyntax operand;

	public SyntaxToken OperatorToken => operatorToken;

	public ExpressionSyntax Operand => operand;

	internal PrefixUnaryExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(operand);
		this.operand = operand;
	}

	internal PrefixUnaryExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(operand);
		this.operand = operand;
	}

	internal PrefixUnaryExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(operand);
		this.operand = operand;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)operatorToken, 
			1 => operand, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPrefixUnaryExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPrefixUnaryExpression(this);
	}

	public PrefixUnaryExpressionSyntax Update(SyntaxToken operatorToken, ExpressionSyntax operand)
	{
		if (operatorToken != OperatorToken || operand != Operand)
		{
			PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax = SyntaxFactory.PrefixUnaryExpression(Kind, operatorToken, operand);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				prefixUnaryExpressionSyntax = prefixUnaryExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				prefixUnaryExpressionSyntax = prefixUnaryExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return prefixUnaryExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new PrefixUnaryExpressionSyntax(Kind, operatorToken, operand, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new PrefixUnaryExpressionSyntax(Kind, operatorToken, operand, GetDiagnostics(), annotations);
	}

	internal PrefixUnaryExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			operatorToken = syntaxToken;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			operand = expressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(operatorToken);
		writer.WriteValue(operand);
	}

	static PrefixUnaryExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(PrefixUnaryExpressionSyntax), (ObjectReader r) => new PrefixUnaryExpressionSyntax(r));
	}
}
