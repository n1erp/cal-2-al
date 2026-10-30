using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalObjectNameOrIdSyntax : InternalSyntaxNode
{
	internal readonly InternalCodeExpressionSyntax _identifier;

	internal override SyntaxNodeDefinition Definition => ObjectNameOrIdSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Identifier => _identifier;

	internal InternalObjectNameOrIdSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalObjectNameOrIdSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalObjectNameOrIdSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _identifier;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ObjectNameOrIdSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectNameOrId(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitObjectNameOrId(this);
	}

	public InternalObjectNameOrIdSyntax Update(InternalCodeExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			return InternalSyntaxFactory.ObjectNameOrId(identifier).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalObjectNameOrIdSyntax(Kind, _identifier, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalObjectNameOrIdSyntax(Kind, _identifier, GetDiagnostics(), annotations);
	}
}
