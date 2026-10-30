using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIdentifierNameSyntax : InternalSimpleNameSyntax
{
	internal readonly InternalSyntaxToken _identifier;

	internal override SyntaxNodeDefinition Definition => IdentifierNameSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken Identifier => _identifier;

	internal InternalIdentifierNameSyntax(SyntaxKind kind, InternalSyntaxToken identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalIdentifierNameSyntax(SyntaxKind kind, InternalSyntaxToken identifier, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalIdentifierNameSyntax(SyntaxKind kind, InternalSyntaxToken identifier)
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
		return new IdentifierNameSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierName(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierName(this);
	}

	public InternalIdentifierNameSyntax Update(InternalSyntaxToken identifier)
	{
		if (identifier != Identifier)
		{
			return InternalSyntaxFactory.IdentifierName(identifier).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIdentifierNameSyntax(Kind, _identifier, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIdentifierNameSyntax(Kind, _identifier, GetDiagnostics(), annotations);
	}
}
