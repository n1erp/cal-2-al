using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InterpolatedStringTextSyntax : InterpolatedStringContentSyntax
{
	internal readonly SyntaxToken textToken;

	public SyntaxToken TextToken => textToken;

	internal InterpolatedStringTextSyntax(SyntaxKind kind, SyntaxToken textToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(textToken);
		this.textToken = textToken;
	}

	internal InterpolatedStringTextSyntax(SyntaxKind kind, SyntaxToken textToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(textToken);
		this.textToken = textToken;
	}

	internal InterpolatedStringTextSyntax(SyntaxKind kind, SyntaxToken textToken)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(textToken);
		this.textToken = textToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return textToken;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringTextSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInterpolatedStringText(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInterpolatedStringText(this);
	}

	public InterpolatedStringTextSyntax Update(SyntaxToken textToken)
	{
		if (textToken != TextToken)
		{
			InterpolatedStringTextSyntax interpolatedStringTextSyntax = SyntaxFactory.InterpolatedStringText(textToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				interpolatedStringTextSyntax = interpolatedStringTextSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				interpolatedStringTextSyntax = interpolatedStringTextSyntax.WithAnnotationsGreen(annotations);
			}
			return interpolatedStringTextSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InterpolatedStringTextSyntax(Kind, textToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InterpolatedStringTextSyntax(Kind, textToken, GetDiagnostics(), annotations);
	}

	internal InterpolatedStringTextSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			textToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(textToken);
	}

	static InterpolatedStringTextSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(InterpolatedStringTextSyntax), (ObjectReader r) => new InterpolatedStringTextSyntax(r));
	}
}
