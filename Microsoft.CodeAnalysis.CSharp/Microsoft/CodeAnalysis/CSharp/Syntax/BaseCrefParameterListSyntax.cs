using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseCrefParameterListSyntax : CSharpSyntaxNode
{
	public abstract SeparatedSyntaxList<CrefParameterSyntax> Parameters { get; }

	internal BaseCrefParameterListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseCrefParameterListSyntax WithParameters(SeparatedSyntaxList<CrefParameterSyntax> parameters)
	{
		return WithParametersCore(parameters);
	}

	internal abstract BaseCrefParameterListSyntax WithParametersCore(SeparatedSyntaxList<CrefParameterSyntax> parameters);

	public BaseCrefParameterListSyntax AddParameters(params CrefParameterSyntax[] items)
	{
		return AddParametersCore(items);
	}

	internal abstract BaseCrefParameterListSyntax AddParametersCore(params CrefParameterSyntax[] items);
}
