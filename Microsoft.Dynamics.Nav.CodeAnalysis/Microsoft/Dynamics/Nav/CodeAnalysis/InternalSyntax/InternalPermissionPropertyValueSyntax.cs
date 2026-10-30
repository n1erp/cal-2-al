using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPermissionPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalSyntaxNode _permissionProperties;

	internal override SyntaxNodeDefinition Definition => PermissionPropertyValueSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalPermissionSyntax> PermissionProperties => new InternalSeparatedSyntaxList<InternalPermissionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_permissionProperties));

	internal InternalPermissionPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode permissionProperties, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (permissionProperties != null)
		{
			AdjustFlagsAndWidth(permissionProperties);
			_permissionProperties = permissionProperties;
		}
	}

	internal InternalPermissionPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode permissionProperties, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (permissionProperties != null)
		{
			AdjustFlagsAndWidth(permissionProperties);
			_permissionProperties = permissionProperties;
		}
	}

	internal InternalPermissionPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode permissionProperties)
		: base(kind)
	{
		SlotCount = 1;
		if (permissionProperties != null)
		{
			AdjustFlagsAndWidth(permissionProperties);
			_permissionProperties = permissionProperties;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _permissionProperties;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PermissionPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPermissionPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPermissionPropertyValue(this);
	}

	public InternalPermissionPropertyValueSyntax Update(InternalSeparatedSyntaxList<InternalPermissionSyntax> permissionProperties)
	{
		if (permissionProperties != PermissionProperties)
		{
			return InternalSyntaxFactory.PermissionPropertyValue(permissionProperties).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPermissionPropertyValueSyntax(Kind, _permissionProperties, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPermissionPropertyValueSyntax(Kind, _permissionProperties, GetDiagnostics(), annotations);
	}
}
