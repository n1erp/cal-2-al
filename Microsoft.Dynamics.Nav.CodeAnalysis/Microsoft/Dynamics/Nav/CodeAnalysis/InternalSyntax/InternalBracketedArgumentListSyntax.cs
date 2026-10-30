using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalBracketedArgumentListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _openBracketToken;

	internal readonly InternalSyntaxNode _arguments;

	internal readonly InternalSyntaxToken _closeBracketToken;

	internal override SyntaxNodeDefinition Definition => BracketedArgumentListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenBracketToken => _openBracketToken;

	public InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> Arguments => new InternalSeparatedSyntaxList<InternalCodeExpressionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_arguments));

	public InternalSyntaxToken CloseBracketToken => _closeBracketToken;

	internal InternalBracketedArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalSyntaxNode arguments, InternalSyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			_arguments = arguments;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal InternalBracketedArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalSyntaxNode arguments, InternalSyntaxToken closeBracketToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			_arguments = arguments;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal InternalBracketedArgumentListSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalSyntaxNode arguments, InternalSyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			_arguments = arguments;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _openBracketToken, 
			1 => _arguments, 
			2 => _closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new BracketedArgumentListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBracketedArgumentList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitBracketedArgumentList(this);
	}

	public InternalBracketedArgumentListSyntax Update(InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments, InternalSyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || arguments != Arguments || closeBracketToken != CloseBracketToken)
		{
			return InternalSyntaxFactory.BracketedArgumentList(openBracketToken, arguments, closeBracketToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalBracketedArgumentListSyntax(Kind, _openBracketToken, _arguments, _closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalBracketedArgumentListSyntax(Kind, _openBracketToken, _arguments, _closeBracketToken, GetDiagnostics(), annotations);
	}
}
