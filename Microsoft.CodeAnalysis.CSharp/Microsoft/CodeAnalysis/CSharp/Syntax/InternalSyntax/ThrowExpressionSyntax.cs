using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ThrowExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken throwKeyword;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken ThrowKeyword => throwKeyword;

	public ExpressionSyntax Expression => expression;

	internal ThrowExpressionSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ThrowExpressionSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ThrowExpressionSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)throwKeyword, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ThrowExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitThrowExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThrowExpression(this);
	}

	public ThrowExpressionSyntax Update(SyntaxToken throwKeyword, ExpressionSyntax expression)
	{
		if (throwKeyword != ThrowKeyword || expression != Expression)
		{
			ThrowExpressionSyntax throwExpressionSyntax = SyntaxFactory.ThrowExpression(throwKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				throwExpressionSyntax = throwExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				throwExpressionSyntax = throwExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return throwExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ThrowExpressionSyntax(Kind, throwKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ThrowExpressionSyntax(Kind, throwKeyword, expression, GetDiagnostics(), annotations);
	}

	internal ThrowExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			throwKeyword = syntaxToken;
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
		writer.WriteValue(throwKeyword);
		writer.WriteValue(expression);
	}

	static ThrowExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ThrowExpressionSyntax), (ObjectReader r) => new ThrowExpressionSyntax(r));
	}
}
