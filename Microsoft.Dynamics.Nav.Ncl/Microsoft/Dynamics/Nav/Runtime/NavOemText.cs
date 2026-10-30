using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavOemText : NavStringValue
{
	private static NavOemText defaultValue = new NavOemText(0, string.Empty);

	private string value;

	private short maxLength;

	private bool isNull;

	public const int MaxSize = 2048;

	private static readonly NavOemText[] cachedNavOemTexts = new NavOemText[2048];

	private static readonly Func<int, NavOemText> createDefault = (int maxLength) => new NavOemText(maxLength, string.Empty);

	public static NavOemText Empty => defaultValue;

	public override string Value => value;

	public override int MaxLength => maxLength;

	public override bool IsNull => isNull;

	public override NavNclType NclType => NavNclType.NavOemText;

	public override object ValueAsObject => Value;

	public override int GetBytesSize => value.Length + 1;

	public override int NavDefinedLengthMetadata => maxLength;

	public NavOemText(int maxLength, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			isNull = value == null;
			value = string.Empty;
		}
		if (value.Length > maxLength)
		{
			throw new NavNCLStringLengthExceededException(maxLength, value.Length, value);
		}
		this.maxLength = (short)maxLength;
		this.value = value;
	}

	internal NavOemText(int maxLength, byte[] data, int startIndex, int length)
	{
		this.maxLength = (short)maxLength;
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length < 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.OemText);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.OemText);
		}
		value = NCLManagedAdapter.ByteToText(data, startIndex, length);
	}

	internal NavOemText(int maxLength, byte[] value)
	{
		this.maxLength = (short)maxLength;
		this.value = NCLManagedAdapter.ByteToText(value);
	}

	public static NavOemText GetDefaultForMaxLength(int maxLength)
	{
		return NavStringValue.GetDefaultForMaxLength(cachedNavOemTexts, maxLength, createDefault);
	}

	public static NavOemText CreateFromBytes(int maxLength, byte[] data, int startIndex, int length)
	{
		return new NavOemText(maxLength, data, startIndex, length);
	}

	public static NavOemText CreateFromObject(int maxLength, object value)
	{
		if (value == null)
		{
			NavOemText navOemText = Default(maxLength);
			navOemText.isNull = true;
			return navOemText;
		}
		if (value is NavValue navValue)
		{
			return new NavOemText(maxLength, navValue.ToString());
		}
		if (value is string text)
		{
			return new NavOemText(maxLength, text);
		}
		if (value is byte[] array)
		{
			return new NavOemText(maxLength, array);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavOemText), value);
	}

	public static NavOemText CreateFromObject(object value)
	{
		return CreateFromObject(2048, value);
	}

	public NavOemText Default()
	{
		return GetDefaultForMaxLength(NavDefinedLengthMetadata);
	}

	public NavOemText ModifyLength(int newLength)
	{
		if (maxLength == newLength)
		{
			return this;
		}
		return new NavOemText(newLength, value);
	}

	public static NavOemText Default(int maxLength)
	{
		return GetDefaultForMaxLength(maxLength);
	}

	public override int ToInt32()
	{
		return int.Parse(Value, NavCurrentThread.Session.WindowsCulture);
	}

	public override long ToInt64()
	{
		return long.Parse(Value, NavCurrentThread.Session.WindowsCulture);
	}

	public override Decimal18 ToDecimal()
	{
		return Decimal18.Parse(Value, NavCurrentThread.Session.WindowsCulture);
	}

	public override byte[] GetBytes()
	{
		return NCLManagedAdapter.TextToByte(value);
	}
}
