using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LetClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken letKeyword;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken equalsToken;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken LetKeyword => letKeyword;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken EqualsToken => equalsToken;

	public ExpressionSyntax Expression => expression;

	internal LetClauseSyntax(SyntaxKind kind, SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(letKeyword);
		this.letKeyword = letKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal LetClauseSyntax(SyntaxKind kind, SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(letKeyword);
		this.letKeyword = letKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal LetClauseSyntax(SyntaxKind kind, SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(letKeyword);
		this.letKeyword = letKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)letKeyword, 
			1 => identifier, 
			2 => equalsToken, 
			3 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LetClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLetClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLetClause(this);
	}

	public LetClauseSyntax Update(SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression)
	{
		if (letKeyword != LetKeyword || identifier != Identifier || equalsToken != EqualsToken || expression != Expression)
		{
			LetClauseSyntax letClauseSyntax = SyntaxFactory.LetClause(letKeyword, identifier, equalsToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				letClauseSyntax = letClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				letClauseSyntax = letClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return letClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new LetClauseSyntax(Kind, letKeyword, identifier, equalsToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new LetClauseSyntax(Kind, letKeyword, identifier, equalsToken, expression, GetDiagnostics(), annotations);
	}

	internal LetClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			letKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			identifier = syntaxToken2;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			equalsToken = syntaxToken3;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(letKeyword);
		writer.WriteValue(identifier);
		writer.WriteValue(equalsToken);
		writer.WriteValue(expression);
	}

	static LetClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(LetClauseSyntax), (ObjectReader r) => new LetClauseSyntax(r));
	}
}
