using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class OmittedTypeArgumentSyntax : TypeSyntax
{
	internal readonly SyntaxToken omittedTypeArgumentToken;

	public SyntaxToken OmittedTypeArgumentToken => omittedTypeArgumentToken;

	internal OmittedTypeArgumentSyntax(SyntaxKind kind, SyntaxToken omittedTypeArgumentToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(omittedTypeArgumentToken);
		this.omittedTypeArgumentToken = omittedTypeArgumentToken;
	}

	internal OmittedTypeArgumentSyntax(SyntaxKind kind, SyntaxToken omittedTypeArgumentToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(omittedTypeArgumentToken);
		this.omittedTypeArgumentToken = omittedTypeArgumentToken;
	}

	internal OmittedTypeArgumentSyntax(SyntaxKind kind, SyntaxToken omittedTypeArgumentToken)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(omittedTypeArgumentToken);
		this.omittedTypeArgumentToken = omittedTypeArgumentToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return omittedTypeArgumentToken;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.OmittedTypeArgumentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOmittedTypeArgument(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOmittedTypeArgument(this);
	}

	public OmittedTypeArgumentSyntax Update(SyntaxToken omittedTypeArgumentToken)
	{
		if (omittedTypeArgumentToken != OmittedTypeArgumentToken)
		{
			OmittedTypeArgumentSyntax omittedTypeArgumentSyntax = SyntaxFactory.OmittedTypeArgument(omittedTypeArgumentToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				omittedTypeArgumentSyntax = omittedTypeArgumentSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				omittedTypeArgumentSyntax = omittedTypeArgumentSyntax.WithAnnotationsGreen(annotations);
			}
			return omittedTypeArgumentSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new OmittedTypeArgumentSyntax(Kind, omittedTypeArgumentToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new OmittedTypeArgumentSyntax(Kind, omittedTypeArgumentToken, GetDiagnostics(), annotations);
	}

	internal OmittedTypeArgumentSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			omittedTypeArgumentToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(omittedTypeArgumentToken);
	}

	static OmittedTypeArgumentSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(OmittedTypeArgumentSyntax), (ObjectReader r) => new OmittedTypeArgumentSyntax(r));
	}
}
