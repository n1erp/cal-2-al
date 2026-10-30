using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalArgumentListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxNode _arguments;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ArgumentListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> Arguments => new InternalSeparatedSyntaxList<InternalCodeExpressionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_arguments));

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode arguments, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			_arguments = arguments;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode arguments, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			_arguments = arguments;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode arguments, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			_arguments = arguments;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _openParenthesisToken, 
			1 => _arguments, 
			2 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ArgumentListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArgumentList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitArgumentList(this);
	}

	public InternalArgumentListSyntax Update(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments, InternalSyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || arguments != Arguments || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ArgumentList(openParenthesisToken, arguments, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalArgumentListSyntax(Kind, _openParenthesisToken, _arguments, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalArgumentListSyntax(Kind, _openParenthesisToken, _arguments, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
