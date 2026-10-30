using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQuerySyntax : InternalApplicationObjectSyntax
{
	internal readonly InternalSyntaxToken _queryKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalQueryElementsSyntax _elements;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => QuerySyntaxDefinition.Instance.Value;

	public InternalSyntaxToken QueryKeyword => _queryKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalQueryElementsSyntax Elements => _elements;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalQuerySyntax(SyntaxKind kind, InternalSyntaxToken queryKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalQueryElementsSyntax elements, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(queryKeyword);
		_queryKeyword = queryKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			_elements = elements;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQuerySyntax(SyntaxKind kind, InternalSyntaxToken queryKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalQueryElementsSyntax elements, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(queryKeyword);
		_queryKeyword = queryKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			_elements = elements;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQuerySyntax(SyntaxKind kind, InternalSyntaxToken queryKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalQueryElementsSyntax elements, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(queryKeyword);
		_queryKeyword = queryKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			_elements = elements;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _queryKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _openBraceToken, 
			4 => _propertyList, 
			5 => _elements, 
			6 => _members, 
			7 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QuerySyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQuery(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQuery(this);
	}

	public InternalQuerySyntax Update(InternalSyntaxToken queryKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalQueryElementsSyntax elements, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (queryKeyword != QueryKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || elements != Elements || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.Query(queryKeyword, objectId, name, openBraceToken, propertyList, elements, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQuerySyntax(Kind, _queryKeyword, _objectId, _name, _openBraceToken, _propertyList, _elements, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQuerySyntax(Kind, _queryKeyword, _objectId, _name, _openBraceToken, _propertyList, _elements, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
