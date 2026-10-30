using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPermissionSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _objectType;

	internal readonly InternalObjectNameOrIdSyntax _objectReference;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _permissions;

	internal override SyntaxNodeDefinition Definition => PermissionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ObjectType => _objectType;

	public InternalObjectNameOrIdSyntax ObjectReference => _objectReference;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken Permissions => _permissions;

	internal InternalPermissionSyntax(SyntaxKind kind, InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectReference, InternalSyntaxToken equalsToken, InternalSyntaxToken permissions, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(objectType);
		_objectType = objectType;
		AdjustFlagsAndWidth(objectReference);
		_objectReference = objectReference;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (permissions != null)
		{
			AdjustFlagsAndWidth(permissions);
			_permissions = permissions;
		}
	}

	internal InternalPermissionSyntax(SyntaxKind kind, InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectReference, InternalSyntaxToken equalsToken, InternalSyntaxToken permissions, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(objectType);
		_objectType = objectType;
		AdjustFlagsAndWidth(objectReference);
		_objectReference = objectReference;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (permissions != null)
		{
			AdjustFlagsAndWidth(permissions);
			_permissions = permissions;
		}
	}

	internal InternalPermissionSyntax(SyntaxKind kind, InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectReference, InternalSyntaxToken equalsToken, InternalSyntaxToken permissions)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(objectType);
		_objectType = objectType;
		AdjustFlagsAndWidth(objectReference);
		_objectReference = objectReference;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (permissions != null)
		{
			AdjustFlagsAndWidth(permissions);
			_permissions = permissions;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_objectType, 
			1 => _objectReference, 
			2 => _equalsToken, 
			3 => _permissions, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PermissionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPermission(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPermission(this);
	}

	public InternalPermissionSyntax Update(InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectReference, InternalSyntaxToken equalsToken, InternalSyntaxToken permissions)
	{
		if (objectType != ObjectType || objectReference != ObjectReference || equalsToken != EqualsToken || permissions != Permissions)
		{
			return InternalSyntaxFactory.Permission(objectType, objectReference, equalsToken, permissions).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPermissionSyntax(Kind, _objectType, _objectReference, _equalsToken, _permissions, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPermissionSyntax(Kind, _objectType, _objectReference, _equalsToken, _permissions, GetDiagnostics(), annotations);
	}
}
