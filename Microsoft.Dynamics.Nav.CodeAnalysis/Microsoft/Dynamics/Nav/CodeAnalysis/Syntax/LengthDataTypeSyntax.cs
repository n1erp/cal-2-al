using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class LengthDataTypeSyntax : DataTypeSyntax
{
	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalLengthDataTypeSyntax)Node)._typeName, Position, 0);

	public SyntaxToken OpenBracketToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalLengthDataTypeSyntax)Node)._openBracketToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken Length
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalLengthDataTypeSyntax)Node)._length;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(2), GetChildIndex(2));
			}
			return default;
		}
	}

	public SyntaxToken CloseBracketToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalLengthDataTypeSyntax)Node)._closeBracketToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	internal LengthDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLengthDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLengthDataType(this);
	}

	public LengthDataTypeSyntax Update(SyntaxToken typeName, SyntaxToken openBracketToken, SyntaxToken length, SyntaxToken closeBracketToken)
	{
		if (typeName != TypeName || openBracketToken != OpenBracketToken || length != Length || closeBracketToken != CloseBracketToken)
		{
			LengthDataTypeSyntax lengthDataTypeSyntax = SyntaxFactory.LengthDataType(typeName, openBracketToken, length, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return lengthDataTypeSyntax.WithAnnotations(annotations);
			}
			return lengthDataTypeSyntax;
		}
		return this;
	}

	public LengthDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName, OpenBracketToken, Length, CloseBracketToken);
	}

	public LengthDataTypeSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(TypeName, openBracketToken, Length, CloseBracketToken);
	}

	public LengthDataTypeSyntax WithLength(SyntaxToken length)
	{
		return Update(TypeName, OpenBracketToken, length, CloseBracketToken);
	}

	public LengthDataTypeSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(TypeName, OpenBracketToken, Length, closeBracketToken);
	}
}
