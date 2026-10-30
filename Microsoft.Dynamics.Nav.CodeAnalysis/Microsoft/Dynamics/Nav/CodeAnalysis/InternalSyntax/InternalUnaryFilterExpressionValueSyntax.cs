using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalUnaryFilterExpressionValueSyntax : InternalFilterExpressionValueSyntax
{
	internal readonly InternalSyntaxToken _operatorToken;

	internal readonly InternalIdentifierOrLiteralExpressionSyntax _identifier;

	internal override SyntaxNodeDefinition Definition => UnaryFilterExpressionValueSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OperatorToken => _operatorToken;

	public InternalIdentifierOrLiteralExpressionSyntax Identifier => _identifier;

	internal InternalUnaryFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalIdentifierOrLiteralExpressionSyntax identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		if (operatorToken != null)
		{
			AdjustFlagsAndWidth(operatorToken);
			_operatorToken = operatorToken;
		}
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalUnaryFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalIdentifierOrLiteralExpressionSyntax identifier, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		if (operatorToken != null)
		{
			AdjustFlagsAndWidth(operatorToken);
			_operatorToken = operatorToken;
		}
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalUnaryFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalIdentifierOrLiteralExpressionSyntax identifier)
		: base(kind)
	{
		SlotCount = 2;
		if (operatorToken != null)
		{
			AdjustFlagsAndWidth(operatorToken);
			_operatorToken = operatorToken;
		}
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_operatorToken, 
			1 => _identifier, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new UnaryFilterExpressionValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUnaryFilterExpressionValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitUnaryFilterExpressionValue(this);
	}

	public InternalUnaryFilterExpressionValueSyntax Update(InternalSyntaxToken operatorToken, InternalIdentifierOrLiteralExpressionSyntax identifier)
	{
		if (operatorToken != OperatorToken || identifier != Identifier)
		{
			return InternalSyntaxFactory.UnaryFilterExpressionValue(Kind, operatorToken, identifier).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalUnaryFilterExpressionValueSyntax(Kind, _operatorToken, _identifier, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalUnaryFilterExpressionValueSyntax(Kind, _operatorToken, _identifier, GetDiagnostics(), annotations);
	}
}
