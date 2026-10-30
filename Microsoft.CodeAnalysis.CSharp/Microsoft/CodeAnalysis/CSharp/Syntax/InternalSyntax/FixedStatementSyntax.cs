using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FixedStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken fixedKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly VariableDeclarationSyntax declaration;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public SyntaxToken FixedKeyword => fixedKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public VariableDeclarationSyntax Declaration => declaration;

	public SyntaxToken CloseParenToken => closeParenToken;

	public StatementSyntax Statement => statement;

	internal FixedStatementSyntax(SyntaxKind kind, SyntaxToken fixedKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(fixedKeyword);
		this.fixedKeyword = fixedKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal FixedStatementSyntax(SyntaxKind kind, SyntaxToken fixedKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 5;
		AdjustFlagsAndWidth(fixedKeyword);
		this.fixedKeyword = fixedKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal FixedStatementSyntax(SyntaxKind kind, SyntaxToken fixedKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(fixedKeyword);
		this.fixedKeyword = fixedKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)fixedKeyword, 
			1 => openParenToken, 
			2 => declaration, 
			3 => closeParenToken, 
			4 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FixedStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFixedStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFixedStatement(this);
	}

	public FixedStatementSyntax Update(SyntaxToken fixedKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (fixedKeyword != FixedKeyword || openParenToken != OpenParenToken || declaration != Declaration || closeParenToken != CloseParenToken || statement != Statement)
		{
			FixedStatementSyntax fixedStatementSyntax = SyntaxFactory.FixedStatement(fixedKeyword, openParenToken, declaration, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				fixedStatementSyntax = fixedStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				fixedStatementSyntax = fixedStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return fixedStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new FixedStatementSyntax(Kind, fixedKeyword, openParenToken, declaration, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new FixedStatementSyntax(Kind, fixedKeyword, openParenToken, declaration, closeParenToken, statement, GetDiagnostics(), annotations);
	}

	internal FixedStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 5;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			fixedKeyword = syntaxToken;
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
		writer.WriteValue(fixedKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(declaration);
		writer.WriteValue(closeParenToken);
		writer.WriteValue(statement);
	}

	static FixedStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(FixedStatementSyntax), (ObjectReader r) => new FixedStatementSyntax(r));
	}
}
