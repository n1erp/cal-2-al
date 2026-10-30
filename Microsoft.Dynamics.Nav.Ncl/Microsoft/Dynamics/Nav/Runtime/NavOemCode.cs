using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavOemCode : NavCodeBase
{
	private readonly string value;

	private readonly short maxDefinedLength;

	private readonly byte alphaIndex;

	public const int MaxSize = 2048;

	private static readonly NavOemCode[] cachedNavOemCodes = new NavOemCode[2048];

	private static readonly Func<int, NavOemCode> createDefault = (int maxLength) => new NavOemCode(maxLength, string.Empty);

	public override byte AlphaIndex => alphaIndex;

	public override string Value => value;

	public override int MaxLength => maxDefinedLength;

	public override NavNclType NclType => NavNclType.NavOemCode;

	public override int GetBytesSize => value.Length + 2;

	public override object ValueAsObject => Value;

	public override int NavDefinedLengthMetadata => maxDefinedLength;

	public NavOemCode(int maxLength, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
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
			this.value = StringHelper.ToUpperUsingNavCultureInfoSafe(value);
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

	public NavOemCode(int maxLength, long numericValue)
	{
		maxDefinedLength = (short)maxLength;
		value = numericValue.ToString(CultureInfo.InvariantCulture);
		alphaIndex = (byte)((numericValue < 0) ? 255u : ((uint)value.Length));
		if (value.Length > maxLength)
		{
			throw new NavNCLStringLengthExceededException(maxLength, value.Length, value);
		}
	}

	internal NavOemCode(int maxLength, byte[] value)
	{
		maxDefinedLength = (short)maxLength;
		if (value.Length == 0)
		{
			this.value = string.Empty;
			return;
		}
		alphaIndex = value[0];
		this.value = NCLManagedAdapter.ByteToText(value, 1, value.Length - 1);
	}

	internal NavOemCode(int maxLength, byte[] data, int startIndex, int length)
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
		value = NCLManagedAdapter.ByteToText(data, startIndex + 1, length - 1);
	}

	public static NavOemCode GetDefaultForMaxLength(int maxLength)
	{
		return NavStringValue.GetDefaultForMaxLength(cachedNavOemCodes, maxLength, createDefault);
	}

	public static NavOemCode CreateFromBytes(int maxLength, byte[] data, int startIndex, int length)
	{
		return new NavOemCode(maxLength, data, startIndex, length);
	}

	public static NavOemCode CreateFromObject(int maxLength, object value)
	{
		if (value == null)
		{
			return Default(maxLength);
		}
		if (value is NavValue navValue)
		{
			return new NavOemCode(maxLength, navValue.ToString());
		}
		if (value is string text)
		{
			return new NavOemCode(maxLength, text);
		}
		if (value is byte[] array)
		{
			return new NavOemCode(maxLength, array);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavOemCode), value);
	}

	public static NavOemCode CreateFromObject(object value)
	{
		return CreateFromObject(2048, value);
	}

	public NavOemCode ModifyLength(int newLength)
	{
		if (maxDefinedLength == newLength)
		{
			return this;
		}
		return new NavOemCode(newLength, value);
	}

	public NavOemCode UpdateWithSqlDataType(SqlDataType sqlDataType)
	{
		if (AlphaIndex != 0 && AlphaIndex != byte.MaxValue && value[0] == '0')
		{
			string text = GetSqlWhereClauseValue(sqlDataType).ToString();
			return new NavOemCode(MaxLength, text);
		}
		return this;
	}

	public NavOemCode Default()
	{
		return GetDefaultForMaxLength(MaxLength);
	}

	public static NavOemCode Default(int maxLength)
	{
		return GetDefaultForMaxLength(maxLength);
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
			if (string.IsNullOrEmpty(value) || value == "''")
			{
				return 0L;
			}
			if (value[0] == '0' || !long.TryParse(value, out var result))
			{
				return long.MaxValue;
			}
			return result;
		}
		case SqlDataType.Integer:
		{
			if (string.IsNullOrEmpty(value) || value == "''")
			{
				return 0;
			}
			if (value[0] == '0' || !int.TryParse(value, out var result2))
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
			long num2 = ToInt64();
			if (num2 < 0 || num2 == long.MaxValue)
			{
				throw new NavCSideException(22928072);
			}
			return num2;
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
			int num = ToInt32();
			if (num < 0 || num == int.MaxValue)
			{
				throw new NavCSideException(22928072);
			}
			return num;
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
		byte[] array = NCLManagedAdapter.TextToByte(" " + value);
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
		NavOemCode navOemCode = other as NavOemCode;
		if (navOemCode != null)
		{
			if (alphaIndex < navOemCode.alphaIndex)
			{
				return -1;
			}
			if (alphaIndex > navOemCode.alphaIndex)
			{
				return 1;
			}
		}
		return base.CompareTo(other);
	}
}
