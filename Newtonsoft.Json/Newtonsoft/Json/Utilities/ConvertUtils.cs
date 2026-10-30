using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal static class ConvertUtils
{
	internal struct TypeConvertKey : IEquatable<TypeConvertKey>
	{
		private readonly Type _initialType;

		private readonly Type _targetType;

		public Type InitialType => _initialType;

		public Type TargetType => _targetType;

		public TypeConvertKey(Type initialType, Type targetType)
		{
			_initialType = initialType;
			_targetType = targetType;
		}

		public override int GetHashCode()
		{
			return _initialType.GetHashCode() ^ _targetType.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TypeConvertKey))
			{
				return false;
			}
			return Equals((TypeConvertKey)obj);
		}

		public bool Equals(TypeConvertKey other)
		{
			if (_initialType == other._initialType)
			{
				return _targetType == other._targetType;
			}
			return false;
		}
	}

	internal enum ConvertResult
	{
		Success,
		CannotConvertNull,
		NotInstantiableType,
		NoValidConversion
	}

	private static class IEEE754
	{
		private static readonly int[] MultExp64Power10 = new int[15]
		{
			4, 7, 10, 14, 17, 20, 24, 27, 30, 34,
			37, 40, 44, 47, 50
		};

		private static readonly ulong[] MultVal64Power10 = new ulong[15]
		{
			11529215046068469760uL, 14411518807585587200uL, 18014398509481984000uL, 11258999068426240000uL, 14073748835532800000uL, 17592186044416000000uL, 10995116277760000000uL, 13743895347200000000uL, 17179869184000000000uL, 10737418240000000000uL,
			13421772800000000000uL, 16777216000000000000uL, 10485760000000000000uL, 13107200000000000000uL, 16384000000000000000uL
		};

		private static readonly ulong[] MultVal64Power10Inv = new ulong[15]
		{
			14757395258967641293uL, 11805916207174113035uL, 9444732965739290428uL, 15111572745182864686uL, 12089258196146291749uL, 9671406556917033399uL, 15474250491067253438uL, 12379400392853802751uL, 9903520314283042201uL, 15845632502852867522uL,
			12676506002282294018uL, 10141204801825835215uL, 16225927682921336344uL, 12980742146337069075uL, 10384593717069655260uL
		};

		private static readonly int[] MultExp64Power10By16 = new int[21]
		{
			54, 107, 160, 213, 266, 319, 373, 426, 479, 532,
			585, 638, 691, 745, 798, 851, 904, 957, 1010, 1064,
			1117
		};

		private static readonly ulong[] MultVal64Power10By16 = new ulong[21]
		{
			10240000000000000000uL, 11368683772161602974uL, 12621774483536188886uL, 14012984643248170708uL, 15557538194652854266uL, 17272337110188889248uL, 9588073174409622172uL, 10644899600020376798uL, 11818212630765741798uL, 13120851772591970216uL,
			14567071740625403792uL, 16172698447808779622uL, 17955302187076837696uL, 9967194951097567532uL, 11065809325636130658uL, 12285516299433008778uL, 13639663065038175358uL, 15143067982934716296uL, 16812182738118149112uL, 9332636185032188787uL,
			10361307573072618722uL
		};

		private static readonly ulong[] MultVal64Power10By16Inv = new ulong[21]
		{
			16615349947311448416uL, 14965776766268445891uL, 13479973333575319909uL, 12141680576410806707uL, 10936253623915059637uL, 9850501549098619819uL, 17745086042373215136uL, 15983352577617880260uL, 14396524142538228461uL, 12967236152753103031uL,
			11679847981112819795uL, 10520271803096747049uL, 9475818434452569218uL, 17070116948172427008uL, 15375394465392026135uL, 13848924157002783096uL, 12474001934591998882uL, 11235582092889474480uL, 10120112665365530972uL, 18230774251475056952uL,
			16420821625123739930uL
		};

		public static double PackDouble(bool negative, ulong val, int scale)
		{
			if (val == 0L)
			{
				if (!negative)
				{
					return 0.0;
				}
				return -0.0;
			}
			int exp = 64;
			if ((val & 0xFFFFFFFF00000000uL) == 0L)
			{
				val <<= 32;
				exp -= 32;
			}
			if ((val & 0xFFFF000000000000uL) == 0L)
			{
				val <<= 16;
				exp -= 16;
			}
			if ((val & 0xFF00000000000000uL) == 0L)
			{
				val <<= 8;
				exp -= 8;
			}
			if ((val & 0xF000000000000000uL) == 0L)
			{
				val <<= 4;
				exp -= 4;
			}
			if ((val & 0xC000000000000000uL) == 0L)
			{
				val <<= 2;
				exp -= 2;
			}
			if ((val & 0x8000000000000000uL) == 0L)
			{
				val <<= 1;
				exp--;
			}
			if (scale < 0)
			{
				scale = -scale;
				if (scale >= 352)
				{
					if (!negative)
					{
						return 0.0;
					}
					return -0.0;
				}
				int num = scale & 0xF;
				if (num != 0)
				{
					exp -= MultExp64Power10[num - 1] - 1;
					val = Mul64Lossy(val, MultVal64Power10Inv[num - 1], ref exp);
				}
				num = scale >> 4;
				if (num != 0)
				{
					exp -= MultExp64Power10By16[num - 1] - 1;
					val = Mul64Lossy(val, MultVal64Power10By16Inv[num - 1], ref exp);
				}
			}
			else
			{
				if (scale >= 352)
				{
					if (!negative)
					{
						return double.PositiveInfinity;
					}
					return double.NegativeInfinity;
				}
				int num2 = scale & 0xF;
				if (num2 != 0)
				{
					exp += MultExp64Power10[num2 - 1];
					val = Mul64Lossy(val, MultVal64Power10[num2 - 1], ref exp);
				}
				num2 = scale >> 4;
				if (num2 != 0)
				{
					exp += MultExp64Power10By16[num2 - 1];
					val = Mul64Lossy(val, MultVal64Power10By16[num2 - 1], ref exp);
				}
			}
			if ((val & 0x400) != 0L)
			{
				ulong num3 = val + (1023 + ((val >> 11) & 1));
				if (num3 < val)
				{
					num3 = (num3 >> 1) | 0x8000000000000000uL;
					exp++;
				}
				val = num3;
			}
			exp += 1022;
			val = ((exp <= 0) ? ((exp == -52 && val >= 9223372036854775896uL) ? 1 : ((exp > -52) ? (val >> -exp + 12) : 0)) : ((exp < 2047) ? ((ulong)((long)exp << 52) | ((val >> 11) & 0xFFFFFFFFFFFFFL)) : 9218868437227405312uL));
			if (negative)
			{
				val |= 0x8000000000000000uL;
			}
			return BitConverter.Int64BitsToDouble((long)val);
		}

		private static ulong Mul64Lossy(ulong a, ulong b, ref int exp)
		{
			ulong num = a >> 32;
			uint num2 = (uint)a;
			ulong num3 = b >> 32;
			uint num4 = (uint)b;
			ulong num5 = num * num3;
			if ((num4 & 0xFFFF0000u) != 0)
			{
				num5 += num * num4 >> 32;
			}
			if ((num2 & 0xFFFF0000u) != 0)
			{
				num5 += num2 * num3 >> 32;
			}
			if ((num5 & 0x8000000000000000uL) == 0L)
			{
				num5 <<= 1;
				exp--;
			}
			return num5;
		}
	}

	private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap = new Dictionary<Type, PrimitiveTypeCode>
	{
		{
			typeof(char),
			PrimitiveTypeCode.Char
		},
		{
			typeof(char?),
			PrimitiveTypeCode.CharNullable
		},
		{
			typeof(bool),
			PrimitiveTypeCode.Boolean
		},
		{
			typeof(bool?),
			PrimitiveTypeCode.BooleanNullable
		},
		{
			typeof(sbyte),
			PrimitiveTypeCode.SByte
		},
		{
			typeof(sbyte?),
			PrimitiveTypeCode.SByteNullable
		},
		{
			typeof(short),
			PrimitiveTypeCode.Int16
		},
		{
			typeof(short?),
			PrimitiveTypeCode.Int16Nullable
		},
		{
			typeof(ushort),
			PrimitiveTypeCode.UInt16
		},
		{
			typeof(ushort?),
			PrimitiveTypeCode.UInt16Nullable
		},
		{
			typeof(int),
			PrimitiveTypeCode.Int32
		},
		{
			typeof(int?),
			PrimitiveTypeCode.Int32Nullable
		},
		{
			typeof(byte),
			PrimitiveTypeCode.Byte
		},
		{
			typeof(byte?),
			PrimitiveTypeCode.ByteNullable
		},
		{
			typeof(uint),
			PrimitiveTypeCode.UInt32
		},
		{
			typeof(uint?),
			PrimitiveTypeCode.UInt32Nullable
		},
		{
			typeof(long),
			PrimitiveTypeCode.Int64
		},
		{
			typeof(long?),
			PrimitiveTypeCode.Int64Nullable
		},
		{
			typeof(ulong),
			PrimitiveTypeCode.UInt64
		},
		{
			typeof(ulong?),
			PrimitiveTypeCode.UInt64Nullable
		},
		{
			typeof(float),
			PrimitiveTypeCode.Single
		},
		{
			typeof(float?),
			PrimitiveTypeCode.SingleNullable
		},
		{
			typeof(double),
			PrimitiveTypeCode.Double
		},
		{
			typeof(double?),
			PrimitiveTypeCode.DoubleNullable
		},
		{
			typeof(DateTime),
			PrimitiveTypeCode.DateTime
		},
		{
			typeof(DateTime?),
			PrimitiveTypeCode.DateTimeNullable
		},
		{
			typeof(DateTimeOffset),
			PrimitiveTypeCode.DateTimeOffset
		},
		{
			typeof(DateTimeOffset?),
			PrimitiveTypeCode.DateTimeOffsetNullable
		},
		{
			typeof(decimal),
			PrimitiveTypeCode.Decimal
		},
		{
			typeof(decimal?),
			PrimitiveTypeCode.DecimalNullable
		},
		{
			typeof(Guid),
			PrimitiveTypeCode.Guid
		},
		{
			typeof(Guid?),
			PrimitiveTypeCode.GuidNullable
		},
		{
			typeof(TimeSpan),
			PrimitiveTypeCode.TimeSpan
		},
		{
			typeof(TimeSpan?),
			PrimitiveTypeCode.TimeSpanNullable
		},
		{
			typeof(BigInteger),
			PrimitiveTypeCode.BigInteger
		},
		{
			typeof(BigInteger?),
			PrimitiveTypeCode.BigIntegerNullable
		},
		{
			typeof(Uri),
			PrimitiveTypeCode.Uri
		},
		{
			typeof(string),
			PrimitiveTypeCode.String
		},
		{
			typeof(byte[]),
			PrimitiveTypeCode.Bytes
		},
		{
			typeof(DBNull),
			PrimitiveTypeCode.DBNull
		}
	};

	private static readonly TypeInformation[] PrimitiveTypeCodes = new TypeInformation[19]
	{
		new TypeInformation
		{
			Type = typeof(object),
			TypeCode = PrimitiveTypeCode.Empty
		},
		new TypeInformation
		{
			Type = typeof(object),
			TypeCode = PrimitiveTypeCode.Object
		},
		new TypeInformation
		{
			Type = typeof(object),
			TypeCode = PrimitiveTypeCode.DBNull
		},
		new TypeInformation
		{
			Type = typeof(bool),
			TypeCode = PrimitiveTypeCode.Boolean
		},
		new TypeInformation
		{
			Type = typeof(char),
			TypeCode = PrimitiveTypeCode.Char
		},
		new TypeInformation
		{
			Type = typeof(sbyte),
			TypeCode = PrimitiveTypeCode.SByte
		},
		new TypeInformation
		{
			Type = typeof(byte),
			TypeCode = PrimitiveTypeCode.Byte
		},
		new TypeInformation
		{
			Type = typeof(short),
			TypeCode = PrimitiveTypeCode.Int16
		},
		new TypeInformation
		{
			Type = typeof(ushort),
			TypeCode = PrimitiveTypeCode.UInt16
		},
		new TypeInformation
		{
			Type = typeof(int),
			TypeCode = PrimitiveTypeCode.Int32
		},
		new TypeInformation
		{
			Type = typeof(uint),
			TypeCode = PrimitiveTypeCode.UInt32
		},
		new TypeInformation
		{
			Type = typeof(long),
			TypeCode = PrimitiveTypeCode.Int64
		},
		new TypeInformation
		{
			Type = typeof(ulong),
			TypeCode = PrimitiveTypeCode.UInt64
		},
		new TypeInformation
		{
			Type = typeof(float),
			TypeCode = PrimitiveTypeCode.Single
		},
		new TypeInformation
		{
			Type = typeof(double),
			TypeCode = PrimitiveTypeCode.Double
		},
		new TypeInformation
		{
			Type = typeof(decimal),
			TypeCode = PrimitiveTypeCode.Decimal
		},
		new TypeInformation
		{
			Type = typeof(DateTime),
			TypeCode = PrimitiveTypeCode.DateTime
		},
		new TypeInformation
		{
			Type = typeof(object),
			TypeCode = PrimitiveTypeCode.Empty
		},
		new TypeInformation
		{
			Type = typeof(string),
			TypeCode = PrimitiveTypeCode.String
		}
	};

	private static readonly ThreadSafeStore<TypeConvertKey, Func<object, object>> CastConverters = new ThreadSafeStore<TypeConvertKey, Func<object, object>>(CreateCastConverter);

	private static decimal[] _decimalFactors;

	private static decimal[] DecimalFactors
	{
		get
		{
			decimal[] array = _decimalFactors;
			if (array == null)
			{
				array = new decimal[28];
				decimal num = 1m;
				for (int i = 0; i < array.Length; i++)
				{
					num = (array[i] = num * 10m);
				}
				_decimalFactors = array;
			}
			return array;
		}
	}

	public static PrimitiveTypeCode GetTypeCode(Type t)
	{
		bool isEnum;
		return GetTypeCode(t, out isEnum);
	}

	public static PrimitiveTypeCode GetTypeCode(Type t, out bool isEnum)
	{
		if (TypeCodeMap.TryGetValue(t, out var value))
		{
			isEnum = false;
			return value;
		}
		if (t.IsEnum())
		{
			isEnum = true;
			return GetTypeCode(Enum.GetUnderlyingType(t));
		}
		if (ReflectionUtils.IsNullableType(t))
		{
			Type underlyingType = Nullable.GetUnderlyingType(t);
			if (underlyingType.IsEnum())
			{
				Type t2 = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(underlyingType));
				isEnum = true;
				return GetTypeCode(t2);
			}
		}
		isEnum = false;
		return PrimitiveTypeCode.Object;
	}

	public static TypeInformation GetTypeInformation(IConvertible convertable)
	{
		return PrimitiveTypeCodes[(int)convertable.GetTypeCode()];
	}

	public static bool IsConvertible(Type t)
	{
		return typeof(IConvertible).IsAssignableFrom(t);
	}

	public static TimeSpan ParseTimeSpan(string input)
	{
		return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
	}

	private static Func<object, object> CreateCastConverter(TypeConvertKey t)
	{
		MethodInfo method = t.TargetType.GetMethod("op_Implicit", new Type[1] { t.InitialType });
		if (method == null)
		{
			method = t.TargetType.GetMethod("op_Explicit", new Type[1] { t.InitialType });
		}
		if (method == null)
		{
			return null;
		}
		MethodCall<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(method);
		return (object o) => call(null, o);
	}

	internal static BigInteger ToBigInteger(object value)
	{
		if (value is BigInteger)
		{
			return (BigInteger)value;
		}
		if (value is string value2)
		{
			return BigInteger.Parse(value2, CultureInfo.InvariantCulture);
		}
		if (value is float)
		{
			return new BigInteger((float)value);
		}
		if (value is double)
		{
			return new BigInteger((double)value);
		}
		if (value is decimal)
		{
			return new BigInteger((decimal)value);
		}
		if (value is int)
		{
			return new BigInteger((int)value);
		}
		if (value is long)
		{
			return new BigInteger((long)value);
		}
		if (value is uint)
		{
			return new BigInteger((uint)value);
		}
		if (value is ulong)
		{
			return new BigInteger((ulong)value);
		}
		if (value is byte[] value3)
		{
			return new BigInteger(value3);
		}
		throw new InvalidCastException("Cannot convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
	}

	public static object FromBigInteger(BigInteger i, Type targetType)
	{
		if (targetType == typeof(decimal))
		{
			return (decimal)i;
		}
		if (targetType == typeof(double))
		{
			return (double)i;
		}
		if (targetType == typeof(float))
		{
			return (float)i;
		}
		if (targetType == typeof(ulong))
		{
			return (ulong)i;
		}
		if (targetType == typeof(bool))
		{
			return i != 0L;
		}
		try
		{
			return System.Convert.ChangeType((long)i, targetType, CultureInfo.InvariantCulture);
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Can not convert from BigInteger to {0}.".FormatWith(CultureInfo.InvariantCulture, targetType), innerException);
		}
	}

	public static object Convert(object initialValue, CultureInfo culture, Type targetType)
	{
		object value;
		return TryConvertInternal(initialValue, culture, targetType, out value) switch
		{
			ConvertResult.Success => value, 
			ConvertResult.CannotConvertNull => throw new Exception("Can not convert null {0} into non-nullable {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType)), 
			ConvertResult.NotInstantiableType => throw new ArgumentException("Target type {0} is not a value type or a non-abstract class.".FormatWith(CultureInfo.InvariantCulture, targetType), "targetType"), 
			ConvertResult.NoValidConversion => throw new InvalidOperationException("Can not convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType)), 
			_ => throw new InvalidOperationException("Unexpected conversion result."), 
		};
	}

	private static bool TryConvert(object initialValue, CultureInfo culture, Type targetType, out object value)
	{
		try
		{
			if (TryConvertInternal(initialValue, culture, targetType, out value) == ConvertResult.Success)
			{
				return true;
			}
			value = null;
			return false;
		}
		catch
		{
			value = null;
			return false;
		}
	}

	private static ConvertResult TryConvertInternal(object initialValue, CultureInfo culture, Type targetType, out object value)
	{
		if (initialValue == null)
		{
			throw new ArgumentNullException("initialValue");
		}
		if (ReflectionUtils.IsNullableType(targetType))
		{
			targetType = Nullable.GetUnderlyingType(targetType);
		}
		Type type = initialValue.GetType();
		if (targetType == type)
		{
			value = initialValue;
			return ConvertResult.Success;
		}
		if (IsConvertible(initialValue.GetType()) && IsConvertible(targetType))
		{
			if (targetType.IsEnum())
			{
				if (initialValue is string)
				{
					value = Enum.Parse(targetType, initialValue.ToString(), ignoreCase: true);
					return ConvertResult.Success;
				}
				if (IsInteger(initialValue))
				{
					value = Enum.ToObject(targetType, initialValue);
					return ConvertResult.Success;
				}
			}
			value = System.Convert.ChangeType(initialValue, targetType, culture);
			return ConvertResult.Success;
		}
		if (initialValue is DateTime && targetType == typeof(DateTimeOffset))
		{
			value = new DateTimeOffset((DateTime)initialValue);
			return ConvertResult.Success;
		}
		if (initialValue is byte[] b && targetType == typeof(Guid))
		{
			value = new Guid(b);
			return ConvertResult.Success;
		}
		if (initialValue is Guid && targetType == typeof(byte[]))
		{
			value = ((Guid)initialValue).ToByteArray();
			return ConvertResult.Success;
		}
		if (initialValue is string text)
		{
			if (targetType == typeof(Guid))
			{
				value = new Guid(text);
				return ConvertResult.Success;
			}
			if (targetType == typeof(Uri))
			{
				value = new Uri(text, UriKind.RelativeOrAbsolute);
				return ConvertResult.Success;
			}
			if (targetType == typeof(TimeSpan))
			{
				value = ParseTimeSpan(text);
				return ConvertResult.Success;
			}
			if (targetType == typeof(byte[]))
			{
				value = System.Convert.FromBase64String(text);
				return ConvertResult.Success;
			}
			if (targetType == typeof(Version))
			{
				if (VersionTryParse(text, out var result))
				{
					value = result;
					return ConvertResult.Success;
				}
				value = null;
				return ConvertResult.NoValidConversion;
			}
			if (typeof(Type).IsAssignableFrom(targetType))
			{
				value = Type.GetType(text, throwOnError: true);
				return ConvertResult.Success;
			}
		}
		if (targetType == typeof(BigInteger))
		{
			value = ToBigInteger(initialValue);
			return ConvertResult.Success;
		}
		if (initialValue is BigInteger)
		{
			value = FromBigInteger((BigInteger)initialValue, targetType);
			return ConvertResult.Success;
		}
		TypeConverter converter = TypeDescriptor.GetConverter(type);
		if (converter != null && converter.CanConvertTo(targetType))
		{
			value = converter.ConvertTo(null, culture, initialValue, targetType);
			return ConvertResult.Success;
		}
		TypeConverter converter2 = TypeDescriptor.GetConverter(targetType);
		if (converter2 != null && converter2.CanConvertFrom(type))
		{
			value = converter2.ConvertFrom(null, culture, initialValue);
			return ConvertResult.Success;
		}
		if (initialValue == DBNull.Value)
		{
			if (ReflectionUtils.IsNullable(targetType))
			{
				value = EnsureTypeAssignable(null, type, targetType);
				return ConvertResult.Success;
			}
			value = null;
			return ConvertResult.CannotConvertNull;
		}
		if (initialValue is INullable nullableValue)
		{
			value = EnsureTypeAssignable(ToValue(nullableValue), type, targetType);
			return ConvertResult.Success;
		}
		if (targetType.IsInterface() || targetType.IsGenericTypeDefinition() || targetType.IsAbstract())
		{
			value = null;
			return ConvertResult.NotInstantiableType;
		}
		value = null;
		return ConvertResult.NoValidConversion;
	}

	public static object ConvertOrCast(object initialValue, CultureInfo culture, Type targetType)
	{
		if (targetType == typeof(object))
		{
			return initialValue;
		}
		if (initialValue == null && ReflectionUtils.IsNullable(targetType))
		{
			return null;
		}
		if (TryConvert(initialValue, culture, targetType, out var value))
		{
			return value;
		}
		return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
	}

	private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
	{
		Type type = value?.GetType();
		if (value != null)
		{
			if (targetType.IsAssignableFrom(type))
			{
				return value;
			}
			Func<object, object> func = CastConverters.Get(new TypeConvertKey(type, targetType));
			if (func != null)
			{
				return func(value);
			}
		}
		else if (ReflectionUtils.IsNullable(targetType))
		{
			return null;
		}
		throw new ArgumentException("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialType?.ToString() ?? "{null}", targetType));
	}

	public static object ToValue(INullable nullableValue)
	{
		if (nullableValue == null)
		{
			return null;
		}
		if (nullableValue is SqlInt32)
		{
			return ToValue((SqlInt32)(object)nullableValue);
		}
		if (nullableValue is SqlInt64)
		{
			return ToValue((SqlInt64)(object)nullableValue);
		}
		if (nullableValue is SqlBoolean)
		{
			return ToValue((SqlBoolean)(object)nullableValue);
		}
		if (nullableValue is SqlString)
		{
			return ToValue((SqlString)(object)nullableValue);
		}
		if (nullableValue is SqlDateTime)
		{
			return ToValue((SqlDateTime)(object)nullableValue);
		}
		throw new ArgumentException("Unsupported INullable type: {0}".FormatWith(CultureInfo.InvariantCulture, nullableValue.GetType()));
	}

	public static bool VersionTryParse(string input, out Version result)
	{
		return Version.TryParse(input, out result);
	}

	public static bool IsInteger(object value)
	{
		switch (GetTypeCode(value.GetType()))
		{
		case PrimitiveTypeCode.SByte:
		case PrimitiveTypeCode.Int16:
		case PrimitiveTypeCode.UInt16:
		case PrimitiveTypeCode.Int32:
		case PrimitiveTypeCode.Byte:
		case PrimitiveTypeCode.UInt32:
		case PrimitiveTypeCode.Int64:
		case PrimitiveTypeCode.UInt64:
			return true;
		default:
			return false;
		}
	}

	public static ParseResult Int32TryParse(char[] chars, int start, int length, out int value)
	{
		value = 0;
		if (length == 0)
		{
			return ParseResult.Invalid;
		}
		bool flag = chars[start] == '-';
		if (flag)
		{
			if (length == 1)
			{
				return ParseResult.Invalid;
			}
			start++;
			length--;
		}
		int num = start + length;
		if (length > 10 || (length == 10 && chars[start] - 48 > 2))
		{
			for (int i = start; i < num; i++)
			{
				int num2 = chars[i] - 48;
				if (num2 < 0 || num2 > 9)
				{
					return ParseResult.Invalid;
				}
			}
			return ParseResult.Overflow;
		}
		for (int j = start; j < num; j++)
		{
			int num3 = chars[j] - 48;
			if (num3 < 0 || num3 > 9)
			{
				return ParseResult.Invalid;
			}
			int num4 = 10 * value - num3;
			if (num4 > value)
			{
				for (j++; j < num; j++)
				{
					num3 = chars[j] - 48;
					if (num3 < 0 || num3 > 9)
					{
						return ParseResult.Invalid;
					}
				}
				return ParseResult.Overflow;
			}
			value = num4;
		}
		if (!flag)
		{
			if (value == int.MinValue)
			{
				return ParseResult.Overflow;
			}
			value = -value;
		}
		return ParseResult.Success;
	}

	public static ParseResult Int64TryParse(char[] chars, int start, int length, out long value)
	{
		value = 0L;
		if (length == 0)
		{
			return ParseResult.Invalid;
		}
		bool flag = chars[start] == '-';
		if (flag)
		{
			if (length == 1)
			{
				return ParseResult.Invalid;
			}
			start++;
			length--;
		}
		int num = start + length;
		if (length > 19)
		{
			for (int i = start; i < num; i++)
			{
				int num2 = chars[i] - 48;
				if (num2 < 0 || num2 > 9)
				{
					return ParseResult.Invalid;
				}
			}
			return ParseResult.Overflow;
		}
		for (int j = start; j < num; j++)
		{
			int num3 = chars[j] - 48;
			if (num3 < 0 || num3 > 9)
			{
				return ParseResult.Invalid;
			}
			long num4 = 10 * value - num3;
			if (num4 > value)
			{
				for (j++; j < num; j++)
				{
					num3 = chars[j] - 48;
					if (num3 < 0 || num3 > 9)
					{
						return ParseResult.Invalid;
					}
				}
				return ParseResult.Overflow;
			}
			value = num4;
		}
		if (!flag)
		{
			if (value == long.MinValue)
			{
				return ParseResult.Overflow;
			}
			value = -value;
		}
		return ParseResult.Success;
	}

	public static ParseResult DoubleTryParse(char[] chars, int start, int length, out double value)
	{
		value = 0.0;
		if (length == 0)
		{
			return ParseResult.Invalid;
		}
		bool flag = chars[start] == '-';
		if (flag)
		{
			if (length == 1)
			{
				return ParseResult.Invalid;
			}
			start++;
			length--;
		}
		int i = start;
		int num = start + length;
		int num2 = num;
		int num3 = num;
		int num4 = 0;
		ulong num5 = 0uL;
		int num6 = 0;
		int num7 = 0;
		for (; i < num; i++)
		{
			char c = chars[i];
			if (c == '.')
			{
				goto IL_0064;
			}
			if (c != 'E' && c != 'e')
			{
				if (c < '0' || c > '9')
				{
					return ParseResult.Invalid;
				}
				if (i == start && c == '0')
				{
					i++;
					if (i != num)
					{
						c = chars[i];
						if (c == '.')
						{
							goto IL_0064;
						}
						if (c != 'e' && c != 'E')
						{
							return ParseResult.Invalid;
						}
						goto IL_0081;
					}
				}
				if (num6 < 19)
				{
					num5 = 10 * num5 + (ulong)(c - 48);
					if (num5 != 0)
					{
						num6++;
					}
				}
				else
				{
					num7++;
				}
				continue;
			}
			goto IL_0081;
			IL_0064:
			if (i == start)
			{
				return ParseResult.Invalid;
			}
			if (i + 1 == num)
			{
				return ParseResult.Invalid;
			}
			if (num2 != num)
			{
				return ParseResult.Invalid;
			}
			num2 = i + 1;
			continue;
			IL_0081:
			if (i == start)
			{
				return ParseResult.Invalid;
			}
			if (i == num2)
			{
				return ParseResult.Invalid;
			}
			i++;
			if (i == num)
			{
				return ParseResult.Invalid;
			}
			if (num2 < num)
			{
				num3 = i - 1;
			}
			c = chars[i];
			bool flag2 = false;
			switch (c)
			{
			case '-':
				flag2 = true;
				i++;
				break;
			case '+':
				i++;
				break;
			}
			for (; i < num; i++)
			{
				c = chars[i];
				if (c < '0' || c > '9')
				{
					return ParseResult.Invalid;
				}
				int num8 = 10 * num4 + (c - 48);
				if (num4 < num8)
				{
					num4 = num8;
				}
			}
			if (flag2)
			{
				num4 = -num4;
			}
		}
		num4 += num7;
		num4 -= num3 - num2;
		value = IEEE754.PackDouble(flag, num5, num4);
		if (!double.IsInfinity(value))
		{
			return ParseResult.Success;
		}
		return ParseResult.Overflow;
	}

	public static ParseResult DecimalTryParse(char[] chars, int start, int length, out decimal value)
	{
		value = 0m;
		if (length == 0)
		{
			return ParseResult.Invalid;
		}
		bool flag = chars[start] == '-';
		if (flag)
		{
			if (length == 1)
			{
				return ParseResult.Invalid;
			}
			start++;
			length--;
		}
		int i = start;
		int num = start + length;
		int num2 = num;
		int num3 = num;
		int num4 = 0;
		ulong num5 = 0uL;
		ulong num6 = 0uL;
		int num7 = 0;
		int num8 = 0;
		bool? flag2 = null;
		bool? flag3 = null;
		for (; i < num; i++)
		{
			char c = chars[i];
			if (c == '.')
			{
				goto IL_0074;
			}
			if (c == 'E' || c == 'e')
			{
				goto IL_0091;
			}
			if (c < '0' || c > '9')
			{
				return ParseResult.Invalid;
			}
			if (i == start && c == '0')
			{
				i++;
				if (i != num)
				{
					c = chars[i];
					if (c == '.')
					{
						goto IL_0074;
					}
					if (c != 'e' && c != 'E')
					{
						return ParseResult.Invalid;
					}
					goto IL_0091;
				}
			}
			if (num7 < 29)
			{
				if (num7 == 28)
				{
					bool? flag4 = flag3;
					bool valueOrDefault;
					if (!flag4.HasValue)
					{
						flag3 = num5 > 7922816251426433759L || (num5 == 7922816251426433759L && (num6 > 354395033 || (num6 == 354395033 && c > '5')));
						bool? flag5 = flag3;
						valueOrDefault = flag5 == true;
					}
					else
					{
						valueOrDefault = flag4 == true;
					}
					if (valueOrDefault)
					{
						goto IL_01ff;
					}
				}
				if (num7 < 19)
				{
					num5 = num5 * 10 + (ulong)(c - 48);
				}
				else
				{
					num6 = num6 * 10 + (ulong)(c - 48);
				}
				num7++;
				continue;
			}
			goto IL_01ff;
			IL_0074:
			if (i == start)
			{
				return ParseResult.Invalid;
			}
			if (i + 1 == num)
			{
				return ParseResult.Invalid;
			}
			if (num2 != num)
			{
				return ParseResult.Invalid;
			}
			num2 = i + 1;
			continue;
			IL_01ff:
			if (!flag2.HasValue)
			{
				flag2 = c >= '5';
			}
			num8++;
			continue;
			IL_0091:
			if (i == start)
			{
				return ParseResult.Invalid;
			}
			if (i == num2)
			{
				return ParseResult.Invalid;
			}
			i++;
			if (i == num)
			{
				return ParseResult.Invalid;
			}
			if (num2 < num)
			{
				num3 = i - 1;
			}
			c = chars[i];
			bool flag6 = false;
			switch (c)
			{
			case '-':
				flag6 = true;
				i++;
				break;
			case '+':
				i++;
				break;
			}
			for (; i < num; i++)
			{
				c = chars[i];
				if (c < '0' || c > '9')
				{
					return ParseResult.Invalid;
				}
				int num9 = 10 * num4 + (c - 48);
				if (num4 < num9)
				{
					num4 = num9;
				}
			}
			if (flag6)
			{
				num4 = -num4;
			}
		}
		num4 += num8;
		num4 -= num3 - num2;
		if (num7 <= 19)
		{
			value = num5;
		}
		else
		{
			value = (decimal)num5 * DecimalFactors[num7 - 20] + (decimal)num6;
		}
		if (num4 > 0)
		{
			num7 += num4;
			if (num7 > 29)
			{
				return ParseResult.Overflow;
			}
			if (num7 == 29)
			{
				if (num4 > 1)
				{
					value *= DecimalFactors[num4 - 2];
					if (value > 7922816251426433759354395033m)
					{
						return ParseResult.Overflow;
					}
				}
				value *= 10m;
			}
			else
			{
				value *= DecimalFactors[num4 - 1];
			}
		}
		else
		{
			bool? flag4 = flag2;
			bool flag7 = true;
			if (flag4 == true == flag7 && flag4.HasValue && num4 >= -28)
			{
				++value;
			}
			if (num4 < 0)
			{
				if (num7 + num4 + 28 <= 0)
				{
					value = 0m;
					return ParseResult.Success;
				}
				if (num4 >= -28)
				{
					value /= DecimalFactors[-num4 - 1];
				}
				else
				{
					decimal[] decimalFactors = DecimalFactors;
					value /= decimalFactors[27];
					value /= decimalFactors[-num4 - 29];
				}
			}
		}
		if (flag)
		{
			value = -value;
		}
		return ParseResult.Success;
	}

	public static bool TryConvertGuid(string s, out Guid g)
	{
		return Guid.TryParseExact(s, "D", out g);
	}

	public static bool TryHexTextToInt(char[] text, int start, int end, out int value)
	{
		value = 0;
		for (int i = start; i < end; i++)
		{
			char c = text[i];
			int num;
			if (c <= '9' && c >= '0')
			{
				num = c - 48;
			}
			else if (c <= 'F' && c >= 'A')
			{
				num = c - 55;
			}
			else
			{
				if (c > 'f' || c < 'a')
				{
					value = 0;
					return false;
				}
				num = c - 87;
			}
			value += num << (end - 1 - i) * 4;
		}
		return true;
	}
}
