using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalXmlPortFieldAttributeSyntax : InternalXmlPortFieldNodeSyntax
{
	internal readonly InternalSyntaxToken _xmlPortFieldAttributeKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalCodeExpressionSyntax _sourceField;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _schema;

	internal readonly InternalSyntaxNode _triggers;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => XmlPortFieldAttributeSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken XmlPortFieldAttributeKeyword => _xmlPortFieldAttributeKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalSyntaxToken SemicolonToken => _semicolonToken;

	public override InternalCodeExpressionSyntax SourceField => _sourceField;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxList<InternalXmlPortNodeSyntax> Schema => new InternalSyntaxList<InternalXmlPortNodeSyntax>(_schema);

	public override InternalSyntaxList<InternalTriggerDeclarationSyntax> Triggers => new InternalSyntaxList<InternalTriggerDeclarationSyntax>(_triggers);

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalXmlPortFieldAttributeSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortFieldAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(xmlPortFieldAttributeKeyword);
		_xmlPortFieldAttributeKeyword = xmlPortFieldAttributeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
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

	internal InternalXmlPortFieldAttributeSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortFieldAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(xmlPortFieldAttributeKeyword);
		_xmlPortFieldAttributeKeyword = xmlPortFieldAttributeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
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

	internal InternalXmlPortFieldAttributeSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortFieldAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(xmlPortFieldAttributeKeyword);
		_xmlPortFieldAttributeKeyword = xmlPortFieldAttributeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
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
			0 => _xmlPortFieldAttributeKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _semicolonToken, 
			4 => _sourceField, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _schema, 
			9 => _triggers, 
			10 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new XmlPortFieldAttributeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortFieldAttribute(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitXmlPortFieldAttribute(this);
	}

	public InternalXmlPortFieldAttributeSyntax Update(InternalSyntaxToken xmlPortFieldAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		if (xmlPortFieldAttributeKeyword != XmlPortFieldAttributeKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || sourceField != SourceField || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.XmlPortFieldAttribute(xmlPortFieldAttributeKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalXmlPortFieldAttributeSyntax(Kind, _xmlPortFieldAttributeKeyword, _openParenthesisToken, _name, _semicolonToken, _sourceField, _closeParenthesisToken, _openBraceToken, _propertyList, _schema, _triggers, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalXmlPortFieldAttributeSyntax(Kind, _xmlPortFieldAttributeKeyword, _openParenthesisToken, _name, _semicolonToken, _sourceField, _closeParenthesisToken, _openBraceToken, _propertyList, _schema, _triggers, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
