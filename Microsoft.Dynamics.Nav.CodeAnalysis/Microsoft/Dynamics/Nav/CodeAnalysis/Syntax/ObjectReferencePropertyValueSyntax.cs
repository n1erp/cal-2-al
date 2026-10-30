using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ObjectReferencePropertyValueSyntax : PropertyValueSyntax
{
	private ObjectNameOrIdSyntax _objectNameOrId;

	public ObjectNameOrIdSyntax ObjectNameOrId => GetRedAtZero(ref _objectNameOrId);

	internal ObjectReferencePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _objectNameOrId);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _objectNameOrId;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectReferencePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitObjectReferencePropertyValue(this);
	}

	public ObjectReferencePropertyValueSyntax Update(ObjectNameOrIdSyntax objectNameOrId)
	{
		if (objectNameOrId != ObjectNameOrId)
		{
			ObjectReferencePropertyValueSyntax objectReferencePropertyValueSyntax = SyntaxFactory.ObjectReferencePropertyValue(objectNameOrId);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return objectReferencePropertyValueSyntax.WithAnnotations(annotations);
			}
			return objectReferencePropertyValueSyntax;
		}
		return this;
	}

	public ObjectReferencePropertyValueSyntax WithObjectNameOrId(ObjectNameOrIdSyntax objectNameOrId)
	{
		return Update(objectNameOrId);
	}
}
