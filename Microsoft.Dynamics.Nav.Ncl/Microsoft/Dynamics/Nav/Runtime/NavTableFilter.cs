using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavTableFilter : NavValue, IEquatable<NavTableFilter>
{
	private static readonly NavTableFilter defaultValue = new NavTableFilter(new byte[504]);

	private byte[] binaryValue;

	private int tableNo;

	private FilterFieldDictionary fieldFilters;

	private NCLMetaTable metaTable;

	private object sqlValue;

	public const int ByteSize = 504;

	public override NavNclType NclType => NavNclType.NavTableFilter;

	public override int GetBytesSize => 504;

	public override object ValueAsObject => GetBytes();

	public override object ClientObject => ToString();

	public override bool IsZeroOrEmpty
	{
		get
		{
			if (metaTable != null || !fieldFilters.IsNullOrEmpty())
			{
				return false;
			}
			if (binaryValue != null)
			{
				for (int i = 0; i < binaryValue.Length; i++)
				{
					if (binaryValue[i] != 0)
					{
						return false;
					}
				}
			}
			fieldFilters = FilterFieldDictionary.Empty;
			return true;
		}
	}

	public static NavTableFilter Default => defaultValue;

	public byte[] Value => GetBytes();

	public NCLMetaTable MetaTable
	{
		get
		{
			EnsureBinaryParsed();
			return metaTable;
		}
	}

	internal FilterFieldDictionary FieldFilters
	{
		get
		{
			EnsureBinaryParsed();
			if (fieldFilters != null)
			{
				Tuple<INavFieldMetadata, FilterExpression>[] items = fieldFilters.Items;
				foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
				{
					if (tuple.Item1 is UnkownFieldMetadata unkownFieldMetadata)
					{
						throw NavNCLFieldNotFoundException.Create(CultureInfo.CurrentCulture, unkownFieldMetadata.FieldNo, (metaTable == null) ? tableNo.ToString(CultureInfo.InvariantCulture) : metaTable.TableCaptionSafe);
					}
				}
			}
			return fieldFilters;
		}
	}

	internal FilterFieldDictionary FieldFiltersWithUnknownFields
	{
		get
		{
			EnsureBinaryParsed();
			return fieldFilters;
		}
	}

	public int TableNo
	{
		get
		{
			EnsureBinaryParsed();
			return tableNo;
		}
	}

	public NavTableFilter(byte[] initValue)
	{
		if (initValue == null)
		{
			binaryValue = new byte[GetBytesSize];
			return;
		}
		if (initValue.Length != GetBytesSize)
		{
			throw new NavNCLArgumentException(Lang._00095_50018);
		}
		binaryValue = initValue;
	}

	public NavTableFilter(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length <= 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.TableFilter);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.TableFilter);
		}
		binaryValue = new byte[length];
		Buffer.BlockCopy(data, startIndex, binaryValue, 0, length);
	}

	internal NavTableFilter(NCLMetaTable metaTable, FilterFieldDictionary fieldFilters)
	{
		if (metaTable == null)
		{
			throw new ArgumentNullException("metaTable");
		}
		if (fieldFilters == null)
		{
			throw new ArgumentNullException("fieldFilters");
		}
		this.metaTable = metaTable;
		this.fieldFilters = fieldFilters;
	}

	public static NavTableFilter CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavTableFilter(data, startIndex, length);
	}

	public static NavTableFilter CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return new NavTableFilter(navValue.GetBytes());
		}
		if (value is byte[] initValue)
		{
			return new NavTableFilter(initValue);
		}
		if (value is string value2)
		{
			return Parse(value2);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavTableFilter), value);
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		if (sqlValue == null)
		{
			sqlValue = new SqlBinary(Value);
		}
		return sqlValue;
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		return GetSqlWhereClauseValue(sqlDataType);
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.Djb2HashCode(Value);
	}

	public override byte[] GetBytes()
	{
		EnsureBinaryWritten();
		return binaryValue;
	}

	public static bool operator ==(NavTableFilter left, NavTableFilter right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(NavTableFilter left, NavTableFilter right)
	{
		return !(left == right);
	}

	public bool Equals(NavTableFilter other)
	{
		if (other == null)
		{
			return false;
		}
		EnsureBinaryWritten();
		other.EnsureBinaryWritten();
		return ByteArrayHelper.Compare(binaryValue, other.binaryValue);
	}

	public override bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (other.NavType == NavType.TableFilter)
		{
			return Equals((NavTableFilter)other);
		}
		return ThrowUnableToCompareException(other);
	}

	public override bool Equals(object obj)
	{
		if (this != obj)
		{
			return Equals(obj as NavTableFilter);
		}
		return true;
	}

	public override int CompareTo(NavValue other)
	{
		if (other == null)
		{
			return 1;
		}
		if (other == this)
		{
			return 0;
		}
		if (other.NavType == NavType.TableFilter)
		{
			return NavRuntimeHelpers.ByteCompare(binaryValue, ((NavTableFilter)other).Value);
		}
		ThrowUnableToCompareException(other);
		return 0;
	}

	private void EnsureBinaryWritten()
	{
		if (binaryValue == null)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(metaTable.TableId);
			memoryStream.Position += 4L;
			Tuple<INavFieldMetadata, FilterExpression>[] items = fieldFilters.Items;
			foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)tuple.Item1;
				FilterExpression item = tuple.Item2;
				binaryWriter.Write(nCLMetaField.FieldNo);
				binaryWriter.Write((ushort)nCLMetaField.FieldNavType);
				item.WriteToBinary(binaryWriter);
			}
			int value = (int)memoryStream.Position - 8;
			memoryStream.Position = 4L;
			binaryWriter.Write(value);
			if (memoryStream.Length > 504)
			{
				throw new NavFilterException(Lang.FilterExpressionTooLarge);
			}
			byte[] array = new byte[504];
			memoryStream.ToArray().CopyTo(array, 0);
			binaryValue = array;
		}
	}

	private void EnsureBinaryParsed()
	{
		if (fieldFilters != null || IsZeroOrEmpty)
		{
			return;
		}
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(binaryValue));
		tableNo = binaryReader.ReadInt32();
		try
		{
			metaTable = NavGlobal.NCLMetadata.GetMetaTableById(tableNo, -1, requireCompiled: false);
		}
		catch (NavMetadataNotFoundException)
		{
		}
		int num = binaryReader.ReadInt32();
		if (num == 0)
		{
			fieldFilters = FilterFieldDictionary.Empty;
			return;
		}
		List<Tuple<INavFieldMetadata, FilterExpression>> list = new List<Tuple<INavFieldMetadata, FilterExpression>>();
		int num2 = (int)binaryReader.BaseStream.Position + num;
		while (binaryReader.BaseStream.Position < num2)
		{
			int fieldNo = binaryReader.ReadInt32();
			NavType navType = (NavType)binaryReader.ReadUInt16();
			INavFieldMetadata navFieldMetadata = null;
			if (metaTable != null && metaTable.TryGetFieldByNo(fieldNo, out var result))
			{
				navFieldMetadata = result;
			}
			if (navFieldMetadata == null)
			{
				navFieldMetadata = new UnkownFieldMetadata(NavNclTypeHelper.GetNclTypeFromNavType(navType), fieldNo);
			}
			FilterExpression item = FilterExpressionParser.ParseFromBinary(binaryReader, navFieldMetadata.ExpressionContext);
			list.Add(Tuple.Create(navFieldMetadata, item));
		}
		fieldFilters = ((list.Count > 0) ? new FilterFieldDictionary(list) : FilterFieldDictionary.Empty);
	}

	public static NavTableFilter Parse(string value)
	{
		NavTableFilterEvaluator.Instance.Evaluate(DataError.ThrowError, out var value2, Default, value, 0);
		return value2;
	}
}
