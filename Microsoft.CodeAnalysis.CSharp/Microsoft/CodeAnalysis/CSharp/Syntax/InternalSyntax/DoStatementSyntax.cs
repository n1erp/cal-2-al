using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DoStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken doKeyword;

	internal readonly StatementSyntax statement;

	internal readonly SyntaxToken whileKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken closeParenToken;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken DoKeyword => doKeyword;

	public StatementSyntax Statement => statement;

	public SyntaxToken WhileKeyword => whileKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Condition => condition;

	public SyntaxToken CloseParenToken => closeParenToken;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal DoStatementSyntax(SyntaxKind kind, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(doKeyword);
		this.doKeyword = doKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal DoStatementSyntax(SyntaxKind kind, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 7;
		AdjustFlagsAndWidth(doKeyword);
		this.doKeyword = doKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal DoStatementSyntax(SyntaxKind kind, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(doKeyword);
		this.doKeyword = doKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)doKeyword, 
			1 => statement, 
			2 => whileKeyword, 
			3 => openParenToken, 
			4 => condition, 
			5 => closeParenToken, 
			6 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDoStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDoStatement(this);
	}

	public DoStatementSyntax Update(SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
	{
		if (doKeyword != DoKeyword || statement != Statement || whileKeyword != WhileKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || semicolonToken != SemicolonToken)
		{
			DoStatementSyntax doStatementSyntax = SyntaxFactory.DoStatement(doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				doStatementSyntax = doStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				doStatementSyntax = doStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return doStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new DoStatementSyntax(Kind, doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new DoStatementSyntax(Kind, doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken, GetDiagnostics(), annotations);
	}

	internal DoStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 7;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			doKeyword = syntaxToken;
		}
		StatementSyntax statementSyntax = (StatementSyntax)reader.ReadValue();
		if (statementSyntax != null)
		{
			AdjustFlagsAndWidth(statementSyntax);
			statement = statementSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			whileKeyword = syntaxToken2;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			openParenToken = syntaxToken3;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			condition = expressionSyntax;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			closeParenToken = syntaxToken4;
		}
		SyntaxToken syntaxToken5 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken5 != null)
		{
			AdjustFlagsAndWidth(syntaxToken5);
			semicolonToken = syntaxToken5;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(doKeyword);
		writer.WriteValue(statement);
		writer.WriteValue(whileKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(condition);
		writer.WriteValue(closeParenToken);
		writer.WriteValue(semicolonToken);
	}

	static DoStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(DoStatementSyntax), (ObjectReader r) => new DoStatementSyntax(r));
	}
}
