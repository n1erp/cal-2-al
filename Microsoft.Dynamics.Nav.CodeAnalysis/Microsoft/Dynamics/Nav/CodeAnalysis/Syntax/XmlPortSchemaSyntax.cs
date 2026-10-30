using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class XmlPortSchemaSyntax : SyntaxNode
{
	private SyntaxNode _xmlPortSchema;

	public SyntaxToken XmlPortSchemaKeyword => new SyntaxToken(this, ((InternalXmlPortSchemaSyntax)Node)._xmlPortSchemaKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalXmlPortSchemaSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<XmlPortNodeSyntax> XmlPortSchema => new SyntaxList<XmlPortNodeSyntax>(GetRed(ref _xmlPortSchema, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalXmlPortSchemaSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal XmlPortSchemaSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _xmlPortSchema, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _xmlPortSchema;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortSchema(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitXmlPortSchema(this);
	}

	public XmlPortSchemaSyntax Update(SyntaxToken xmlPortSchemaKeyword, SyntaxToken openBraceToken, SyntaxList<XmlPortNodeSyntax> xmlPortSchema, SyntaxToken closeBraceToken)
	{
		if (xmlPortSchemaKeyword != XmlPortSchemaKeyword || openBraceToken != OpenBraceToken || xmlPortSchema != XmlPortSchema || closeBraceToken != CloseBraceToken)
		{
			XmlPortSchemaSyntax xmlPortSchemaSyntax = SyntaxFactory.XmlPortSchema(xmlPortSchemaKeyword, openBraceToken, xmlPortSchema, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPortSchemaSyntax.WithAnnotations(annotations);
			}
			return xmlPortSchemaSyntax;
		}
		return this;
	}

	public XmlPortSchemaSyntax WithXmlPortSchemaKeyword(SyntaxToken xmlPortSchemaKeyword)
	{
		return Update(xmlPortSchemaKeyword, OpenBraceToken, XmlPortSchema, CloseBraceToken);
	}

	public XmlPortSchemaSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(XmlPortSchemaKeyword, openBraceToken, XmlPortSchema, CloseBraceToken);
	}

	public XmlPortSchemaSyntax WithXmlPortSchema(SyntaxList<XmlPortNodeSyntax> xmlPortSchema)
	{
		return Update(XmlPortSchemaKeyword, OpenBraceToken, xmlPortSchema, CloseBraceToken);
	}

	public XmlPortSchemaSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(XmlPortSchemaKeyword, OpenBraceToken, XmlPortSchema, closeBraceToken);
	}

	public XmlPortSchemaSyntax AddXmlPortSchema(params XmlPortNodeSyntax[] items)
	{
		return WithXmlPortSchema(XmlPortSchema.AddRange(items));
	}
}
