using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IfStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken ifKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	internal readonly ElseClauseSyntax @else;

	public SyntaxToken IfKeyword => ifKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Condition => condition;

	public SyntaxToken CloseParenToken => closeParenToken;

	public StatementSyntax Statement => statement;

	public ElseClauseSyntax Else => @else;

	internal IfStatementSyntax(SyntaxKind kind, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		if (@else != null)
		{
			AdjustFlagsAndWidth(@else);
			this.@else = @else;
		}
	}

	internal IfStatementSyntax(SyntaxKind kind, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 6;
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		if (@else != null)
		{
			AdjustFlagsAndWidth(@else);
			this.@else = @else;
		}
	}

	internal IfStatementSyntax(SyntaxKind kind, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		if (@else != null)
		{
			AdjustFlagsAndWidth(@else);
			this.@else = @else;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)ifKeyword, 
			1 => openParenToken, 
			2 => condition, 
			3 => closeParenToken, 
			4 => statement, 
			5 => @else, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIfStatement(this);
	}

	public IfStatementSyntax Update(SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
	{
		if (ifKeyword != IfKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || statement != Statement || @else != Else)
		{
			IfStatementSyntax ifStatementSyntax = SyntaxFactory.IfStatement(ifKeyword, openParenToken, condition, closeParenToken, statement, @else);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				ifStatementSyntax = ifStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				ifStatementSyntax = ifStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return ifStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new IfStatementSyntax(Kind, ifKeyword, openParenToken, condition, closeParenToken, statement, @else, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new IfStatementSyntax(Kind, ifKeyword, openParenToken, condition, closeParenToken, statement, @else, GetDiagnostics(), annotations);
	}

	internal IfStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 6;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			ifKeyword = syntaxToken;
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
			condition = expressionSyntax;
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
		ElseClauseSyntax elseClauseSyntax = (ElseClauseSyntax)reader.ReadValue();
		if (elseClauseSyntax != null)
		{
			AdjustFlagsAndWidth(elseClauseSyntax);
			@else = elseClauseSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(ifKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(condition);
		writer.WriteValue(closeParenToken);
		writer.WriteValue(statement);
		writer.WriteValue(@else);
	}

	static IfStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(IfStatementSyntax), (ObjectReader r) => new IfStatementSyntax(r));
	}
}
