using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ParameterListSyntax : BaseParameterListSyntax
{
	private SyntaxNode parameters;

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)Green).openParenToken, Position, 0);

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

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax)Green).closeParenToken, GetChildPosition(2), GetChildIndex(2));

	internal int ParameterCount
	{
		get
		{
			int num = 0;
			foreach (ParameterSyntax parameter in Parameters)
			{
				if (!parameter.IsArgList)
				{
					num++;
				}
			}
			return num;
		}
	}

	internal ParameterListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParameterList(this);
	}

	public ParameterListSyntax Update(SyntaxToken openParenToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || parameters != Parameters || closeParenToken != CloseParenToken)
		{
			ParameterListSyntax parameterListSyntax = SyntaxFactory.ParameterList(openParenToken, parameters, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parameterListSyntax.WithAnnotations(annotations);
			}
			return parameterListSyntax;
		}
		return this;
	}

	public ParameterListSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(openParenToken, Parameters, CloseParenToken);
	}

	internal override BaseParameterListSyntax WithParametersCore(SeparatedSyntaxList<ParameterSyntax> parameters)
	{
		return WithParameters(parameters);
	}

	public new ParameterListSyntax WithParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
	{
		return Update(OpenParenToken, parameters, CloseParenToken);
	}

	public ParameterListSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(OpenParenToken, Parameters, closeParenToken);
	}

	internal override BaseParameterListSyntax AddParametersCore(params ParameterSyntax[] items)
	{
		return AddParameters(items);
	}

	public new ParameterListSyntax AddParameters(params ParameterSyntax[] items)
	{
		return WithParameters(Parameters.AddRange(items));
	}
}
