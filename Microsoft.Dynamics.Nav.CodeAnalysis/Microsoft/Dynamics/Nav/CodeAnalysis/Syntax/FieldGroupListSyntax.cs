using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldGroupListSyntax : SyntaxNode
{
	private SyntaxNode _fieldGroups;

	public SyntaxToken FieldGroupsKeyword => new SyntaxToken(this, ((InternalFieldGroupListSyntax)Node)._fieldGroupsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldGroupListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<FieldGroupSyntax> FieldGroups => new SyntaxList<FieldGroupSyntax>(GetRed(ref _fieldGroups, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldGroupListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal FieldGroupListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _fieldGroups, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _fieldGroups;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroupList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldGroupList(this);
	}

	public FieldGroupListSyntax Update(SyntaxToken fieldGroupsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldGroupSyntax> fieldGroups, SyntaxToken closeBraceToken)
	{
		if (fieldGroupsKeyword != FieldGroupsKeyword || openBraceToken != OpenBraceToken || fieldGroups != FieldGroups || closeBraceToken != CloseBraceToken)
		{
			FieldGroupListSyntax fieldGroupListSyntax = SyntaxFactory.FieldGroupList(fieldGroupsKeyword, openBraceToken, fieldGroups, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldGroupListSyntax.WithAnnotations(annotations);
			}
			return fieldGroupListSyntax;
		}
		return this;
	}

	public FieldGroupListSyntax WithFieldGroupsKeyword(SyntaxToken fieldGroupsKeyword)
	{
		return Update(fieldGroupsKeyword, OpenBraceToken, FieldGroups, CloseBraceToken);
	}

	public FieldGroupListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FieldGroupsKeyword, openBraceToken, FieldGroups, CloseBraceToken);
	}

	public FieldGroupListSyntax WithFieldGroups(SyntaxList<FieldGroupSyntax> fieldGroups)
	{
		return Update(FieldGroupsKeyword, OpenBraceToken, fieldGroups, CloseBraceToken);
	}

	public FieldGroupListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FieldGroupsKeyword, OpenBraceToken, FieldGroups, closeBraceToken);
	}

	public FieldGroupListSyntax AddFieldGroups(params FieldGroupSyntax[] items)
	{
		return WithFieldGroups(FieldGroups.AddRange(items));
	}
}
