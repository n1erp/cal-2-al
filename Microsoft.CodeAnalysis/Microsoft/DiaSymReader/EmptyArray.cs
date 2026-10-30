namespace Microsoft.DiaSymReader;

internal static class EmptyArray<T>
{
	public static readonly T[] Instance = new T[0];
}
