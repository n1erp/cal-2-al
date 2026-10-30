using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlTextAttributeSyntax : XmlAttributeSyntax
{
	internal readonly XmlNameSyntax name;

	internal readonly SyntaxToken equalsToken;

	internal readonly SyntaxToken startQuoteToken;

	internal readonly GreenNode textTokens;

	internal readonly SyntaxToken endQuoteToken;

	public override XmlNameSyntax Name => name;

	public override SyntaxToken EqualsToken => equalsToken;

	public override SyntaxToken StartQuoteToken => startQuoteToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> TextTokens => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(textTokens);

	public override SyntaxToken EndQuoteToken => endQuoteToken;

	internal XmlTextAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, GreenNode textTokens, SyntaxToken endQuoteToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlTextAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, GreenNode textTokens, SyntaxToken endQuoteToken, SyntaxFactoryContext context)
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
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlTextAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, GreenNode textTokens, SyntaxToken endQuoteToken)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => name, 
			1 => equalsToken, 
			2 => startQuoteToken, 
			3 => textTokens, 
			4 => endQuoteToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextAttributeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlTextAttribute(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlTextAttribute(this);
	}

	public XmlTextAttributeSyntax Update(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endQuoteToken)
	{
		if (name != Name || equalsToken != EqualsToken || startQuoteToken != StartQuoteToken || textTokens != TextTokens || endQuoteToken != EndQuoteToken)
		{
			XmlTextAttributeSyntax xmlTextAttributeSyntax = SyntaxFactory.XmlTextAttribute(name, equalsToken, startQuoteToken, textTokens, endQuoteToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlTextAttributeSyntax = xmlTextAttributeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlTextAttributeSyntax = xmlTextAttributeSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlTextAttributeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlTextAttributeSyntax(Kind, name, equalsToken, startQuoteToken, textTokens, endQuoteToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlTextAttributeSyntax(Kind, name, equalsToken, startQuoteToken, textTokens, endQuoteToken, GetDiagnostics(), annotations);
	}

	internal XmlTextAttributeSyntax(ObjectReader reader)
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
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			textTokens = greenNode;
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
		writer.WriteValue(textTokens);
		writer.WriteValue(endQuoteToken);
	}

	static XmlTextAttributeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlTextAttributeSyntax), (ObjectReader r) => new XmlTextAttributeSyntax(r));
	}
}
