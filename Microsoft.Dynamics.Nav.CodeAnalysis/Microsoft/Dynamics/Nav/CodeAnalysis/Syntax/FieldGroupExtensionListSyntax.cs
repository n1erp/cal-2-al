using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldGroupExtensionListSyntax : SyntaxNode
{
	private SyntaxNode _changes;

	public SyntaxToken FieldGroupsKeyword => new SyntaxToken(this, ((InternalFieldGroupExtensionListSyntax)Node)._fieldGroupsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldGroupExtensionListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<FieldGroupChangeBaseSyntax> Changes => new SyntaxList<FieldGroupChangeBaseSyntax>(GetRed(ref _changes, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldGroupExtensionListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal FieldGroupExtensionListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _changes, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _changes;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroupExtensionList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldGroupExtensionList(this);
	}

	public FieldGroupExtensionListSyntax Update(SyntaxToken fieldGroupsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldGroupChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		if (fieldGroupsKeyword != FieldGroupsKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			FieldGroupExtensionListSyntax fieldGroupExtensionListSyntax = SyntaxFactory.FieldGroupExtensionList(fieldGroupsKeyword, openBraceToken, changes, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldGroupExtensionListSyntax.WithAnnotations(annotations);
			}
			return fieldGroupExtensionListSyntax;
		}
		return this;
	}

	public FieldGroupExtensionListSyntax WithFieldGroupsKeyword(SyntaxToken fieldGroupsKeyword)
	{
		return Update(fieldGroupsKeyword, OpenBraceToken, Changes, CloseBraceToken);
	}

	public FieldGroupExtensionListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FieldGroupsKeyword, openBraceToken, Changes, CloseBraceToken);
	}

	public FieldGroupExtensionListSyntax WithChanges(SyntaxList<FieldGroupChangeBaseSyntax> changes)
	{
		return Update(FieldGroupsKeyword, OpenBraceToken, changes, CloseBraceToken);
	}

	public FieldGroupExtensionListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FieldGroupsKeyword, OpenBraceToken, Changes, closeBraceToken);
	}

	public FieldGroupExtensionListSyntax AddChanges(params FieldGroupChangeBaseSyntax[] items)
	{
		return WithChanges(Changes.AddRange(items));
	}
}
