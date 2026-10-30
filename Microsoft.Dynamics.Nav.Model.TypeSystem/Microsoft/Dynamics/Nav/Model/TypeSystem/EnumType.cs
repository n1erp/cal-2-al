using System;
using System.IO;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class EnumType<TEnum> : IEnumType, IModelType where TEnum : struct
{
	private static DataTypes dataType;

	private readonly TEnum value;

	private static EnumType<TEnum>[] cache;

	private static readonly EnumType<TEnum> Empty;

	private static int cacheBase;

	public DataTypes DataType
	{
		get
		{
			if (dataType == DataTypes.Unknown)
			{
				dataType = ModelTypeInfo.EnumToDataType(typeof(TEnum));
			}
			return dataType;
		}
	}

	public TEnum Value => value;

	public bool HasValue => EnumHelper<TEnum>.EnumToInt32(value) >= 0;

	public CompiledState CompiledState => CompiledState.Compiled;

	public Type TypeOfEnum => typeof(TEnum);

	public Type TypeOfEnumType => typeof(EnumType<TEnum>);

	private EnumType()
	{
		value = EnumHelper<TEnum>.Int32ToEnum(-1);
	}

	private EnumType(TEnum value)
	{
		this.value = value;
	}

	public string ValueAsString()
	{
		return EnumNameLookup<TEnum>.EnumToString(value);
	}

	public int ValueAsInt32()
	{
		return EnumHelper<TEnum>.EnumToInt32(value);
	}

	static EnumType()
	{
		Empty = new EnumType<TEnum>();
		int[] array = (int[])Enum.GetValues(typeof(TEnum));
		int num = array.Max();
		int num2 = array.Min();
		if (array.Length == num - num2 + 1 || num - num2 < 128)
		{
			cacheBase = num2;
			cache = new EnumType<TEnum>[num - num2 + 1];
			int[] array2 = array;
			foreach (int num3 in array2)
			{
				cache[num3 - cacheBase] = new EnumType<TEnum>(EnumHelper<TEnum>.Int32ToEnum(num3 - cacheBase));
			}
		}
	}

	public static EnumType<TEnum> Create(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Empty;
		}
		int result;
		if (EnumNameLookup<TEnum>.TryStringToEnum(value, out var arg))
		{
			result = EnumHelper<TEnum>.EnumToInt32(arg);
		}
		else
		{
			if (!int.TryParse(value, out result))
			{
				throw new InvalidDataException($"Invalid specification of Enum-value: '{value}'. Value must be either a known member of the enumeration or an integer value.");
			}
			arg = EnumHelper<TEnum>.Int32ToEnum(result);
		}
		if (cache != null && result >= cacheBase && result < cacheBase + cache.Length)
		{
			return cache[result - cacheBase];
		}
		return new EnumType<TEnum>(arg);
	}
}
