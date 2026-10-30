using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class JoinClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken joinKeyword;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken inKeyword;

	internal readonly ExpressionSyntax inExpression;

	internal readonly SyntaxToken onKeyword;

	internal readonly ExpressionSyntax leftExpression;

	internal readonly SyntaxToken equalsKeyword;

	internal readonly ExpressionSyntax rightExpression;

	internal readonly JoinIntoClauseSyntax into;

	public SyntaxToken JoinKeyword => joinKeyword;

	public TypeSyntax Type => type;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken InKeyword => inKeyword;

	public ExpressionSyntax InExpression => inExpression;

	public SyntaxToken OnKeyword => onKeyword;

	public ExpressionSyntax LeftExpression => leftExpression;

	public SyntaxToken EqualsKeyword => equalsKeyword;

	public ExpressionSyntax RightExpression => rightExpression;

	public JoinIntoClauseSyntax Into => into;

	internal JoinClauseSyntax(SyntaxKind kind, SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(joinKeyword);
		this.joinKeyword = joinKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(inExpression);
		this.inExpression = inExpression;
		AdjustFlagsAndWidth(onKeyword);
		this.onKeyword = onKeyword;
		AdjustFlagsAndWidth(leftExpression);
		this.leftExpression = leftExpression;
		AdjustFlagsAndWidth(equalsKeyword);
		this.equalsKeyword = equalsKeyword;
		AdjustFlagsAndWidth(rightExpression);
		this.rightExpression = rightExpression;
		if (into != null)
		{
			AdjustFlagsAndWidth(into);
			this.into = into;
		}
	}

	internal JoinClauseSyntax(SyntaxKind kind, SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 10;
		AdjustFlagsAndWidth(joinKeyword);
		this.joinKeyword = joinKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(inExpression);
		this.inExpression = inExpression;
		AdjustFlagsAndWidth(onKeyword);
		this.onKeyword = onKeyword;
		AdjustFlagsAndWidth(leftExpression);
		this.leftExpression = leftExpression;
		AdjustFlagsAndWidth(equalsKeyword);
		this.equalsKeyword = equalsKeyword;
		AdjustFlagsAndWidth(rightExpression);
		this.rightExpression = rightExpression;
		if (into != null)
		{
			AdjustFlagsAndWidth(into);
			this.into = into;
		}
	}

	internal JoinClauseSyntax(SyntaxKind kind, SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into)
		: base(kind)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(joinKeyword);
		this.joinKeyword = joinKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(inExpression);
		this.inExpression = inExpression;
		AdjustFlagsAndWidth(onKeyword);
		this.onKeyword = onKeyword;
		AdjustFlagsAndWidth(leftExpression);
		this.leftExpression = leftExpression;
		AdjustFlagsAndWidth(equalsKeyword);
		this.equalsKeyword = equalsKeyword;
		AdjustFlagsAndWidth(rightExpression);
		this.rightExpression = rightExpression;
		if (into != null)
		{
			AdjustFlagsAndWidth(into);
			this.into = into;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)joinKeyword, 
			1 => type, 
			2 => identifier, 
			3 => inKeyword, 
			4 => inExpression, 
			5 => onKeyword, 
			6 => leftExpression, 
			7 => equalsKeyword, 
			8 => rightExpression, 
			9 => into, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.JoinClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitJoinClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitJoinClause(this);
	}

	public JoinClauseSyntax Update(SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into)
	{
		if (joinKeyword != JoinKeyword || type != Type || identifier != Identifier || inKeyword != InKeyword || inExpression != InExpression || onKeyword != OnKeyword || leftExpression != LeftExpression || equalsKeyword != EqualsKeyword || rightExpression != RightExpression || into != Into)
		{
			JoinClauseSyntax joinClauseSyntax = SyntaxFactory.JoinClause(joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				joinClauseSyntax = joinClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				joinClauseSyntax = joinClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return joinClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new JoinClauseSyntax(Kind, joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new JoinClauseSyntax(Kind, joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into, GetDiagnostics(), annotations);
	}

	internal JoinClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 10;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			joinKeyword = syntaxToken;
		}
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
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
			inKeyword = syntaxToken3;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			inExpression = expressionSyntax;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			onKeyword = syntaxToken4;
		}
		ExpressionSyntax expressionSyntax2 = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax2 != null)
		{
			AdjustFlagsAndWidth(expressionSyntax2);
			leftExpression = expressionSyntax2;
		}
		SyntaxToken syntaxToken5 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken5 != null)
		{
			AdjustFlagsAndWidth(syntaxToken5);
			equalsKeyword = syntaxToken5;
		}
		ExpressionSyntax expressionSyntax3 = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax3 != null)
		{
			AdjustFlagsAndWidth(expressionSyntax3);
			rightExpression = expressionSyntax3;
		}
		JoinIntoClauseSyntax joinIntoClauseSyntax = (JoinIntoClauseSyntax)reader.ReadValue();
		if (joinIntoClauseSyntax != null)
		{
			AdjustFlagsAndWidth(joinIntoClauseSyntax);
			into = joinIntoClauseSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(joinKeyword);
		writer.WriteValue(type);
		writer.WriteValue(identifier);
		writer.WriteValue(inKeyword);
		writer.WriteValue(inExpression);
		writer.WriteValue(onKeyword);
		writer.WriteValue(leftExpression);
		writer.WriteValue(equalsKeyword);
		writer.WriteValue(rightExpression);
		writer.WriteValue(into);
	}

	static JoinClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(JoinClauseSyntax), (ObjectReader r) => new JoinClauseSyntax(r));
	}
}
