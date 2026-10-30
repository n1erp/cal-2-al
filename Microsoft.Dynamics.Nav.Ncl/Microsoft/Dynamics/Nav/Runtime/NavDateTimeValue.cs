using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavDateTimeValue : NavValue
{
	protected internal DateTime value;

	public DateTime Value => value;

	public override object ValueAsObject => value;

	public override object ClientObject
	{
		get
		{
			if (value.Kind != DateTimeKind.Local)
			{
				return value;
			}
			return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
		}
	}

	public override bool IsZeroOrEmpty => Value == NavDateTimeHelper.DateTimeUndefined;

	public static implicit operator DateTime(NavDateTimeValue value)
	{
		return value?.Value ?? default(DateTime);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override DateTime ToDateTime()
	{
		return value;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj == this)
		{
			return true;
		}
		if (obj is NavValue other)
		{
			return Equals(other);
		}
		if (obj is DateTime)
		{
			return Value == (DateTime)obj;
		}
		return ThrowUnableToCompareException(obj);
	}

	public override bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (other.NavType == NavType.Date)
		{
			return Value == other.ToDateTime();
		}
		if (other is NavDateTimeValue)
		{
			return Value == other.ToDateTime();
		}
		return ThrowUnableToCompareException(other);
	}

	public static bool operator ==(NavDateTimeValue right, DateTime left)
	{
		if (right == null)
		{
			return left == default(DateTime);
		}
		return right.Value == left;
	}

	public static bool operator !=(NavDateTimeValue right, DateTime left)
	{
		return !(right == left);
	}

	public override int CompareTo(NavValue other)
	{
		if (other is NavDateTimeValue other2)
		{
			return CompareTo(other2);
		}
		ThrowUnableToCompareException(other);
		return 0;
	}

	public virtual int CompareTo(NavDateTimeValue other)
	{
		if (other == null)
		{
			return 1;
		}
		if ((object)other == this)
		{
			return 0;
		}
		return DateTime.Compare(Value, other.Value);
	}

	public static int Compare(NavDateTimeValue left, NavDateTimeValue right)
	{
		if (right == null)
		{
			if (left != null)
			{
				return 1;
			}
			return 0;
		}
		if (left == null)
		{
			return -1;
		}
		if ((object)left == right)
		{
			return 0;
		}
		return left.CompareTo(right);
	}

	public static bool operator <(NavDateTimeValue left, NavDateTimeValue right)
	{
		return Compare(left, right) < 0;
	}

	public static bool operator <=(NavDateTimeValue left, NavDateTimeValue right)
	{
		return Compare(left, right) <= 0;
	}

	public static bool operator >=(NavDateTimeValue left, NavDateTimeValue right)
	{
		return Compare(left, right) >= 0;
	}

	public static bool operator >(NavDateTimeValue left, NavDateTimeValue right)
	{
		return Compare(left, right) > 0;
	}
}
