using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
	internal readonly GreenNode tokens;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Tokens => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(tokens);

	internal SkippedTokensTriviaSyntax(SyntaxKind kind, GreenNode tokens, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (tokens != null)
		{
			AdjustFlagsAndWidth(tokens);
			this.tokens = tokens;
		}
	}

	internal SkippedTokensTriviaSyntax(SyntaxKind kind, GreenNode tokens, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		if (tokens != null)
		{
			AdjustFlagsAndWidth(tokens);
			this.tokens = tokens;
		}
	}

	internal SkippedTokensTriviaSyntax(SyntaxKind kind, GreenNode tokens)
		: base(kind)
	{
		SlotCount = 1;
		if (tokens != null)
		{
			AdjustFlagsAndWidth(tokens);
			this.tokens = tokens;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return tokens;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SkippedTokensTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSkippedTokensTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSkippedTokensTrivia(this);
	}

	public SkippedTokensTriviaSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> tokens)
	{
		if (tokens != Tokens)
		{
			SkippedTokensTriviaSyntax skippedTokensTriviaSyntax = SyntaxFactory.SkippedTokensTrivia(tokens);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				skippedTokensTriviaSyntax = skippedTokensTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				skippedTokensTriviaSyntax = skippedTokensTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return skippedTokensTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new SkippedTokensTriviaSyntax(Kind, tokens, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new SkippedTokensTriviaSyntax(Kind, tokens, GetDiagnostics(), annotations);
	}

	internal SkippedTokensTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			tokens = greenNode;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(tokens);
	}

	static SkippedTokensTriviaSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(SkippedTokensTriviaSyntax), (ObjectReader r) => new SkippedTokensTriviaSyntax(r));
	}
}
