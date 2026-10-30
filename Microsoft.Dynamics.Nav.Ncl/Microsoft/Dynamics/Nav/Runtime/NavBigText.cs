using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavBigText : NavValue
{
	private static NavBigText defaultValue = new NavBigText(string.Empty);

	private string value;

	public int ALLength => value.Length;

	public static NavBigText Default => defaultValue;

	public string Value => value;

	public override NavNclType NclType => NavNclType.NavBigText;

	public override object ValueAsObject => value;

	public override int GetBytesSize => value.Length + 1;

	public override bool IsZeroOrEmpty => string.IsNullOrEmpty(value);

	public static NavBigText ALAddText(NavBigText bigText, string variable)
	{
		if (bigText == null)
		{
			throw new ArgumentNullException("bigText");
		}
		if (variable == null)
		{
			throw new ArgumentNullException("variable");
		}
		return ALAddText(bigText, variable, bigText.ALLength + 1);
	}

	public static NavBigText ALAddText(NavBigText bigText, string variable, int toPos1Based)
	{
		if (bigText == null)
		{
			throw new ArgumentNullException("bigText");
		}
		if (variable == null)
		{
			throw new ArgumentNullException("variable");
		}
		return new NavBigText(ALSystemString.ALInsStr(bigText.value, variable, toPos1Based));
	}

	public int ALGetSubText(ByRef<NavText> variable, int fromPos1Based)
	{
		string text = ALSystemString.ALCopyStr(Value, fromPos1Based, ALLength);
		variable.Value = new NavText(variable.Value.NavDefinedLengthMetadata, text);
		return text.Length;
	}

	public int ALGetSubText(ByRef<NavText> variable, int fromPos1Based, int length)
	{
		string text = ALSystemString.ALCopyStr(Value, fromPos1Based, length);
		variable.Value = new NavText(variable.Value.NavDefinedLengthMetadata, text);
		return text.Length;
	}

	public int ALGetSubText(ByRef<NavOemText> variable, int fromPos1Based)
	{
		string text = ALSystemString.ALCopyStr(Value, fromPos1Based, ALLength);
		variable.Value = new NavOemText(variable.Value.NavDefinedLengthMetadata, text);
		return text.Length;
	}

	public int ALGetSubText(ByRef<NavOemText> variable, int fromPos1Based, int length)
	{
		string text = ALSystemString.ALCopyStr(Value, fromPos1Based, length);
		variable.Value = new NavOemText(variable.Value.NavDefinedLengthMetadata, text);
		return text.Length;
	}

	public int ALGetSubText(ByRef<NavBigText> variable, int fromPos1Based)
	{
		string text = ALSystemString.ALCopyStr(Value, fromPos1Based, ALLength);
		variable.Value = new NavBigText(text);
		return text.Length;
	}

	public int ALGetSubText(ByRef<NavBigText> variable, int fromPos1Based, int length)
	{
		string text = ALSystemString.ALCopyStr(Value, fromPos1Based, length);
		variable.Value = new NavBigText(text);
		return text.Length;
	}

	public int ALTextPos(string substring)
	{
		return ALSystemString.ALStrPos(Value, substring);
	}

	public static bool ALRead(ByRef<NavBigText> text, NavInStream inStream)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (inStream == null)
		{
			throw new ArgumentNullException("inStream");
		}
		long num = (inStream.InternalStream.CanSeek ? inStream.Length : int.MaxValue);
		long num2 = 0L;
		string text2 = string.Empty;
		while (num > 0)
		{
			string empty = string.Empty;
			int num3 = ((num > 1000000) ? 1000000 : Convert.ToInt32(num));
			int num4 = ALStream.ALRead(inStream, DataError.ThrowError, ref empty, num3);
			if (((INavStreamMode)inStream).Mode == NavStreamMode.Stream)
			{
				if (inStream.TextEncoding == TextEncoding.UTF8)
				{
					num4--;
				}
				else if (inStream.TextEncoding == TextEncoding.UTF16)
				{
					num4 -= 2;
				}
			}
			num2 += num4;
			text2 = string.Join(string.Empty, text2, empty);
			if (num3 > num4)
			{
				break;
			}
			num -= num4;
		}
		text.Value = new NavBigText(text2);
		return num2 >= 0;
	}

	public bool ALWrite(NavOutStream outStream)
	{
		if (outStream == null)
		{
			throw new ArgumentNullException("outStream");
		}
		int num = ALStream.ALWriteStringBuffer(outStream, DataError.ThrowError, value);
		return num >= 0;
	}

	public NavBigText()
	{
		value = string.Empty;
	}

	public NavBigText(string value)
	{
		this.value = value;
	}

	internal NavBigText(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length <= 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.BigText);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.BigText);
		}
		value = NCLManagedAdapter.ByteToText(data, startIndex, length);
	}

	internal NavBigText(byte[] value)
	{
		this.value = NCLManagedAdapter.ByteToText(value);
	}

	public static NavBigText CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavBigText(data, startIndex, length);
	}

	public static NavBigText CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return new NavBigText(navValue.ToString());
		}
		if (value is string text)
		{
			return new NavBigText(text);
		}
		if (value is byte[] array)
		{
			return new NavBigText(array);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavBigText), value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override int ToInt32()
	{
		return int.Parse(value, NavCurrentThread.Session.WindowsCulture);
	}

	public override long ToInt64()
	{
		return long.Parse(value, NavCurrentThread.Session.WindowsCulture);
	}

	public override Decimal18 ToDecimal()
	{
		return Decimal18.Parse(value, NavCurrentThread.Session.WindowsCulture);
	}

	public override byte[] GetBytes()
	{
		return NCLManagedAdapter.TextToByte(value);
	}

	public static implicit operator string(NavBigText value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return value.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is NavBigText navBigText)
		{
			return Value.Equals(navBigText.Value);
		}
		if (obj is string text)
		{
			return Value.Equals(text);
		}
		return false;
	}
}
