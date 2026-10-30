using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CrefBracketedParameterListSyntax : BaseCrefParameterListSyntax
{
	private SyntaxNode parameters;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefBracketedParameterListSyntax)Green).openBracketToken, Position, 0);

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

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefBracketedParameterListSyntax)Green).closeBracketToken, GetChildPosition(2), GetChildIndex(2));

	internal CrefBracketedParameterListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitCrefBracketedParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefBracketedParameterList(this);
	}

	public CrefBracketedParameterListSyntax Update(SyntaxToken openBracketToken, SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || parameters != Parameters || closeBracketToken != CloseBracketToken)
		{
			CrefBracketedParameterListSyntax crefBracketedParameterListSyntax = SyntaxFactory.CrefBracketedParameterList(openBracketToken, parameters, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return crefBracketedParameterListSyntax.WithAnnotations(annotations);
			}
			return crefBracketedParameterListSyntax;
		}
		return this;
	}

	public CrefBracketedParameterListSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Parameters, CloseBracketToken);
	}

	internal override BaseCrefParameterListSyntax WithParametersCore(SeparatedSyntaxList<CrefParameterSyntax> parameters)
	{
		return WithParameters(parameters);
	}

	public new CrefBracketedParameterListSyntax WithParameters(SeparatedSyntaxList<CrefParameterSyntax> parameters)
	{
		return Update(OpenBracketToken, parameters, CloseBracketToken);
	}

	public CrefBracketedParameterListSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Parameters, closeBracketToken);
	}

	internal override BaseCrefParameterListSyntax AddParametersCore(params CrefParameterSyntax[] items)
	{
		return AddParameters(items);
	}

	public new CrefBracketedParameterListSyntax AddParameters(params CrefParameterSyntax[] items)
	{
		return WithParameters(Parameters.AddRange(items));
	}
}
