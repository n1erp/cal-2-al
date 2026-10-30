using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQualifiedObjectReferencePropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalSyntaxToken _objectType;

	internal readonly InternalObjectNameOrIdSyntax _objectNameOrId;

	internal override SyntaxNodeDefinition Definition => QualifiedObjectReferencePropertyValueSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ObjectType => _objectType;

	public InternalObjectNameOrIdSyntax ObjectNameOrId => _objectNameOrId;

	internal InternalQualifiedObjectReferencePropertyValueSyntax(SyntaxKind kind, InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectNameOrId, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(objectType);
		_objectType = objectType;
		AdjustFlagsAndWidth(objectNameOrId);
		_objectNameOrId = objectNameOrId;
	}

	internal InternalQualifiedObjectReferencePropertyValueSyntax(SyntaxKind kind, InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectNameOrId, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(objectType);
		_objectType = objectType;
		AdjustFlagsAndWidth(objectNameOrId);
		_objectNameOrId = objectNameOrId;
	}

	internal InternalQualifiedObjectReferencePropertyValueSyntax(SyntaxKind kind, InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectNameOrId)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(objectType);
		_objectType = objectType;
		AdjustFlagsAndWidth(objectNameOrId);
		_objectNameOrId = objectNameOrId;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_objectType, 
			1 => _objectNameOrId, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QualifiedObjectReferencePropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQualifiedObjectReferencePropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQualifiedObjectReferencePropertyValue(this);
	}

	public InternalQualifiedObjectReferencePropertyValueSyntax Update(InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectNameOrId)
	{
		if (objectType != ObjectType || objectNameOrId != ObjectNameOrId)
		{
			return InternalSyntaxFactory.QualifiedObjectReferencePropertyValue(objectType, objectNameOrId).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQualifiedObjectReferencePropertyValueSyntax(Kind, _objectType, _objectNameOrId, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQualifiedObjectReferencePropertyValueSyntax(Kind, _objectType, _objectNameOrId, GetDiagnostics(), annotations);
	}
}
