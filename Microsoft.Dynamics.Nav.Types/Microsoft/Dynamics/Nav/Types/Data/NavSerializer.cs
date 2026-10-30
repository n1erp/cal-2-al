using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Types.Data;

internal static class NavSerializer
{
	private enum ObjectValue : byte
	{
		DBNull,
		DBNotChanged,
		NewValue,
		Zero
	}

	internal static object ReadTypeAndItem(BinaryReader binReader)
	{
		NavTypeCode navTypeCode = (NavTypeCode)ReadItem(binReader, NavTypeCode.Byte);
		return ReadItem(binReader, navTypeCode);
	}

	internal static void WriteTypeAndItem(BinaryWriter binWriter, RuntimeTypeHandle typeHandle, object value, bool useCompactSerialization = false)
	{
		NavTypeCode typeCode = GetTypeCode(typeHandle);
		WriteItem(binWriter, NavTypeCode.Byte, typeCode, useCompactSerialization);
		WriteItem(binWriter, typeCode, value, useCompactSerialization);
	}

	internal static NavTypeCode GetTypeCode(RuntimeTypeHandle typeHandle)
	{
		if (typeof(DBNull).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.DBNull;
		}
		if (typeof(bool).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Boolean;
		}
		if (typeof(char).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Char;
		}
		if (typeof(sbyte).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.SByte;
		}
		if (typeof(byte).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Byte;
		}
		if (typeof(short).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Int16;
		}
		if (typeof(ushort).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.UInt16;
		}
		if (typeof(int).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Int32;
		}
		if (typeof(uint).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.UInt32;
		}
		if (typeof(long).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Int64;
		}
		if (typeof(ulong).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.UInt64;
		}
		if (typeof(float).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Single;
		}
		if (typeof(double).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Double;
		}
		if (typeof(decimal).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Decimal;
		}
		if (typeof(DateTime).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.DateTime;
		}
		if (typeof(TimeSpan).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.TimeSpan;
		}
		if (typeof(string).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.String;
		}
		if (typeof(Guid).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Guid;
		}
		if (typeof(byte[]).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.ByteArray;
		}
		if (typeof(NSMediaWrapper).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.Media;
		}
		if (typeof(NSMediaSetWrapper).TypeHandle.Equals(typeHandle))
		{
			return NavTypeCode.MediaSet;
		}
		throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' is not known to the NavSerializer.", Type.GetTypeFromHandle(typeHandle).ToString()), "typeHandle");
	}

	public static Type GetType(NavTypeCode typeCode)
	{
		return typeCode switch
		{
			NavTypeCode.DBNull => typeof(DBNull), 
			NavTypeCode.Boolean => typeof(bool), 
			NavTypeCode.Char => typeof(char), 
			NavTypeCode.SByte => typeof(sbyte), 
			NavTypeCode.Byte => typeof(byte), 
			NavTypeCode.Int16 => typeof(short), 
			NavTypeCode.UInt16 => typeof(ushort), 
			NavTypeCode.Int32 => typeof(int), 
			NavTypeCode.UInt32 => typeof(uint), 
			NavTypeCode.Int64 => typeof(long), 
			NavTypeCode.UInt64 => typeof(ulong), 
			NavTypeCode.Single => typeof(float), 
			NavTypeCode.Double => typeof(double), 
			NavTypeCode.Decimal => typeof(decimal), 
			NavTypeCode.DateTime => typeof(DateTime), 
			NavTypeCode.TimeSpan => typeof(TimeSpan), 
			NavTypeCode.String => typeof(string), 
			NavTypeCode.Guid => typeof(Guid), 
			NavTypeCode.ByteArray => typeof(byte[]), 
			NavTypeCode.Media => typeof(Guid), 
			NavTypeCode.MediaSet => typeof(Guid), 
			_ => throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' is not known to the NavSerializer.", typeCode), "typeCode"), 
		};
	}

