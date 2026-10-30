using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DefaultExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken Keyword => keyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public TypeSyntax Type => type;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal DefaultExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal DefaultExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal DefaultExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)keyword, 
			1 => openParenToken, 
			2 => type, 
			3 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DefaultExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDefaultExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDefaultExpression(this);
	}

	public DefaultExpressionSyntax Update(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		if (keyword != Keyword || openParenToken != OpenParenToken || type != Type || closeParenToken != CloseParenToken)
		{
			DefaultExpressionSyntax defaultExpressionSyntax = SyntaxFactory.DefaultExpression(keyword, openParenToken, type, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				defaultExpressionSyntax = defaultExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				defaultExpressionSyntax = defaultExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return defaultExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new DefaultExpressionSyntax(Kind, keyword, openParenToken, type, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new DefaultExpressionSyntax(Kind, keyword, openParenToken, type, closeParenToken, GetDiagnostics(), annotations);
	}

	internal DefaultExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			keyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			openParenToken = syntaxToken2;
		}
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
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
		writer.WriteValue(keyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(type);
		writer.WriteValue(closeParenToken);
	}

	static DefaultExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(DefaultExpressionSyntax), (ObjectReader r) => new DefaultExpressionSyntax(r));
	}
}
