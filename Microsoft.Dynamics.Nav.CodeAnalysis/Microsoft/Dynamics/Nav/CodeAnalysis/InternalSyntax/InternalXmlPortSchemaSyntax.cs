using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalXmlPortSchemaSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _xmlPortSchemaKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _xmlPortSchema;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => XmlPortSchemaSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken XmlPortSchemaKeyword => _xmlPortSchemaKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalXmlPortNodeSyntax> XmlPortSchema => new InternalSyntaxList<InternalXmlPortNodeSyntax>(_xmlPortSchema);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalXmlPortSchemaSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortSchemaKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode xmlPortSchema, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(xmlPortSchemaKeyword);
		_xmlPortSchemaKeyword = xmlPortSchemaKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (xmlPortSchema != null)
		{
			AdjustFlagsAndWidth(xmlPortSchema);
			_xmlPortSchema = xmlPortSchema;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalXmlPortSchemaSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortSchemaKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode xmlPortSchema, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(xmlPortSchemaKeyword);
		_xmlPortSchemaKeyword = xmlPortSchemaKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (xmlPortSchema != null)
		{
			AdjustFlagsAndWidth(xmlPortSchema);
			_xmlPortSchema = xmlPortSchema;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalXmlPortSchemaSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortSchemaKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode xmlPortSchema, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(xmlPortSchemaKeyword);
		_xmlPortSchemaKeyword = xmlPortSchemaKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (xmlPortSchema != null)
		{
			AdjustFlagsAndWidth(xmlPortSchema);
			_xmlPortSchema = xmlPortSchema;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _xmlPortSchemaKeyword, 
			1 => _openBraceToken, 
			2 => _xmlPortSchema, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new XmlPortSchemaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortSchema(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitXmlPortSchema(this);
	}

	public InternalXmlPortSchemaSyntax Update(InternalSyntaxToken xmlPortSchemaKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalXmlPortNodeSyntax> xmlPortSchema, InternalSyntaxToken closeBraceToken)
	{
		if (xmlPortSchemaKeyword != XmlPortSchemaKeyword || openBraceToken != OpenBraceToken || xmlPortSchema != XmlPortSchema || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.XmlPortSchema(xmlPortSchemaKeyword, openBraceToken, xmlPortSchema, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalXmlPortSchemaSyntax(Kind, _xmlPortSchemaKeyword, _openBraceToken, _xmlPortSchema, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalXmlPortSchemaSyntax(Kind, _xmlPortSchemaKeyword, _openBraceToken, _xmlPortSchema, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
