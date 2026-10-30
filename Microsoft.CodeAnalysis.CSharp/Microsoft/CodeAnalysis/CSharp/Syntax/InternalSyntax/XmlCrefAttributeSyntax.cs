using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlCrefAttributeSyntax : XmlAttributeSyntax
{
	internal readonly XmlNameSyntax name;

	internal readonly SyntaxToken equalsToken;

	internal readonly SyntaxToken startQuoteToken;

	internal readonly CrefSyntax cref;

	internal readonly SyntaxToken endQuoteToken;

	public override XmlNameSyntax Name => name;

	public override SyntaxToken EqualsToken => equalsToken;

	public override SyntaxToken StartQuoteToken => startQuoteToken;

	public CrefSyntax Cref => cref;

	public override SyntaxToken EndQuoteToken => endQuoteToken;

	internal XmlCrefAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(cref);
		this.cref = cref;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlCrefAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(cref);
		this.cref = cref;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlCrefAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(cref);
		this.cref = cref;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)name, 
			1 => equalsToken, 
			2 => startQuoteToken, 
			3 => cref, 
			4 => endQuoteToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlCrefAttribute(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlCrefAttribute(this);
	}

	public XmlCrefAttributeSyntax Update(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken)
	{
		if (name != Name || equalsToken != EqualsToken || startQuoteToken != StartQuoteToken || cref != Cref || endQuoteToken != EndQuoteToken)
		{
			XmlCrefAttributeSyntax xmlCrefAttributeSyntax = SyntaxFactory.XmlCrefAttribute(name, equalsToken, startQuoteToken, cref, endQuoteToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlCrefAttributeSyntax = xmlCrefAttributeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlCrefAttributeSyntax = xmlCrefAttributeSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlCrefAttributeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlCrefAttributeSyntax(Kind, name, equalsToken, startQuoteToken, cref, endQuoteToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlCrefAttributeSyntax(Kind, name, equalsToken, startQuoteToken, cref, endQuoteToken, GetDiagnostics(), annotations);
	}

	internal XmlCrefAttributeSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 5;
		XmlNameSyntax xmlNameSyntax = (XmlNameSyntax)reader.ReadValue();
		if (xmlNameSyntax != null)
		{
			AdjustFlagsAndWidth(xmlNameSyntax);
			name = xmlNameSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			equalsToken = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			startQuoteToken = syntaxToken2;
		}
		CrefSyntax crefSyntax = (CrefSyntax)reader.ReadValue();
		if (crefSyntax != null)
		{
			AdjustFlagsAndWidth(crefSyntax);
			cref = crefSyntax;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			endQuoteToken = syntaxToken3;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(name);
		writer.WriteValue(equalsToken);
		writer.WriteValue(startQuoteToken);
		writer.WriteValue(cref);
		writer.WriteValue(endQuoteToken);
	}

	static XmlCrefAttributeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlCrefAttributeSyntax), (ObjectReader r) => new XmlCrefAttributeSyntax(r));
	}
}
