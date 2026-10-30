using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal struct TypedConstantValue : IEquatable<TypedConstantValue>
{
	private readonly object _value;

	public bool IsNull => _value == null;

	public ImmutableArray<TypedConstant> Array
	{
		get
		{
			if (_value != null)
			{
				return (ImmutableArray<TypedConstant>)_value;
			}
			return default;
		}
	}

	public object Object => _value;

	internal TypedConstantValue(object value)
	{
		_value = value;
	}

	internal TypedConstantValue(ImmutableArray<TypedConstant> array)
	{
		_value = (array.IsDefault ? null : ((object)array));
	}

	public override int GetHashCode()
	{
		return _value?.GetHashCode() ?? 0;
	}

	public override bool Equals(object obj)
	{
		if (obj is TypedConstantValue)
		{
			return Equals((TypedConstantValue)obj);
		}
		return false;
	}

	public bool Equals(TypedConstantValue other)
	{
		return object.Equals(_value, other._value);
	}
}
