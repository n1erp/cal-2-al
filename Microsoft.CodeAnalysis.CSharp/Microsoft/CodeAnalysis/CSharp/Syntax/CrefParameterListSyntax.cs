using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CrefParameterListSyntax : BaseCrefParameterListSyntax
{
	private SyntaxNode parameters;

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterListSyntax)Green).openParenToken, Position, 0);

	public override SeparatedSyntaxList<CrefParameterSyntax> Parameters
	{
		get
		{
			SyntaxNode red = GetRed(ref parameters, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<CrefParameterSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterListSyntax)Green).closeParenToken, GetChildPosition(2), GetChildIndex(2));

	internal CrefParameterListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref parameters, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return parameters;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCrefParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefParameterList(this);
	}

	public CrefParameterListSyntax Update(SyntaxToken openParenToken, SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || parameters != Parameters || closeParenToken != CloseParenToken)
		{
			CrefParameterListSyntax crefParameterListSyntax = SyntaxFactory.CrefParameterList(openParenToken, parameters, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return crefParameterListSyntax.WithAnnotations(annotations);
			}
			return crefParameterListSyntax;
		}
		return this;
	}

	public CrefParameterListSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(openParenToken, Parameters, CloseParenToken);
	}

	internal override BaseCrefParameterListSyntax WithParametersCore(SeparatedSyntaxList<CrefParameterSyntax> parameters)
	{
		return WithParameters(parameters);
	}

	public new CrefParameterListSyntax WithParameters(SeparatedSyntaxList<CrefParameterSyntax> parameters)
	{
		return Update(OpenParenToken, parameters, CloseParenToken);
	}

	public CrefParameterListSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(OpenParenToken, Parameters, closeParenToken);
	}

	internal override BaseCrefParameterListSyntax AddParametersCore(params CrefParameterSyntax[] items)
	{
		return AddParameters(items);
	}

	public new CrefParameterListSyntax AddParameters(params CrefParameterSyntax[] items)
	{
		return WithParameters(Parameters.AddRange(items));
	}
}
