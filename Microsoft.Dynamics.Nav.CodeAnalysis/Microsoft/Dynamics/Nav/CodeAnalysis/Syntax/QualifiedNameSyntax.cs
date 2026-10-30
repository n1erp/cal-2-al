using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QualifiedNameSyntax : NameSyntax
{
	private NameSyntax _left;

	private SimpleNameSyntax _right;

	public NameSyntax Left => GetRedAtZero(ref _left);

	public SyntaxToken DotToken => new SyntaxToken(this, ((InternalQualifiedNameSyntax)Node)._dotToken, GetChildPosition(1), GetChildIndex(1));

	public SimpleNameSyntax Right => GetRed(ref _right, 2);

	internal QualifiedNameSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _left), 
			2 => GetRed(ref _right, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_left, 
			2 => _right, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQualifiedName(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQualifiedName(this);
	}

	public QualifiedNameSyntax Update(NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right)
	{
		if (left != Left || dotToken != DotToken || right != Right)
		{
			QualifiedNameSyntax qualifiedNameSyntax = SyntaxFactory.QualifiedName(left, dotToken, right);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return qualifiedNameSyntax.WithAnnotations(annotations);
			}
			return qualifiedNameSyntax;
		}
		return this;
	}

	public QualifiedNameSyntax WithLeft(NameSyntax left)
	{
		return Update(left, DotToken, Right);
	}

	public QualifiedNameSyntax WithDotToken(SyntaxToken dotToken)
	{
		return Update(Left, dotToken, Right);
	}

	public QualifiedNameSyntax WithRight(SimpleNameSyntax right)
	{
		return Update(Left, DotToken, right);
	}
}
