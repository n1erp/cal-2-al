using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavCode : NavCodeBase
{
	private string value;

	private bool needsUpperCasing;

	private readonly short maxDefinedLength;

	private readonly byte alphaIndex;

	private bool isNull;

	public const int MaxSize = 2048;

	private static readonly NavCode[] cachedNavCodes = new NavCode[2048];

	private static readonly NavCode[] cachedNavCodeNulls = new NavCode[2048];

	private static readonly Func<int, NavCode> createDefault = (int maxLength) => new NavCode(maxLength, string.Empty);

	private static readonly Func<int, NavCode> createDefaultNull = (int maxLength) => new NavCode(maxLength, (string)null);

	public override byte AlphaIndex => alphaIndex;

	public override string Value
	{
		get
		{
			EnsureValueIsUppercasedIfNeeded();
			return value;
		}
	}

	public override int MaxLength => maxDefinedLength;

	public new char this[int index] => ALSystemString.GetChar(Value, index);

	public override bool IsNull => isNull;

	public override NavNclType NclType => NavNclType.NavCode;

	public override int GetBytesSize => (value.Length + 1) * 2 + 1;

	public override object ValueAsObject => Value;

	public override int NavDefinedLengthMetadata => maxDefinedLength;

	public NavCode(int maxLength, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			isNull = value == null;
			value = string.Empty;
		}
		value = value.Trim();
		if (value.Length > maxLength)
		{
			throw new NavNCLStringLengthExceededException(maxLength, value.Length, value);
		}
		maxDefinedLength = (short)maxLength;
		if (value.Length == 0)
		{
			this.value = value;
			return;
		}
		int num = value.Length - 1;
		while (num >= 0 && char.IsDigit(value[num]))
		{
			num--;
		}
		if (num >= 0)
		{
			alphaIndex = byte.MaxValue;
			needsUpperCasing = true;
			this.value = value;
			return;
		}
		if (value.Length < 255)
		{
			alphaIndex = (byte)value.Length;
		}
		else
		{
			alphaIndex = byte.MaxValue;
		}
		this.value = value;
	}

	public NavCode(int maxLength, long numericValue)
	{
		maxDefinedLength = (short)maxLength;
		value = numericValue.ToString(CultureInfo.InvariantCulture);
		alphaIndex = (byte)((numericValue < 0) ? 255u : ((uint)value.Length));
		if (value.Length > maxLength)
		{
			throw new NavNCLStringLengthExceededException(maxLength, value.Length, value);
		}
	}

	internal NavCode(int maxLength, byte[] value)
	{
		maxDefinedLength = (short)maxLength;
		if (value.Length == 0)
		{
			this.value = string.Empty;
			return;
		}
		alphaIndex = value[0];
		this.value = NavStringValue.GetStringFromBytes(value, 1, value.Length - 1);
		if (this.value.Length <= maxLength)
		{
			return;
		}
		throw new NavNCLStringLengthExceededException(maxLength, this.value.Length, this.value);
	}

	internal NavCode(int maxLength, byte[] value, TextEncoding encoding)
	{
		maxDefinedLength = (short)maxLength;
		if (value.Length == 0)
		{
			this.value = string.Empty;
			return;
		}
		alphaIndex = value[0];
		if (encoding == TextEncoding.MSDos || encoding == TextEncoding.Windows)
		{
			this.value = NCLManagedAdapter.ByteToText(value, 1, value.Length - 1, encoding);
		}
		else
		{
			this.value = NavStringValue.GetStringFromBytes(value, 1, value.Length - 1);
		}
		if (this.value.Length <= maxLength)
		{
			return;
		}
		throw new NavNCLStringLengthExceededException(maxLength, this.value.Length, this.value);
	}

	internal NavCode(int maxLength, byte[] data, int startIndex, int length)
	{
		maxDefinedLength = (short)maxLength;
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length <= 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Boolean);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Boolean);
		}
		alphaIndex = data[startIndex];
		value = NavStringValue.GetStringFromBytes(data, startIndex + 1, length - 1);
		if (value.Length > maxLength)
		{
			throw new NavNCLStringLengthExceededException(maxLength, value.Length, value);
		}
	}

	public static NavCode GetDefaultForMaxLength(int maxLength)
	{
		return NavStringValue.GetDefaultForMaxLength(cachedNavCodes, maxLength, createDefault);
	}

	public static NavCode CreateFromBytes(int maxLength, byte[] data, int startIndex, int length)
	{
		return new NavCode(maxLength, data, startIndex, length);
	}

	public static NavCode CreateFromObject(int maxLength, object value)
	{
		if (value == null)
		{
			NavCode navCode = Default(maxLength);
			navCode.isNull = true;
			return navCode;
		}
		if (value is NavValue navValue)
		{
			return new NavCode(maxLength, navValue.ToString());
		}
		if (value is string text)
		{
			return new NavCode(maxLength, text);
		}
		if (value is byte[] array)
		{
			return new NavCode(maxLength, array);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavCode), value);
	}

	public static NavCode CreateFromObject(object value)
	{
		return CreateFromObject(2048, value);
	}

	public static NavCode CreateTruncated(int maxLength, string value)
	{
		return new NavCode(maxLength, NavStringValue.Truncate(value, maxLength));
	}

	public NavCode ModifyLength(int newLength)
	{
		if (maxDefinedLength == newLength)
		{
			return this;
		}
		return new NavCode(newLength, value);
	}

	public NavCode UpdateWithSqlDataType(SqlDataType sqlDataType)
	{
		if (AlphaIndex != 0 && AlphaIndex != byte.MaxValue && value[0] == '0')
		{
			string text = GetSqlWhereClauseValue(sqlDataType).ToString();
			return new NavCode(MaxLength, text);
		}
		return this;
	}

	public NavCode Default()
	{
		return GetDefaultForMaxLength(NavDefinedLengthMetadata);
	}

	public static NavCode Default(int maxLength)
	{
		return GetDefaultForMaxLength(maxLength);
	}

	public static NavCode NullWithMaxLength(int maxLength)
	{
		return GetNullForMaxLength(maxLength);
	}

	public static NavCode GetNullForMaxLength(int maxLength)
	{
		return NavStringValue.GetDefaultForMaxLength(cachedNavCodeNulls, maxLength, createDefaultNull);
	}

	private void EnsureValueIsUppercasedIfNeeded()
	{
		if (needsUpperCasing)
		{
			value = StringHelper.ToUpperUsingNavCultureInfoSafe(value);
			needsUpperCasing = false;
		}
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		switch (sqlDataType)
		{
		case SqlDataType.VarChar:
		case SqlDataType.Default:
			return Value;
		case SqlDataType.BigInt:
		{
			long result = 0L;
			if (string.IsNullOrEmpty(value) || value == "''")
			{
				return 0L;
			}
			if (value[0] == '0' || !long.TryParse(value, out result))
			{
				return long.MaxValue;
			}
			return result;
		}
		case SqlDataType.Integer:
		{
			int result2 = 0;
			if (string.IsNullOrEmpty(value) || value == "''")
			{
				return 0;
			}
			if (value[0] == '0' || !int.TryParse(value, out result2))
			{
				return int.MaxValue;
			}
			return result2;
		}
		case SqlDataType.Variant:
			if (AlphaIndex == byte.MaxValue || AlphaIndex == 0)
			{
				return Value;
			}
			return ToInt64();
		default:
			throw new NotSupportedException();
		}
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		switch (sqlDataType)
		{
		case SqlDataType.VarChar:
		case SqlDataType.Default:
			if (value.Length > maxLength)
			{
				throw new NavCSideException(22928068);
			}
			return Value;
		case SqlDataType.BigInt:
		{
			if (string.IsNullOrEmpty(value))
			{
				return 0L;
			}
			if (value[0] == '0')
			{
				throw new NavCSideException(22928070);
			}
			long result = 0L;
			if (!long.TryParse(value, out result) || result < 0 || result == long.MaxValue)
			{
				throw new NavCSideException(22928072);
			}
			return result;
		}
		case SqlDataType.Integer:
		{
			if (string.IsNullOrEmpty(value))
			{
				return 0;
			}
			if (value[0] == '0')
			{
				throw new NavCSideException(22928070);
			}
			int result2 = 0;
			if (!int.TryParse(value, out result2) || result2 < 0 || result2 == int.MaxValue)
			{
				throw new NavCSideException(22928072);
			}
			return result2;
		}
		case SqlDataType.Variant:
			if (AlphaIndex == byte.MaxValue || AlphaIndex == 0)
			{
				if (value.Length > maxLength)
				{
					throw new NavCSideException(22928068);
				}
				return Value;
			}
			return ToInt64();
		default:
			throw new NotSupportedException();
		}
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
		return GetBytes(TextEncoding.UTF16);
	}

	internal byte[] GetBytes(TextEncoding encoding)
	{
		EnsureValueIsUppercasedIfNeeded();
		byte[] array;
		if (encoding == TextEncoding.MSDos || encoding == TextEncoding.Windows)
		{
			array = NCLManagedAdapter.TextToByte(" " + value, encoding);
		}
		else
		{
			int num = ((encoding == TextEncoding.UTF16) ? (Encoding.Unicode.GetByteCount(value) + 3) : (Encoding.UTF8.GetByteCount(value) + 2));
			array = new byte[num];
			if (encoding == TextEncoding.UTF16)
			{
				Encoding.Unicode.GetBytes(value, 0, value.Length, array, 1);
			}
			else
			{
				Encoding.UTF8.GetBytes(value, 0, value.Length, array, 1);
			}
		}
		array[0] = alphaIndex;
		return array;
	}

	public override int CompareTo(NavStringValue other)
	{
		if ((object)other == null)
		{
			return 1;
		}
		if ((object)other == this)
		{
			return 0;
		}
		NavCode navCode = other as NavCode;
		if (navCode != null)
		{
			if (alphaIndex < navCode.alphaIndex)
			{
				return -1;
			}
			if (alphaIndex > navCode.alphaIndex)
			{
				return 1;
			}
		}
		return base.CompareTo(other);
	}
}
