using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavRecordId : NavValue
{
	private static readonly NavRecordId defaultValue = new NavRecordId(0, new NavValue[0]);

	private int tableNo;

	private NavValue[] primaryKeyValues;

	private byte[] cachedBytes;

	private bool hashCodeCalculated;

	private int hashCode;

	private object sqlValue;

	public const int MaxByteSize = 448;

	public override NavNclType NclType => NavNclType.NavRecordId;

	public override int GetBytesSize => Value.Length;

	public override bool IsZeroOrEmpty
	{
		get
		{
			if (tableNo == 0)
			{
				return primaryKeyValues.Length == 0;
			}
			return false;
		}
	}

	public override object ValueAsObject => Value;

	public override object ClientObject => Value;

	public static NavRecordId Default => defaultValue;

	public byte[] Value => GetBytes();

	public int TableNo => tableNo;

	public int FieldCount => primaryKeyValues.Length;

	internal ReadOnlyCollection<NavValue> Fields => new ReadOnlyCollection<NavValue>(primaryKeyValues);

	private NavRecordId()
	{
	}

	public NavRecordId(byte[] initValue)
	{
		if (initValue == null)
		{
			throw new ArgumentNullException("initValue");
		}
		CreateNavValuesFromBytes(initValue, 0, -1);
	}

	public NavRecordId(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length <= 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.RecordID);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.RecordID);
		}
		CreateNavValuesFromBytes(data, startIndex, length);
	}

	internal NavRecordId(int tableId, NavValue[] primaryKeyValues)
	{
		if (primaryKeyValues == null)
		{
			throw new ArgumentNullException("primaryKeyValues");
		}
		tableNo = tableId;
		this.primaryKeyValues = primaryKeyValues;
	}

	public static NavRecordId CreateFromNavRecord(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		return record.ALRecordId;
	}

	public static NavRecordId CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavRecordId(data, startIndex, length);
	}

	public static NavRecordId CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavRecord navRecord)
		{
			return navRecord.ALRecordId;
		}
		if (value is NavValue navValue)
		{
			return new NavRecordId(navValue.GetBytes());
		}
		if (value is byte[] initValue)
		{
			return new NavRecordId(initValue);
		}
		if (value is string source)
		{
			return Parse(source);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavRecordId), value);
	}

	public static NavRecordId CreateFromValues(int tableId, NavValue[] primaryKeyValues)
	{
		return new NavRecordId(tableId, primaryKeyValues);
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

	public override bool Equals(object obj)
	{
		NavValue navValue = obj as NavRecordId;
		if (navValue != null)
		{
			return Equals(navValue);
		}
		return false;
	}

	public override bool Equals(NavValue other)
	{
		if (other == this)
		{
			return true;
		}
		NavRecordId other2 = other as NavRecordId;
		return Equals(other2);
	}

	public bool Equals(NavRecordId other)
	{
		if ((object)other == this)
		{
			return true;
		}
		if ((object)other == null || tableNo != other.tableNo)
		{
			return false;
		}
		if (primaryKeyValues == other.primaryKeyValues)
		{
			return true;
		}
		if (GetHashCode() != other.GetHashCode())
		{
			return false;
		}
		if (!PrimaryKeyArrayEquals(other.primaryKeyValues, out var allowPrimaryKeyArrayReuse))
		{
			return false;
		}
		if (allowPrimaryKeyArrayReuse)
		{
			ReusePrimaryKeyArray(other);
		}
		return true;
	}

	private bool PrimaryKeyArrayEquals(NavValue[] otherPrimaryKeyValues, out bool allowPrimaryKeyArrayReuse)
	{
		allowPrimaryKeyArrayReuse = true;
		if (primaryKeyValues.Length != otherPrimaryKeyValues.Length)
		{
			return false;
		}
		for (int i = 0; i < primaryKeyValues.Length; i++)
		{
			NavValue navValue = primaryKeyValues[i];
			NavValue navValue2 = otherPrimaryKeyValues[i];
			bool flag;
			if (navValue is NavStringValue leftValue)
			{
				flag = CollationAwareEquals(leftValue, navValue2);
			}
			else
			{
				flag = navValue.Equals(navValue2);
				if ((flag & allowPrimaryKeyArrayReuse) && navValue is NavOption navOption && navValue2 is NavOption navOption2)
				{
					allowPrimaryKeyArrayReuse = navOption.NavOptionMetadata == navOption2.NavOptionMetadata;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override int CompareTo(NavValue other)
	{
		if (other == this)
		{
			return 0;
		}
		if (other.NclType != NavNclType.NavRecordId)
		{
			return base.CompareTo(other);
		}
		NavRecordId navRecordId = (NavRecordId)other;
		if (primaryKeyValues == navRecordId.primaryKeyValues)
		{
			return 0;
		}
		int num = tableNo - navRecordId.tableNo;
		if (num != 0)
		{
			return num;
		}
		num = PrimaryKeyCompareTo(navRecordId.primaryKeyValues, out var allowPrimaryKeyArrayReuse);
		if (num != 0)
		{
			return num;
		}
		if (allowPrimaryKeyArrayReuse)
		{
			ReusePrimaryKeyArray(navRecordId);
		}
		return 0;
	}

	private int PrimaryKeyCompareTo(NavValue[] otherPrimaryKeyValues, out bool allowPrimaryKeyArrayReuse)
	{
		allowPrimaryKeyArrayReuse = true;
		int num = primaryKeyValues.Length - otherPrimaryKeyValues.Length;
		if (num != 0)
		{
			return num;
		}
		for (int i = 0; i < primaryKeyValues.Length; i++)
		{
			NavValue navValue = primaryKeyValues[i];
			NavValue navValue2 = otherPrimaryKeyValues[i];
			if (navValue is NavStringValue leftValue)
			{
				num = CollationAwareCompare(leftValue, navValue2);
			}
			else
			{
				num = navValue.CompareTo(navValue2);
				if (((num == 0) & allowPrimaryKeyArrayReuse) && navValue is NavOption navOption && navValue2 is NavOption navOption2)
				{
					allowPrimaryKeyArrayReuse = navOption.NavOptionMetadata == navOption2.NavOptionMetadata;
				}
			}
			if (num != 0)
			{
				return num;
			}
		}
		return num;
	}

	private unsafe void ReusePrimaryKeyArray(NavRecordId other)
	{
		bool flag;
		fixed (int* ptr = &tableNo)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &other.tableNo)
			{
				void* ptr4 = ptr3;
				flag = ptr2 < ptr4;
			}
		}
		if (flag)
		{
			other.primaryKeyValues = primaryKeyValues;
		}
		else
		{
			primaryKeyValues = other.primaryKeyValues;
		}
	}

	public static bool operator ==(NavRecordId left, NavRecordId right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(NavRecordId left, NavRecordId right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		if (!hashCodeCalculated)
		{
			int h = tableNo;
			for (int i = 0; i < primaryKeyValues.Length; i++)
			{
				NavValue navValue = primaryKeyValues[i];
				NavStringValue navStringValue = navValue as NavStringValue;
				h = HashCodeHelper.CombineHashCodes(h, (navStringValue != null) ? GetCollationAwareHashCode(navStringValue) : navValue.GetHashCode());
			}
			hashCode = h;
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	public override byte[] GetBytes()
	{
		return cachedBytes ?? (cachedBytes = CreateByteArrayFromPrimaryKeyValues(includeTableNo: true));
	}

	public NavValue FieldValueByIndex(INavValueMetadata field, int index)
	{
		NavValue navValue = primaryKeyValues[index];
		if (field.NclType != navValue.NclType)
		{
			navValue = (primaryKeyValues[index] = NavValue.CreateNavValueFromObject(field, navValue));
		}
		return navValue;
	}

	public NavRecordRef ALGetRecord(ITreeObject parentOfResult, CompilationTarget compilationTarget = CompilationTarget.Internal)
	{
		NavRecordRef navRecordRef = new NavRecordRef(parentOfResult, TableNo, isTempAllocation: true, SecurityFiltering.Validated);
		navRecordRef.InitFromRecordId(this, compilationTarget);
		return navRecordRef;
	}

	public static NavRecordId Parse(string source)
	{
		NavRecordIdEvaluator.Instance.Evaluate(DataError.ThrowError, out var value, Default, source, 0);
		return value;
	}

	public bool StartsWithPrimaryKeyFieldTypes(NavRecordId from)
	{
		if (primaryKeyValues.Length < from.primaryKeyValues.Length)
		{
			return false;
		}
		for (int i = 0; i < from.primaryKeyValues.Length; i++)
		{
			if (primaryKeyValues[i].NavType != from.primaryKeyValues[i].NavType)
			{
				return false;
			}
		}
		return true;
	}

	private int CreateNavValuesFromBytes(byte[] bytes, int startIndex, int length)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (startIndex == 0 && (bytes.Length == 0 || (bytes.Length == 4 && BitConverter.ToInt32(bytes, 0) == 0)))
		{
			primaryKeyValues = new NavValue[0];
			return 0;
		}
		int num = ((length < 0) ? 6 : length);
		if (startIndex < 0 || num < 6 || startIndex > bytes.Length - num)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		tableNo = BitConverter.ToInt32(bytes, startIndex);
		List<NavValue> list = new List<NavValue>(20);
		int num2 = startIndex + 4;
		while (num2 <= bytes.Length - 2)
		{
			ushort num3 = BitConverter.ToUInt16(bytes, num2);
			num2 += 2;
			if (num3 == 0)
			{
				primaryKeyValues = list.ToArray();
				return num2 - startIndex;
			}
			NavType navType = (NavType)num3;
			switch (navType)
			{
			case NavType.BigInteger:
				CheckDataLength(bytes, num2, 8);
				list.Add(NavBigInteger.Create(BitConverter.ToInt64(bytes, num2)));
				num2 += 8;
				break;
			case NavType.Boolean:
				CheckDataLength(bytes, num2, 4);
				list.Add(NavBoolean.Create(bytes[num2] != 0));
				num2 += 4;
				break;
			case NavType.Byte:
				CheckDataLength(bytes, num2, 1);
				list.Add(NavByte.Create(bytes[num2]));
				num2++;
				break;
			case NavType.OemCode:
				for (num = num2 + 1; num < bytes.Length && bytes[num] != 0; num++)
				{
				}
				list.Add(NavOemCode.CreateFromBytes(2048, bytes, num2, num - num2));
				num2 = num + 1;
				break;
			case NavType.Date:
				CheckDataLength(bytes, num2, 4);
				list.Add(NavDate.CreateFromBytes(bytes, num2, 4));
				num2 += 4;
				break;
			case NavType.DateTime:
				CheckDataLength(bytes, num2, 8);
				list.Add(NavDateTime.CreateFromBytes(bytes, num2, 8));
				num2 += 8;
				break;
			case NavType.Decimal:
				CheckDataLength(bytes, num2, 12);
				list.Add(NavDecimal.Create(bytes, num2, 12));
				num2 += 12;
				break;
			case NavType.Duration:
				CheckDataLength(bytes, num2, 8);
				list.Add(NavDuration.CreateFromBytes(bytes, num2, 8));
				num2 += 8;
				break;
			case NavType.GUID:
				CheckDataLength(bytes, num2, 16);
				list.Add(NavGuid.CreateFromBytes(bytes, num2, 16));
				num2 += 16;
				break;
			case NavType.Media:
				CheckDataLength(bytes, num2, 16);
				list.Add(NavMedia.CreateFromBytes(bytes, num2, 16));
				num2 += 16;
				break;
			case NavType.MediaSet:
				CheckDataLength(bytes, num2, 16);
				list.Add(NavMediaSet.CreateFromBytes(bytes, num2, 16));
				num2 += 16;
				break;
			case NavType.Integer:
				CheckDataLength(bytes, num2, 4);
				list.Add(NavInteger.Create(bytes, num2, 4));
				num2 += 4;
				break;
			case NavType.Option:
			{
				int count = list.Count;
				INavValueMetadata navValueMetadata = null;
				if (NavGlobal.NCLMetadata.TryGetMetaTableById(tableNo, out var metaTable, -1, requireCompiled: false) && metaTable.PrimaryKey.KeyFieldCount > count)
				{
					navValueMetadata = metaTable.PrimaryKey.KeyFieldsList[count];
				}
				if (navValueMetadata == null || navValueMetadata.NclType != NavNclType.NavOption)
				{
					navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclType.NavOption);
				}
				CheckDataLength(bytes, num2, 4);
				list.Add(NavValue.CreateNavValueFromBytes(navValueMetadata, bytes, num2, 4));
				num2 += 4;
				break;
			}
			case NavType.RecordID:
			{
				NavRecordId navRecordId = new NavRecordId();
				int num4 = navRecordId.CreateNavValuesFromBytes(bytes, num2, -1);
				list.Add(navRecordId);
				num2 += num4;
				break;
			}
			case NavType.OemText:
				for (num = num2; num < bytes.Length && bytes[num] != 0; num++)
				{
				}
				list.Add(NavOemText.CreateFromBytes(2048, bytes, num2, num - num2));
				num2 = num + 1;
				break;
			case NavType.Time:
				CheckDataLength(bytes, num2, 4);
				list.Add(NavTime.CreateFromBytes(bytes, num2, 4));
				num2 += 4;
				break;
			case NavType.Text:
				for (num = num2; num < bytes.Length - 1 && (bytes[num] != 0 || bytes[num + 1] != 0); num += 2)
				{
				}
				list.Add(NavText.CreateFromBytes(int.MaxValue, bytes, num2, num - num2));
				num2 = num + 2;
				break;
			case NavType.Code:
				for (num = num2 + 1; num < bytes.Length - 1 && (bytes[num] != 0 || bytes[num + 1] != 0); num += 2)
				{
				}
				list.Add(NavCode.CreateFromBytes(2048, bytes, num2, num - num2));
				num2 = num + 2;
				break;
			case NavType.DateFormula:
				CheckDataLength(bytes, num2, 32);
				list.Add(NavDateFormula.CreateFromBytes(bytes, num2, 32));
				num2 += 32;
				break;
			default:
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000024K", Category.AL, Verbosity.Error, "A record's keys, in table: {0}, could not be deserialized into NavRecordId because the field type {1} is not a valid NavType.The primary key value that caused an invalid NavType: {2}, startIndex: {3}, and length: {4}", DiagnosticParameter.SystemMetadata(tableNo), DiagnosticParameter.CustomerContent(navType), DiagnosticParameter.CustomerContent(BitConverter.ToString(bytes)), DiagnosticParameter.SystemMetadata(startIndex), DiagnosticParameter.SystemMetadata(length));
				throw new NavNCLNotSupportedOperationException();
			}
		}
		throw new NavNCLNotSupportedOperationException();
	}

	private static void CheckDataLength(byte[] bytes, int index, int length)
	{
		if (bytes.Length - index < length)
		{
			throw new NavNCLNotSupportedOperationException();
		}
	}

	private byte[] CreateByteArrayFromPrimaryKeyValues(bool includeTableNo)
	{
		if (IsZeroOrEmpty)
		{
			return new byte[6];
		}
		using MemoryStream memoryStream = new MemoryStream(448);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		if (includeTableNo)
		{
			binaryWriter.Write(tableNo);
		}
		for (int i = 0; i < primaryKeyValues.Length; i++)
		{
			NavValue navValue = primaryKeyValues[i];
			binaryWriter.Write((short)navValue.NavType);
			switch (navValue.NclType)
			{
			case NavNclType.NavBigInteger:
			case NavNclType.NavDuration:
			case NavNclType.NavDateTime:
				binaryWriter.Write(navValue.ToInt64());
				break;
			case NavNclType.NavBoolean:
			case NavNclType.NavInteger:
			case NavNclType.NavOption:
			case NavNclType.NavDate:
			case NavNclType.NavTime:
				binaryWriter.Write(navValue.ToInt32());
				break;
			case NavNclType.NavByte:
				binaryWriter.Write(navValue.ToByte());
				break;
			case NavNclType.NavDecimal:
			case NavNclType.NavOemText:
			case NavNclType.NavOemCode:
			case NavNclType.NavText:
			case NavNclType.NavCode:
			case NavNclType.NavMedia:
			case NavNclType.NavMediaSet:
			case NavNclType.NavGuid:
			case NavNclType.NavRecordId:
			case NavNclType.NavDateFormula:
			case NavNclType.NavTableFilter:
				binaryWriter.Write(navValue.GetBytes());
				break;
			default:
				throw new NavNCLInvalidTypeException();
			}
		}
		binaryWriter.Write((ushort)0);
		binaryWriter.Flush();
		return memoryStream.ToArray();
	}

	private static int GetCollationAwareHashCode(NavStringValue value)
	{
		return NavGlobal.AppDatabase.CollationAwareStringComparer.GetHashCode(value);
	}

	private static bool CollationAwareEquals(NavStringValue leftValue, NavValue rightValue)
	{
		NavStringValue navStringValue = rightValue as NavStringValue;
		if (navStringValue == null)
		{
			return false;
		}
		return NavGlobal.AppDatabase.CollationAwareStringComparer.Equals(leftValue.Value, navStringValue.Value);
	}

	private static int CollationAwareCompare(NavStringValue leftValue, NavValue rightValue)
	{
		NavStringValue navStringValue = rightValue as NavStringValue;
		if (navStringValue == null)
		{
			return 1;
		}
		return NavGlobal.AppDatabase.CollationAwareStringComparer.Compare(leftValue.Value, navStringValue.Value);
	}
}
