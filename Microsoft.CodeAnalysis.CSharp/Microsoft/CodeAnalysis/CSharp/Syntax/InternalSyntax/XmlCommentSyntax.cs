using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlCommentSyntax : XmlNodeSyntax
{
	internal readonly SyntaxToken lessThanExclamationMinusMinusToken;

	internal readonly GreenNode textTokens;

	internal readonly SyntaxToken minusMinusGreaterThanToken;

	public SyntaxToken LessThanExclamationMinusMinusToken => lessThanExclamationMinusMinusToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> TextTokens => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(textTokens);

	public SyntaxToken MinusMinusGreaterThanToken => minusMinusGreaterThanToken;

	internal XmlCommentSyntax(SyntaxKind kind, SyntaxToken lessThanExclamationMinusMinusToken, GreenNode textTokens, SyntaxToken minusMinusGreaterThanToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanExclamationMinusMinusToken);
		this.lessThanExclamationMinusMinusToken = lessThanExclamationMinusMinusToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(minusMinusGreaterThanToken);
		this.minusMinusGreaterThanToken = minusMinusGreaterThanToken;
	}

	internal XmlCommentSyntax(SyntaxKind kind, SyntaxToken lessThanExclamationMinusMinusToken, GreenNode textTokens, SyntaxToken minusMinusGreaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanExclamationMinusMinusToken);
		this.lessThanExclamationMinusMinusToken = lessThanExclamationMinusMinusToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(minusMinusGreaterThanToken);
		this.minusMinusGreaterThanToken = minusMinusGreaterThanToken;
	}

	internal XmlCommentSyntax(SyntaxKind kind, SyntaxToken lessThanExclamationMinusMinusToken, GreenNode textTokens, SyntaxToken minusMinusGreaterThanToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanExclamationMinusMinusToken);
		this.lessThanExclamationMinusMinusToken = lessThanExclamationMinusMinusToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(minusMinusGreaterThanToken);
		this.minusMinusGreaterThanToken = minusMinusGreaterThanToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanExclamationMinusMinusToken, 
			1 => textTokens, 
			2 => minusMinusGreaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlCommentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlComment(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlComment(this);
	}

	public XmlCommentSyntax Update(SyntaxToken lessThanExclamationMinusMinusToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken minusMinusGreaterThanToken)
	{
		if (lessThanExclamationMinusMinusToken != LessThanExclamationMinusMinusToken || textTokens != TextTokens || minusMinusGreaterThanToken != MinusMinusGreaterThanToken)
		{
			XmlCommentSyntax xmlCommentSyntax = SyntaxFactory.XmlComment(lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlCommentSyntax = xmlCommentSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlCommentSyntax = xmlCommentSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlCommentSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new XmlCommentSyntax(Kind, lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new XmlCommentSyntax(Kind, lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken, GetDiagnostics(), annotations);
	}

	internal XmlCommentSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			lessThanExclamationMinusMinusToken = syntaxToken;
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
			minusMinusGreaterThanToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(lessThanExclamationMinusMinusToken);
		writer.WriteValue(textTokens);
		writer.WriteValue(minusMinusGreaterThanToken);
	}

	static XmlCommentSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(XmlCommentSyntax), (ObjectReader r) => new XmlCommentSyntax(r));
	}
}
