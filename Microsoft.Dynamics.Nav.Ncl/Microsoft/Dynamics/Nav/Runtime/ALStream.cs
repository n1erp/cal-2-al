using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALStream
{
	private enum ReadOptions
	{
		None,
		StopOnZeroAndIncludeIfPresent,
		StopOnZeroOrLFIgnoreCR,
		StopOnZeroCharAndIncludeIfPresent,
		StopOnZeroCharOrLFIgnoreCR
	}

	private static readonly byte[] CRLF = new byte[2] { 13, 10 };

	private static readonly byte[] CRLFUTF16 = new byte[4] { 13, 0, 10, 0 };

	public const int DefaultLength = int.MinValue;

	internal const int ReadLimitChunk = 1000000;

	private const int BigIntegerSizeInStream = 8;

	private const int BooleanSizeInStream = 4;

	private const int ByteSizeInStream = 1;

	private const int DateSizeInStream = 4;

	private const int DateTimeSizeInStream = 8;

	private const int DecimalSizeInStream = 12;

	private const int DurationSizeInStream = 8;

	private const int GuidSizeInStream = 16;

	private const int IntegerSizeInStream = 4;

	private const int OptionSizeInStream = 4;

	private const int TimeSizeInStream = 4;

	private const int DateFormulaSizeInStream = 32;

	private const int TableFilterSizeInStream = 504;

	private const int CharSizeInStream = 1;

	internal static int ReadLimit => ServerUserSettings.Instance.MaxStreamReadSize.Value;

	private static byte[] GetCRLFBytes(TextEncoding encoding)
	{
		if (encoding != TextEncoding.UTF16)
		{
			return CRLF;
		}
		return CRLFUTF16;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<bool> passByRef, int length = int.MinValue)
	{
		bool value = false;
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<byte> passByRef, int length = int.MinValue)
	{
		byte value = 0;
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<Decimal18> passByRef, int length = int.MinValue)
	{
		Decimal18 value = 0;
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<Guid> passByRef, int length = int.MinValue)
	{
		Guid value = Guid.Empty;
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<int> passByRef, int length = int.MinValue)
	{
		int value = 0;
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<long> passByRef, int length = int.MinValue)
	{
		long value = 0L;
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<char> passByRef, int length = int.MinValue)
	{
		char value = '\0';
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ByRef<string> passByRef, int length = int.MinValue)
	{
		string value = string.Empty;
		int result = ALRead(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALRead<T>(INavStreamReader reader, DataError dataError, ByRef<T> passByRef, int length = int.MinValue) where T : NavValue
	{
		INavValueMetadata value = passByRef.Value;
		int result = ALReadNavValue(reader, dataError, value, out var value2, length);
		if (value2 != null)
		{
			passByRef.Value = (T)value2;
		}
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref bool value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 4);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length != 0)
			{
				value = array[0] != 0;
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref byte value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 1);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length != 0)
			{
				value = array[0];
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref Decimal18 value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 12);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 12)
			{
				value = NavDecimalHelper.NavDecimalToDecimal(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref Guid value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 16);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 16)
			{
				value = new Guid(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref int value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 4);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 4)
			{
				value = BitConverter.ToInt32(array, 0);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref long value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 8);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 8)
			{
				value = BitConverter.ToInt64(array, 0);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref char value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			int num = ((reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows) ? 1 : 2);
			CheckOrSetLength(ref length, num);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == num)
			{
				if (reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows)
				{
					value = NCLManagedAdapter.ByteToTextChar(array[0], reader.TextEncoding);
				}
				else
				{
					value = (char)((array[1] << 8) + array[0]);
				}
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref string value, int length)
	{
		NavText value2 = new NavText(int.MaxValue, value);
		int result = ALRead(reader, dataError, ref value2, length);
		value = value2.Value;
		return result;
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavDate value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 4);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 4)
			{
				value = NavDate.Create(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavDateFormula value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 32);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 32)
			{
				value = new NavDateFormula(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavDateTime value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 8);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 8)
			{
				value = NavDateTime.Create(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavDuration value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 8);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 8)
			{
				value = new NavDuration(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavOption value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 4);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 4)
			{
				value = NavOption.CreateFromBytes(value.NavOptionMetadata, array, 0, array.Length);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavRecordId value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavTableFilter value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 504);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 504)
			{
				value = new NavTableFilter(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavTime value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (reader.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
		{
			CheckOrSetLength(ref length, 4);
			byte[] array = ReadBytes(reader, dataError, length);
			if (array.Length == 4)
			{
				value = NavTime.Create(array);
			}
			return array.Length;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavText value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.MaxLength != int.MaxValue && value.MaxLength > 2048)
		{
			throw NavNCLStreamWideTextMaxLengthException.Create(CultureInfo.CurrentCulture);
		}
		if (length < 0 || length > value.MaxLength)
		{
			length = value.MaxLength;
		}
		switch (reader.Mode)
		{
		case NavStreamMode.Stream:
		{
			if (reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows)
			{
				byte[] array = ReadBytes(reader, dataError, length, ReadOptions.StopOnZeroAndIncludeIfPresent);
				value = new NavText(value.MaxLength, NCLManagedAdapter.ByteToText(array, reader.TextEncoding));
				return array.Length;
			}
			string value3 = ReadString(reader, length, ReadOptions.StopOnZeroCharAndIncludeIfPresent, skipAlphaIndicator: false);
			value = new NavText(value.MaxLength, value3);
			if (reader.TextEncoding != TextEncoding.UTF16)
			{
				return Encoding.UTF8.GetByteCount(value) + 1;
			}
			return Encoding.Unicode.GetByteCount(value) + 2;
		}
		case NavStreamMode.FileBinary:
		{
			if (reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows)
			{
				byte[] array = ReadBytes(reader, DataError.TrapError, length + 1);
				if (array.Length > length && array.Length != 0 && array[length] != 0)
				{
					throw new NavNCLInvalidStreamDataException();
				}
				value = new NavText(value.MaxLength, NCLManagedAdapter.ByteToText(array, reader.TextEncoding));
				return array.Length;
			}
			string value2 = ReadString(reader, length, ReadOptions.StopOnZeroCharAndIncludeIfPresent, skipAlphaIndicator: false);
			value = new NavText(value.MaxLength, value2);
			if (reader.TextEncoding != TextEncoding.UTF16)
			{
				return Encoding.UTF8.GetByteCount(value) + 1;
			}
			return Encoding.Unicode.GetByteCount(value) + 2;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALRead(INavStreamReader reader, DataError dataError, ref NavCode value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.MaxLength > 2048)
		{
			throw NavNCLStreamWideTextMaxLengthException.Create(CultureInfo.CurrentCulture);
		}
		if (length < 0 || length > value.MaxLength)
		{
			length = value.MaxLength;
		}
		switch (reader.Mode)
		{
		case NavStreamMode.Stream:
		{
			if (reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows)
			{
				byte[] array = ReadBytes(reader, dataError, length, ReadOptions.StopOnZeroAndIncludeIfPresent);
				value = new NavCode(value.MaxLength, NCLManagedAdapter.ByteToText(array, reader.TextEncoding));
				return array.Length;
			}
			string value3 = ReadString(reader, length, ReadOptions.StopOnZeroCharAndIncludeIfPresent, skipAlphaIndicator: false);
			value = new NavCode(value.MaxLength, value3);
			if (reader.TextEncoding != TextEncoding.UTF16)
			{
				return Encoding.UTF8.GetByteCount(value) + 1;
			}
			return Encoding.Unicode.GetByteCount(value) + 2;
		}
		case NavStreamMode.FileBinary:
		{
			if (reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows)
			{
				byte[] array = ReadBytes(reader, DataError.TrapError, length + 2);
				if (array.Length > length && array.Length != 0 && array[length + 1] != 0)
				{
					throw new NavNCLInvalidStreamDataException();
				}
				value = new NavCode(value.MaxLength, array, reader.TextEncoding);
				return array.Length;
			}
			string value2 = ReadString(reader, length, ReadOptions.StopOnZeroCharAndIncludeIfPresent, skipAlphaIndicator: true);
			value = new NavCode(value.MaxLength, value2);
			if (reader.TextEncoding != TextEncoding.UTF16)
			{
				return Encoding.UTF8.GetByteCount(value) + 2;
			}
			return Encoding.Unicode.GetByteCount(value) + 3;
		}
		case NavStreamMode.FileText:
		{
			string text = ReadLine(reader, dataError);
			if (!ALSystemVariable.ALEvaluate(dataError, ref value, text, 0))
			{
				return 0;
			}
			return text.Length;
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, bool value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		CheckOrSetLength(ref length, 4);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, BitConverter.GetBytes(value ? 1 : 0));
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(NavBoolean.Create(value));
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, byte value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		CheckOrSetLength(ref length, 1);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, new byte[1] { value });
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(NavByte.Create(value));
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavBigText value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, Decimal18 value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		CheckOrSetLength(ref length, 12);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, NavDecimalHelper.DecimalToNavDecimal(value));
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(NavDecimal.Create(value));
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, Guid value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		CheckOrSetLength(ref length, 16);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.ToByteArray());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(new NavGuid(value));
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, int value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		CheckOrSetLength(ref length, 4);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, BitConverter.GetBytes(value));
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(NavInteger.Create(value));
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, long value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		CheckOrSetLength(ref length, 8);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, BitConverter.GetBytes(value));
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(NavBigInteger.Create(value));
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, char value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		CheckOrSetLength(ref length, 1);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			if (writer.TextEncoding == TextEncoding.MSDos || writer.TextEncoding == TextEncoding.Windows)
			{
				return WriteBytes(writer, dataError, new byte[1] { NCLManagedAdapter.TextCharToByte(value, writer.TextEncoding) });
			}
			return WriteBytes(writer, dataError, BitConverter.GetBytes(value));
		case NavStreamMode.FileText:
			return WriteLine(writer, dataError, value.ToString());
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavCode value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		byte[] bytes = value.GetBytes(writer.TextEncoding);
		if (length < 0 || length > bytes.Length - 1)
		{
			length = bytes.Length - 1;
		}
		switch (writer.Mode)
		{
		case NavStreamMode.Stream:
		{
			byte[] array2 = new byte[bytes.Length - 1];
			Array.Copy(bytes, 1, array2, 0, array2.Length);
			return WriteBytes(writer, dataError, array2);
		}
		case NavStreamMode.FileBinary:
		{
			int num2 = ((writer.TextEncoding == TextEncoding.MSDos || writer.TextEncoding == TextEncoding.Windows) ? (value.MaxLength + 2 - bytes.Length) : 0);
			byte[] bytes2 = new byte[num2];
			return WriteBytes(writer, dataError, bytes) + WriteBytes(writer, dataError, bytes2);
		}
		case NavStreamMode.FileText:
		{
			int num = ((writer.TextEncoding == TextEncoding.UTF16) ? 3 : 2);
			byte[] array = new byte[bytes.Length - num];
			Array.Copy(bytes, 1, array, 0, array.Length);
			return WriteBytes(writer, dataError, array) + WriteBytes(writer, dataError, GetCRLFBytes(writer.TextEncoding));
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavTextConstant value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return ALWriteString(writer, dataError, value.Value, length, value.Value.Length);
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavDate value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CheckOrSetLength(ref length, 4);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.GetBytes());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavDateFormula value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CheckOrSetLength(ref length, 32);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.GetBytes());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavDateTime value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CheckOrSetLength(ref length, 8);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.GetBytes());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavDuration value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CheckOrSetLength(ref length, 8);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.GetBytes());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavOption value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CheckOrSetLength(ref length, 4);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.GetBytes());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavTime value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CheckOrSetLength(ref length, 4);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.GetBytes());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavRecord value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (writer.Mode)
		{
		case NavStreamMode.FileText:
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (NCLMetaField field in value.MetaTable.Fields)
			{
				if (field.FieldIndex != 0)
				{
					if (field.FieldNclType != NavNclType.NavBlob)
					{
						stringBuilder.Append(NavFormatEvaluateHelper.Format(value.GetFieldValue(field)));
					}
					else
					{
						stringBuilder.Append(NavFormatEvaluateHelper.Format(new NavOemText(1, "*")));
					}
					stringBuilder.Append(NCLManagedAdapter.ByteToText(new byte[1] { 9 }));
				}
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			return WriteLine(writer, dataError, stringBuilder.ToString());
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavRecordId value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (writer.Mode)
		{
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavTableFilter value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CheckOrSetLength(ref length, 504);
		switch (writer.Mode)
		{
		case NavStreamMode.FileBinary:
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, value.GetBytes());
		case NavStreamMode.FileText:
		{
			string value2 = NavFormatEvaluateHelper.Format(value);
			return WriteLine(writer, dataError, value2);
		}
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, NavText value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.MaxLength > 2048 && writer.Mode == NavStreamMode.FileBinary)
		{
			throw NavNCLStreamWideTextMaxLengthException.Create(CultureInfo.CurrentCulture);
		}
		return ALWriteString(writer, dataError, value.ToString(), length, value.MaxLength);
	}

	public static int ALWrite(INavStreamWriter writer, DataError dataError, string value, int length = int.MinValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string value2 = ((length > 0 && length < value.Length) ? value.Substring(0, length) : value);
		return ALWriteString(writer, dataError, value2);
	}

	public static int ALReadText(INavStreamReader reader, DataError dataError, ByRef<NavText> passByRef, int length = int.MinValue)
	{
		NavText value = passByRef.Value;
		int result = ALReadText(reader, dataError, ref value, length);
		passByRef.Value = value;
		return result;
	}

	public static int ALReadText(INavStreamReader reader, DataError dataError, ref NavText value, int length)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (reader.Mode != NavStreamMode.Stream)
		{
			throw NavNCLInvalidStreamModeException.CreateInvalidReadMode(CultureInfo.InvariantCulture, reader.Mode.ToString());
		}
		if (length == int.MinValue || length > value.MaxLength)
		{
			length = value.MaxLength;
		}
		else if (length < 1)
		{
			throw NavNCLInvalidStreamLengthException.CreateInRange(CultureInfo.InvariantCulture, length, 1, value.MaxLength);
		}
		if (reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows)
		{
			byte[] array = ReadBytes(reader, dataError, length, ReadOptions.StopOnZeroOrLFIgnoreCR);
			value = new NavText(value.MaxLength, NCLManagedAdapter.ByteToText(array, reader.TextEncoding));
			return array.Length;
		}
		string value2 = ReadString(reader, length, ReadOptions.StopOnZeroCharOrLFIgnoreCR, skipAlphaIndicator: false);
		value = new NavText(value.MaxLength, value2);
		if (reader.TextEncoding != TextEncoding.UTF16)
		{
			return Encoding.UTF8.GetByteCount(value);
		}
		return Encoding.Unicode.GetByteCount(value);
	}

	private static byte[] GetStringBytes(string value, TextEncoding encoding, bool addZero)
	{
		int num = ((encoding != TextEncoding.UTF16) ? (Encoding.UTF8.GetByteCount(value) + (addZero ? 1 : 0)) : (Encoding.Unicode.GetByteCount(value) + (addZero ? 2 : 0)));
		byte[] array = new byte[num];
		if (encoding == TextEncoding.UTF16)
		{
			Encoding.Unicode.GetBytes(value, 0, value.Length, array, 0);
		}
		else
		{
			Encoding.UTF8.GetBytes(value, 0, value.Length, array, 0);
		}
		return array;
	}

	public static int ALWriteText(INavStreamWriter writer, DataError dataError)
	{
		return WriteBytes(writer, dataError, GetCRLFBytes(writer.TextEncoding));
	}

	public static int ALWriteText(INavStreamWriter writer, DataError dataError, string value, int length)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (length < 0 || length > value.Length)
		{
			length = value.Length;
		}
		byte[] array;
		int length2;
		if (writer.TextEncoding == TextEncoding.MSDos || writer.TextEncoding == TextEncoding.Windows)
		{
			array = NCLManagedAdapter.TextToByte(value.Substring(0, length), writer.TextEncoding);
			length2 = array.Length - 1;
		}
		else
		{
			array = GetStringBytes(value, writer.TextEncoding, addZero: false);
			length2 = array.Length;
		}
		return WriteBytes(writer, dataError, array, length2);
	}

	public static int ALWriteString(INavStreamWriter writer, DataError dataError, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return ALWriteString(writer, dataError, value, value.Length, value.Length);
	}

	public static int ALWriteString(INavStreamWriter writer, DataError dataError, string value, int length)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return ALWriteString(writer, dataError, value, length, value.Length);
	}

	internal static int ALWriteString(INavStreamWriter writer, DataError dataError, string value, int length, int maxLength)
	{
		if (length < 0 || length > value.Length)
		{
			length = value.Length;
		}
		byte[] array = ((writer.TextEncoding != TextEncoding.MSDos && writer.TextEncoding != TextEncoding.Windows) ? GetStringBytes(value, writer.TextEncoding, addZero: true) : NCLManagedAdapter.TextToByte(value.Substring(0, length), writer.TextEncoding));
		int length2 = array.Length;
		switch (writer.Mode)
		{
		case NavStreamMode.Stream:
			return WriteBytes(writer, dataError, array, length2);
		case NavStreamMode.FileBinary:
		{
			int num = WriteBytes(writer, dataError, array, length2);
			int num2 = ((maxLength == int.MaxValue) ? maxLength : (maxLength + 1));
			if (num < num2)
			{
				int num3 = ((writer.TextEncoding == TextEncoding.MSDos || writer.TextEncoding == TextEncoding.Windows) ? (num2 - num) : 0);
				byte[] bytes = new byte[num3];
				num += WriteBytes(writer, dataError, bytes);
			}
			return num;
		}
		case NavStreamMode.FileText:
			return WriteLine(writer, dataError, value);
		default:
			throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString());
		}
	}

	internal static int ALWriteStringBuffer(INavStreamWriter writer, DataError dataError, string value)
	{
		byte[] array;
		if (writer.TextEncoding == TextEncoding.MSDos || writer.TextEncoding == TextEncoding.Windows)
		{
			array = NCLManagedAdapter.TextToByte(value, writer.TextEncoding);
			Array.Resize(ref array, array.Length - 1);
		}
		else
		{
			array = GetStringBytes(value, writer.TextEncoding, addZero: false);
		}
		int length = array.Length;
		return writer.Mode switch
		{
			NavStreamMode.Stream => WriteBytes(writer, dataError, array, length), 
			NavStreamMode.FileBinary => WriteBytes(writer, dataError, array, length), 
			NavStreamMode.FileText => WriteLine(writer, dataError, value), 
			_ => throw NavNCLInvalidStreamModeException.CreateInvalidWriteMode(CultureInfo.InvariantCulture, writer.Mode.ToString()), 
		};
	}

	public static int ALWriteBytes(INavStreamWriter writer, DataError dataError, byte[] bytes, int length)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (length < 0 || length > bytes.Length)
		{
			length = bytes.Length;
		}
		return WriteBytes(writer, dataError, bytes, length);
	}

	public static byte[] ALReadBytes(INavStreamReader reader, DataError dataError, int length)
	{
		if (length < 0)
		{
			throw new ArgumentException(Lang.ReadBytesInvalidLength, "length");
		}
		return ReadBytes(reader, dataError, length);
	}

	public static int ALReadNavValue(INavStreamReader reader, DataError dataError, INavValueMetadata metadata, out NavValue value, int length)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		bool flag = length == int.MinValue;
		int result;
		switch (metadata.NavType)
		{
		case NavType.BigInteger:
		{
			long value7 = 0L;
			if (flag)
			{
				length = 8;
			}
			result = ALRead(reader, dataError, ref value7, length);
			value = NavBigInteger.Create(value7);
			break;
		}
		case NavType.Boolean:
		{
			bool value10 = false;
			if (flag)
			{
				length = 4;
			}
			result = ALRead(reader, dataError, ref value10, length);
			value = NavBoolean.Create(value10);
			break;
		}
		case NavType.Byte:
		{
			byte value3 = 0;
			if (flag)
			{
				length = 1;
			}
			result = ALRead(reader, dataError, ref value3, length);
			value = NavByte.Create(value3);
			break;
		}
		case NavType.Date:
		{
			NavDate value17 = NavDate.Default;
			if (flag)
			{
				length = 4;
			}
			result = ALRead(reader, dataError, ref value17, length);
			value = value17;
			break;
		}
		case NavType.DateFormula:
		{
			NavDateFormula value13 = NavDateFormula.Default;
			if (flag)
			{
				length = 32;
			}
			result = ALRead(reader, dataError, ref value13, length);
			value = value13;
			break;
		}
		case NavType.DateTime:
		{
			NavDateTime value9 = NavDateTime.Default;
			if (flag)
			{
				length = 8;
			}
			result = ALRead(reader, dataError, ref value9, length);
			value = value9;
			break;
		}
		case NavType.Decimal:
		{
			Decimal18 value16 = 0;
			if (flag)
			{
				length = 12;
			}
			result = ALRead(reader, dataError, ref value16, length);
			value = NavDecimal.Create(value16);
			break;
		}
		case NavType.Duration:
		{
			NavDuration value14 = NavDuration.Default;
			if (flag)
			{
				length = 8;
			}
			result = ALRead(reader, dataError, ref value14, length);
			value = value14;
			break;
		}
		case NavType.GUID:
		{
			Guid value6 = Guid.Empty;
			if (flag)
			{
				length = 16;
			}
			result = ALRead(reader, dataError, ref value6, length);
			value = new NavGuid(value6);
			break;
		}
		case NavType.Integer:
		{
			int value4 = 0;
			if (flag)
			{
				length = 4;
			}
			result = ALRead(reader, dataError, ref value4, length);
			value = NavInteger.Create(value4);
			break;
		}
		case NavType.Option:
		{
			NavOption value15 = NavOption.Create(metadata.NavOptionMetadata, 0);
			if (flag)
			{
				length = 4;
			}
			result = ALRead(reader, dataError, ref value15, length);
			value = value15;
			break;
		}
		case NavType.Time:
		{
			NavTime value12 = NavTime.Default;
			if (flag)
			{
				length = 4;
			}
			result = ALRead(reader, dataError, ref value12, length);
			value = value12;
			break;
		}
		case NavType.RecordID:
		{
			NavRecordId value11 = NavRecordId.Default;
			result = ALRead(reader, dataError, ref value11, 0);
			value = value11;
			break;
		}
		case NavType.TableFilter:
		{
			NavTableFilter value8 = NavTableFilter.Default;
			if (flag)
			{
				length = 504;
			}
			result = ALRead(reader, dataError, ref value8, length);
			value = value8;
			break;
		}
		case NavType.Text:
		{
			NavText value5 = new NavText(metadata.NavDefinedLengthMetadata, string.Empty);
			if (flag)
			{
				length = value5.MaxLength;
			}
			result = ALRead(reader, dataError, ref value5, length);
			value = value5;
			break;
		}
		case NavType.Code:
		{
			NavCode value2 = new NavCode(metadata.NavDefinedLengthMetadata, string.Empty);
			if (flag)
			{
				length = value2.MaxLength;
			}
			result = ALRead(reader, dataError, ref value2, length);
			value = value2;
			break;
		}
		default:
			throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentCulture, metadata.NavType.ToString());
		}
		return result;
	}

	public static int ALWriteNavValue(INavStreamWriter writer, DataError dataError, NavValue value, int length)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((value.NavType == NavType.FieldRef || value.NavType == NavType.Variant) && value.IsInitialized)
		{
			return ALWriteNavValue(writer, dataError, value.InnerValue, length);
		}
		return value.NavType switch
		{
			NavType.BigInteger => ALWrite(writer, dataError, value.ToInt64(), length), 
			NavType.Boolean => ALWrite(writer, dataError, value.ToBoolean(), length), 
			NavType.Byte => ALWrite(writer, dataError, value.ToByte(), length), 
			NavType.Date => ALWrite(writer, dataError, (NavDate)value, length), 
			NavType.DateFormula => ALWrite(writer, dataError, (NavDateFormula)value, length), 
			NavType.DateTime => ALWrite(writer, dataError, (NavDateTime)value, length), 
			NavType.Decimal => ALWrite(writer, dataError, value.ToDecimal(), length), 
			NavType.Duration => ALWrite(writer, dataError, (NavDuration)value, length), 
			NavType.GUID => ALWrite(writer, dataError, value.ToGuid(), length), 
			NavType.Integer => ALWrite(writer, dataError, value.ToInt32(), length), 
			NavType.Option => ALWrite(writer, dataError, (NavOption)value, length), 
			NavType.Time => ALWrite(writer, dataError, (NavTime)value, length), 
			NavType.Record => ALWrite(writer, dataError, (NavRecord)value, length), 
			NavType.RecordID => ALWrite(writer, dataError, (NavRecordId)value, length), 
			NavType.TableFilter => ALWrite(writer, dataError, (NavTableFilter)value, length), 
			NavType.Text => ALWrite(writer, dataError, (NavText)value, length), 
			NavType.Code => ALWrite(writer, dataError, (NavCode)value, length), 
			NavType.TextConst => ALWrite(writer, dataError, (NavTextConstant)value, length), 
			_ => throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentCulture, value.NavType.ToString()), 
		};
	}

	private static void CheckOrSetLength(ref int actual, int expected)
	{
		if (actual == int.MinValue)
		{
			actual = expected;
		}
		else if (actual != expected)
		{
			throw NavNCLInvalidStreamLengthException.Create(CultureInfo.InvariantCulture, actual, expected);
		}
	}

	private static byte[] ReadBytes(INavStreamReader reader, DataError dataError, int length)
	{
		byte[] array = reader.ReadBytes(length);
		if (dataError == DataError.ThrowError && array.Length != length)
		{
			throw new NavNCLInvalidStreamDataException();
		}
		return array;
	}

	private static byte[] ReadBytes(INavStreamReader reader, DataError dataError, int length, ReadOptions readoptions)
	{
		switch (readoptions)
		{
		case ReadOptions.StopOnZeroAndIncludeIfPresent:
		{
			if (length == int.MaxValue)
			{
				length--;
			}
			List<byte> list = new List<byte>(1024);
			int num = 1;
			while (list.Count < length)
			{
				num = reader.ReadByte();
				if (num < 0)
				{
					break;
				}
				list.Add((byte)num);
				if (num == 0)
				{
					break;
				}
			}
			if (num > 0 && reader.PeekByte() == 0)
			{
				list.Add((byte)reader.ReadByte());
			}
			return list.ToArray();
		}
		case ReadOptions.StopOnZeroOrLFIgnoreCR:
		{
			List<byte> list = new List<byte>(1024);
			while (list.Count < length)
			{
				int num = reader.ReadByte();
				if (num > 0)
				{
					switch (num)
					{
					default:
						list.Add((byte)num);
						continue;
					case 13:
						continue;
					case 10:
						break;
					}
				}
				break;
			}
			return list.ToArray();
		}
		default:
			return ReadBytes(reader, dataError, length);
		}
	}

	private static string ReadString(INavStreamReader reader, int length, ReadOptions readoptions, bool skipAlphaIndicator)
	{
		if (length != int.MaxValue && length > ReadLimit)
		{
			length = ReadLimit + 1;
		}
		return readoptions switch
		{
			ReadOptions.StopOnZeroCharAndIncludeIfPresent => ReadStringStopOnZeroCharAndIncludeIfPresent(reader, length, skipAlphaIndicator), 
			ReadOptions.StopOnZeroCharOrLFIgnoreCR => ReadStringStopOnZeroCharOrLFIgnoreCR(reader, length), 
			_ => throw new NavNCLNotSupportedOperationException(), 
		};
	}

	private static string ReadStringStopOnZeroCharOrLFIgnoreCR(INavStreamReader reader, int length)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			while (num < length)
			{
				string text = reader.ReadChar();
				if (!string.IsNullOrEmpty(text) && text[0] != '\0' && text[0] != '\n')
				{
					if (text[0] != '\r')
					{
						stringBuilder.Append(text);
						num++;
					}
					continue;
				}
				break;
			}
		}
		catch (EndOfStreamException)
		{
		}
		if (length != int.MaxValue && num > ReadLimit + 1)
		{
			throw new NavNCLInvalidStreamDataException();
		}
		return stringBuilder.ToString();
	}

	private static string ReadStringStopOnZeroCharAndIncludeIfPresent(INavStreamReader reader, int length, bool skipAlphaIndicator)
	{
		int num = 0;
		string text = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			while (num < length)
			{
				if ((num == 0) & skipAlphaIndicator)
				{
					reader.ReadByte();
					skipAlphaIndicator = false;
					continue;
				}
				text = reader.ReadChar();
				if (!string.IsNullOrEmpty(text) && text[0] != '\0')
				{
					stringBuilder.Append(text);
					num++;
					continue;
				}
				break;
			}
		}
		catch (EndOfStreamException)
		{
		}
		if (length != int.MaxValue && num > ReadLimit)
		{
			throw new NavNCLInvalidStreamDataException();
		}
		if (!string.IsNullOrEmpty(text) && text[0] > '\0')
		{
			byte[] array = reader.PeekBytes(2);
			if (array.Length == 2 && array[0] == 0 && array[1] == 0)
			{
				reader.ReadChar();
			}
		}
		return stringBuilder.ToString();
	}

	private static string ReadLine(INavStreamReader reader, DataError dataError)
	{
		if (reader.TextEncoding == TextEncoding.MSDos || reader.TextEncoding == TextEncoding.Windows)
		{
			byte[] array = ReadBytes(reader, DataError.TrapError, ReadLimit + 1, ReadOptions.StopOnZeroOrLFIgnoreCR);
			if (array.Length > ReadLimit)
			{
				throw new NavNCLInvalidStreamDataException();
			}
			return NCLManagedAdapter.ByteToText(array, reader.TextEncoding);
		}
		string text = ReadString(reader, ReadLimit + 1, ReadOptions.StopOnZeroCharOrLFIgnoreCR, skipAlphaIndicator: false);
		if (text.Length > ReadLimit)
		{
			throw new NavNCLInvalidStreamDataException();
		}
		return text;
	}

	private static int WriteBytes(INavStreamWriter writer, DataError dataError, byte[] bytes)
	{
		return WriteBytes(writer, dataError, bytes, bytes.Length);
	}

	private static int WriteBytes(INavStreamWriter writer, DataError dataError, byte[] bytes, int length)
	{
		try
		{
			writer.WriteBytes(bytes, length);
			return length;
		}
		catch when (dataError == DataError.TrapError)
		{
			return 0;
		}
		catch (Exception ex) when (!(ex is NavNCLNotInitializedException))
		{
			throw new NavNCLStreamIOException(Lang.UnableToWriteDataToStream, ex);
		}
	}

	private static int WriteLine(INavStreamWriter writer, DataError dataError, string value)
	{
		if (writer.TextEncoding == TextEncoding.MSDos || writer.TextEncoding == TextEncoding.Windows)
		{
			byte[] array = NCLManagedAdapter.TextToByte(value, writer.TextEncoding);
			return WriteBytes(writer, dataError, array, array.Length - 1) + WriteBytes(writer, dataError, GetCRLFBytes(writer.TextEncoding));
		}
		byte[] stringBytes = GetStringBytes(value, writer.TextEncoding, addZero: false);
		return WriteBytes(writer, dataError, stringBytes, stringBytes.Length) + WriteBytes(writer, dataError, GetCRLFBytes(writer.TextEncoding));
	}
}
