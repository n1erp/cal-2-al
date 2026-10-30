namespace Microsoft.CodeAnalysis;

public readonly struct Optional<T>(T value)
{
	private readonly bool _hasValue = true;

	private readonly T _value = value;

	public bool HasValue => _hasValue;

	public T Value => _value;

	public static implicit operator Optional<T>(T value)
	{
		return new Optional<T>(value);
	}

	public override string ToString()
	{
		if (!_hasValue)
		{
			return "unspecified";
		}
		return _value?.ToString() ?? "null";
	}
}
