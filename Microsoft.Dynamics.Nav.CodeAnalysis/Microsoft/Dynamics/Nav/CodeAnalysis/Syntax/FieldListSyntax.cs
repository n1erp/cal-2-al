using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldListSyntax : SyntaxNode
{
	private SyntaxNode _fields;

	public SyntaxToken FieldsKeyword => new SyntaxToken(this, ((InternalFieldListSyntax)Node)._fieldsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<FieldSyntax> Fields => new SyntaxList<FieldSyntax>(GetRed(ref _fields, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal FieldListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitFieldList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldList(this);
	}

	public FieldListSyntax Update(SyntaxToken fieldsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldSyntax> fields, SyntaxToken closeBraceToken)
	{
		if (fieldsKeyword != FieldsKeyword || openBraceToken != OpenBraceToken || fields != Fields || closeBraceToken != CloseBraceToken)
		{
			FieldListSyntax fieldListSyntax = SyntaxFactory.FieldList(fieldsKeyword, openBraceToken, fields, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldListSyntax.WithAnnotations(annotations);
			}
			return fieldListSyntax;
		}
		return this;
	}

	public FieldListSyntax WithFieldsKeyword(SyntaxToken fieldsKeyword)
	{
		return Update(fieldsKeyword, OpenBraceToken, Fields, CloseBraceToken);
	}

	public FieldListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FieldsKeyword, openBraceToken, Fields, CloseBraceToken);
	}

	public FieldListSyntax WithFields(SyntaxList<FieldSyntax> fields)
	{
		return Update(FieldsKeyword, OpenBraceToken, fields, CloseBraceToken);
	}

	public FieldListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FieldsKeyword, OpenBraceToken, Fields, closeBraceToken);
	}

	public FieldListSyntax AddFields(params FieldSyntax[] items)
	{
		return WithFields(Fields.AddRange(items));
	}
}
