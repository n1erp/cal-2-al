using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ParameterListSyntax : SyntaxNode
{
	private SyntaxNode _parameters;

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalParameterListSyntax)Node)._openParenthesisToken, Position, 0);

	public SeparatedSyntaxList<ParameterSyntax> Parameters
	{
		get
		{
			SyntaxNode red = GetRed(ref _parameters, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<ParameterSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalParameterListSyntax)Node)._closeParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	internal ParameterListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _parameters, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _parameters;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParameterList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitParameterList(this);
	}

	public ParameterListSyntax Update(SyntaxToken openParenthesisToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || parameters != Parameters || closeParenthesisToken != CloseParenthesisToken)
		{
			ParameterListSyntax parameterListSyntax = SyntaxFactory.ParameterList(openParenthesisToken, parameters, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parameterListSyntax.WithAnnotations(annotations);
			}
			return parameterListSyntax;
		}
		return this;
	}

	public ParameterListSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(openParenthesisToken, Parameters, CloseParenthesisToken);
	}

	public ParameterListSyntax WithParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
	{
		return Update(OpenParenthesisToken, parameters, CloseParenthesisToken);
	}

	public ParameterListSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OpenParenthesisToken, Parameters, closeParenthesisToken);
	}

	public ParameterListSyntax AddParameters(params ParameterSyntax[] items)
	{
		return WithParameters(Parameters.AddRange(items));
	}
}
