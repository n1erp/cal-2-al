using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlElementEndTagSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken lessThanSlashToken;

	internal readonly XmlNameSyntax name;

	internal readonly SyntaxToken greaterThanToken;

	public SyntaxToken LessThanSlashToken => lessThanSlashToken;

	public XmlNameSyntax Name => name;

	public SyntaxToken GreaterThanToken => greaterThanToken;

	internal XmlElementEndTagSyntax(SyntaxKind kind, SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanSlashToken);
		this.lessThanSlashToken = lessThanSlashToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal XmlElementEndTagSyntax(SyntaxKind kind, SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanSlashToken);
		this.lessThanSlashToken = lessThanSlashToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal XmlElementEndTagSyntax(SyntaxKind kind, SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanSlashToken);
		this.lessThanSlashToken = lessThanSlashToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)lessThanSlashToken, 
			1 => name, 
			2 => greaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlElementEndTag(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlElementEndTag(this);
	}

	public XmlElementEndTagSyntax Update(SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken)
	{
		if (lessThanSlashToken != LessThanSlashToken || name != Name || greaterThanToken != GreaterThanToken)
		{
			XmlElementEndTagSyntax xmlElementEndTagSyntax = SyntaxFactory.XmlElementEndTag(lessThanSlashToken, name, greaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlElementEndTagSyntax = xmlElementEndTagSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlElementEndTagSyntax = xmlElementEndTagSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlElementEndTagSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlElementEndTagSyntax(Kind, lessThanSlashToken, name, greaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlElementEndTagSyntax(Kind, lessThanSlashToken, name, greaterThanToken, GetDiagnostics(), annotations);
	}

	internal XmlElementEndTagSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			lessThanSlashToken = syntaxToken;
		}
		XmlNameSyntax xmlNameSyntax = (XmlNameSyntax)reader.ReadValue();
		if (xmlNameSyntax != null)
		{
			AdjustFlagsAndWidth(xmlNameSyntax);
			name = xmlNameSyntax;
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
		writer.WriteValue(lessThanSlashToken);
		writer.WriteValue(name);
		writer.WriteValue(greaterThanToken);
	}

	static XmlElementEndTagSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlElementEndTagSyntax), (ObjectReader r) => new XmlElementEndTagSyntax(r));
	}
}
