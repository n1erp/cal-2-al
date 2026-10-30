using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALCompiler
{
	private static NavOemText stdFmt220 = new NavOemText(8, "#,##0.00");

	private static NavOemText stdFmt250 = new NavOemText(11, "#,##0.00###");

	private static NavOemText stdFmt000 = new NavOemText(5, "#,##0");

	private static NavOemText stdFmt030 = new NavOemText(9, "#,##0.###");

	private static NavOemText stdFmt330 = new NavOemText(9, "#,##0.000");

	private static NavOemText failFormat = new NavOemText(8, "#,##0.00");

	internal const string CustomSpecifier = "<Custom,";

	internal const string PrecisionSpecifier = "<Precision,";

	internal const string FormatSpecifier = "<Standard Format,";

	public static bool CompareNavValues(NavValue first, NavValue second)
	{
		if (first == second)
		{
			return true;
		}
		if (first == null || second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	private static T ConvertEnumType<T>(NavDotNet fromValue)
	{
		if (fromValue != null)
		{
			object enumValue = fromValue.GetEnumValue<T>();
			return (T)ConvertObjectToClrValueOfType(enumValue, typeof(T));
		}
		throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(NavDotNet), fromValue, typeof(T));
	}

	private static object ConvertObjectToClrValueOfType(object fromValue, Type toType)
	{
		if (fromValue != null)
		{
			try
			{
				return Convert.ChangeType(fromValue, toType, NavCurrentThread.Session.WindowsCulture);
			}
			catch (OverflowException)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, fromValue.GetType(), fromValue, toType);
			}
			catch (InvalidCastException)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, fromValue.GetType(), fromValue, toType);
			}
		}
		throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(object), fromValue, toType);
	}

	internal static object ConvertRuntimeValueToDotNetValue(object fromValue, Type toType)
	{
		object obj = fromValue;
		if (fromValue is NavValue { NavType: var navType } navValue)
		{
			switch (navType)
			{
			case NavType.Date:
			case NavType.Time:
			case NavType.DateTime:
				obj = navValue.ToDateTime();
				break;
			case NavType.Duration:
				obj = TimeSpan.FromMilliseconds(((NavDuration)navValue).Value);
				break;
			case NavType.OemText:
			case NavType.Decimal:
			case NavType.TextConst:
			case NavType.Variant:
			case NavType.Char:
			case NavType.Text:
			case NavType.Code:
			case NavType.Boolean:
			case NavType.Integer:
			case NavType.Byte:
			case NavType.Option:
			case NavType.BigInteger:
			case NavType.GUID:
			case NavType.String:
				obj = ConvertNavValueToClrValue(navValue);
				break;
			default:
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, fromValue.GetType(), fromValue, toType);
			}
		}
		if (toType.IsEnum)
		{
			try
			{
				return Enum.ToObject(toType, fromValue);
			}
			catch (ArgumentException)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, fromValue.GetType(), fromValue, toType);
			}
		}
		if (toType.IsAssignableFrom(obj.GetType()))
		{
			return obj;
		}
		return ConvertObjectToClrValueOfType(obj, toType);
	}

	public static byte ToByte(sbyte fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(sbyte), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(ushort fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(ushort), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(uint fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(uint), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(ulong fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(ulong), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(int fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(int), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(long fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(Decimal18 fromValue)
	{
		try
		{
			byte b = (byte)fromValue;
			if (b != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(byte));
			}
			return b;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(NavDotNet fromValue)
	{
		return ConvertEnumType<byte>(fromValue);
	}

	public static byte ToByte(float fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(float), fromValue, typeof(byte));
		}
	}

	public static byte ToByte(double fromValue)
	{
		try
		{
			return checked((byte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), fromValue, typeof(byte));
		}
	}

	public static sbyte ToSByte(int fromValue)
	{
		try
		{
			return checked((sbyte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(int), fromValue, typeof(sbyte));
		}
	}

	public static sbyte ToSByte(byte fromValue)
	{
		try
		{
			return checked((sbyte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(byte), fromValue, typeof(sbyte));
		}
	}

	public static sbyte ToSByte(ushort fromValue)
	{
		try
		{
			return checked((sbyte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(ushort), fromValue, typeof(sbyte));
		}
	}

	public static sbyte ToSByte(long fromValue)
	{
		try
		{
			return checked((sbyte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), fromValue, typeof(sbyte));
		}
	}

	public static sbyte ToSByte(Decimal18 fromValue)
	{
		try
		{
			sbyte b = (sbyte)fromValue;
			if (b != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(sbyte));
			}
			return b;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(sbyte));
		}
	}

	public static sbyte ToSByte(NavDotNet fromValue)
	{
		return ConvertEnumType<sbyte>(fromValue);
	}

	public static sbyte ToSByte(char fromValue)
	{
		try
		{
			return checked((sbyte)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(char), fromValue, typeof(sbyte));
		}
	}

	public static short ToInt16(int fromValue)
	{
		try
		{
			return checked((short)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(int), fromValue, typeof(short));
		}
	}

	public static short ToInt16(long fromValue)
	{
		try
		{
			return checked((short)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), fromValue, typeof(short));
		}
	}

	public static short ToInt16(Decimal18 fromValue)
	{
		try
		{
			short num = (short)fromValue;
			if (num != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(short));
			}
			return num;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(short));
		}
	}

	public static short ToInt16(NavDotNet fromValue)
	{
		return ConvertEnumType<short>(fromValue);
	}

	public static int ToInt32(long fromValue)
	{
		try
		{
			return checked((int)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), fromValue, typeof(int));
		}
	}

	public static int ToInt32(ushort fromValue)
	{
		try
		{
			return fromValue;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(ushort), fromValue, typeof(int));
		}
	}

	public static int ToInt32(uint fromValue)
	{
		try
		{
			return checked((int)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(uint), fromValue, typeof(int));
		}
	}

	public static int ToInt32(ulong fromValue)
	{
		try
		{
			return checked((int)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(ulong), fromValue, typeof(int));
		}
	}

	public static int ToInt32(Decimal18 fromValue)
	{
		try
		{
			int num = (int)fromValue;
			if (num != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(int));
			}
			return num;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(int));
		}
	}

	public static int ToInt32(float fromValue)
	{
		try
		{
			return checked((int)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(float), fromValue, typeof(int));
		}
	}

	public static int ToInt32(double fromValue)
	{
		try
		{
			return checked((int)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), fromValue, typeof(int));
		}
	}

	public static int ToInt32(NavDotNet fromValue)
	{
		return ConvertEnumType<int>(fromValue);
	}

	public static long ToInt64(Decimal18 fromValue)
	{
		try
		{
			long num = (long)fromValue;
			if (num != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(long));
			}
			return num;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(long));
		}
	}

	public static long ToInt64(ulong fromValue)
	{
		try
		{
			return checked((long)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(long));
		}
	}

	public static long ToInt64(NavDotNet fromValue)
	{
		return ConvertEnumType<long>(fromValue);
	}

	public static long ToInt64(float fromValue)
	{
		try
		{
			return checked((long)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(float), fromValue, typeof(long));
		}
	}

	public static long ToInt64(double fromValue)
	{
		try
		{
			return checked((long)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), fromValue, typeof(long));
		}
	}

	public static long ToInt64(NavDuration fromValue)
	{
		try
		{
			return fromValue;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), fromValue, typeof(long));
		}
	}

	public static ushort ToUInt16(byte fromValue)
	{
		try
		{
			return fromValue;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(byte), fromValue, typeof(ushort));
		}
	}

	public static ushort ToUInt16(int fromValue)
	{
		try
		{
			return checked((ushort)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(int), fromValue, typeof(ushort));
		}
	}

	public static ushort ToUInt16(long fromValue)
	{
		try
		{
			return checked((ushort)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), fromValue, typeof(ushort));
		}
	}

	public static ushort ToUInt16(Decimal18 fromValue)
	{
		try
		{
			ushort num = (ushort)fromValue;
			if (num != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(ushort));
			}
			return num;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(ushort));
		}
	}

	public static ushort ToUInt16(NavDotNet fromValue)
	{
		return ConvertEnumType<ushort>(fromValue);
	}

	public static uint ToUInt32(int fromValue)
	{
		try
		{
			return checked((uint)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(int), fromValue, typeof(uint));
		}
	}

	public static uint ToUInt32(long fromValue)
	{
		try
		{
			return checked((uint)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), fromValue, typeof(uint));
		}
	}

	public static uint ToUInt32(Decimal18 fromValue)
	{
		try
		{
			uint num = (uint)fromValue;
			if (num != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(uint));
			}
			return num;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(uint));
		}
	}

	public static uint ToUInt32(NavDotNet fromValue)
	{
		return ConvertEnumType<uint>(fromValue);
	}

	public static ulong ToUInt64(int fromValue)
	{
		try
		{
			return checked((ulong)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(int), fromValue, typeof(ulong));
		}
	}

	public static ulong ToUInt64(long fromValue)
	{
		try
		{
			return checked((ulong)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), fromValue, typeof(ulong));
		}
	}

	public static ulong ToUInt64(Decimal18 fromValue)
	{
		try
		{
			ulong num = (ulong)fromValue;
			if (num != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(ulong));
			}
			return num;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(ulong));
		}
	}

	public static ulong ToUInt64(NavDotNet fromValue)
	{
		return ConvertEnumType<ulong>(fromValue);
	}

	public static Decimal18 ToDecimal(float fromValue)
	{
		try
		{
			return (Decimal18)fromValue;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(float), fromValue, typeof(Decimal18));
		}
	}

	public static Decimal18 ToDecimal(NavDotNet fromValue)
	{
		object obj = null;
		try
		{
			obj = fromValue.GetValue<object>();
			if (obj is decimal)
			{
				return (decimal)obj;
			}
			if (obj is double)
			{
				return (Decimal18)(double)obj;
			}
			if (obj is float)
			{
				return (Decimal18)(float)obj;
			}
			if (obj is byte)
			{
				return (byte)obj;
			}
			if (obj is short)
			{
				return (short)obj;
			}
			if (obj is int)
			{
				return (int)obj;
			}
			if (obj is long)
			{
				return (long)obj;
			}
			if (obj is ushort)
			{
				return (ushort)obj;
			}
			if (obj is uint)
			{
				return (uint)obj;
			}
			if (obj is ulong)
			{
				return (ulong)obj;
			}
			return (Decimal18)(double)obj;
		}
		catch
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, (obj == null) ? typeof(object) : obj.GetType(), fromValue, typeof(Decimal18));
		}
	}

	public static Decimal18 ToDecimal(double fromValue)
	{
		try
		{
			return (Decimal18)fromValue;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), fromValue, typeof(Decimal18));
		}
	}

	public static float ToSingle(Decimal18 fromValue)
	{
		try
		{
			return (float)fromValue;
		}
		catch
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(float));
		}
	}

	public static double ToDouble(Decimal18 fromValue)
	{
		try
		{
			return (double)fromValue;
		}
		catch
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(double));
		}
	}

	public static float ToSingle(double fromValue)
	{
		try
		{
			return (float)fromValue;
		}
		catch
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), fromValue, typeof(float));
		}
	}

	public static byte[] ToBinary(byte[] value, INavValueMetadata targetMetadata)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (targetMetadata == null)
		{
			throw new ArgumentNullException("targetMetadata");
		}
		if (value.Length != targetMetadata.NavDefinedLengthMetadata)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(byte[]), typeof(byte[]));
		}
		byte[] array = new byte[targetMetadata.NavDefinedLengthMetadata];
		Array.Copy(value, array, value.Length);
		return array;
	}

	public static char ToChar(int value)
	{
		try
		{
			return (char)checked((ushort)value);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(int), value, typeof(char));
		}
	}

	public static char ToChar(long value)
	{
		try
		{
			return (char)checked((ushort)value);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(long), value, typeof(char));
		}
	}

	public static object SafeCastCheck<T>(object value)
	{
		if (!(value is T))
		{
			throw new NavNCLConversionCastException(Lang.InvalidObjectCodeRecompile);
		}
		return value;
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, object value)
	{
		return ObjectToNavVariant(parentOfResult, value);
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, byte value)
	{
		return new NavVariant(parentOfResult, NavByte.Create(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, char value)
	{
		return new NavVariant(parentOfResult, NavChar.Create(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, bool value)
	{
		return new NavVariant(parentOfResult, NavBoolean.Create(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, int value)
	{
		return new NavVariant(parentOfResult, NavInteger.Create(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, long value)
	{
		return new NavVariant(parentOfResult, NavBigInteger.Create(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, float value)
	{
		return ToVariant(parentOfResult, (decimal)value);
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, double value)
	{
		return ToVariant(parentOfResult, (decimal)value);
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, ulong value)
	{
		return ToVariant(parentOfResult, (long)value);
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, Decimal18 value)
	{
		return new NavVariant(parentOfResult, NavDecimal.Create(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, Guid value)
	{
		return new NavVariant(parentOfResult, new NavGuid(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return new NavVariant(parentOfResult, NavText.Create(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, FieldClass value)
	{
		return new NavVariant(parentOfResult, new NavFieldClass(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, SecurityFiltering value)
	{
		return new NavVariant(parentOfResult, new NavSecurityFiltering(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, TransactionType value)
	{
		return new NavVariant(parentOfResult, new NavTransactionType(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, NavObjectType value)
	{
		return new NavVariant(parentOfResult, new NavEnumObjectType(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, NavClientType value)
	{
		return new NavVariant(parentOfResult, new NavEnumClientType(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, TextEncoding value)
	{
		return new NavVariant(parentOfResult, new NavEnumTextEncoding(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, NavReportFormat value)
	{
		return new NavVariant(parentOfResult, new NavEnumReportFormat(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, NavDefaultLayout value)
	{
		return new NavVariant(parentOfResult, new NavEnumDefaultLayout(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, NavTestPermissions value)
	{
		return new NavVariant(parentOfResult, new NavEnumTestPermissions(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, TableConnectionType value)
	{
		return new NavVariant(parentOfResult, new NavTableConnectionType(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, DataClassification value)
	{
		return new NavVariant(parentOfResult, new NavDataClassification(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, ALVerbosity value)
	{
		return new NavVariant(parentOfResult, new NavVerbosity(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, TestTransactionModel value)
	{
		return new NavVariant(parentOfResult, new NavTestTransactionModel(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, NotificationScope value)
	{
		return new NavVariant(parentOfResult, new NavNotificationScope(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, FormResult value)
	{
		return new NavVariant(parentOfResult, new NavFormResult(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, ExecutionMode value)
	{
		return new NavVariant(parentOfResult, new NavExecutionMode(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, ExecutionContext value)
	{
		return new NavVariant(parentOfResult, new NavExecutionContext(value));
	}

	public static NavVariant ToVariant(ITreeObject parentOfResult, NavType value)
	{
		return new NavVariant(parentOfResult, new NavFieldType(value));
	}

	public static NavVariant NavRecordToVariant(ITreeObject parentOfResult, INavRecordHandle value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		NavVariant navVariant = NavVariant.Default(parentOfResult);
		navVariant.ALAssign(value.Target);
		return navVariant;
	}

	public static NavVariant NavValueToVariant(ITreeObject parentOfResult, NavValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return new NavVariant(parentOfResult, value);
	}

	public static T NavValueToNavValue<T>(NavValue obj) where T : NavValue
	{
		return ObjectToExactNavValue<T>(obj);
	}

	public static T NavValueToNavValue<T>(NavValue obj, INavValueMetadata metadata) where T : NavValue
	{
		return ObjectToExactNavValue<T>(obj, metadata);
	}

	public static TResult TryExecuteAndHandleTypeConversionException<TResult>(Func<TResult> func, NavType fromType, NavType toType)
	{
		try
		{
			return func();
		}
		catch (OverflowException innerException)
		{
			throw new NavNCLTypeConversionException(fromType, toType, innerException);
		}
		catch (FormatException innerException2)
		{
			throw new NavNCLTypeConversionException(fromType, toType, innerException2);
		}
		catch (NavNCLTypeConversionException innerException3)
		{
			throw new NavNCLTypeConversionException(fromType, toType, innerException3);
		}
	}

	public static bool NavIndirectValueToBoolean(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.InnerValue.NclType == NavNclType.NavDotNet)
		{
			return TryExecuteAndHandleTypeConversionException(() => ToBoolean(value.InnerValue as NavDotNet), value.NavType, NavType.Boolean);
		}
		return TryExecuteAndHandleTypeConversionException(() => value.InnerValue.ToBoolean(), value.NavType, NavType.Boolean);
	}

	public static byte NavIndirectValueToByte(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		NavValue innerValue = value.InnerValue;
		NavStringValue navStringValue = innerValue as NavStringValue;
		if (navStringValue != null && navStringValue.Value.Length == 1)
		{
			return NCLManagedAdapter.TextCharToByte(navStringValue.Value[0]);
		}
		if (innerValue is NavByte navByte)
		{
			return navByte.Value;
		}
		if (value.InnerValue.NclType == NavNclType.NavDotNet)
		{
			return TryExecuteAndHandleTypeConversionException(() => ToByte(value.InnerValue as NavDotNet), value.NavType, NavType.Byte);
		}
		return TryExecuteAndHandleTypeConversionException(() => innerValue.ToByte(), value.NavType, NavType.Byte);
	}

	public static char NavIndirectValueToChar(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		NavValue innerValue = value.InnerValue;
		if (innerValue is NavChar navChar)
		{
			return navChar.Value;
		}
		if (innerValue is NavByte navByte)
		{
			return ToChar(navByte.Value);
		}
		NavStringValue navStringValue = innerValue as NavStringValue;
		if (navStringValue != null && navStringValue.Value.Length == 1)
		{
			return navStringValue.Value[0];
		}
		if (value.InnerValue.NclType == NavNclType.NavDotNet)
		{
			return TryExecuteAndHandleTypeConversionException(() => ToChar(value.InnerValue as NavDotNet), value.NavType, NavType.Char);
		}
		return TryExecuteAndHandleTypeConversionException(() => ToChar(innerValue.ToInt32()), value.NavType, NavType.Char);
	}

	public static int NavIndirectValueToInt32(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.InnerValue.NclType == NavNclType.NavDotNet)
		{
			return TryExecuteAndHandleTypeConversionException(() => ToInt32(value.InnerValue as NavDotNet), value.NavType, NavType.Integer);
		}
		return TryExecuteAndHandleTypeConversionException(() => value.InnerValue.ToInt32(), value.NavType, NavType.Integer);
	}

	public static long NavIndirectValueToInt64(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.InnerValue.NclType == NavNclType.NavDotNet)
		{
			return TryExecuteAndHandleTypeConversionException(() => ToInt64(value.InnerValue as NavDotNet), value.NavType, NavType.BigInteger);
		}
		return TryExecuteAndHandleTypeConversionException(() => value.InnerValue.ToInt64(), value.NavType, NavType.BigInteger);
	}

	public static Decimal18 NavIndirectValueToDecimal(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.InnerValue.NclType == NavNclType.NavDotNet)
		{
			return TryExecuteAndHandleTypeConversionException(() => ToDecimal(value.InnerValue as NavDotNet), value.NavType, NavType.Decimal);
		}
		return TryExecuteAndHandleTypeConversionException(() => value.InnerValue.ToDecimal(), value.NavType, NavType.Decimal);
	}

	public static Guid NavIndirectValueToGuid(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.InnerValue is NavStringValue)
		{
			return StringToGuid(((NavStringValue)value.InnerValue).Value);
		}
		if (value.InnerValue.NclType == NavNclType.NavDotNet)
		{
			return TryExecuteAndHandleTypeConversionException(() => ToGuid(value.InnerValue as NavDotNet), value.NavType, NavType.GUID);
		}
		if (!value.IsOfNavType(NavType.GUID))
		{
			throw new NavNCLTypeConversionException(value.NavType, NavType.GUID);
		}
		return ((NavGuid)value.InnerValue).Value;
	}

	public static string NavIndirectValueToString(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value.CheckInitialized();
		if (value.InnerValue is NavStringValue || value.InnerValue.NclType == NavNclType.NavChar || value.InnerValue.NclType == NavNclType.NavByte || value.InnerValue.NclType == NavNclType.NavGuid)
		{
			return value.InnerValue.ToString();
		}
		throw new NavNCLTypeConversionException(value.NavType, NavType.String);
	}

	public static FieldClass NavIndirectValueToFieldClass(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.IsOfNavType(NavType.FieldClass))
		{
			throw new NavNCLTypeConversionException(value.NavType, NavType.FieldClass);
		}
		return ((NavFieldClass)value.InnerValue).Value;
	}

	public static TransactionType NavIndirectValueToTransactionType(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.IsOfNavType(NavType.TransactionType))
		{
			throw new NavNCLTypeConversionException(value.NavType, NavType.TransactionType);
		}
		return ((NavTransactionType)value.InnerValue).Value;
	}

	public static FormResult NavIndirectValueToFormResult(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.IsOfNavType(NavType.Action))
		{
			throw new NavNCLTypeConversionException(value.NavType, NavType.Action);
		}
		return ((NavFormResult)value.InnerValue).Value;
	}

	public static ExecutionMode NavIndirectValueToExecutionMode(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.IsOfNavType(NavType.ExecutionMode))
		{
			throw new NavNCLTypeConversionException(value.NavType, NavType.ExecutionMode);
		}
		return ((NavExecutionMode)value.InnerValue).Value;
	}

	public static ExecutionContext NavIndirectValueToExecutionContext(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.IsOfNavType(NavType.ExecutionContext))
		{
			throw new NavNCLTypeConversionException(value.NavType, NavType.ExecutionContext);
		}
		return ((NavExecutionContext)value.InnerValue).Value;
	}

	public static T NavIndirectValueToNavValue<T>(NavIndirectValue value) where T : NavValue
	{
		return NavIndirectValueToNavValue<T>(value, NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNclTypeFromType(typeof(T))));
	}

	public static T NavIndirectValueToNavValue<T>(NavIndirectValue value, INavValueMetadata metadata) where T : NavValue
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.IsInitialized)
		{
			return (T)NavValue.CreateNavValueFromObject(metadata, null);
		}
		if ((typeof(T).IsSubclassOf(typeof(NavStringValue)) || typeof(T) == typeof(NavByte) || typeof(T) == typeof(NavChar)) && (value.InnerValue is NavStringValue || value.InnerValue.NclType == NavNclType.NavByte || value.InnerValue.NclType == NavNclType.NavChar || value.InnerValue.NclType == NavNclType.NavGuid))
		{
			return (T)NavValue.CreateNavValueFromObject(metadata, value.ToString());
		}
		if (typeof(T) == typeof(NavOption))
		{
			return (T)(NavValue)NavOption.Create(metadata.NavOptionMetadata, value.ToInt32());
		}
		if (typeof(T) == typeof(NavDate) && value.InnerValue.NclType == NavNclType.NavDateTime)
		{
			return (T)(NavValue)ToNavDate((NavDateTime)value.InnerValue);
		}
		if (typeof(T) == typeof(NavDateTime) && value.InnerValue.NclType == NavNclType.NavDate)
		{
			return (T)(NavValue)NavDateTime.Create(((NavDate)value.InnerValue).Value, DateTimeReferenceFrame.Client);
		}
		if (typeof(T) == typeof(NavDuration))
		{
			return (T)(NavValue)new NavDuration(value.ToInt64());
		}
		if (typeof(T) != typeof(NavVariant) && value.InnerValue is T)
		{
			return (T)value.InnerValue;
		}
		if (typeof(T) == typeof(NavDotNet) && !(value.InnerValue is T))
		{
			return (T)(NavValue)ToNavDotNet(new NavScope(NavCurrentThread.Session), value.InnerValue);
		}
		throw new NavNCLTypeConversionException(typeof(NavIndirectValue), typeof(T));
	}

	public static NavVariant NavIndirectValueToNavVariant(ITreeObject parentOfResult, NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return new NavVariant(parentOfResult, value.InnerValue);
	}

	public static NavAutomation NavIndirectValueToNavAutomation(ITreeObject parentOfResult, NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return new NavAutomation(parentOfResult, value.InnerValue);
	}

	public static NavCodeunitHandle NavIndirectValueToNavCodeunitHandle(ITreeObject parentOfResult, NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value.CheckInitialized();
		NavValue innerValue = value.InnerValue;
		if (innerValue is NavCodeunitHandle result)
		{
			return result;
		}
		if (innerValue is NavCodeunit initialTarget)
		{
			return new NavCodeunitHandle(parentOfResult, initialTarget);
		}
		throw new NavNCLConversionException(value.InnerValue.GetType(), typeof(NavCodeunitHandle));
	}

	public static NavReportHandle NavIndirectValueToNavReportHandle(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value.CheckInitialized();
		NavValue innerValue = value.InnerValue;
		if (innerValue is NavReportHandle result)
		{
			return result;
		}
		throw new NavNCLConversionException(value.InnerValue.GetType(), typeof(NavReportHandle));
	}

	public static NavFormHandle NavIndirectValueToNavFormHandle(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value.CheckInitialized();
		NavValue innerValue = value.InnerValue;
		if (innerValue is NavFormHandle result)
		{
			return result;
		}
		throw new NavNCLConversionException(value.InnerValue.GetType(), typeof(NavFormHandle));
	}

	public static NavXmlPortHandle NavIndirectValueToNavXmlPortHandle(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value.CheckInitialized();
		NavValue innerValue = value.InnerValue;
		if (innerValue is NavXmlPortHandle result)
		{
			return result;
		}
		throw new NavNCLConversionException(value.InnerValue.GetType(), typeof(NavXmlPortHandle));
	}

	public static INavRecordHandle NavIndirectValueToINavRecordHandle(NavIndirectValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value.CheckInitialized();
		NavValue innerValue = value.InnerValue;
		if (innerValue is INavRecordHandle result)
		{
			return result;
		}
		if (innerValue is NavRecordRef navRecordRef)
		{
			INavRecordHandle record = navRecordRef.Record;
			if (record != null)
			{
				return record;
			}
		}
		throw new NavNCLConversionException(value.InnerValue.GetType(), typeof(INavRecordHandle));
	}

	public static NavValue ToNavValue(byte value)
	{
		return NavByte.Create(value);
	}

	public static NavValue ToNavValue(sbyte value)
	{
		return NavInteger.Create(value);
	}

	public static NavValue ToNavValue(bool value)
	{
		return NavBoolean.Create(value);
	}

	public static NavValue ToNavValue(int value)
	{
		return NavInteger.Create(value);
	}

	public static NavValue ToNavValue(ulong value)
	{
		return ToNavValue((long)value);
	}

	public static NavValue ToNavValue(long value)
	{
		return NavBigInteger.Create(value);
	}

	public static NavValue ToNavValue(Decimal18 value)
	{
		return NavDecimal.Create(value);
	}

	public static NavValue ToNavValue(double value)
	{
		return NavDecimal.Create(value);
	}

	public static NavValue ToNavValue(Guid value)
	{
		return new NavGuid(value);
	}

	public static NavValue ToNavValue(string value)
	{
		return NavText.Create(value);
	}

	public static NavValue ToNavValue(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return new NavByteArray(value);
	}

	public static NavValue ToNavValue(FieldClass value)
	{
		return new NavFieldClass(value);
	}

	public static NavValue ToNavValue(TransactionType value)
	{
		return new NavTransactionType(value);
	}

	public static NavValue ToNavValue(FormResult value)
	{
		return new NavFormResult(value);
	}

	public static NavValue ToNavValue(ExecutionMode value)
	{
		return new NavExecutionMode(value);
	}

	public static NavValue ToNavValue(ExecutionContext value)
	{
		return new NavExecutionContext(value);
	}

	public static NavValue ToNavValue(SecurityFiltering value)
	{
		return new NavSecurityFiltering(value);
	}

	public static NavValue ToNavValue(NavClientType value)
	{
		return new NavEnumClientType(value);
	}

	public static NavValue ToNavValue(TextEncoding value)
	{
		return new NavEnumTextEncoding(value);
	}

	public static NavValue ToNavValue(NavReportFormat value)
	{
		return new NavEnumReportFormat(value);
	}

	public static NavValue ToNavValue(NavDefaultLayout value)
	{
		return new NavEnumDefaultLayout(value);
	}

	public static NavValue ToNavValue(NavTestPermissions value)
	{
		return new NavEnumTestPermissions(value);
	}

	public static NavValue ToNavValue(TableConnectionType value)
	{
		return new NavTableConnectionType(value);
	}

	public static NavValue ToNavValue(DataClassification value)
	{
		return new NavDataClassification(value);
	}

	public static NavValue ToNavValue(ALVerbosity value)
	{
		return new NavVerbosity(value);
	}

	public static NavValue ToNavValue(TestTransactionModel value)
	{
		return new NavTestTransactionModel(value);
	}

	public static NavValue ToNavValue(ALErrorType value)
	{
		return new NavALErrorType(value);
	}

	public static NavValue ToNavValue(NavObjectType value)
	{
		return new NavEnumObjectType(value);
	}

	public static NavValue ToNavValue(NavType value)
	{
		return new NavFieldType(value);
	}

	public static NavValue ToNavValue(char value)
	{
		return NavChar.Create(value);
	}

	public static NavValue ToNavValue(NotificationScope value)
	{
		return new NavNotificationScope(value);
	}

	public static NavValue ToNavValue(WebServiceActionResultCode value)
	{
		return new NavWebServiceActionResultCode(value);
	}

	public static NavValue ToNavValue(DataScope value)
	{
		return new NavEnumDataScope(value);
	}

	public static NavValue ToNavValue(NavSessionSettings value)
	{
		return new NavSessionSettings(value);
	}

	public static NavValue ToNavValue(object value)
	{
		throw new NavNCLTypeConversionException(value.GetType(), typeof(NavValue));
	}

	public static FieldClass NavValueToFieldClass(NavValue value)
	{
		return NavValueToRuntimeEnum<FieldClass>(value, NavType.FieldClass);
	}

	public static TableConnectionType NavValueToTableConnectionType(NavValue value)
	{
		return NavValueToRuntimeEnum<TableConnectionType>(value, NavType.TableConnectionType);
	}

	public static NavTestPermissions NavValueToTestPermissions(NavValue value)
	{
		return NavValueToRuntimeEnum<NavTestPermissions>(value, NavType.TestPermissions);
	}

	public static TestTransactionModel NavValueToTestTransactionModel(NavValue value)
	{
		return NavValueToRuntimeEnum<TestTransactionModel>(value, NavType.TestTransactionModel);
	}

	public static TextEncoding NavValueToTextEncoding(NavValue value)
	{
		return NavValueToRuntimeEnum<TextEncoding>(value, NavType.TextEncoding);
	}

	public static TransactionType NavValueToTransactionType(NavValue value)
	{
		return NavValueToRuntimeEnum<TransactionType>(value, NavType.TransactionType);
	}

	public static FormResult NavValueToFormResult(NavValue value)
	{
		return NavValueToRuntimeEnum<FormResult>(value, NavType.Action);
	}

	public static NavClientType NavValueToClientType(NavValue value)
	{
		return NavValueToRuntimeEnum<NavClientType>(value, NavType.ClientType);
	}

	public static DataClassification NavValueToDataClassification(NavValue value)
	{
		return NavValueToRuntimeEnum<DataClassification>(value, NavType.DataClassification);
	}

	public static NavDefaultLayout NavValueToDefaultLayout(NavValue value)
	{
		return NavValueToRuntimeEnum<NavDefaultLayout>(value, NavType.DefaultLayout);
	}

	public static ExecutionContext NavValueToExecutionContext(NavValue value)
	{
		return NavValueToRuntimeEnum<ExecutionContext>(value, NavType.ExecutionContext);
	}

	public static ExecutionMode NavValueToExecutionMode(NavValue value)
	{
		return NavValueToRuntimeEnum<ExecutionMode>(value, NavType.ExecutionMode);
	}

	public static NavType NavValueToNavType(NavValue value)
	{
		return NavValueToRuntimeEnum<NavType>(value, NavType.TypeId);
	}

	public static NotificationScope NavValueToNotificationScope(NavValue value)
	{
		return NavValueToRuntimeEnum<NotificationScope>(value, NavType.NotificationScope);
	}

	public static NavObjectType NavValueToObjectType(NavValue value)
	{
		return NavValueToRuntimeEnum<NavObjectType>(value, NavType.ObjectType);
	}

	public static WebServiceActionResultCode NavValueToWebServiceActionResultCode(NavValue value)
	{
		return NavValueToRuntimeEnum<WebServiceActionResultCode>(value, NavType.WebServiceActionResultCode);
	}

	public static NavReportFormat NavValueToReportFormat(NavValue value)
	{
		return NavValueToRuntimeEnum<NavReportFormat>(value, NavType.ReportFormat);
	}

	public static SecurityFiltering NavValueToSecurityFiltering(NavValue value)
	{
		return NavValueToRuntimeEnum<SecurityFiltering>(value, NavType.SecurityFiltering);
	}

	public static ALVerbosity NavValueToVerbosity(NavValue value)
	{
		return NavValueToRuntimeEnum<ALVerbosity>(value, NavType.Verbosity);
	}

	public static ALErrorType NavValueToErrorType(NavValue value)
	{
		return NavValueToRuntimeEnum<ALErrorType>(value, NavType.ALErrorType);
	}

	public static bool ObjectToBoolean(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj.GetType() == typeof(bool))
		{
			return (bool)obj;
		}
		if (obj is NavValue navValue)
		{
			return navValue.ToBoolean();
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(bool));
	}

	public static byte ObjectToByte(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Type type = obj.GetType();
		if (type == typeof(byte))
		{
			return (byte)obj;
		}
		if (obj is NavValue { NclType: var nclType } navValue)
		{
			switch (nclType)
			{
			case NavNclType.NavByte:
				return navValue.ToByte();
			case NavNclType.NavBoolean:
			case NavNclType.NavInteger:
			case NavNclType.NavOption:
			case NavNclType.NavDuration:
				return ToByte(navValue.ToInt32());
			case NavNclType.NavBigInteger:
				return ToByte(navValue.ToInt64());
			case NavNclType.NavDecimal:
				return ToByte(navValue.ToDecimal());
			case NavNclType.NavDotNet:
			{
				NavValue navValue2 = ObjectEnumConversionHelper<byte>(navValue);
				if (navValue2 != null)
				{
					return navValue2.ToByte();
				}
				break;
			}
			}
		}
		else
		{
			if (type == typeof(int))
			{
				return ToByte((int)obj);
			}
			if (type == typeof(long))
			{
				return ToByte((long)obj);
			}
			if (type == typeof(decimal))
			{
				return ToByte((decimal)obj);
			}
			if (type == typeof(Decimal18))
			{
				return ToByte((Decimal18)obj);
			}
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(byte));
	}

	public static char ObjectToChar(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Type type = obj.GetType();
		if (type == typeof(char))
		{
			return (char)obj;
		}
		if (obj is NavValue { NclType: var nclType } navValue)
		{
			switch (nclType)
			{
			case NavNclType.NavByte:
				return ToChar(navValue.ToByte());
			case NavNclType.NavChar:
				return ((NavChar)navValue).Value;
			case NavNclType.NavBoolean:
			case NavNclType.NavInteger:
			case NavNclType.NavOption:
			case NavNclType.NavDuration:
				return ToChar(navValue.ToInt32());
			case NavNclType.NavBigInteger:
				return ToChar(navValue.ToInt64());
			case NavNclType.NavDecimal:
				return ToChar(navValue.ToDecimal());
			case NavNclType.NavDotNet:
			{
				NavValue navValue2 = ObjectEnumConversionHelper<char>(navValue);
				if (navValue2 != null)
				{
					return ToChar(navValue2.ToInt32());
				}
				break;
			}
			}
		}
		else
		{
			if (type == typeof(byte))
			{
				return ToChar((byte)obj);
			}
			if (type == typeof(int))
			{
				return ToChar((int)obj);
			}
			if (type == typeof(long))
			{
				return ToChar((long)obj);
			}
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(char));
	}

	public static int ObjectToInt32(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Type type = obj.GetType();
		if (type == typeof(int))
		{
			return (int)obj;
		}
		if (obj is NavValue { NclType: var nclType } navValue)
		{
			switch (nclType)
			{
			case NavNclType.NavBoolean:
			case NavNclType.NavByte:
			case NavNclType.NavInteger:
			case NavNclType.NavOption:
			case NavNclType.NavDuration:
				return navValue.ToInt32();
			case NavNclType.NavBigInteger:
				return ToInt32(navValue.ToInt64());
			case NavNclType.NavDecimal:
				return ToInt32(navValue.ToDecimal());
			case NavNclType.NavDotNet:
			{
				NavValue navValue2 = ObjectEnumConversionHelper<int>(navValue);
				if (navValue2 != null)
				{
					return navValue2.ToInt32();
				}
				break;
			}
			}
		}
		else
		{
			if (type == typeof(byte))
			{
				return (byte)obj;
			}
			if (type == typeof(long))
			{
				return ToInt32((long)obj);
			}
			if (type == typeof(decimal))
			{
				return ToInt32((decimal)obj);
			}
			if (type == typeof(Decimal18))
			{
				return ToInt32((Decimal18)obj);
			}
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(int));
	}

	public static long ObjectToInt64(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Type type = obj.GetType();
		if (type == typeof(long))
		{
			return (long)obj;
		}
		if (obj is NavValue { NclType: var nclType } navValue)
		{
			switch (nclType)
			{
			case NavNclType.NavBoolean:
			case NavNclType.NavByte:
			case NavNclType.NavInteger:
			case NavNclType.NavBigInteger:
			case NavNclType.NavOption:
			case NavNclType.NavDuration:
				return navValue.ToInt64();
			case NavNclType.NavDecimal:
				return ToInt64(navValue.ToDecimal());
			case NavNclType.NavDotNet:
			{
				NavValue navValue2 = ObjectEnumConversionHelper<long>(navValue);
				if (navValue2 != null)
				{
					return navValue2.ToInt64();
				}
				break;
			}
			}
		}
		else
		{
			if (type == typeof(byte))
			{
				return (byte)obj;
			}
			if (type == typeof(int))
			{
				return (int)obj;
			}
			if (type == typeof(decimal))
			{
				return ToInt64((decimal)obj);
			}
			if (type == typeof(Decimal18))
			{
				return ToInt64((Decimal18)obj);
			}
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(long));
	}

	public static Decimal18 ObjectToDecimal(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Type type = obj.GetType();
		if (type == typeof(decimal))
		{
			return (decimal)obj;
		}
		if (type == typeof(Decimal18))
		{
			return (Decimal18)obj;
		}
		if (obj is NavValue { NclType: var nclType } navValue)
		{
			if ((uint)(nclType - 1) <= 6u)
			{
				return navValue.ToDecimal();
			}
		}
		else
		{
			if (type == typeof(byte))
			{
				return (byte)obj;
			}
			if (type == typeof(int))
			{
				return (int)obj;
			}
			if (type == typeof(long))
			{
				return (long)obj;
			}
			if (type == typeof(double))
			{
				return (decimal)(double)obj;
			}
			if (type == typeof(float))
			{
				return (decimal)(float)obj;
			}
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(Decimal18));
	}

	public static Guid ObjectToGuid(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj.GetType() == typeof(Guid))
		{
			return (Guid)obj;
		}
		if (obj is NavValue navValue)
		{
			return navValue.ToGuid();
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(Guid));
	}

	public static string ObjectToString(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj is string result)
		{
			return result;
		}
		char? c = obj as char?;
		if (c.HasValue)
		{
			return c.Value.ToString();
		}
		if (obj is NavValue navValue && IsStringifiableNavValue(navValue))
		{
			return navValue.ToString();
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(string));
	}

	private static bool IsStringifiableNavValue(NavValue nv)
	{
		NavNclType nclType = nv.NclType;
		if ((uint)(nclType - 8) <= 6u)
		{
			return true;
		}
		return false;
	}

	public static FormResult ObjectToFormResult(object obj)
	{
		return ObjectToRuntimeEnum<FormResult>(obj, NavNclType.NavFormResult);
	}

	public static TransactionType ObjectToTransactionType(object obj)
	{
		return ObjectToRuntimeEnum<TransactionType>(obj, NavNclType.NavTransactionType);
	}

	public static ExecutionMode ObjectToExecutionMode(object obj)
	{
		return ObjectToRuntimeEnum<ExecutionMode>(obj, NavNclType.NavExecutionMode);
	}

	public static FieldClass ObjectToFieldClass(object value)
	{
		return ObjectToRuntimeEnum<FieldClass>(value, NavNclType.NavFieldClass);
	}

	public static ExecutionContext ObjectToExecutionContext(object value)
	{
		return ObjectToRuntimeEnum<ExecutionContext>(value, NavNclType.NavExecutionContext);
	}

	public static TableConnectionType ObjectToTableConnectionType(object obj)
	{
		return ObjectToRuntimeEnum<TableConnectionType>(obj, NavNclType.NavTableConnectionType);
	}

	public static SecurityFiltering ObjectToSecurityFiltering(object obj)
	{
		return ObjectToRuntimeEnum<SecurityFiltering>(obj, NavNclType.NavSecurityFiltering);
	}

	public static NavClientType ObjectToClientType(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj.GetType() == typeof(NavClientType) || obj.GetType() == typeof(ClientType))
		{
			return (NavClientType)obj;
		}
		if (obj is NavValue { NclType: NavNclType.NavClientType } navValue)
		{
			return ((NavEnumClientType)navValue).Value;
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavClientType));
	}

	public static TextEncoding ObjectToEncoding(object value)
	{
		return ObjectToRuntimeEnum<TextEncoding>(value, NavNclType.NavTextEncoding);
	}

	public static TextEncoding ObjectToTextEncoding(object value)
	{
		return ObjectToEncoding(value);
	}

	public static NavReportFormat ObjectToReportFormat(object obj)
	{
		return ObjectToRuntimeEnum<NavReportFormat>(obj, NavNclType.NavReportFormat);
	}

	public static NavDefaultLayout ObjectToDefaultLayout(object obj)
	{
		return ObjectToRuntimeEnum<NavDefaultLayout>(obj, NavNclType.NavDefaultLayout);
	}

	public static NotificationScope ObjectToNotificationScope(object sourceValue)
	{
		return ObjectToRuntimeEnum<NotificationScope>(sourceValue, NavNclType.NavNotificationScope);
	}

	public static NavTestPermissions ObjectToTestPermissions(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj.GetType() == typeof(NavTestPermissions))
		{
			return (NavTestPermissions)obj;
		}
		if (obj is NavValue { NclType: NavNclType.NavTestPermissions } navValue)
		{
			return ((NavEnumTestPermissions)navValue).Value;
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavTestPermissions));
	}

	public static DataClassification ObjectToDataClassification(object value)
	{
		return ObjectToRuntimeEnum<DataClassification>(value, NavNclType.NavDataClassification);
	}

	public static WebServiceActionResultCode ObjectToWebServiceActionResultCode(object value)
	{
		return ObjectToRuntimeEnum<WebServiceActionResultCode>(value, NavNclType.NavWebServiceActionResultCode);
	}

	public static DataScope ObjectToDataScope(object value)
	{
		return ObjectToRuntimeEnum<DataScope>(value, NavNclType.NavDataScope);
	}

	public static ALVerbosity ObjectToVerbosity(object value)
	{
		return ObjectToRuntimeEnum<ALVerbosity>(value, NavNclType.NavVerbosity);
	}

	public static NavType ObjectToNavType(object value)
	{
		return ObjectToRuntimeEnum<NavType>(value, NavNclType.NavFieldType);
	}

	public static TestTransactionModel ObjectToTestTransactionModel(object value)
	{
		return ObjectToRuntimeEnum<TestTransactionModel>(value, NavNclType.NavTestTransactionModel);
	}

	public static NavSessionSettings ObjectToSessionSettings(object sourceValue)
	{
		if (sourceValue == null)
		{
			throw new ArgumentNullException("sourceValue");
		}
		if (sourceValue.GetType() == typeof(NavSessionSettings))
		{
			return (NavSessionSettings)sourceValue;
		}
		if (sourceValue is NavValue { NclType: NavNclType.NavSessionSettings } navValue)
		{
			return (NavSessionSettings)navValue;
		}
		throw new NavNCLConversionException(sourceValue.GetType(), typeof(NavSessionSettings));
	}

	public static NavObjectType ObjectToObjectType(object obj)
	{
		return ObjectToRuntimeEnum<NavObjectType>(obj, NavNclType.NavObjectType);
	}

	public static ALErrorType ObjectToALErrorType(object value)
	{
		return ObjectToRuntimeEnum<ALErrorType>(value, NavNclType.NavALErrorType);
	}

	public static NavFilterPageBuilder ObjectToFilterPageBuilder(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj.GetType() == typeof(NavFilterPageBuilder))
		{
			return (NavFilterPageBuilder)obj;
		}
		if (obj is NavValue { NclType: NavNclType.NavFilterPageBuilder } navValue)
		{
			return (NavFilterPageBuilder)navValue;
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavFilterPageBuilder));
	}

	public static byte[] ObjectToByteArray(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj.GetType() == typeof(byte[]))
		{
			return (byte[])obj;
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(byte[]));
	}

	public static NavALErrorInfo ObjectToNavErrorInfo(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.GetType() == typeof(NavALErrorInfo))
		{
			return (NavALErrorInfo)value;
		}
		throw new NavNCLConversionException(value.GetType(), typeof(NavALErrorInfo));
	}

	public static T ObjectToExactNavValue<T>(object obj) where T : NavValue
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is T result)
		{
			return result;
		}
		NavValueMetadata metadata = NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromType(typeof(T)));
		return (T)NavValue.CreateNavValueFromObject(metadata, obj);
	}

	private static NavValue ObjectEnumConversionHelper<T>(NavValue navValue)
	{
		if (navValue is NavDotNet { RunOnClient: false } navDotNet)
		{
			object navObjectFromDotNetObject = navDotNet.GetNavObjectFromDotNetObject(typeof(T), navDotNet);
			if (navObjectFromDotNetObject != null)
			{
				return navObjectFromDotNetObject as NavValue;
			}
		}
		return null;
	}

	public static T ObjectToExactNavValue<T>(object obj, INavValueMetadata metadata) where T : NavValue
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is T result)
		{
			return result;
		}
		if (metadata == null)
		{
			metadata = NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromType(typeof(T)));
		}
		return (T)NavValue.CreateNavValueFromObject(metadata, obj);
	}

	public static INavRecordHandle ObjectToExactINavRecordHandle(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is INavRecordHandle result)
		{
			return result;
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(INavRecordHandle));
	}

	public static NavCodeunitHandle ObjectToExactNavCodeunitHandle(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is NavCodeunitHandle result)
		{
			return result;
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavCodeunitHandle));
	}

	public static NavValue ObjectToNavValue(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		return NavValue.CreateNavValueFromObject(NavValueMetadata.CalcMetadataFromObject(obj), obj);
	}

	public static NavVariant ObjectToNavVariant(ITreeObject parentOfResult, object obj)
	{
		if (obj == null)
		{
			return NavVariant.Default(parentOfResult);
		}
		if (obj is NavVariant result)
		{
			return result;
		}
		if (obj is NavValue initValue)
		{
			return new NavVariant(parentOfResult, initValue);
		}
		return new NavVariant(parentOfResult, NavValue.CreateNavValueFromObject(NavValueMetadata.CalcMetadataFromDotNetObject(obj), obj));
	}

	public static NavOutStream ObjectToNavOutStream(ITreeObject parentOfResult, object obj)
	{
		if (obj == null)
		{
			return NavOutStream.Default(parentOfResult);
		}
		if (obj is NavVariant navVariant)
		{
			obj = navVariant.InnerValue;
		}
		if (obj is NavOutStream result)
		{
			return result;
		}
		if (obj is IStream stream)
		{
			return new NavOutStream(parentOfResult, new NavStreamProvider(stream));
		}
		if (obj is NavAutomation { Value: IStream value })
		{
			return new NavOutStream(parentOfResult, new NavStreamProvider(value));
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavOutStream));
	}

	public static NavOutStream DotNetToNavOutStream(ITreeObject parentOfResult, NavDotNet obj)
	{
		if (obj == null)
		{
			return NavOutStream.Default(parentOfResult);
		}
		if (obj.Value is Stream stream)
		{
			return new NavOutStream(parentOfResult, new NavStreamProvider(stream));
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavOutStream));
	}

	public static NavDotNet OutStreamToDotNet(ITreeObject parentOfResult, NavStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new NavDotNet(parentOfResult, stream.InternalStreamChecked, runOnClient: false, suppressDispose: true);
	}

	public static NavInStream ObjectToNavInStream(ITreeObject parentOfResult, object obj)
	{
		if (obj == null)
		{
			return NavInStream.Default(parentOfResult);
		}
		if (obj is NavVariant navVariant)
		{
			obj = navVariant.InnerValue;
		}
		if (obj is NavInStream result)
		{
			return result;
		}
		if (obj is IStream stream)
		{
			return new NavInStream(parentOfResult, new NavStreamProvider(stream));
		}
		if (obj is NavAutomation { Value: IStream value })
		{
			return new NavInStream(parentOfResult, new NavStreamProvider(value));
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavInStream));
	}

	public static NavInStream DotNetToNavInStream(ITreeObject parentOfResult, NavDotNet obj)
	{
		if (obj == null)
		{
			return NavInStream.Default(parentOfResult);
		}
		if (obj.Value is Stream stream)
		{
			return new NavInStream(parentOfResult, new NavStreamProvider(stream));
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(NavInStream));
	}

	public static NavDotNet InStreamToDotNet(ITreeObject parentOfResult, NavStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new NavDotNet(parentOfResult, stream.InternalStreamChecked, runOnClient: false, suppressDispose: true);
	}

	private static T ObjectToRuntimeEnum<T>(object value, NavNclType targetNclType) where T : struct
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.GetType() == typeof(T))
		{
			return (T)value;
		}
		if (value is NavValue navValue && navValue.NclType == targetNclType)
		{
			return ((NavEnumBase<T>)navValue).Value;
		}
		throw new NavNCLConversionException(value.GetType(), typeof(T));
	}

	private static T NavValueToRuntimeEnum<T>(NavValue value, NavType navType) where T : struct
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.NavType != navType)
		{
			throw new NavNCLConversionException(value.NavType, navType);
		}
		return ((NavEnumBase<T>)value).Value;
	}

	public static NavDuration ToDuration(char fromValue)
	{
		return new NavDuration(fromValue);
	}

	public static NavDuration ToDuration(long fromValue)
	{
		return new NavDuration(fromValue);
	}

	public static NavDuration ToDuration(Decimal18 fromValue)
	{
		return new NavDuration(ToInt64(fromValue));
	}

	public static NavDuration ToDuration(NavOption fromValue)
	{
		if (fromValue == null)
		{
			throw new ArgumentNullException("fromValue");
		}
		return new NavDuration(fromValue.Value);
	}

	public static Guid StringToGuid(string value)
	{
		if (value != null && value.Length > 38)
		{
			value = value.Substring(0, 38);
		}
		return new NavGuid(value).Value;
	}

	public static Guid ToGuid(NavDotNet fromValue)
	{
		object obj = null;
		try
		{
			obj = fromValue.GetValue<object>();
			if (obj is Guid)
			{
				return (Guid)obj;
			}
			throw new InvalidCastException();
		}
		catch
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, (obj == null) ? typeof(object) : obj.GetType(), fromValue, typeof(Guid));
		}
	}

	public static string GuidToString(Guid guid)
	{
		return guid.ToString("B").ToUpperInvariant();
	}

	public static NavOemText GuidToNavOemText(Guid value)
	{
		string text = value.ToString("B").ToUpperInvariant();
		return new NavOemText(text.Length, text);
	}

	public static NavOemCode GuidToNavOemCode(Guid value)
	{
		string text = value.ToString("B").ToUpperInvariant();
		return new NavOemCode(text.Length, text);
	}

	public static NavText GuidToNavText(Guid value)
	{
		string text = value.ToString("B").ToUpperInvariant();
		return new NavText(text.Length, text);
	}

	public static NavCode GuidToNavCode(Guid value)
	{
		string text = value.ToString("B").ToUpperInvariant();
		return new NavCode(text.Length, text);
	}

	public static NavAutomation ToNavAutomation(object value)
	{
		return (NavAutomation)value;
	}

	public static object ToObject(NavAutomation fromValue)
	{
		return fromValue;
	}

	public static object GetObjectValue(NavAutomation fromValue)
	{
		if (fromValue == null)
		{
			throw new ArgumentNullException("fromValue");
		}
		return fromValue.Value;
	}

	public static NavDotNet ToNavDotNet(ITreeObject parentOfResult, object value)
	{
		if (value is NavDotNet result)
		{
			return result;
		}
		if (value is NavValue navValue)
		{
			value = ConvertNavValueToClrValue(navValue);
		}
		return new NavDotNet(parentOfResult, value, runOnClient: false);
	}

	public static object ConvertNavValueToClrValue(NavValue navValue)
	{
		NavType navType = navValue.NavType;
		switch (navType)
		{
		case NavType.OemText:
		case NavType.Date:
		case NavType.Decimal:
		case NavType.TextConst:
		case NavType.Text:
		case NavType.Code:
		case NavType.BLOB:
		case NavType.Boolean:
		case NavType.Integer:
		case NavType.Byte:
		case NavType.OemCode:
		case NavType.BigInteger:
		case NavType.GUID:
		case NavType.DateTime:
		case NavType.BigText:
		case NavType.String:
			return navValue.ClientObject;
		case NavType.Char:
			return (navValue.ToInt32() == 0) ? string.Empty : navValue.ToString();
		case NavType.TableFilter:
		case NavType.RecordID:
		case NavType.Option:
			return navValue.ToString();
		case NavType.Time:
		case NavType.DateFormula:
		case NavType.Duration:
			return NavFormatEvaluateHelper.Format(navValue, 0, 9);
		case NavType.Media:
		case NavType.MediaSet:
			return navValue.ToGuid();
		default:
			throw new NotSupportedException(navType.ToString());
		}
	}

	public static object ToObject(NavDotNet fromValue)
	{
		return fromValue;
	}

	public static NavInStream ToNavInStream(ITreeObject parentOfResult, Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new NavInStream(parentOfResult, new NavStreamProvider(stream));
	}

	public static NavInStream ToNavInStream(ITreeObject parentOfResult, NavStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new NavInStream(parentOfResult, new NavStreamProvider(stream.InternalStream));
	}

	public static NavOutStream ToNavOutStream(ITreeObject parentOfResult, Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new NavOutStream(parentOfResult, new NavStreamProvider(stream));
	}

	public static NavOutStream ToNavOutStream(ITreeObject parentOfResult, NavStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new NavOutStream(parentOfResult, new NavStreamProvider(stream.InternalStream));
	}

	public static Stream ToStream(NavInStream inStream)
	{
		if (inStream == null)
		{
			throw new ArgumentNullException("inStream");
		}
		return inStream.InternalStreamChecked;
	}

	public static Stream ToStream(NavOutStream outStream)
	{
		if (outStream == null)
		{
			throw new ArgumentNullException("outStream");
		}
		return outStream.InternalStreamChecked;
	}

	public static NavOemText DotNetToNavOemText(NavDotNet obj)
	{
		if (obj == null)
		{
			return NavOemText.Default(2048);
		}
		object value = obj.GetValue<string>();
		if (value != null && value.GetType() == typeof(string))
		{
			return new NavOemText(2048, (string)value);
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(string));
	}

	public static NavDotNet NavOemTextToDotNet(ITreeObject parentOfResult, NavOemText text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new NavDotNet(parentOfResult, text.Value, runOnClient: false);
	}

	public static NavText DotNetToNavText(NavDotNet obj)
	{
		if (obj == null)
		{
			return NavText.Default(int.MaxValue);
		}
		object value = obj.GetValue<string>();
		if (value != null && value.GetType() == typeof(string))
		{
			return new NavText(int.MaxValue, (string)value);
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(string));
	}

	public static NavDotNet NavTextToDotNet(ITreeObject parentOfResult, NavText text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new NavDotNet(parentOfResult, text.Value, runOnClient: false);
	}

	public static NavCode DotNetToNavCode(NavDotNet obj)
	{
		if (obj == null)
		{
			return NavCode.Default(2048);
		}
		object value = obj.GetValue<string>();
		if (value != null && value.GetType() == typeof(string))
		{
			return new NavCode(2048, (string)value);
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(string));
	}

	public static NavDotNet NavCodeToDotNet(ITreeObject parentOfResult, NavCode text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new NavDotNet(parentOfResult, text.Value, runOnClient: false);
	}

	public static NavOemCode DotNetToNavOemCode(NavDotNet obj)
	{
		if (obj == null)
		{
			return NavOemCode.Default(2048);
		}
		object value = obj.GetValue<string>();
		if (value != null && value.GetType() == typeof(string))
		{
			return new NavOemCode(2048, (string)value);
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(string));
	}

	public static NavDotNet NavOemCodeToDotNet(ITreeObject parentOfResult, NavOemCode text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new NavDotNet(parentOfResult, text.Value, runOnClient: false);
	}

	public static NavDotNet ClrStringToDotNet(ITreeObject parentOfResult, string value)
	{
		return new NavDotNet(parentOfResult, value, runOnClient: false);
	}

	public static NavDotNet NavTextConstantToDotNet(ITreeObject parentOfResult, NavTextConstant text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new NavDotNet(parentOfResult, text.Value, runOnClient: false);
	}

	public static NavDate ToNavDate(NavDateTime dateTime)
	{
		return ALSystemDate.ALDT2Date(dateTime);
	}

	public static NavDateTime DotNetToNavDateTime(NavDotNet obj)
	{
		if (obj == null)
		{
			return NavDateTime.Default;
		}
		object value = obj.GetValue<DateTime>();
		if (value != null && value.GetType() == typeof(DateTime))
		{
			return NavDateTime.Create((DateTime)value, DateTimeReferenceFrame.Client);
		}
		throw new NavNCLConversionException(obj.GetType(), typeof(DateTime));
	}

	public static NavDotNet DateTimeToDotNet(ITreeObject parentOfResult, NavDateTime dateTime)
	{
		if (dateTime == null)
		{
			throw new ArgumentNullException("dateTime");
		}
		return new NavDotNet(parentOfResult, dateTime.Value, runOnClient: false);
	}

	public static int ToInt32(bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	public static bool ToBoolean(int value)
	{
		return value != 0;
	}

	public static bool ToBoolean(NavDotNet value)
	{
		try
		{
			return (bool)value.GetValue<bool>();
		}
		catch
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(bool), value, typeof(bool));
		}
	}

	public static byte ToByte(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		return NCLManagedAdapter.TextCharToByte(text[0]);
	}

	public static byte ToByte(char value)
	{
		return NCLManagedAdapter.TextCharToByte(value);
	}

	public static char ToChar(byte oemValue)
	{
		return NCLManagedAdapter.ByteToTextChar(oemValue);
	}

	public static char ToChar(ushort fromValue)
	{
		try
		{
			return (char)fromValue;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(ushort), fromValue, typeof(char));
		}
	}

	public static char ToChar(float fromValue)
	{
		try
		{
			return (char)checked((ushort)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(float), fromValue, typeof(char));
		}
	}

	public static char ToChar(double fromValue)
	{
		try
		{
			return (char)checked((ushort)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), fromValue, typeof(char));
		}
	}

	public static char ToChar(ulong fromValue)
	{
		try
		{
			return (char)checked((ushort)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(ulong), fromValue, typeof(char));
		}
	}

	public static char ToChar(uint fromValue)
	{
		try
		{
			return (char)checked((ushort)fromValue);
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(uint), fromValue, typeof(char));
		}
	}

	public static char ToChar(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return '\0';
		}
		return text[0];
	}

	public static char ToChar(Decimal18 fromValue)
	{
		try
		{
			char c = (char)(ushort)fromValue;
			if (c != fromValue)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(char));
			}
			return c;
		}
		catch (OverflowException)
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(Decimal18), fromValue, typeof(char));
		}
	}

	public static char ToChar(NavDotNet value)
	{
		try
		{
			return (char)value.GetValue<char>();
		}
		catch
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(char), value, typeof(char));
		}
	}

	public static char NavDotNetToChar(NavDotNet fromValue)
	{
		return ConvertEnumType<char>(fromValue);
	}

	public static char NavValueToChar(NavValue value)
	{
		return ToChar(value.ToInt32());
	}

	public static string ToString(byte oemValue)
	{
		return NCLManagedAdapter.ByteToTextChar(oemValue).ToString();
	}

	public static NavOemText ConvertToDotNetFormatString(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		switch (format)
		{
		case "<Precision,2:2><Standard format,0>":
		case "<Precision,2:2><Standard Format,0>":
			return stdFmt220;
		case "<Precision,2:5><Standard format,0>":
		case "<Precision,2:5><Standard Format,0>":
			return stdFmt250;
		case "<Precision,0:0><Standard format,0>":
		case "<Precision,0:0><Standard Format,0>":
			return stdFmt000;
		case "<Precision,0:3><Standard format,0>":
		case "<Precision,0:3><Standard Format,0>":
			return stdFmt030;
		case "<Precision,3:3><Standard format,0>":
		case "<Precision,3:3><Standard Format,0>":
			return stdFmt330;
		default:
			if (!string.IsNullOrEmpty(format) && format.StartsWith("<Custom,", StringComparison.Ordinal) && format.EndsWith(">", StringComparison.Ordinal))
			{
				format = format.Substring("<Custom,".Length, format.Length - 1 - "<Custom,".Length);
			}
			return BuildClrFormatString(format);
		}
	}

	public static NavText InvokeAutoFormatTranslate(int autoFormatType, string autoFormatExpression)
	{
		return NavCurrentThread.Session.SystemCodeunitFactory.UIHelperTriggers.InvokeAutoFormatTranslate(autoFormatType, autoFormatExpression);
	}

	private static NavOemText BuildClrFormatString(string format)
	{
		if (string.IsNullOrEmpty(format))
		{
			return failFormat;
		}
		try
		{
			int num = format.LastIndexOf("<Precision,", StringComparison.OrdinalIgnoreCase);
			int num2 = format.LastIndexOf("<Standard Format,", StringComparison.OrdinalIgnoreCase);
			if (num == -1 || num2 == -1)
			{
				return failFormat;
			}
			int num3 = format.IndexOf('>', num2);
			int num4 = format.IndexOf('>', num);
			string text = format.Substring(num + "<Precision,".Length, num4 - "<Precision,".Length - num);
			int num5 = text.IndexOf(':');
			string text2;
			string text3;
			if (num5 == -1)
			{
				text2 = text;
				text3 = text;
			}
			else
			{
				text2 = text.Substring(0, num5++);
				text3 = text.Substring(num5, text.Length - num5);
				if (string.IsNullOrEmpty(text2) && string.IsNullOrEmpty(text3))
				{
					text2 = (text3 = "2");
				}
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "0";
				}
				if (string.IsNullOrEmpty(text3))
				{
					text3 = text2;
				}
			}
			int num6 = int.Parse(text2, CultureInfo.InvariantCulture);
			int num7 = int.Parse(text3, CultureInfo.InvariantCulture);
			num5 = num2 + "<Standard Format,".Length;
			int standardFormat = int.Parse(format.Substring(num5, num3 - num5), CultureInfo.InvariantCulture);
			string returnFormat = GetReturnFormat(standardFormat);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 1; i <= num7; i++)
			{
				if (i <= num6)
				{
					stringBuilder.Append('0');
				}
				else
				{
					stringBuilder.Append('#');
				}
			}
			string text4 = string.Format(CultureInfo.InvariantCulture, returnFormat, stringBuilder.ToString());
			int num8 = Math.Min(num, num2);
			if (num8 > 0)
			{
				text4 = format.Substring(0, num8) + text4;
			}
			int num9 = Math.Max(num4, num3);
			if (num9 < format.Length - 1)
			{
				text4 += format.Substring(num9 + 1, format.Length - num9 - 1);
			}
			return new NavOemText(text4.Length, text4);
		}
		catch (FormatException)
		{
			return failFormat;
		}
	}

	private static string GetReturnFormat(int standardFormat)
	{
		switch (standardFormat)
		{
		case 1:
		case 2:
		case 9:
			return "###0.{0}";
		case 3:
			return "#,##0.{0};#,##0.{0}-";
		case 4:
			return "###0.{0};###0.{0}-";
		default:
			return "#,##0.{0}";
		}
	}
}
