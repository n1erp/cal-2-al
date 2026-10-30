using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class RefExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken refKeyword;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken RefKeyword => refKeyword;

	public ExpressionSyntax Expression => expression;

	internal RefExpressionSyntax(SyntaxKind kind, SyntaxToken refKeyword, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(refKeyword);
		this.refKeyword = refKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal RefExpressionSyntax(SyntaxKind kind, SyntaxToken refKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(refKeyword);
		this.refKeyword = refKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal RefExpressionSyntax(SyntaxKind kind, SyntaxToken refKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(refKeyword);
		this.refKeyword = refKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)refKeyword, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.RefExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRefExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRefExpression(this);
	}

	public RefExpressionSyntax Update(SyntaxToken refKeyword, ExpressionSyntax expression)
	{
		if (refKeyword != RefKeyword || expression != Expression)
		{
			RefExpressionSyntax refExpressionSyntax = SyntaxFactory.RefExpression(refKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				refExpressionSyntax = refExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				refExpressionSyntax = refExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return refExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new RefExpressionSyntax(Kind, refKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new RefExpressionSyntax(Kind, refKeyword, expression, GetDiagnostics(), annotations);
	}

	internal RefExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			refKeyword = syntaxToken;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(refKeyword);
		writer.WriteValue(expression);
	}

	static RefExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(RefExpressionSyntax), (ObjectReader r) => new RefExpressionSyntax(r));
	}
}
