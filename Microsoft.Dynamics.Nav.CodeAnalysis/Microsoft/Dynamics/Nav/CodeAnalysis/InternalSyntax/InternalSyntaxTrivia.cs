using System;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InternalSyntaxTrivia : InternalSyntaxNode
{
	internal readonly string Text;

	internal InternalSyntaxTrivia(SyntaxKind kind, string text)
		: base(kind, text.Length)
	{
		Text = text;
	}

	internal InternalSyntaxTrivia(SyntaxKind kind, string text, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations, text.Length)
	{
		Text = text;
	}

	internal static InternalSyntaxTrivia Create(SyntaxKind kind, string text)
	{
		return new InternalSyntaxTrivia(kind, text);
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		throw new NotSupportedException();
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		throw new NotSupportedException();
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		throw new NotSupportedException();
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		throw new NotSupportedException();
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalSyntaxTrivia(Kind, Text, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalSyntaxTrivia(Kind, Text, GetDiagnostics(), annotations);
	}

	protected internal override void WriteTo(TextWriter writer, bool leading, bool trailing)
	{
		writer.Write(Text);
	}

	public static implicit operator SyntaxTrivia(InternalSyntaxTrivia trivia)
	{
		return new SyntaxTrivia(default, trivia, 0, 0);
	}
}
