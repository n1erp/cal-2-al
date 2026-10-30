using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AwaitExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken awaitKeyword;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken AwaitKeyword => awaitKeyword;

	public ExpressionSyntax Expression => expression;

	internal AwaitExpressionSyntax(SyntaxKind kind, SyntaxToken awaitKeyword, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(awaitKeyword);
		this.awaitKeyword = awaitKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal AwaitExpressionSyntax(SyntaxKind kind, SyntaxToken awaitKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(awaitKeyword);
		this.awaitKeyword = awaitKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal AwaitExpressionSyntax(SyntaxKind kind, SyntaxToken awaitKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(awaitKeyword);
		this.awaitKeyword = awaitKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)awaitKeyword, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAwaitExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAwaitExpression(this);
	}

	public AwaitExpressionSyntax Update(SyntaxToken awaitKeyword, ExpressionSyntax expression)
	{
		if (awaitKeyword != AwaitKeyword || expression != Expression)
		{
			AwaitExpressionSyntax awaitExpressionSyntax = SyntaxFactory.AwaitExpression(awaitKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				awaitExpressionSyntax = awaitExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				awaitExpressionSyntax = awaitExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return awaitExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AwaitExpressionSyntax(Kind, awaitKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AwaitExpressionSyntax(Kind, awaitKeyword, expression, GetDiagnostics(), annotations);
	}

	internal AwaitExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			awaitKeyword = syntaxToken;
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
		writer.WriteValue(awaitKeyword);
		writer.WriteValue(expression);
	}

	static AwaitExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AwaitExpressionSyntax), (ObjectReader r) => new AwaitExpressionSyntax(r));
	}
}