	internal static string PrepareValueForOutput(object itemValue, NavTypeCode code)
	{
		return code switch
		{
			NavTypeCode.Boolean => ((bool)itemValue).ToString(CultureInfo.CurrentCulture), 
			NavTypeCode.DateTime => ((DateTime)itemValue).ToString(CultureInfo.CurrentCulture), 
			NavTypeCode.Decimal => ((decimal)itemValue).ToString(CultureInfo.CurrentCulture), 
			NavTypeCode.Media => ((itemValue as NSMediaWrapper) ?? new NSMediaWrapper((NavMediaDescriptionCollection)itemValue)).ToString(), 
			NavTypeCode.MediaSet => ((itemValue as NSMediaSetWrapper) ?? new NSMediaSetWrapper((NavMediaDescriptionCollection)itemValue)).ToString(), 
			_ => itemValue.ToString(), 
		};
	}

	internal static string PrepareValueForXmlOutput(object itemValue, NavTypeCode code)
	{
		return code switch
		{
			NavTypeCode.Boolean => XmlConvert.ToString((bool)itemValue), 
			NavTypeCode.DateTime => XmlConvert.ToString((DateTime)itemValue, XmlDateTimeSerializationMode.Local), 
			NavTypeCode.Decimal => XmlConvert.ToString((decimal)itemValue), 
			NavTypeCode.ByteArray => Convert.ToBase64String((byte[])itemValue), 
			NavTypeCode.Media => ((itemValue as NSMediaWrapper) ?? ((itemValue is byte[] data) ? new NSMediaWrapper(data) : new NSMediaWrapper((NavMediaDescriptionCollection)itemValue))).XmlString(), 
			NavTypeCode.MediaSet => ((itemValue as NSMediaSetWrapper) ?? ((itemValue is byte[] data2) ? new NSMediaSetWrapper(data2) : new NSMediaSetWrapper((NavMediaDescriptionCollection)itemValue))).XmlString(), 
			_ => itemValue.ToString(), 
		};
	}

	internal static void WriteItem(BinaryWriter binWriter, NavTypeCode navTypeCode, object value, bool useCompactSerialization = false)
	{
		if (value == DBNull.Value)
		{
			binWriter.Write((byte)0);
			return;
		}
		if (value == DBNotChanged.Value)
		{
			binWriter.Write((byte)1);
			return;
		}
		if (useCompactSerialization && navTypeCode != NavTypeCode.Media && navTypeCode != NavTypeCode.MediaSet && (value.Equals(0) || value.Equals(0m)))
		{
			binWriter.Write((byte)3);
			return;
		}
		binWriter.Write((byte)2);
		switch (navTypeCode)
		{
		case NavTypeCode.Boolean:
			binWriter.Write((bool)value);
			break;
		case NavTypeCode.Byte:
			binWriter.Write((byte)value);
			break;
		case NavTypeCode.Media:
			WriteMediaItem(binWriter, value);
			break;
		case NavTypeCode.MediaSet:
			WriteMediaItem(binWriter, value);
			break;
		case NavTypeCode.ByteArray:
		{
			byte[] array = (byte[])value;
			binWriter.Write(array.Length);
			binWriter.Write(array);
			break;
		}
		case NavTypeCode.Char:
			binWriter.Write(Convert.ToChar(value, CultureInfo.InvariantCulture));
			break;
		case NavTypeCode.DateTime:
		{
			DateTime dateTime = (DateTime)value;
			binWriter.Write(dateTime.Ticks);
			binWriter.Write((int)dateTime.Kind);
			break;
		}
		case NavTypeCode.Decimal:
			binWriter.Write((decimal)value);
			break;
		case NavTypeCode.Double:
			binWriter.Write((double)value);
			break;
		case NavTypeCode.Guid:
			binWriter.Write(((Guid)value).ToByteArray());
			break;
		case NavTypeCode.Int16:
			binWriter.Write((short)value);
			break;
		case NavTypeCode.Int32:
			binWriter.Write((int)value);
			break;
		case NavTypeCode.Int64:
			binWriter.Write((long)value);
			break;
		case NavTypeCode.SByte:
			binWriter.Write((sbyte)value);
			break;
		case NavTypeCode.Single:
			binWriter.Write((float)value);
			break;
		case NavTypeCode.String:
			binWriter.Write(Convert.ToString(value, CultureInfo.InvariantCulture));
			break;
		case NavTypeCode.TimeSpan:
			binWriter.Write(((TimeSpan)value).Ticks);
			break;
		case NavTypeCode.UInt16:
			binWriter.Write((ushort)value);
			break;
		case NavTypeCode.UInt32:
			binWriter.Write((uint)value);
			break;
		case NavTypeCode.UInt64:
			binWriter.Write((ulong)value);
			break;
		case NavTypeCode.DBNull:
			break;
		}
	}

