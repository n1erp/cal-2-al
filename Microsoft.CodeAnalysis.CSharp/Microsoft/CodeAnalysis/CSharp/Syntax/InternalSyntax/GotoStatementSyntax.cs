using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class GotoStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken gotoKeyword;

	internal readonly SyntaxToken caseOrDefaultKeyword;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken GotoKeyword => gotoKeyword;

	public SyntaxToken CaseOrDefaultKeyword => caseOrDefaultKeyword;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal GotoStatementSyntax(SyntaxKind kind, SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(gotoKeyword);
		this.gotoKeyword = gotoKeyword;
		if (caseOrDefaultKeyword != null)
		{
			AdjustFlagsAndWidth(caseOrDefaultKeyword);
			this.caseOrDefaultKeyword = caseOrDefaultKeyword;
		}
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal GotoStatementSyntax(SyntaxKind kind, SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(gotoKeyword);
		this.gotoKeyword = gotoKeyword;
		if (caseOrDefaultKeyword != null)
		{
			AdjustFlagsAndWidth(caseOrDefaultKeyword);
			this.caseOrDefaultKeyword = caseOrDefaultKeyword;
		}
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal GotoStatementSyntax(SyntaxKind kind, SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(gotoKeyword);
		this.gotoKeyword = gotoKeyword;
		if (caseOrDefaultKeyword != null)
		{
			AdjustFlagsAndWidth(caseOrDefaultKeyword);
			this.caseOrDefaultKeyword = caseOrDefaultKeyword;
		}
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
			0 => (GreenNode)gotoKeyword, 
			1 => caseOrDefaultKeyword, 
			2 => expression, 
			3 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGotoStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitGotoStatement(this);
	}

	public GotoStatementSyntax Update(SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (gotoKeyword != GotoKeyword || caseOrDefaultKeyword != CaseOrDefaultKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			GotoStatementSyntax gotoStatementSyntax = SyntaxFactory.GotoStatement(Kind, gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				gotoStatementSyntax = gotoStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				gotoStatementSyntax = gotoStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return gotoStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new GotoStatementSyntax(Kind, gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new GotoStatementSyntax(Kind, gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken, GetDiagnostics(), annotations);
	}

	internal GotoStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			gotoKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			caseOrDefaultKeyword = syntaxToken2;
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
		writer.WriteValue(gotoKeyword);
		writer.WriteValue(caseOrDefaultKeyword);
		writer.WriteValue(expression);
		writer.WriteValue(semicolonToken);
	}

	static GotoStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(GotoStatementSyntax), (ObjectReader r) => new GotoStatementSyntax(r));
	}
}
