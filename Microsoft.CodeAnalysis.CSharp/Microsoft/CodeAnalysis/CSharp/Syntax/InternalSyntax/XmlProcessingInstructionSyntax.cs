using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlProcessingInstructionSyntax : XmlNodeSyntax
{
	internal readonly SyntaxToken startProcessingInstructionToken;

	internal readonly XmlNameSyntax name;

	internal readonly GreenNode textTokens;

	internal readonly SyntaxToken endProcessingInstructionToken;

	public SyntaxToken StartProcessingInstructionToken => startProcessingInstructionToken;

	public XmlNameSyntax Name => name;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> TextTokens => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(textTokens);

	public SyntaxToken EndProcessingInstructionToken => endProcessingInstructionToken;

	internal XmlProcessingInstructionSyntax(SyntaxKind kind, SyntaxToken startProcessingInstructionToken, XmlNameSyntax name, GreenNode textTokens, SyntaxToken endProcessingInstructionToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(startProcessingInstructionToken);
		this.startProcessingInstructionToken = startProcessingInstructionToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endProcessingInstructionToken);
		this.endProcessingInstructionToken = endProcessingInstructionToken;
	}

	internal XmlProcessingInstructionSyntax(SyntaxKind kind, SyntaxToken startProcessingInstructionToken, XmlNameSyntax name, GreenNode textTokens, SyntaxToken endProcessingInstructionToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(startProcessingInstructionToken);
		this.startProcessingInstructionToken = startProcessingInstructionToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endProcessingInstructionToken);
		this.endProcessingInstructionToken = endProcessingInstructionToken;
	}

	internal XmlProcessingInstructionSyntax(SyntaxKind kind, SyntaxToken startProcessingInstructionToken, XmlNameSyntax name, GreenNode textTokens, SyntaxToken endProcessingInstructionToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(startProcessingInstructionToken);
		this.startProcessingInstructionToken = startProcessingInstructionToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(endProcessingInstructionToken);
		this.endProcessingInstructionToken = endProcessingInstructionToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => startProcessingInstructionToken, 
			1 => name, 
			2 => textTokens, 
			3 => endProcessingInstructionToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlProcessingInstructionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlProcessingInstruction(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlProcessingInstruction(this);
	}

	public XmlProcessingInstructionSyntax Update(SyntaxToken startProcessingInstructionToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken endProcessingInstructionToken)
	{
		if (startProcessingInstructionToken != StartProcessingInstructionToken || name != Name || textTokens != TextTokens || endProcessingInstructionToken != EndProcessingInstructionToken)
		{
			XmlProcessingInstructionSyntax xmlProcessingInstructionSyntax = SyntaxFactory.XmlProcessingInstruction(startProcessingInstructionToken, name, textTokens, endProcessingInstructionToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlProcessingInstructionSyntax = xmlProcessingInstructionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlProcessingInstructionSyntax = xmlProcessingInstructionSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlProcessingInstructionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlProcessingInstructionSyntax(Kind, startProcessingInstructionToken, name, textTokens, endProcessingInstructionToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlProcessingInstructionSyntax(Kind, startProcessingInstructionToken, name, textTokens, endProcessingInstructionToken, GetDiagnostics(), annotations);
	}

	internal XmlProcessingInstructionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			startProcessingInstructionToken = syntaxToken;
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
			textTokens = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			endProcessingInstructionToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(startProcessingInstructionToken);
		writer.WriteValue(name);
		writer.WriteValue(textTokens);
		writer.WriteValue(endProcessingInstructionToken);
	}

	static XmlProcessingInstructionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlProcessingInstructionSyntax), (ObjectReader r) => new XmlProcessingInstructionSyntax(r));
	}
}
