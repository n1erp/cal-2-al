namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class DictionaryTypeConstraintValidator : TypeConstraintValidator
{
	public static IGenericTypeConstraintValidator Instance { get; } = new DictionaryTypeConstraintValidator();

	public override int NumberOfTemplateArguments { get; } = 2;

	private DictionaryTypeConstraintValidator()
	{
	}

	protected override bool IsValidTypeArgument(int position, TypeSymbol symbol)
	{
		NavTypeKind navTypeKindSafe = symbol.GetNavTypeKindSafe();
		if (navTypeKindSafe.IsNonComplexValue())
		{
			return navTypeKindSafe != NavTypeKind.Label;
		}
		return false;
	}
}
