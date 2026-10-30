using System;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavText : NavStringValue
{
	private static readonly NavText defaultValue = new NavText(0, string.Empty);

	private static readonly NavText nullValue = new NavText(0, (string)null);

	private string value;

	private int maxLength;

	private bool isNull;

	public const int MaxSize = int.MaxValue;

	public const int MaxDefinedSize = 2048;

	private static readonly NavText[] cachedNavTexts = new NavText[2048];

	private static readonly NavText[] cachedNavTextNulls = new NavText[2048];

	private static readonly Func<int, NavText> createDefault = (int maxLength) => new NavText(maxLength, string.Empty);

	private static readonly Func<int, NavText> createDefaultNull = (int maxLength) => new NavText(maxLength, (string)null);

	public static NavText Empty => defaultValue;

	public static NavText Null => nullValue;

	public override string Value => value;

	public override int MaxLength => maxLength;

	public new char this[int index] => ALSystemString.GetChar(Value, index);

	public override bool IsNull => isNull;

	public override NavNclType NclType => NavNclType.NavText;

	public override object ValueAsObject => Value;

	public override int GetBytesSize => (value.Length + 1) * 2;

	public override int NavDefinedLengthMetadata
	{
		get
		{
			if (maxLength != int.MaxValue)
			{
				return maxLength;
			}
			return 0;
		}
	}

	public NavText(int maxLength, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			isNull = value == null;
			value = string.Empty;
		}
		SetMaxLength(maxLength);
		if (this.maxLength > 0 && value.Length > this.maxLength)
		{
			throw new NavNCLStringLengthExceededException(this.maxLength, value.Length, value);
		}
		this.value = value;
	}

	public NavText(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			isNull = value == null;
			value = string.Empty;
		}
		maxLength = int.MaxValue;
		this.value = value;
	}

	internal NavText(int maxLength, byte[] data, int startIndex, int length)
	{
		SetMaxLength(maxLength);
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length < 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Text);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Text);
		}
		value = NavStringValue.GetStringFromBytes(data, startIndex, length);
	}

	internal NavText(int maxLength, byte[] value)
	{
		SetMaxLength(maxLength);
		if ((value.Length & 1) != 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Text);
		}
		this.value = NavStringValue.GetStringFromBytes(value, 0, value.Length);
	}

	internal unsafe NavText(int maxLength, IntPtr source)
	{
		SetMaxLength(maxLength);
		char* ptr = (char*)source.ToPointer();
		value = new string(ptr);
	}

	public static NavText GetDefaultForMaxLength(int maxLength)
	{
		return NavStringValue.GetDefaultForMaxLength(cachedNavTexts, maxLength, createDefault);
	}

	public static NavText GetNullForMaxLength(int maxLength)
	{
		return NavStringValue.GetDefaultForMaxLength(cachedNavTextNulls, maxLength, createDefaultNull);
	}

	public static NavText CreateFromBytes(int maxLength, byte[] data, int startIndex, int length)
	{
		return new NavText(maxLength, data, startIndex, length);
	}

	public static NavText CreateFromObject(int maxLength, object value)
	{
		if (value == null)
		{
			NavText navText = Default(maxLength);
			navText.isNull = true;
			return navText;
		}
		if (value is NavValue navValue)
		{
			return new NavText(maxLength, navValue.ToString());
		}
		if (value is string text)
		{
			return new NavText(maxLength, text);
		}
		if (value is byte[] array)
		{
			return new NavText(maxLength, array);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavText), value);
	}

	public static NavText CreateFromObject(object value)
	{
		return CreateFromObject(int.MaxValue, value);
	}

	public static NavText Create(string value)
	{
		return new NavText(value);
	}

	public static NavText CreateTruncated(int maxLength, string value)
	{
		return new NavText(maxLength, NavStringValue.Truncate(value, maxLength));
	}

	private void SetMaxLength(int newMaxLength)
	{
		maxLength = ((newMaxLength == 0) ? int.MaxValue : newMaxLength);
	}

	public NavText Default()
	{
		return GetDefaultForMaxLength(NavDefinedLengthMetadata);
	}

	public NavText ModifyLength(int newLength)
	{
		if (maxLength == newLength)
		{
			return this;
		}
		return new NavText(newLength, value);
	}

	public static NavText Default(int maxLength)
	{
		return GetDefaultForMaxLength(maxLength);
	}

	public static NavText NullWithMaxLength(int maxLength)
	{
		return GetNullForMaxLength(maxLength);
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
		byte[] array = new byte[(value.Length + 1) * 2];
		Encoding.Unicode.GetBytes(value, 0, value.Length, array, 0);
		return array;
	}
}
