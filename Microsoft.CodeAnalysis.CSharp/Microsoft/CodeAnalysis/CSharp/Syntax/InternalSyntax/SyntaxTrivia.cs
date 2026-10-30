using System.IO;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class SyntaxTrivia : CSharpSyntaxNode
{
	public readonly string Text;

	public override bool IsTrivia => true;

	internal override bool ShouldReuseInSerialization
	{
		get
		{
			if (Kind == SyntaxKind.WhitespaceTrivia)
			{
				return FullWidth < 42;
			}
			return false;
		}
	}

	public override int Width => FullWidth;

	internal SyntaxTrivia(SyntaxKind kind, string text, DiagnosticInfo[] diagnostics = null, SyntaxAnnotation[] annotations = null)
		: base(kind, diagnostics, annotations, text.Length)
	{
		Text = text;
		if (kind == SyntaxKind.PreprocessingMessageTrivia)
		{
			flags |= NodeFlags.ContainsSkippedText;
		}
	}

	internal SyntaxTrivia(ObjectReader reader)
		: base(reader)
	{
		Text = reader.ReadString();
		FullWidth = Text.Length;
	}

	static SyntaxTrivia()
	{
		ObjectBinder.RegisterTypeReader(typeof(SyntaxTrivia), (ObjectReader r) => new SyntaxTrivia(r));
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteString(Text);
	}

	internal static SyntaxTrivia Create(SyntaxKind kind, string text)
	{
		return new SyntaxTrivia(kind, text);
	}

	public override string ToFullString()
	{
		return Text;
	}

	public override string ToString()
	{
		return Text;
	}

	internal override GreenNode GetSlot(int index)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override int GetLeadingTriviaWidth()
	{
		return 0;
	}

	public override int GetTrailingTriviaWidth()
	{
		return 0;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new SyntaxTrivia(Kind, Text, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new SyntaxTrivia(Kind, Text, GetDiagnostics(), annotations);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTrivia(this);
	}

	protected override void WriteTriviaTo(TextWriter writer)
	{
		writer.Write(Text);
	}

	public static implicit operator Microsoft.CodeAnalysis.SyntaxTrivia(SyntaxTrivia trivia)
	{
		return new Microsoft.CodeAnalysis.SyntaxTrivia(default(Microsoft.CodeAnalysis.SyntaxToken), trivia, 0, 0);
	}

	public override bool IsEquivalentTo(GreenNode other)
	{
		if (!base.IsEquivalentTo(other))
		{
			return false;
		}
		if (Text != ((SyntaxTrivia)other).Text)
		{
			return false;
		}
		return true;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
