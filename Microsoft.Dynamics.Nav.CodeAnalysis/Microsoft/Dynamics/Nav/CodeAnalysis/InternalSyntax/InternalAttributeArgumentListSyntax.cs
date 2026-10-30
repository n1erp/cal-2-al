using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalAttributeArgumentListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxNode _arguments;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => AttributeArgumentListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> Arguments => new InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_arguments));

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalAttributeArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode arguments, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
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

	internal InternalAttributeArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode arguments, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
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

	internal InternalAttributeArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode arguments, InternalSyntaxToken closeParenthesisToken)
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
		return new AttributeArgumentListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeArgumentList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitAttributeArgumentList(this);
	}

	public InternalAttributeArgumentListSyntax Update(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> arguments, InternalSyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || arguments != Arguments || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.AttributeArgumentList(openParenthesisToken, arguments, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalAttributeArgumentListSyntax(Kind, _openParenthesisToken, _arguments, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalAttributeArgumentListSyntax(Kind, _openParenthesisToken, _arguments, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
