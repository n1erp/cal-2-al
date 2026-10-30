using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ForEachVariableStatementSyntax : CommonForEachStatementSyntax
{
	internal readonly SyntaxToken forEachKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax variable;

	internal readonly SyntaxToken inKeyword;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public override SyntaxToken ForEachKeyword => forEachKeyword;

	public override SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Variable => variable;

	public override SyntaxToken InKeyword => inKeyword;

	public override ExpressionSyntax Expression => expression;

	public override SyntaxToken CloseParenToken => closeParenToken;

	public override StatementSyntax Statement => statement;

	internal ForEachVariableStatementSyntax(SyntaxKind kind, SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(forEachKeyword);
		this.forEachKeyword = forEachKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(variable);
		this.variable = variable;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForEachVariableStatementSyntax(SyntaxKind kind, SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 7;
		AdjustFlagsAndWidth(forEachKeyword);
		this.forEachKeyword = forEachKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(variable);
		this.variable = variable;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForEachVariableStatementSyntax(SyntaxKind kind, SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(forEachKeyword);
		this.forEachKeyword = forEachKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(variable);
		this.variable = variable;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)forEachKeyword, 
			1 => openParenToken, 
			2 => variable, 
			3 => inKeyword, 
			4 => expression, 
			5 => closeParenToken, 
			6 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForEachVariableStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitForEachVariableStatement(this);
	}

	public ForEachVariableStatementSyntax Update(SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (forEachKeyword != ForEachKeyword || openParenToken != OpenParenToken || variable != Variable || inKeyword != InKeyword || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			ForEachVariableStatementSyntax forEachVariableStatementSyntax = SyntaxFactory.ForEachVariableStatement(forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				forEachVariableStatementSyntax = forEachVariableStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				forEachVariableStatementSyntax = forEachVariableStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return forEachVariableStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ForEachVariableStatementSyntax(Kind, forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ForEachVariableStatementSyntax(Kind, forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement, GetDiagnostics(), annotations);
	}

	internal ForEachVariableStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 7;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			forEachKeyword = syntaxToken;
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
			variable = expressionSyntax;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			inKeyword = syntaxToken3;
		}
		ExpressionSyntax expressionSyntax2 = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax2 != null)
		{
			AdjustFlagsAndWidth(expressionSyntax2);
			expression = expressionSyntax2;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			closeParenToken = syntaxToken4;
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
		writer.WriteValue(forEachKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(variable);
		writer.WriteValue(inKeyword);
		writer.WriteValue(expression);
		writer.WriteValue(closeParenToken);
		writer.WriteValue(statement);
	}

	static ForEachVariableStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ForEachVariableStatementSyntax), (ObjectReader r) => new ForEachVariableStatementSyntax(r));
	}
}
