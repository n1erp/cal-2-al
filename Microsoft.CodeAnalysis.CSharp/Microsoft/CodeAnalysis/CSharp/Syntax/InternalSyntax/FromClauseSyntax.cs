using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FromClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken fromKeyword;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken inKeyword;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken FromKeyword => fromKeyword;

	public TypeSyntax Type => type;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken InKeyword => inKeyword;

	public ExpressionSyntax Expression => expression;

	internal FromClauseSyntax(SyntaxKind kind, SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(fromKeyword);
		this.fromKeyword = fromKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal FromClauseSyntax(SyntaxKind kind, SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 5;
		AdjustFlagsAndWidth(fromKeyword);
		this.fromKeyword = fromKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal FromClauseSyntax(SyntaxKind kind, SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(fromKeyword);
		this.fromKeyword = fromKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)fromKeyword, 
			1 => type, 
			2 => identifier, 
			3 => inKeyword, 
			4 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFromClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFromClause(this);
	}

	public FromClauseSyntax Update(SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression)
	{
		if (fromKeyword != FromKeyword || type != Type || identifier != Identifier || inKeyword != InKeyword || expression != Expression)
		{
			FromClauseSyntax fromClauseSyntax = SyntaxFactory.FromClause(fromKeyword, type, identifier, inKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				fromClauseSyntax = fromClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				fromClauseSyntax = fromClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return fromClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new FromClauseSyntax(Kind, fromKeyword, type, identifier, inKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new FromClauseSyntax(Kind, fromKeyword, type, identifier, inKeyword, expression, GetDiagnostics(), annotations);
	}

	internal FromClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 5;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			fromKeyword = syntaxToken;
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
			expression = expressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(fromKeyword);
		writer.WriteValue(type);
		writer.WriteValue(identifier);
		writer.WriteValue(inKeyword);
		writer.WriteValue(expression);
	}

	static FromClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(FromClauseSyntax), (ObjectReader r) => new FromClauseSyntax(r));
	}
}
