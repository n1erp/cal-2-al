using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ArgumentSyntax : CSharpSyntaxNode
{
	internal readonly NameColonSyntax nameColon;

	internal readonly SyntaxToken refKindKeyword;

	internal readonly ExpressionSyntax expression;

	public NameColonSyntax NameColon => nameColon;

	public SyntaxToken RefKindKeyword => refKindKeyword;

	public ExpressionSyntax Expression => expression;

	internal ArgumentSyntax(SyntaxKind kind, NameColonSyntax nameColon, SyntaxToken refKindKeyword, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		if (nameColon != null)
		{
			AdjustFlagsAndWidth(nameColon);
			this.nameColon = nameColon;
		}
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ArgumentSyntax(SyntaxKind kind, NameColonSyntax nameColon, SyntaxToken refKindKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		if (nameColon != null)
		{
			AdjustFlagsAndWidth(nameColon);
			this.nameColon = nameColon;
		}
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ArgumentSyntax(SyntaxKind kind, NameColonSyntax nameColon, SyntaxToken refKindKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 3;
		if (nameColon != null)
		{
			AdjustFlagsAndWidth(nameColon);
			this.nameColon = nameColon;
		}
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)nameColon, 
			1 => refKindKeyword, 
			2 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArgument(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArgument(this);
	}

	public ArgumentSyntax Update(NameColonSyntax nameColon, SyntaxToken refKindKeyword, ExpressionSyntax expression)
	{
		if (nameColon != NameColon || refKindKeyword != RefKindKeyword || expression != Expression)
		{
			ArgumentSyntax argumentSyntax = SyntaxFactory.Argument(nameColon, refKindKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				argumentSyntax = argumentSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				argumentSyntax = argumentSyntax.WithAnnotationsGreen(annotations);
			}
			return argumentSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ArgumentSyntax(Kind, nameColon, refKindKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ArgumentSyntax(Kind, nameColon, refKindKeyword, expression, GetDiagnostics(), annotations);
	}

	internal ArgumentSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		NameColonSyntax nameColonSyntax = (NameColonSyntax)reader.ReadValue();
		if (nameColonSyntax != null)
		{
			AdjustFlagsAndWidth(nameColonSyntax);
			nameColon = nameColonSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			refKindKeyword = syntaxToken;
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
		writer.WriteValue(nameColon);
		writer.WriteValue(refKindKeyword);
		writer.WriteValue(expression);
	}

	static ArgumentSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ArgumentSyntax), (ObjectReader r) => new ArgumentSyntax(r));
	}
}
