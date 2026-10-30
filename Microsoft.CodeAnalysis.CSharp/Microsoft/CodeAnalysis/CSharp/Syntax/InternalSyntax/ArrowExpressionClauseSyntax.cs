using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ArrowExpressionClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken arrowToken;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken ArrowToken => arrowToken;

	public ExpressionSyntax Expression => expression;

	internal ArrowExpressionClauseSyntax(SyntaxKind kind, SyntaxToken arrowToken, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ArrowExpressionClauseSyntax(SyntaxKind kind, SyntaxToken arrowToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ArrowExpressionClauseSyntax(SyntaxKind kind, SyntaxToken arrowToken, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)arrowToken, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArrowExpressionClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArrowExpressionClause(this);
	}

	public ArrowExpressionClauseSyntax Update(SyntaxToken arrowToken, ExpressionSyntax expression)
	{
		if (arrowToken != ArrowToken || expression != Expression)
		{
			ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = SyntaxFactory.ArrowExpressionClause(arrowToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				arrowExpressionClauseSyntax = arrowExpressionClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				arrowExpressionClauseSyntax = arrowExpressionClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return arrowExpressionClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ArrowExpressionClauseSyntax(Kind, arrowToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ArrowExpressionClauseSyntax(Kind, arrowToken, expression, GetDiagnostics(), annotations);
	}

	internal ArrowExpressionClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			arrowToken = syntaxToken;
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
		writer.WriteValue(arrowToken);
		writer.WriteValue(expression);
	}

	static ArrowExpressionClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ArrowExpressionClauseSyntax), (ObjectReader r) => new ArrowExpressionClauseSyntax(r));
	}
}
