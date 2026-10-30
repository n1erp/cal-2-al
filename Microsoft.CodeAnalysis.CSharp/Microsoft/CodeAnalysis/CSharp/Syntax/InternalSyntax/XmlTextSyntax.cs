using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlTextSyntax : XmlNodeSyntax
{
	internal readonly GreenNode textTokens;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> TextTokens => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(textTokens);

	internal XmlTextSyntax(SyntaxKind kind, GreenNode textTokens, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
	}

	internal XmlTextSyntax(SyntaxKind kind, GreenNode textTokens, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
	}

	internal XmlTextSyntax(SyntaxKind kind, GreenNode textTokens)
		: base(kind)
	{
		SlotCount = 1;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return textTokens;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlTextSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlText(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlText(this);
	}

	public XmlTextSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens)
	{
		if (textTokens != TextTokens)
		{
			XmlTextSyntax xmlTextSyntax = SyntaxFactory.XmlText(textTokens);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlTextSyntax = xmlTextSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlTextSyntax = xmlTextSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlTextSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlTextSyntax(Kind, textTokens, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlTextSyntax(Kind, textTokens, GetDiagnostics(), annotations);
	}

	internal XmlTextSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			textTokens = greenNode;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(textTokens);
	}

	static XmlTextSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlTextSyntax), (ObjectReader r) => new XmlTextSyntax(r));
	}
}
