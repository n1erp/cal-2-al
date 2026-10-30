using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIdentifierEqualsStringSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalSyntaxToken _identifier;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalStringLiteralValueSyntax _stringLiteral;

	internal override SyntaxNodeDefinition Definition => IdentifierEqualsStringSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken Identifier => _identifier;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalStringLiteralValueSyntax StringLiteral => _stringLiteral;

	internal InternalIdentifierEqualsStringSyntax(SyntaxKind kind, InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalStringLiteralValueSyntax stringLiteral, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(stringLiteral);
		_stringLiteral = stringLiteral;
	}

	internal InternalIdentifierEqualsStringSyntax(SyntaxKind kind, InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalStringLiteralValueSyntax stringLiteral, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(stringLiteral);
		_stringLiteral = stringLiteral;
	}

	internal InternalIdentifierEqualsStringSyntax(SyntaxKind kind, InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalStringLiteralValueSyntax stringLiteral)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(stringLiteral);
		_stringLiteral = stringLiteral;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_identifier, 
			1 => _equalsToken, 
			2 => _stringLiteral, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new IdentifierEqualsStringSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierEqualsString(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierEqualsString(this);
	}

	public InternalIdentifierEqualsStringSyntax Update(InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalStringLiteralValueSyntax stringLiteral)
	{
		if (identifier != Identifier || equalsToken != EqualsToken || stringLiteral != StringLiteral)
		{
			return InternalSyntaxFactory.IdentifierEqualsString(identifier, equalsToken, stringLiteral).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIdentifierEqualsStringSyntax(Kind, _identifier, _equalsToken, _stringLiteral, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIdentifierEqualsStringSyntax(Kind, _identifier, _equalsToken, _stringLiteral, GetDiagnostics(), annotations);
	}
}
