using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlPrefixSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken prefix;

	internal readonly SyntaxToken colonToken;

	public SyntaxToken Prefix => prefix;

	public SyntaxToken ColonToken => colonToken;

	internal XmlPrefixSyntax(SyntaxKind kind, SyntaxToken prefix, SyntaxToken colonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(prefix);
		this.prefix = prefix;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal XmlPrefixSyntax(SyntaxKind kind, SyntaxToken prefix, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(prefix);
		this.prefix = prefix;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal XmlPrefixSyntax(SyntaxKind kind, SyntaxToken prefix, SyntaxToken colonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(prefix);
		this.prefix = prefix;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)prefix, 
			1 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlPrefixSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPrefix(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlPrefix(this);
	}

	public XmlPrefixSyntax Update(SyntaxToken prefix, SyntaxToken colonToken)
	{
		if (prefix != Prefix || colonToken != ColonToken)
		{
			XmlPrefixSyntax xmlPrefixSyntax = SyntaxFactory.XmlPrefix(prefix, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlPrefixSyntax = xmlPrefixSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlPrefixSyntax = xmlPrefixSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlPrefixSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlPrefixSyntax(Kind, prefix, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlPrefixSyntax(Kind, prefix, colonToken, GetDiagnostics(), annotations);
	}

	internal XmlPrefixSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			prefix = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			colonToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(prefix);
		writer.WriteValue(colonToken);
	}

	static XmlPrefixSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlPrefixSyntax), (ObjectReader r) => new XmlPrefixSyntax(r));
	}
}
