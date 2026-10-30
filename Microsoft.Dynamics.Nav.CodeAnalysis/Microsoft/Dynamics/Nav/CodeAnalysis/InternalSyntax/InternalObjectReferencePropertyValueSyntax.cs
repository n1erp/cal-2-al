using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalObjectReferencePropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalObjectNameOrIdSyntax _objectNameOrId;

	internal override SyntaxNodeDefinition Definition => ObjectReferencePropertyValueSyntaxDefinition.Instance.Value;

	public InternalObjectNameOrIdSyntax ObjectNameOrId => _objectNameOrId;

	internal InternalObjectReferencePropertyValueSyntax(SyntaxKind kind, InternalObjectNameOrIdSyntax objectNameOrId, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(objectNameOrId);
		_objectNameOrId = objectNameOrId;
	}

	internal InternalObjectReferencePropertyValueSyntax(SyntaxKind kind, InternalObjectNameOrIdSyntax objectNameOrId, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(objectNameOrId);
		_objectNameOrId = objectNameOrId;
	}

	internal InternalObjectReferencePropertyValueSyntax(SyntaxKind kind, InternalObjectNameOrIdSyntax objectNameOrId)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(objectNameOrId);
		_objectNameOrId = objectNameOrId;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _objectNameOrId;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ObjectReferencePropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectReferencePropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitObjectReferencePropertyValue(this);
	}

	public InternalObjectReferencePropertyValueSyntax Update(InternalObjectNameOrIdSyntax objectNameOrId)
	{
		if (objectNameOrId != ObjectNameOrId)
		{
			return InternalSyntaxFactory.ObjectReferencePropertyValue(objectNameOrId).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalObjectReferencePropertyValueSyntax(Kind, _objectNameOrId, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalObjectReferencePropertyValueSyntax(Kind, _objectNameOrId, GetDiagnostics(), annotations);
	}
}
