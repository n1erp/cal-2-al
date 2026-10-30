using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class BracketedParameterListSyntax : BaseParameterListSyntax
{
	private SyntaxNode parameters;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedParameterListSyntax)Green).openBracketToken, Position, 0);

	public override SeparatedSyntaxList<ParameterSyntax> Parameters
	{
		get
		{
			SyntaxNode red = GetRed(ref parameters, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<ParameterSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedParameterListSyntax)Green).closeBracketToken, GetChildPosition(2), GetChildIndex(2));

	internal BracketedParameterListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitBracketedParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBracketedParameterList(this);
	}

	public BracketedParameterListSyntax Update(SyntaxToken openBracketToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || parameters != Parameters || closeBracketToken != CloseBracketToken)
		{
			BracketedParameterListSyntax bracketedParameterListSyntax = SyntaxFactory.BracketedParameterList(openBracketToken, parameters, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return bracketedParameterListSyntax.WithAnnotations(annotations);
			}
			return bracketedParameterListSyntax;
		}
		return this;
	}

	public BracketedParameterListSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Parameters, CloseBracketToken);
	}

	internal override BaseParameterListSyntax WithParametersCore(SeparatedSyntaxList<ParameterSyntax> parameters)
	{
		return WithParameters(parameters);
	}

	public new BracketedParameterListSyntax WithParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
	{
		return Update(OpenBracketToken, parameters, CloseBracketToken);
	}

	public BracketedParameterListSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Parameters, closeBracketToken);
	}

	internal override BaseParameterListSyntax AddParametersCore(params ParameterSyntax[] items)
	{
		return AddParameters(items);
	}

	public new BracketedParameterListSyntax AddParameters(params ParameterSyntax[] items)
	{
		return WithParameters(Parameters.AddRange(items));
	}
}
