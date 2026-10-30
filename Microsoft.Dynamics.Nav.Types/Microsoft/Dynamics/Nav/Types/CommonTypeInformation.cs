using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Dynamics.Nav.Types.Media;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Types;

public static class CommonTypeInformation
{
	private static readonly ConcurrentDictionary<string, Type> typeCache = new ConcurrentDictionary<string, Type>();

	public static Type ResolveClrType(NavType type)
	{
		return type switch
		{
			NavType.Code => typeof(string), 
			NavType.String => typeof(string), 
			NavType.Decimal => typeof(decimal), 
			NavType.Option => typeof(int), 
			NavType.Integer => typeof(int), 
			NavType.Boolean => typeof(bool), 
			NavType.RecordID => typeof(byte[]), 
			NavType.OemCode => typeof(string), 
			NavType.Text => typeof(string), 
			NavType.Date => typeof(DateTime), 
			NavType.DateFormula => typeof(string), 
			NavType.DateTime => typeof(DateTime), 
			NavType.Duration => typeof(TimeSpan), 
			NavType.GUID => typeof(Guid), 
			NavType.Byte => typeof(byte), 
			NavType.InStream => typeof(Stream), 
			NavType.BLOB => typeof(byte[]), 
			NavType.OutStream => typeof(Stream), 
			NavType.BigText => typeof(string), 
			NavType.BigInteger => typeof(long), 
			NavType.TableFilter => typeof(string), 
			NavType.OemText => typeof(string), 
			NavType.TextConst => typeof(string), 
			NavType.Time => typeof(DateTime), 
			NavType.Char => typeof(char), 
			NavType.Variant => typeof(object), 
			NavType.ClrByteArray => typeof(byte[]), 
			NavType.DotNet => typeof(object), 
			NavType.Media => typeof(NSMediaWrapper), 
			NavType.MediaSet => typeof(NSMediaSetWrapper), 
			NavType.Notification => typeof(NotificationInfo), 
			NavType.JsonObject => typeof(JObject), 
			NavType.JsonArray => typeof(JArray), 
			NavType.JsonToken => typeof(JToken), 
			NavType.JsonValue => typeof(JValue), 
			_ => throw new ArgumentException("UnsupportedType"), 
		};
	}

	public static Type ResolveClrType(MetaField metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		if (metaField.FieldClass == FieldClass.FlowFilter)
		{
			return typeof(string);
		}
		if (!typeCache.TryGetValue(metaField.ClrType, out var value))
		{
			value = Type.GetType(metaField.ClrType);
			typeCache.TryAdd(metaField.ClrType, value);
		}
		return value;
	}

	public static Type ResolveClrType(MetaQueryColumn metaQueryColumn)
	{
		if (metaQueryColumn == null)
		{
			throw new ArgumentNullException("metaQueryColumn");
		}
		if (metaQueryColumn.ColumnType == NavType.Option)
		{
			return typeof(string);
		}
		return ResolveClrType(metaQueryColumn.ColumnType);
	}

	public static NavType DataTypeToNavType(DataType dataType)
	{
		switch (dataType)
		{
		case DataType.Code:
			return NavType.Code;
		case DataType.Text:
			return NavType.Text;
		case DataType.String:
			return NavType.String;
		case DataType.Boolean:
			return NavType.Boolean;
		case DataType.OemCode:
			return NavType.OemCode;
		case DataType.Char:
			return NavType.Char;
		case DataType.Byte:
			return NavType.Byte;
		case DataType.Integer:
			return NavType.Integer;
		case DataType.Option:
			return NavType.Option;
		case DataType.Date:
			return NavType.Date;
		case DataType.DateFormula:
			return NavType.DateFormula;
		case DataType.DateTime:
			return NavType.DateTime;
		case DataType.Decimal:
			return NavType.Decimal;
		case DataType.Duration:
			return NavType.Duration;
		case DataType.GUID:
			return NavType.GUID;
		case DataType.RecordID:
			return NavType.RecordID;
		case DataType.TableFilter:
			return NavType.TableFilter;
		case DataType.OemText:
			return NavType.OemText;
		case DataType.TextConst:
		case DataType.Label:
			return NavType.TextConst;
		case DataType.Time:
			return NavType.Time;
		case DataType.BigText:
			return NavType.BigText;
		case DataType.BigInteger:
			return NavType.BigInteger;
		case DataType.Binary:
			return NavType.ClrByteArray;
		case DataType.BLOB:
			return NavType.BLOB;
		case DataType.Media:
			return NavType.Media;
		case DataType.MediaSet:
			return NavType.MediaSet;
		default:
			throw new ArgumentException("UnsupportedType");
		}
	}

	public static DataType NavTypeToDataType(NavType navType)
	{
		return navType switch
		{
			NavType.Code => DataType.Code, 
			NavType.Boolean => DataType.Boolean, 
			NavType.Text => DataType.Text, 
			NavType.String => DataType.String, 
			NavType.OemCode => DataType.OemCode, 
			NavType.Char => DataType.Char, 
			NavType.Integer => DataType.Integer, 
			NavType.Byte => DataType.Byte, 
			NavType.Date => DataType.Date, 
			NavType.DateFormula => DataType.DateFormula, 
			NavType.DateTime => DataType.DateTime, 
			NavType.Decimal => DataType.Decimal, 
			NavType.Duration => DataType.Duration, 
			NavType.GUID => DataType.GUID, 
			NavType.Option => DataType.Option, 
			NavType.RecordID => DataType.RecordID, 
			NavType.TableFilter => DataType.TableFilter, 
			NavType.OemText => DataType.OemText, 
			NavType.TextConst => DataType.TextConst, 
			NavType.Time => DataType.Time, 
			NavType.BigText => DataType.BigText, 
			NavType.BigInteger => DataType.BigInteger, 
			NavType.BLOB => DataType.BLOB, 
			NavType.ClrByteArray => DataType.Binary, 
			NavType.Media => DataType.Media, 
			NavType.MediaSet => DataType.MediaSet, 
			_ => throw new ArgumentException("UnsupportedType"), 
		};
	}
}
