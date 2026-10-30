using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class UsingStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken usingKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly VariableDeclarationSyntax declaration;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public SyntaxToken UsingKeyword => usingKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public VariableDeclarationSyntax Declaration => declaration;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken CloseParenToken => closeParenToken;

	public StatementSyntax Statement => statement;

	internal UsingStatementSyntax(SyntaxKind kind, SyntaxToken usingKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal UsingStatementSyntax(SyntaxKind kind, SyntaxToken usingKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 6;
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal UsingStatementSyntax(SyntaxKind kind, SyntaxToken usingKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)usingKeyword, 
			1 => openParenToken, 
			2 => declaration, 
			3 => expression, 
			4 => closeParenToken, 
			5 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUsingStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUsingStatement(this);
	}

	public UsingStatementSyntax Update(SyntaxToken usingKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (usingKeyword != UsingKeyword || openParenToken != OpenParenToken || declaration != Declaration || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			UsingStatementSyntax usingStatementSyntax = SyntaxFactory.UsingStatement(usingKeyword, openParenToken, declaration, expression, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				usingStatementSyntax = usingStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				usingStatementSyntax = usingStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return usingStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new UsingStatementSyntax(Kind, usingKeyword, openParenToken, declaration, expression, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new UsingStatementSyntax(Kind, usingKeyword, openParenToken, declaration, expression, closeParenToken, statement, GetDiagnostics(), annotations);
	}

	internal UsingStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 6;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			usingKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			openParenToken = syntaxToken2;
		}
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)reader.ReadValue();
		if (variableDeclarationSyntax != null)
		{
			AdjustFlagsAndWidth(variableDeclarationSyntax);
			declaration = variableDeclarationSyntax;
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
			closeParenToken = syntaxToken3;
		}
		StatementSyntax statementSyntax = (StatementSyntax)reader.ReadValue();
		if (statementSyntax != null)
		{
			AdjustFlagsAndWidth(statementSyntax);
			statement = statementSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(usingKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(declaration);
		writer.WriteValue(expression);
		writer.WriteValue(closeParenToken);
		writer.WriteValue(statement);
	}

	static UsingStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(UsingStatementSyntax), (ObjectReader r) => new UsingStatementSyntax(r));
	}
}
