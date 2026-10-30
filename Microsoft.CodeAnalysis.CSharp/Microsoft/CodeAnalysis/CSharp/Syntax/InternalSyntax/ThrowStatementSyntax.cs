using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ThrowStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken throwKeyword;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken ThrowKeyword => throwKeyword;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal ThrowStatementSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ThrowStatementSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ThrowStatementSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)throwKeyword, 
			1 => expression, 
			2 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitThrowStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThrowStatement(this);
	}

	public ThrowStatementSyntax Update(SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (throwKeyword != ThrowKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			ThrowStatementSyntax throwStatementSyntax = SyntaxFactory.ThrowStatement(throwKeyword, expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				throwStatementSyntax = throwStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				throwStatementSyntax = throwStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return throwStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ThrowStatementSyntax(Kind, throwKeyword, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ThrowStatementSyntax(Kind, throwKeyword, expression, semicolonToken, GetDiagnostics(), annotations);
	}

	internal ThrowStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
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
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			semicolonToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(throwKeyword);
		writer.WriteValue(expression);
		writer.WriteValue(semicolonToken);
	}

	static ThrowStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ThrowStatementSyntax), (ObjectReader r) => new ThrowStatementSyntax(r));
	}
}
