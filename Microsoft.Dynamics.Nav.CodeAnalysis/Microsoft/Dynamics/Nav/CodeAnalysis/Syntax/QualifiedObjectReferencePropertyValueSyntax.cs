using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QualifiedObjectReferencePropertyValueSyntax : PropertyValueSyntax
{
	private ObjectNameOrIdSyntax _objectNameOrId;

	public SyntaxToken ObjectType => new SyntaxToken(this, ((InternalQualifiedObjectReferencePropertyValueSyntax)Node)._objectType, Position, 0);

	public ObjectNameOrIdSyntax ObjectNameOrId => GetRed(ref _objectNameOrId, 1);

	internal QualifiedObjectReferencePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _objectNameOrId, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _objectNameOrId;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQualifiedObjectReferencePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQualifiedObjectReferencePropertyValue(this);
	}

	public QualifiedObjectReferencePropertyValueSyntax Update(SyntaxToken objectType, ObjectNameOrIdSyntax objectNameOrId)
	{
		if (objectType != ObjectType || objectNameOrId != ObjectNameOrId)
		{
			QualifiedObjectReferencePropertyValueSyntax qualifiedObjectReferencePropertyValueSyntax = SyntaxFactory.QualifiedObjectReferencePropertyValue(objectType, objectNameOrId);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return qualifiedObjectReferencePropertyValueSyntax.WithAnnotations(annotations);
			}
			return qualifiedObjectReferencePropertyValueSyntax;
		}
		return this;
	}

	public QualifiedObjectReferencePropertyValueSyntax WithObjectType(SyntaxToken objectType)
	{
		return Update(objectType, ObjectNameOrId);
	}

	public QualifiedObjectReferencePropertyValueSyntax WithObjectNameOrId(ObjectNameOrIdSyntax objectNameOrId)
	{
		return Update(ObjectType, objectNameOrId);
	}
}
