using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavDateFormula : NavStringValue
{
	private static readonly NavDateFormula defaultValue = new NavDateFormula(string.Empty, isTokenString: true);

	private readonly string value;

	private byte[] bytes;

	public const int ByteSize = 32;

	internal const int MaxCharSize = 32;

	public override NavNclType NclType => NavNclType.NavDateFormula;

	public override int GetBytesSize => 32;

	public override object ValueAsObject => value;

	public override object ClientObject => NavDateFormulaFormatter.Format(this, NavCurrentThread.Session.FormatSettings, 0);

	public override bool IsZeroOrEmpty => string.IsNullOrEmpty(value);

	public string TokenString => value;

	public static NavDateFormula Default => defaultValue;

	public override string Value => value;

	public override int MaxLength => 32;

	public NavDateFormula(byte[] initValue)
	{
		if (initValue == null)
		{
			value = string.Empty;
			return;
		}
		bytes = initValue;
		value = NCLManagedAdapter.ByteToText(initValue);
	}

	public NavDateFormula(string initValue)
		: this(initValue, isTokenString: false)
	{
	}

	public NavDateFormula(string initValue, bool isTokenString)
	{
		if (isTokenString)
		{
			value = initValue;
		}
		else
		{
			value = NavDateFormulaEvaluator.Parse(initValue, 0);
		}
	}

	public static NavDateFormula Create(string initValue, bool isTokenString)
	{
		if (string.IsNullOrEmpty(initValue))
		{
			return Default;
		}
		return new NavDateFormula(initValue, isTokenString);
	}

	public static NavDateFormula CreateFromBytes(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length < 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.DateFormula);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.DateFormula);
		}
		byte[] array = new byte[length];
		Array.ConstrainedCopy(data, startIndex, array, 0, length);
		return new NavDateFormula(array);
	}

	public static NavDateFormula CreateFromBytes(byte[] data)
	{
		if (data == null)
		{
			return Default;
		}
		return new NavDateFormula(data);
	}

	public static NavDateFormula CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return new NavDateFormula(navValue.GetBytes());
		}
		if (value is byte[] initValue)
		{
			return new NavDateFormula(initValue);
		}
		if (value is string initValue2)
		{
			return new NavDateFormula(initValue2);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavDateFormula), value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override byte[] GetBytes()
	{
		if (bytes == null || bytes.Length != 32)
		{
			byte[] array = bytes ?? NCLManagedAdapter.TextToByte(value);
			Array.Resize(ref array, 32);
			bytes = array;
		}
		return bytes;
	}

	public NavDate CalcDate(NavDate date)
	{
		return NavDateFormulaEvaluator.CalcDate(value, date);
	}

	public override string ToString()
	{
		return NavDateFormulaFormatter.Format(this, NavCurrentThread.Session.FormatSettings, 0);
	}

	public override bool Equals(NavValue other)
	{
		if (this == other)
		{
			return true;
		}
		if (!(other is NavDateFormula navDateFormula))
		{
			return false;
		}
		return string.Equals(value, navDateFormula.value, StringComparison.Ordinal);
	}
}
