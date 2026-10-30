using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIdentifierNameOrEmptySyntax : InternalSyntaxNode
{
	internal readonly InternalIdentifierNameSyntax _identifierName;

	internal readonly InternalSyntaxToken _empty;

	internal override SyntaxNodeDefinition Definition => IdentifierNameOrEmptySyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax IdentifierName => _identifierName;

	public InternalSyntaxToken Empty => _empty;

	internal InternalIdentifierNameOrEmptySyntax(SyntaxKind kind, InternalIdentifierNameSyntax identifierName, InternalSyntaxToken empty, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		if (identifierName != null)
		{
			AdjustFlagsAndWidth(identifierName);
			_identifierName = identifierName;
		}
		if (empty != null)
		{
			AdjustFlagsAndWidth(empty);
			_empty = empty;
		}
	}

	internal InternalIdentifierNameOrEmptySyntax(SyntaxKind kind, InternalIdentifierNameSyntax identifierName, InternalSyntaxToken empty, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		if (identifierName != null)
		{
			AdjustFlagsAndWidth(identifierName);
			_identifierName = identifierName;
		}
		if (empty != null)
		{
			AdjustFlagsAndWidth(empty);
			_empty = empty;
		}
	}

	internal InternalIdentifierNameOrEmptySyntax(SyntaxKind kind, InternalIdentifierNameSyntax identifierName, InternalSyntaxToken empty)
		: base(kind)
	{
		SlotCount = 2;
		if (identifierName != null)
		{
			AdjustFlagsAndWidth(identifierName);
			_identifierName = identifierName;
		}
		if (empty != null)
		{
			AdjustFlagsAndWidth(empty);
			_empty = empty;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_identifierName, 
			1 => _empty, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new IdentifierNameOrEmptySyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierNameOrEmpty(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierNameOrEmpty(this);
	}

	public InternalIdentifierNameOrEmptySyntax Update(InternalIdentifierNameSyntax identifierName, InternalSyntaxToken empty)
	{
		if (identifierName != IdentifierName || empty != Empty)
		{
			return InternalSyntaxFactory.IdentifierNameOrEmpty(identifierName, empty).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIdentifierNameOrEmptySyntax(Kind, _identifierName, _empty, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIdentifierNameOrEmptySyntax(Kind, _identifierName, _empty, GetDiagnostics(), annotations);
	}
}