	private static void WriteMediaItem(BinaryWriter binWriter, object value)
	{
		if (value is byte[] array)
		{
			binWriter.Write(value: false);
			binWriter.Write(array.Length);
			binWriter.Write(array);
		}
		else
		{
			byte[] array2 = ((NSMediaWrapperBase)value).ToArray();
			binWriter.Write(value: true);
			binWriter.Write(array2.Length);
			binWriter.Write(array2);
		}
	}

	internal static object ReadItem(BinaryReader binReader, NavTypeCode navTypeCode)
	{
		switch ((ObjectValue)binReader.ReadByte())
		{
		case ObjectValue.DBNull:
			return DBNull.Value;
		case ObjectValue.DBNotChanged:
			return DBNotChanged.Value;
		case ObjectValue.Zero:
			if (navTypeCode == NavTypeCode.Decimal)
			{
				return 0m;
			}
			return 0;
		default:
			switch (navTypeCode)
			{
			case NavTypeCode.Boolean:
				return binReader.ReadBoolean();
			case NavTypeCode.Byte:
				return binReader.ReadByte();
			case NavTypeCode.Media:
			{
				bool flag = binReader.ReadBoolean();
				int count = binReader.ReadInt32();
				byte[] array = binReader.ReadBytes(count);
				if (!flag)
				{
					return array;
				}
				return new NSMediaWrapper(array);
			}
			case NavTypeCode.MediaSet:
			{
				bool flag2 = binReader.ReadBoolean();
				int count = binReader.ReadInt32();
				byte[] array = binReader.ReadBytes(count);
				if (!flag2)
				{
					return array;
				}
				return new NSMediaSetWrapper(array);
			}
			case NavTypeCode.ByteArray:
			{
				int count2 = binReader.ReadInt32();
				return binReader.ReadBytes(count2);
			}
			case NavTypeCode.Char:
				return ((IConvertible)binReader.ReadChar()).ToChar((IFormatProvider)CultureInfo.InvariantCulture);
			case NavTypeCode.DateTime:
			{
				long ticks = binReader.ReadInt64();
				DateTimeKind dateTimeKind = (DateTimeKind)binReader.ReadInt32();
				return new DateTime(ticks, (dateTimeKind == DateTimeKind.Unspecified) ? DateTimeKind.Local : dateTimeKind);
			}
			case NavTypeCode.DBNull:
				return DBNull.Value;
			case NavTypeCode.Decimal:
				return binReader.ReadDecimal();
			case NavTypeCode.Double:
				return binReader.ReadDouble();
			case NavTypeCode.Guid:
				return new Guid(binReader.ReadBytes(16));
			case NavTypeCode.Int16:
				return binReader.ReadInt16();
			case NavTypeCode.Int32:
				return binReader.ReadInt32();
			case NavTypeCode.Int64:
				return binReader.ReadInt64();
			case NavTypeCode.SByte:
				return binReader.ReadSByte();
			case NavTypeCode.Single:
				return binReader.ReadSingle();
			case NavTypeCode.String:
				return binReader.ReadString();
			case NavTypeCode.TimeSpan:
				return new TimeSpan(binReader.ReadInt64());
			case NavTypeCode.UInt16:
				return binReader.ReadUInt16();
			case NavTypeCode.UInt32:
				return binReader.ReadUInt32();
			case NavTypeCode.UInt64:
				return binReader.ReadUInt64();
			default:
				return null;
			}
		}
	}
}
