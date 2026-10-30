using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlNameAttributeSyntax : XmlAttributeSyntax
{
	internal readonly XmlNameSyntax name;

	internal readonly SyntaxToken equalsToken;

	internal readonly SyntaxToken startQuoteToken;

	internal readonly IdentifierNameSyntax identifier;

	internal readonly SyntaxToken endQuoteToken;

	public override XmlNameSyntax Name => name;

	public override SyntaxToken EqualsToken => equalsToken;

	public override SyntaxToken StartQuoteToken => startQuoteToken;

	public IdentifierNameSyntax Identifier => identifier;

	public override SyntaxToken EndQuoteToken => endQuoteToken;

	internal XmlNameAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, IdentifierNameSyntax identifier, SyntaxToken endQuoteToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlNameAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, IdentifierNameSyntax identifier, SyntaxToken endQuoteToken, SyntaxFactoryContext context)
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
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlNameAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, IdentifierNameSyntax identifier, SyntaxToken endQuoteToken)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
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
			3 => identifier, 
			4 => endQuoteToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameAttributeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlNameAttribute(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlNameAttribute(this);
	}

	public XmlNameAttributeSyntax Update(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, IdentifierNameSyntax identifier, SyntaxToken endQuoteToken)
	{
		if (name != Name || equalsToken != EqualsToken || startQuoteToken != StartQuoteToken || identifier != Identifier || endQuoteToken != EndQuoteToken)
		{
			XmlNameAttributeSyntax xmlNameAttributeSyntax = SyntaxFactory.XmlNameAttribute(name, equalsToken, startQuoteToken, identifier, endQuoteToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlNameAttributeSyntax = xmlNameAttributeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlNameAttributeSyntax = xmlNameAttributeSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlNameAttributeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlNameAttributeSyntax(Kind, name, equalsToken, startQuoteToken, identifier, endQuoteToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlNameAttributeSyntax(Kind, name, equalsToken, startQuoteToken, identifier, endQuoteToken, GetDiagnostics(), annotations);
	}

	internal XmlNameAttributeSyntax(ObjectReader reader)
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
		IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)reader.ReadValue();
		if (identifierNameSyntax != null)
		{
			AdjustFlagsAndWidth(identifierNameSyntax);
			identifier = identifierNameSyntax;
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
		writer.WriteValue(identifier);
		writer.WriteValue(endQuoteToken);
	}

	static XmlNameAttributeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlNameAttributeSyntax), (ObjectReader r) => new XmlNameAttributeSyntax(r));
	}
}
