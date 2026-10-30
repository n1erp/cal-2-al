using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ObjectCompilationUnitSyntax : CompilationUnitSyntax
{
	private SyntaxNode _objects;

	public SyntaxList<ObjectSyntax> Objects => new SyntaxList<ObjectSyntax>(GetRed(ref _objects, 0));

	public SyntaxToken EndOfFileToken => new SyntaxToken(this, ((InternalObjectCompilationUnitSyntax)Node)._endOfFileToken, GetChildPosition(1), GetChildIndex(1));

	internal ObjectCompilationUnitSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _objects);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _objects;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectCompilationUnit(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitObjectCompilationUnit(this);
	}

	public ObjectCompilationUnitSyntax Update(SyntaxList<ObjectSyntax> objects, SyntaxToken endOfFileToken)
	{
		if (objects != Objects || endOfFileToken != EndOfFileToken)
		{
			ObjectCompilationUnitSyntax objectCompilationUnitSyntax = SyntaxFactory.ObjectCompilationUnit(objects, endOfFileToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return objectCompilationUnitSyntax.WithAnnotations(annotations);
			}
			return objectCompilationUnitSyntax;
		}
		return this;
	}

	public ObjectCompilationUnitSyntax WithObjects(SyntaxList<ObjectSyntax> objects)
	{
		return Update(objects, EndOfFileToken);
	}

	public ObjectCompilationUnitSyntax WithEndOfFileToken(SyntaxToken endOfFileToken)
	{
		return Update(Objects, endOfFileToken);
	}

	public ObjectCompilationUnitSyntax AddObjects(params ObjectSyntax[] items)
	{
		return WithObjects(Objects.AddRange(items));
	}

	internal override SyntaxToken GetEndOfFileToken()
	{
		return EndOfFileToken;
	}
}
