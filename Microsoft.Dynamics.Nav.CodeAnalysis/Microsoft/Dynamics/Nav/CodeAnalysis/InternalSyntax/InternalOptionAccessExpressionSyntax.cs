using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOptionAccessExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalSyntaxToken _colonColonToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal override SyntaxNodeDefinition Definition => OptionAccessExpressionSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalSyntaxToken ColonColonToken => _colonColonToken;

	public InternalIdentifierNameSyntax Name => _name;

	internal InternalOptionAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(colonColonToken);
		_colonColonToken = colonColonToken;
		AdjustFlagsAndWidth(name);
		_name = name;
	}

	internal InternalOptionAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(colonColonToken);
		_colonColonToken = colonColonToken;
		AdjustFlagsAndWidth(name);
		_name = name;
	}

	internal InternalOptionAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(colonColonToken);
		_colonColonToken = colonColonToken;
		AdjustFlagsAndWidth(name);
		_name = name;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_expression, 
			1 => _colonColonToken, 
			2 => _name, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OptionAccessExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionAccessExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOptionAccessExpression(this);
	}

	public InternalOptionAccessExpressionSyntax Update(InternalCodeExpressionSyntax expression, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name)
	{
		if (expression != Expression || colonColonToken != ColonColonToken || name != Name)
		{
			return InternalSyntaxFactory.OptionAccessExpression(expression, colonColonToken, name).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOptionAccessExpressionSyntax(Kind, _expression, _colonColonToken, _name, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOptionAccessExpressionSyntax(Kind, _expression, _colonColonToken, _name, GetDiagnostics(), annotations);
	}
}
