using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InterpolationFormatClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken colonToken;

	internal readonly SyntaxToken formatStringToken;

	public SyntaxToken ColonToken => colonToken;

	public SyntaxToken FormatStringToken => formatStringToken;

	internal InterpolationFormatClauseSyntax(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken formatStringToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(formatStringToken);
		this.formatStringToken = formatStringToken;
	}

	internal InterpolationFormatClauseSyntax(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken formatStringToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(formatStringToken);
		this.formatStringToken = formatStringToken;
	}

	internal InterpolationFormatClauseSyntax(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken formatStringToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(formatStringToken);
		this.formatStringToken = formatStringToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)colonToken, 
			1 => formatStringToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationFormatClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInterpolationFormatClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInterpolationFormatClause(this);
	}

	public InterpolationFormatClauseSyntax Update(SyntaxToken colonToken, SyntaxToken formatStringToken)
	{
		if (colonToken != ColonToken || formatStringToken != FormatStringToken)
		{
			InterpolationFormatClauseSyntax interpolationFormatClauseSyntax = SyntaxFactory.InterpolationFormatClause(colonToken, formatStringToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				interpolationFormatClauseSyntax = interpolationFormatClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				interpolationFormatClauseSyntax = interpolationFormatClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return interpolationFormatClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InterpolationFormatClauseSyntax(Kind, colonToken, formatStringToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InterpolationFormatClauseSyntax(Kind, colonToken, formatStringToken, GetDiagnostics(), annotations);
	}

	internal InterpolationFormatClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			colonToken = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			formatStringToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(colonToken);
		writer.WriteValue(formatStringToken);
	}

	static InterpolationFormatClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(InterpolationFormatClauseSyntax), (ObjectReader r) => new InterpolationFormatClauseSyntax(r));
	}
}
