using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalXmlPortTableElementSyntax : InternalXmlPortNodeSyntax
{
	internal readonly InternalSyntaxToken _xmlPortTableElementKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalObjectNameOrIdSyntax _sourceTable;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _schema;

	internal readonly InternalSyntaxNode _triggers;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => XmlPortTableElementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken XmlPortTableElementKeyword => _xmlPortTableElementKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalObjectNameOrIdSyntax SourceTable => _sourceTable;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxList<InternalXmlPortNodeSyntax> Schema => new InternalSyntaxList<InternalXmlPortNodeSyntax>(_schema);

	public override InternalSyntaxList<InternalTriggerDeclarationSyntax> Triggers => new InternalSyntaxList<InternalTriggerDeclarationSyntax>(_triggers);

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalXmlPortTableElementSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortTableElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax sourceTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(xmlPortTableElementKeyword);
		_xmlPortTableElementKeyword = xmlPortTableElementKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceTable);
		_sourceTable = sourceTable;
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

	internal InternalXmlPortTableElementSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortTableElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax sourceTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(xmlPortTableElementKeyword);
		_xmlPortTableElementKeyword = xmlPortTableElementKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceTable);
		_sourceTable = sourceTable;
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

	internal InternalXmlPortTableElementSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortTableElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax sourceTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode schema, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(xmlPortTableElementKeyword);
		_xmlPortTableElementKeyword = xmlPortTableElementKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceTable);
		_sourceTable = sourceTable;
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
			0 => _xmlPortTableElementKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _semicolonToken, 
			4 => _sourceTable, 
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
		return new XmlPortTableElementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortTableElement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitXmlPortTableElement(this);
	}

	public InternalXmlPortTableElementSyntax Update(InternalSyntaxToken xmlPortTableElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax sourceTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		if (xmlPortTableElementKeyword != XmlPortTableElementKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || sourceTable != SourceTable || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.XmlPortTableElement(xmlPortTableElementKeyword, openParenthesisToken, name, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalXmlPortTableElementSyntax(Kind, _xmlPortTableElementKeyword, _openParenthesisToken, _name, _semicolonToken, _sourceTable, _closeParenthesisToken, _openBraceToken, _propertyList, _schema, _triggers, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalXmlPortTableElementSyntax(Kind, _xmlPortTableElementKeyword, _openParenthesisToken, _name, _semicolonToken, _sourceTable, _closeParenthesisToken, _openBraceToken, _propertyList, _schema, _triggers, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
