using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ForStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken forKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly VariableDeclarationSyntax declaration;

	internal readonly GreenNode initializers;

	internal readonly SyntaxToken firstSemicolonToken;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken secondSemicolonToken;

	internal readonly GreenNode incrementors;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public SyntaxToken ForKeyword => forKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public VariableDeclarationSyntax Declaration => declaration;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> Initializers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(initializers));

	public SyntaxToken FirstSemicolonToken => firstSemicolonToken;

	public ExpressionSyntax Condition => condition;

	public SyntaxToken SecondSemicolonToken => secondSemicolonToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> Incrementors => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(incrementors));

	public SyntaxToken CloseParenToken => closeParenToken;

	public StatementSyntax Statement => statement;

	internal ForStatementSyntax(SyntaxKind kind, SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, GreenNode initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, GreenNode incrementors, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(forKeyword);
		this.forKeyword = forKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (initializers != null)
		{
			AdjustFlagsAndWidth(initializers);
			this.initializers = initializers;
		}
		AdjustFlagsAndWidth(firstSemicolonToken);
		this.firstSemicolonToken = firstSemicolonToken;
		if (condition != null)
		{
			AdjustFlagsAndWidth(condition);
			this.condition = condition;
		}
		AdjustFlagsAndWidth(secondSemicolonToken);
		this.secondSemicolonToken = secondSemicolonToken;
		if (incrementors != null)
		{
			AdjustFlagsAndWidth(incrementors);
			this.incrementors = incrementors;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForStatementSyntax(SyntaxKind kind, SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, GreenNode initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, GreenNode incrementors, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 10;
		AdjustFlagsAndWidth(forKeyword);
		this.forKeyword = forKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (initializers != null)
		{
			AdjustFlagsAndWidth(initializers);
			this.initializers = initializers;
		}
		AdjustFlagsAndWidth(firstSemicolonToken);
		this.firstSemicolonToken = firstSemicolonToken;
		if (condition != null)
		{
			AdjustFlagsAndWidth(condition);
			this.condition = condition;
		}
		AdjustFlagsAndWidth(secondSemicolonToken);
		this.secondSemicolonToken = secondSemicolonToken;
		if (incrementors != null)
		{
			AdjustFlagsAndWidth(incrementors);
			this.incrementors = incrementors;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForStatementSyntax(SyntaxKind kind, SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, GreenNode initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, GreenNode incrementors, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(forKeyword);
		this.forKeyword = forKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (initializers != null)
		{
			AdjustFlagsAndWidth(initializers);
			this.initializers = initializers;
		}
		AdjustFlagsAndWidth(firstSemicolonToken);
		this.firstSemicolonToken = firstSemicolonToken;
		if (condition != null)
		{
			AdjustFlagsAndWidth(condition);
			this.condition = condition;
		}
		AdjustFlagsAndWidth(secondSemicolonToken);
		this.secondSemicolonToken = secondSemicolonToken;
		if (incrementors != null)
		{
			AdjustFlagsAndWidth(incrementors);
			this.incrementors = incrementors;
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
			0 => forKeyword, 
			1 => openParenToken, 
			2 => declaration, 
			3 => initializers, 
			4 => firstSemicolonToken, 
			5 => condition, 
			6 => secondSemicolonToken, 
			7 => incrementors, 
			8 => closeParenToken, 
			9 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitForStatement(this);
	}

	public ForStatementSyntax Update(SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> incrementors, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (forKeyword != ForKeyword || openParenToken != OpenParenToken || declaration != Declaration || initializers != Initializers || firstSemicolonToken != FirstSemicolonToken || condition != Condition || secondSemicolonToken != SecondSemicolonToken || incrementors != Incrementors || closeParenToken != CloseParenToken || statement != Statement)
		{
			ForStatementSyntax forStatementSyntax = SyntaxFactory.ForStatement(forKeyword, openParenToken, declaration, initializers, firstSemicolonToken, condition, secondSemicolonToken, incrementors, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				forStatementSyntax = forStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				forStatementSyntax = forStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return forStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ForStatementSyntax(Kind, forKeyword, openParenToken, declaration, initializers, firstSemicolonToken, condition, secondSemicolonToken, incrementors, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ForStatementSyntax(Kind, forKeyword, openParenToken, declaration, initializers, firstSemicolonToken, condition, secondSemicolonToken, incrementors, closeParenToken, statement, GetDiagnostics(), annotations);
	}

	internal ForStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 10;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			forKeyword = syntaxToken;
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
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			initializers = greenNode;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			firstSemicolonToken = syntaxToken3;
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
			secondSemicolonToken = syntaxToken4;
		}
		GreenNode greenNode2 = (GreenNode)reader.ReadValue();
		if (greenNode2 != null)
		{
			AdjustFlagsAndWidth(greenNode2);
			incrementors = greenNode2;
		}
		SyntaxToken syntaxToken5 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken5 != null)
		{
			AdjustFlagsAndWidth(syntaxToken5);
			closeParenToken = syntaxToken5;
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
		writer.WriteValue(forKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(declaration);
		writer.WriteValue(initializers);
		writer.WriteValue(firstSemicolonToken);
		writer.WriteValue(condition);
		writer.WriteValue(secondSemicolonToken);
		writer.WriteValue(incrementors);
		writer.WriteValue(closeParenToken);
		writer.WriteValue(statement);
	}

	static ForStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ForStatementSyntax), (ObjectReader r) => new ForStatementSyntax(r));
	}
}
