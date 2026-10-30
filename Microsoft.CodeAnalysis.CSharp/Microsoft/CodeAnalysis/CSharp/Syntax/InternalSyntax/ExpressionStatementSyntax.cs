using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExpressionStatementSyntax : StatementSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken semicolonToken;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal ExpressionStatementSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ExpressionStatementSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ExpressionStatementSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)expression, 
			1 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExpressionStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExpressionStatement(this);
	}

	public ExpressionStatementSyntax Update(ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (expression != Expression || semicolonToken != SemicolonToken)
		{
			ExpressionStatementSyntax expressionStatementSyntax = SyntaxFactory.ExpressionStatement(expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				expressionStatementSyntax = expressionStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				expressionStatementSyntax = expressionStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return expressionStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ExpressionStatementSyntax(Kind, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ExpressionStatementSyntax(Kind, expression, semicolonToken, GetDiagnostics(), annotations);
	}

	internal ExpressionStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
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
			semicolonToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(expression);
		writer.WriteValue(semicolonToken);
	}

	static ExpressionStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ExpressionStatementSyntax), (ObjectReader r) => new ExpressionStatementSyntax(r));
	}
}
