using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class YieldStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken yieldKeyword;

	internal readonly SyntaxToken returnOrBreakKeyword;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken YieldKeyword => yieldKeyword;

	public SyntaxToken ReturnOrBreakKeyword => returnOrBreakKeyword;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal YieldStatementSyntax(SyntaxKind kind, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(yieldKeyword);
		this.yieldKeyword = yieldKeyword;
		AdjustFlagsAndWidth(returnOrBreakKeyword);
		this.returnOrBreakKeyword = returnOrBreakKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal YieldStatementSyntax(SyntaxKind kind, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(yieldKeyword);
		this.yieldKeyword = yieldKeyword;
		AdjustFlagsAndWidth(returnOrBreakKeyword);
		this.returnOrBreakKeyword = returnOrBreakKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal YieldStatementSyntax(SyntaxKind kind, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(yieldKeyword);
		this.yieldKeyword = yieldKeyword;
		AdjustFlagsAndWidth(returnOrBreakKeyword);
		this.returnOrBreakKeyword = returnOrBreakKeyword;
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
			0 => (GreenNode)yieldKeyword, 
			1 => returnOrBreakKeyword, 
			2 => expression, 
			3 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitYieldStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitYieldStatement(this);
	}

	public YieldStatementSyntax Update(SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (yieldKeyword != YieldKeyword || returnOrBreakKeyword != ReturnOrBreakKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			YieldStatementSyntax yieldStatementSyntax = SyntaxFactory.YieldStatement(Kind, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				yieldStatementSyntax = yieldStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				yieldStatementSyntax = yieldStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return yieldStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new YieldStatementSyntax(Kind, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new YieldStatementSyntax(Kind, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken, GetDiagnostics(), annotations);
	}

	internal YieldStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			yieldKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			returnOrBreakKeyword = syntaxToken2;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			semicolonToken = syntaxToken3;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(yieldKeyword);
		writer.WriteValue(returnOrBreakKeyword);
		writer.WriteValue(expression);
		writer.WriteValue(semicolonToken);
	}

	static YieldStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(YieldStatementSyntax), (ObjectReader r) => new YieldStatementSyntax(r));
	}
}
