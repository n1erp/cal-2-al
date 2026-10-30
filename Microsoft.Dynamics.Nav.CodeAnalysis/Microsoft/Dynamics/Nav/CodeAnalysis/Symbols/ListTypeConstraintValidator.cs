namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ListTypeConstraintValidator : TypeConstraintValidator
{
	public static IGenericTypeConstraintValidator Instance { get; } = new ListTypeConstraintValidator();

	public override int NumberOfTemplateArguments { get; } = 1;

	private ListTypeConstraintValidator()
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
