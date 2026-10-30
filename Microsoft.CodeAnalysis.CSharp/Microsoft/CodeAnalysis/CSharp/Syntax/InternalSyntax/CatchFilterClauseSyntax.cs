using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CatchFilterClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken whenKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax filterExpression;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken WhenKeyword => whenKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax FilterExpression => filterExpression;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal CatchFilterClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(filterExpression);
		this.filterExpression = filterExpression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CatchFilterClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(filterExpression);
		this.filterExpression = filterExpression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CatchFilterClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(filterExpression);
		this.filterExpression = filterExpression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)whenKeyword, 
			1 => openParenToken, 
			2 => filterExpression, 
			3 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCatchFilterClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCatchFilterClause(this);
	}

	public CatchFilterClauseSyntax Update(SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken)
	{
		if (whenKeyword != WhenKeyword || openParenToken != OpenParenToken || filterExpression != FilterExpression || closeParenToken != CloseParenToken)
		{
			CatchFilterClauseSyntax catchFilterClauseSyntax = SyntaxFactory.CatchFilterClause(whenKeyword, openParenToken, filterExpression, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				catchFilterClauseSyntax = catchFilterClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				catchFilterClauseSyntax = catchFilterClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return catchFilterClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CatchFilterClauseSyntax(Kind, whenKeyword, openParenToken, filterExpression, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CatchFilterClauseSyntax(Kind, whenKeyword, openParenToken, filterExpression, closeParenToken, GetDiagnostics(), annotations);
	}

	internal CatchFilterClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			whenKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			openParenToken = syntaxToken2;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			filterExpression = expressionSyntax;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			closeParenToken = syntaxToken3;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(whenKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(filterExpression);
		writer.WriteValue(closeParenToken);
	}

	static CatchFilterClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CatchFilterClauseSyntax), (ObjectReader r) => new CatchFilterClauseSyntax(r));
	}
}
