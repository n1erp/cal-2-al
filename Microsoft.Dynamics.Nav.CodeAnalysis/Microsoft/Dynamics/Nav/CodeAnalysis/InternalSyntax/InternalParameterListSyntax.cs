using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalParameterListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxNode _parameters;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ParameterListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSeparatedSyntaxList<InternalParameterSyntax> Parameters => new InternalSeparatedSyntaxList<InternalParameterSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_parameters));

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalParameterListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode parameters, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			_parameters = parameters;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalParameterListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode parameters, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			_parameters = parameters;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalParameterListSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode parameters, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			_parameters = parameters;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _openParenthesisToken, 
			1 => _parameters, 
			2 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ParameterListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParameterList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitParameterList(this);
	}

	public InternalParameterListSyntax Update(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalParameterSyntax> parameters, InternalSyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || parameters != Parameters || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ParameterList(openParenthesisToken, parameters, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalParameterListSyntax(Kind, _openParenthesisToken, _parameters, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalParameterListSyntax(Kind, _openParenthesisToken, _parameters, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
