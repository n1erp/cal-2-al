using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldExtensionListSyntax : SyntaxNode
{
	private SyntaxNode _fields;

	public SyntaxToken FieldsKeyword => new SyntaxToken(this, ((InternalFieldExtensionListSyntax)Node)._fieldsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldExtensionListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<FieldBaseSyntax> Fields => new SyntaxList<FieldBaseSyntax>(GetRed(ref _fields, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldExtensionListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal FieldExtensionListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _fields, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _fields;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldExtensionList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldExtensionList(this);
	}

	public FieldExtensionListSyntax Update(SyntaxToken fieldsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldBaseSyntax> fields, SyntaxToken closeBraceToken)
	{
		if (fieldsKeyword != FieldsKeyword || openBraceToken != OpenBraceToken || fields != Fields || closeBraceToken != CloseBraceToken)
		{
			FieldExtensionListSyntax fieldExtensionListSyntax = SyntaxFactory.FieldExtensionList(fieldsKeyword, openBraceToken, fields, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldExtensionListSyntax.WithAnnotations(annotations);
			}
			return fieldExtensionListSyntax;
		}
		return this;
	}

	public FieldExtensionListSyntax WithFieldsKeyword(SyntaxToken fieldsKeyword)
	{
		return Update(fieldsKeyword, OpenBraceToken, Fields, CloseBraceToken);
	}

	public FieldExtensionListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FieldsKeyword, openBraceToken, Fields, CloseBraceToken);
	}

	public FieldExtensionListSyntax WithFields(SyntaxList<FieldBaseSyntax> fields)
	{
		return Update(FieldsKeyword, OpenBraceToken, fields, CloseBraceToken);
	}

	public FieldExtensionListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FieldsKeyword, OpenBraceToken, Fields, closeBraceToken);
	}

	public FieldExtensionListSyntax AddFields(params FieldBaseSyntax[] items)
	{
		return WithFields(Fields.AddRange(items));
	}
}
