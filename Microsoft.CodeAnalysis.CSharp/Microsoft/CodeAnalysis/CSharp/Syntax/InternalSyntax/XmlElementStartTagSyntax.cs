using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlElementStartTagSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken lessThanToken;

	internal readonly XmlNameSyntax name;

	internal readonly GreenNode attributes;

	internal readonly SyntaxToken greaterThanToken;

	public SyntaxToken LessThanToken => lessThanToken;

	public XmlNameSyntax Name => name;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> Attributes => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax>(attributes);

	public SyntaxToken GreaterThanToken => greaterThanToken;

	internal XmlElementStartTagSyntax(SyntaxKind kind, SyntaxToken lessThanToken, XmlNameSyntax name, GreenNode attributes, SyntaxToken greaterThanToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal XmlElementStartTagSyntax(SyntaxKind kind, SyntaxToken lessThanToken, XmlNameSyntax name, GreenNode attributes, SyntaxToken greaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal XmlElementStartTagSyntax(SyntaxKind kind, SyntaxToken lessThanToken, XmlNameSyntax name, GreenNode attributes, SyntaxToken greaterThanToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanToken, 
			1 => name, 
			2 => attributes, 
			3 => greaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlElementStartTag(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlElementStartTag(this);
	}

	public XmlElementStartTagSyntax Update(SyntaxToken lessThanToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken greaterThanToken)
	{
		if (lessThanToken != LessThanToken || name != Name || attributes != Attributes || greaterThanToken != GreaterThanToken)
		{
			XmlElementStartTagSyntax xmlElementStartTagSyntax = SyntaxFactory.XmlElementStartTag(lessThanToken, name, attributes, greaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlElementStartTagSyntax = xmlElementStartTagSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlElementStartTagSyntax = xmlElementStartTagSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlElementStartTagSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlElementStartTagSyntax(Kind, lessThanToken, name, attributes, greaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlElementStartTagSyntax(Kind, lessThanToken, name, attributes, greaterThanToken, GetDiagnostics(), annotations);
	}

	internal XmlElementStartTagSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			lessThanToken = syntaxToken;
		}
		XmlNameSyntax xmlNameSyntax = (XmlNameSyntax)reader.ReadValue();
		if (xmlNameSyntax != null)
		{
			AdjustFlagsAndWidth(xmlNameSyntax);
			name = xmlNameSyntax;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			attributes = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			greaterThanToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(lessThanToken);
		writer.WriteValue(name);
		writer.WriteValue(attributes);
		writer.WriteValue(greaterThanToken);
	}

	static XmlElementStartTagSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlElementStartTagSyntax), (ObjectReader r) => new XmlElementStartTagSyntax(r));
	}
}
