using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIdentifierEqualsLiteralSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalSyntaxToken _identifier;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalLiteralValueSyntax _literal;

	internal override SyntaxNodeDefinition Definition => IdentifierEqualsLiteralSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken Identifier => _identifier;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalLiteralValueSyntax Literal => _literal;

	internal InternalIdentifierEqualsLiteralSyntax(SyntaxKind kind, InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalLiteralValueSyntax literal, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal InternalIdentifierEqualsLiteralSyntax(SyntaxKind kind, InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalLiteralValueSyntax literal, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal InternalIdentifierEqualsLiteralSyntax(SyntaxKind kind, InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalLiteralValueSyntax literal)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_identifier, 
			1 => _equalsToken, 
			2 => _literal, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new IdentifierEqualsLiteralSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierEqualsLiteral(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierEqualsLiteral(this);
	}

	public InternalIdentifierEqualsLiteralSyntax Update(InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalLiteralValueSyntax literal)
	{
		if (identifier != Identifier || equalsToken != EqualsToken || literal != Literal)
		{
			return InternalSyntaxFactory.IdentifierEqualsLiteral(identifier, equalsToken, literal).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIdentifierEqualsLiteralSyntax(Kind, _identifier, _equalsToken, _literal, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIdentifierEqualsLiteralSyntax(Kind, _identifier, _equalsToken, _literal, GetDiagnostics(), annotations);
	}
}
