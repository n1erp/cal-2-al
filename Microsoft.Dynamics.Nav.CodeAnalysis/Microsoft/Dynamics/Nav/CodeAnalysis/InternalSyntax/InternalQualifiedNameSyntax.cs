using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQualifiedNameSyntax : InternalNameSyntax
{
	internal readonly InternalNameSyntax _left;

	internal readonly InternalSyntaxToken _dotToken;

	internal readonly InternalSimpleNameSyntax _right;

	internal override SyntaxNodeDefinition Definition => QualifiedNameSyntaxDefinition.Instance.Value;

	public InternalNameSyntax Left => _left;

	public InternalSyntaxToken DotToken => _dotToken;

	public InternalSimpleNameSyntax Right => _right;

	internal InternalQualifiedNameSyntax(SyntaxKind kind, InternalNameSyntax left, InternalSyntaxToken dotToken, InternalSimpleNameSyntax right, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(dotToken);
		_dotToken = dotToken;
		AdjustFlagsAndWidth(right);
		_right = right;
	}

	internal InternalQualifiedNameSyntax(SyntaxKind kind, InternalNameSyntax left, InternalSyntaxToken dotToken, InternalSimpleNameSyntax right, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(dotToken);
		_dotToken = dotToken;
		AdjustFlagsAndWidth(right);
		_right = right;
	}

	internal InternalQualifiedNameSyntax(SyntaxKind kind, InternalNameSyntax left, InternalSyntaxToken dotToken, InternalSimpleNameSyntax right)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(dotToken);
		_dotToken = dotToken;
		AdjustFlagsAndWidth(right);
		_right = right;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_left, 
			1 => _dotToken, 
			2 => _right, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QualifiedNameSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQualifiedName(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQualifiedName(this);
	}

	public InternalQualifiedNameSyntax Update(InternalNameSyntax left, InternalSyntaxToken dotToken, InternalSimpleNameSyntax right)
	{
		if (left != Left || dotToken != DotToken || right != Right)
		{
			return InternalSyntaxFactory.QualifiedName(left, dotToken, right).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQualifiedNameSyntax(Kind, _left, _dotToken, _right, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQualifiedNameSyntax(Kind, _left, _dotToken, _right, GetDiagnostics(), annotations);
	}
}
