using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInt64SignedLiteralValueSyntax : InternalLiteralValueSyntax
{
	internal readonly InternalSyntaxToken _sign;

	internal readonly InternalSyntaxToken _number;

	internal override SyntaxNodeDefinition Definition => Int64SignedLiteralValueSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken Sign => _sign;

	public InternalSyntaxToken Number => _number;

	internal InternalInt64SignedLiteralValueSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken number, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		if (sign != null)
		{
			AdjustFlagsAndWidth(sign);
			_sign = sign;
		}
		AdjustFlagsAndWidth(number);
		_number = number;
	}

	internal InternalInt64SignedLiteralValueSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken number, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		if (sign != null)
		{
			AdjustFlagsAndWidth(sign);
			_sign = sign;
		}
		AdjustFlagsAndWidth(number);
		_number = number;
	}

	internal InternalInt64SignedLiteralValueSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken number)
		: base(kind)
	{
		SlotCount = 2;
		if (sign != null)
		{
			AdjustFlagsAndWidth(sign);
			_sign = sign;
		}
		AdjustFlagsAndWidth(number);
		_number = number;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_sign, 
			1 => _number, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Int64SignedLiteralValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInt64SignedLiteralValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInt64SignedLiteralValue(this);
	}

	public InternalInt64SignedLiteralValueSyntax Update(InternalSyntaxToken sign, InternalSyntaxToken number)
	{
		if (sign != Sign || number != Number)
		{
			return InternalSyntaxFactory.Int64SignedLiteralValue(sign, number).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInt64SignedLiteralValueSyntax(Kind, _sign, _number, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInt64SignedLiteralValueSyntax(Kind, _sign, _number, GetDiagnostics(), annotations);
	}
}
