using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal abstract class TypeConstraintValidator : IGenericTypeConstraintValidator
{
	public abstract int NumberOfTemplateArguments { get; }

	public ImmutableArray<bool> ValidateTemplateArguments(TypeSymbol[] templateArguments)
	{
		ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance(templateArguments.Length);
		int i;
		for (i = 0; i < templateArguments.Length && i < NumberOfTemplateArguments; i++)
		{
			instance.Add(IsValidTypeArgument(i, templateArguments[i]));
		}
		for (; i < templateArguments.Length; i++)
		{
			instance.Add(item: false);
		}
		return instance.ToImmutableAndFree();
	}

	protected abstract bool IsValidTypeArgument(int position, TypeSymbol symbol);
}
