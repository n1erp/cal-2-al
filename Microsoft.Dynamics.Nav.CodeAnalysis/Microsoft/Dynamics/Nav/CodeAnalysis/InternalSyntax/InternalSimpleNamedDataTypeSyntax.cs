using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalSimpleNamedDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal override SyntaxNodeDefinition Definition => SimpleNamedDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	internal InternalSimpleNamedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
	}

	internal InternalSimpleNamedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
	}

	internal InternalSimpleNamedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _typeName;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new SimpleNamedDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleNamedDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitSimpleNamedDataType(this);
	}

	public InternalSimpleNamedDataTypeSyntax Update(InternalSyntaxToken typeName)
	{
		if (typeName != TypeName)
		{
			return InternalSyntaxFactory.SimpleNamedDataType(typeName).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalSimpleNamedDataTypeSyntax(Kind, _typeName, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalSimpleNamedDataTypeSyntax(Kind, _typeName, GetDiagnostics(), annotations);
	}
}
