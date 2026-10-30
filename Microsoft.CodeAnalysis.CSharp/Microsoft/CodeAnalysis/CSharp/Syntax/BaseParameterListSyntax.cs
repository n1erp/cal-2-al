using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseParameterListSyntax : CSharpSyntaxNode
{
	public abstract SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

	internal BaseParameterListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseParameterListSyntax WithParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
	{
		return WithParametersCore(parameters);
	}

	internal abstract BaseParameterListSyntax WithParametersCore(SeparatedSyntaxList<ParameterSyntax> parameters);

	public BaseParameterListSyntax AddParameters(params ParameterSyntax[] items)
	{
		return AddParametersCore(items);
	}

	internal abstract BaseParameterListSyntax AddParametersCore(params ParameterSyntax[] items);
}
