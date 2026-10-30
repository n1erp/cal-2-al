using System;
using System.Globalization;
using System.IO;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class DecimalPlacesType : SimpleType
{
	private byte minDecimals;

	private byte maxDecimals;

	private byte hasValue;

	private const int HighCache = 16;

	private static DecimalPlacesType[,] cache = new DecimalPlacesType[16, 16];

	private static DecimalPlacesType empty = new DecimalPlacesType();

	public override DataTypes DataType => DataTypes.DecimalPlaces;

	public override bool HasValue => hasValue > 0;

	public override string ValueAsString
	{
		get
		{
			if (HasValue)
			{
				return minDecimals.ToString(CultureInfo.InvariantCulture) + ":" + ((maxDecimals == byte.MaxValue) ? string.Empty : maxDecimals.ToString(CultureInfo.InvariantCulture));
			}
			return string.Empty;
		}
	}

	public static DecimalPlacesType Empty => empty;

	private DecimalPlacesType()
	{
	}

	private DecimalPlacesType(byte minDecimals, byte maxDecimals)
	{
		this.minDecimals = minDecimals;
		this.maxDecimals = maxDecimals;
		hasValue = 1;
	}

	public static DecimalPlacesType Create(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return empty;
		}
		int num = value.IndexOf(':');
		byte b;
		byte b2;
		try
		{
			if (num < 0)
			{
				b = byte.Parse(value);
				b2 = b;
			}
			else
			{
				b = (byte)((num != 0) ? byte.Parse(value.Substring(0, num)) : 0);
				b2 = ((num == value.Length - 1) ? byte.MaxValue : byte.Parse(value.Substring(num + 1)));
			}
		}
		catch (Exception)
		{
			throw new InvalidDataException($"Invalid specification of Decimalplaces: '{value}'. Decimalplaces must be specified as <min>:<max> where <min> and <max> are in the range 0-255 and <min> must be less or equal to <max>.");
		}
		if (b < 16 && b2 < 16)
		{
			DecimalPlacesType decimalPlacesType = cache[b, b2];
			if (decimalPlacesType == null)
			{
				decimalPlacesType = new DecimalPlacesType(b, b2);
				cache[b, b2] = decimalPlacesType;
			}
			return decimalPlacesType;
		}
		return new DecimalPlacesType(b, b2);
	}
}
