using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalMemberAccessExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalSyntaxToken _dotToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal override SyntaxNodeDefinition Definition => MemberAccessExpressionSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalSyntaxToken DotToken => _dotToken;

	public InternalIdentifierNameSyntax Name => _name;

	internal InternalMemberAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken dotToken, InternalIdentifierNameSyntax name, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(dotToken);
		_dotToken = dotToken;
		AdjustFlagsAndWidth(name);
		_name = name;
	}

	internal InternalMemberAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken dotToken, InternalIdentifierNameSyntax name, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(dotToken);
		_dotToken = dotToken;
		AdjustFlagsAndWidth(name);
		_name = name;
	}

	internal InternalMemberAccessExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax expression, InternalSyntaxToken dotToken, InternalIdentifierNameSyntax name)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(dotToken);
		_dotToken = dotToken;
		AdjustFlagsAndWidth(name);
		_name = name;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_expression, 
			1 => _dotToken, 
			2 => _name, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new MemberAccessExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberAccessExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitMemberAccessExpression(this);
	}

	public InternalMemberAccessExpressionSyntax Update(InternalCodeExpressionSyntax expression, InternalSyntaxToken dotToken, InternalIdentifierNameSyntax name)
	{
		if (expression != Expression || dotToken != DotToken || name != Name)
		{
			return InternalSyntaxFactory.MemberAccessExpression(expression, dotToken, name).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalMemberAccessExpressionSyntax(Kind, _expression, _dotToken, _name, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalMemberAccessExpressionSyntax(Kind, _expression, _dotToken, _name, GetDiagnostics(), annotations);
	}
}
