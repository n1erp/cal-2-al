using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlCDataSectionSyntax : XmlNodeSyntax
{
	internal readonly SyntaxToken startCDataToken;

	internal readonly GreenNode textTokens;

	internal readonly SyntaxToken endCDataToken;

	public SyntaxToken StartCDataToken => startCDataToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> TextTokens => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(textTokens);

	public SyntaxToken EndCDataToken => endCDataToken;

	internal XmlCDataSectionSyntax(SyntaxKind kind, SyntaxToken startCDataToken, GreenNode textTokens, SyntaxToken endCDataToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(startCDataToken);
		this.startCDataToken = startCDataToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endCDataToken);
		this.endCDataToken = endCDataToken;
	}

	internal XmlCDataSectionSyntax(SyntaxKind kind, SyntaxToken startCDataToken, GreenNode textTokens, SyntaxToken endCDataToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(startCDataToken);
		this.startCDataToken = startCDataToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endCDataToken);
		this.endCDataToken = endCDataToken;
	}

	internal XmlCDataSectionSyntax(SyntaxKind kind, SyntaxToken startCDataToken, GreenNode textTokens, SyntaxToken endCDataToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(startCDataToken);
		this.startCDataToken = startCDataToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endCDataToken);
		this.endCDataToken = endCDataToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => startCDataToken, 
			1 => textTokens, 
			2 => endCDataToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlCDataSectionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlCDataSection(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlCDataSection(this);
	}

	public XmlCDataSectionSyntax Update(SyntaxToken startCDataToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endCDataToken)
	{
		if (startCDataToken != StartCDataToken || textTokens != TextTokens || endCDataToken != EndCDataToken)
		{
			XmlCDataSectionSyntax xmlCDataSectionSyntax = SyntaxFactory.XmlCDataSection(startCDataToken, textTokens, endCDataToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlCDataSectionSyntax = xmlCDataSectionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlCDataSectionSyntax = xmlCDataSectionSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlCDataSectionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlCDataSectionSyntax(Kind, startCDataToken, textTokens, endCDataToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlCDataSectionSyntax(Kind, startCDataToken, textTokens, endCDataToken, GetDiagnostics(), annotations);
	}

	internal XmlCDataSectionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			startCDataToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			textTokens = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			endCDataToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(startCDataToken);
		writer.WriteValue(textTokens);
		writer.WriteValue(endCDataToken);
	}

	static XmlCDataSectionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlCDataSectionSyntax), (ObjectReader r) => new XmlCDataSectionSyntax(r));
	}
}
