namespace Microsoft.CodeAnalysis;

internal static class TypeCompareKindExtension
{
	public static TypeCompareKind AddIgnoreCustomModifiersAndArraySizesAndLowerBounds(this TypeCompareKind self, bool condition)
	{
		if (!condition)
		{
			return self;
		}
		return self | TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds;
	}

	public static TypeCompareKind AddIgnoreDynamic(this TypeCompareKind self, bool condition)
	{
		if (!condition)
		{
			return self;
		}
		return self | TypeCompareKind.IgnoreDynamic;
	}

	public static TypeCompareKind AddIgnoreTupleNames(this TypeCompareKind self, bool condition)
	{
		if (!condition)
		{
			return self;
		}
		return self | TypeCompareKind.IgnoreTupleNames;
	}
}
