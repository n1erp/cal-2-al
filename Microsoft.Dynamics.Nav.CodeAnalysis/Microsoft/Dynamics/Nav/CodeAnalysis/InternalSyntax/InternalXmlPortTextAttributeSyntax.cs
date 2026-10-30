using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalXmlPortTextAttributeSyntax : InternalXmlPortTextNodeSyntax
{
	internal readonly InternalSyntaxToken _xmlPortTextAttributeKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _schema;

	internal readonly InternalSyntaxNode _triggers;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => XmlPortTextAttributeSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken XmlPortTextAttributeKeyword => _xmlPortTextAttributeKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxList<InternalXmlPortNodeSyntax> Schema => new InternalSyntaxList<InternalXmlPortNodeSyntax>(_schema);

	public override InternalSyntaxList<InternalTriggerDeclarationSyntax> Triggers => new InternalSyntaxList<InternalTriggerDeclarationSyntax>(_triggers);

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalXmlPortTextAttributeSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortTextAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(xmlPortTextAttributeKeyword);
		_xmlPortTextAttributeKeyword = xmlPortTextAttributeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (schema != null)
		{
			AdjustFlagsAndWidth(schema);
			_schema = schema;
		}
		if (triggers != null)
		{
			AdjustFlagsAndWidth(triggers);
			_triggers = triggers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalXmlPortTextAttributeSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortTextAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(xmlPortTextAttributeKeyword);
		_xmlPortTextAttributeKeyword = xmlPortTextAttributeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (schema != null)
		{
			AdjustFlagsAndWidth(schema);
			_schema = schema;
		}
		if (triggers != null)
		{
			AdjustFlagsAndWidth(triggers);
			_triggers = triggers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalXmlPortTextAttributeSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortTextAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(xmlPortTextAttributeKeyword);
		_xmlPortTextAttributeKeyword = xmlPortTextAttributeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (schema != null)
		{
			AdjustFlagsAndWidth(schema);
			_schema = schema;
		}
		if (triggers != null)
		{
			AdjustFlagsAndWidth(triggers);
			_triggers = triggers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _xmlPortTextAttributeKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _closeParenthesisToken, 
			4 => _openBraceToken, 
			5 => _propertyList, 
			6 => _schema, 
			7 => _triggers, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new XmlPortTextAttributeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortTextAttribute(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitXmlPortTextAttribute(this);
	}

	public InternalXmlPortTextAttributeSyntax Update(InternalSyntaxToken xmlPortTextAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		if (xmlPortTextAttributeKeyword != XmlPortTextAttributeKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.XmlPortTextAttribute(xmlPortTextAttributeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalXmlPortTextAttributeSyntax(Kind, _xmlPortTextAttributeKeyword, _openParenthesisToken, _name, _closeParenthesisToken, _openBraceToken, _propertyList, _schema, _triggers, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalXmlPortTextAttributeSyntax(Kind, _xmlPortTextAttributeKeyword, _openParenthesisToken, _name, _closeParenthesisToken, _openBraceToken, _propertyList, _schema, _triggers, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
