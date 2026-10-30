using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ReturnStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken returnKeyword;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken ReturnKeyword => returnKeyword;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal ReturnStatementSyntax(SyntaxKind kind, SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(returnKeyword);
		this.returnKeyword = returnKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ReturnStatementSyntax(SyntaxKind kind, SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(returnKeyword);
		this.returnKeyword = returnKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ReturnStatementSyntax(SyntaxKind kind, SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(returnKeyword);
		this.returnKeyword = returnKeyword;
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
			0 => (GreenNode)returnKeyword, 
			1 => expression, 
			2 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReturnStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitReturnStatement(this);
	}

	public ReturnStatementSyntax Update(SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (returnKeyword != ReturnKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			ReturnStatementSyntax returnStatementSyntax = SyntaxFactory.ReturnStatement(returnKeyword, expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				returnStatementSyntax = returnStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				returnStatementSyntax = returnStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return returnStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ReturnStatementSyntax(Kind, returnKeyword, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ReturnStatementSyntax(Kind, returnKeyword, expression, semicolonToken, GetDiagnostics(), annotations);
	}

	internal ReturnStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			returnKeyword = syntaxToken;
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
		writer.WriteValue(returnKeyword);
		writer.WriteValue(expression);
		writer.WriteValue(semicolonToken);
	}

	static ReturnStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ReturnStatementSyntax), (ObjectReader r) => new ReturnStatementSyntax(r));
	}
}
