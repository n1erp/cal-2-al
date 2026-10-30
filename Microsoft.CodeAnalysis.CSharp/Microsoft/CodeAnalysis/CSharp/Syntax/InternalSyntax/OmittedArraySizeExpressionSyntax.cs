using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class OmittedArraySizeExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken omittedArraySizeExpressionToken;

	public SyntaxToken OmittedArraySizeExpressionToken => omittedArraySizeExpressionToken;

	internal OmittedArraySizeExpressionSyntax(SyntaxKind kind, SyntaxToken omittedArraySizeExpressionToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(omittedArraySizeExpressionToken);
		this.omittedArraySizeExpressionToken = omittedArraySizeExpressionToken;
	}

	internal OmittedArraySizeExpressionSyntax(SyntaxKind kind, SyntaxToken omittedArraySizeExpressionToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(omittedArraySizeExpressionToken);
		this.omittedArraySizeExpressionToken = omittedArraySizeExpressionToken;
	}

	internal OmittedArraySizeExpressionSyntax(SyntaxKind kind, SyntaxToken omittedArraySizeExpressionToken)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(omittedArraySizeExpressionToken);
		this.omittedArraySizeExpressionToken = omittedArraySizeExpressionToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return omittedArraySizeExpressionToken;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.OmittedArraySizeExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOmittedArraySizeExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOmittedArraySizeExpression(this);
	}

	public OmittedArraySizeExpressionSyntax Update(SyntaxToken omittedArraySizeExpressionToken)
	{
		if (omittedArraySizeExpressionToken != OmittedArraySizeExpressionToken)
		{
			OmittedArraySizeExpressionSyntax omittedArraySizeExpressionSyntax = SyntaxFactory.OmittedArraySizeExpression(omittedArraySizeExpressionToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				omittedArraySizeExpressionSyntax = omittedArraySizeExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				omittedArraySizeExpressionSyntax = omittedArraySizeExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return omittedArraySizeExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new OmittedArraySizeExpressionSyntax(Kind, omittedArraySizeExpressionToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new OmittedArraySizeExpressionSyntax(Kind, omittedArraySizeExpressionToken, GetDiagnostics(), annotations);
	}

	internal OmittedArraySizeExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			omittedArraySizeExpressionToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(omittedArraySizeExpressionToken);
	}

	static OmittedArraySizeExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(OmittedArraySizeExpressionSyntax), (ObjectReader r) => new OmittedArraySizeExpressionSyntax(r));
	}
}
