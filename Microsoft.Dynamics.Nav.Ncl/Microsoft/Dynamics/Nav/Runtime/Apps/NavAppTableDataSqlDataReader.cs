using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppTableDataSqlDataReader : IDataReader, IDisposable, IDataRecord
{
	private readonly NCLMetaTable table;

	private readonly BinaryReader reader;

	private readonly CompanyId? companyId;

	private readonly NCLMetaField[] fields;

	private object[] bufferedValues;

	private bool bufferEntireRowOnRead;

	public int Depth => 0;

	public bool IsClosed => false;

	public int RecordsAffected => 0;

	public int FieldCount => fields.Length + (companyId.HasValue ? 1 : 0);

	public object this[string name] => null;

	public object this[int i] => null;

	public NavAppTableDataSqlDataReader(NCLMetaTable table, CompanyId? companyId, BinaryReader reader)
	{
		this.table = table;
		this.reader = reader;
		this.companyId = companyId;
		fields = table.SqlReadableFields.Where((NCLMetaField field) => !field.FieldIsTimestamp).ToArray();
	}

	public void Close()
	{
		throw new NotImplementedException();
	}

	public DataTable GetSchemaTable()
	{
		throw new NotImplementedException();
	}

	public bool NextResult()
	{
		throw new NotImplementedException();
	}

	public bool Read()
	{
		bool result = reader.ReadBoolean();
		bufferedValues = null;
		return result;
	}

	private void Dispose(bool disposing)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public bool GetBoolean(int i)
	{
		throw new NotImplementedException();
	}

	public byte GetByte(int i)
	{
		return reader.ReadByte();
	}

	public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
	{
		byte[] array = reader.ReadBytes(length);
		Array.Copy(array, buffer, length);
		return array.Length;
	}

	public char GetChar(int i)
	{
		throw new NotImplementedException();
	}

	public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
	{
		throw new NotImplementedException();
	}

	public IDataReader GetData(int i)
	{
		throw new NotImplementedException();
	}

	public string GetDataTypeName(int i)
	{
		throw new NotImplementedException();
	}

	public DateTime GetDateTime(int i)
	{
		return DateTime.FromBinary(reader.ReadInt64());
	}

	public decimal GetDecimal(int i)
	{
		return reader.ReadDecimal();
	}

	public double GetDouble(int i)
	{
		throw new NotImplementedException();
	}

	public Type GetFieldType(int i)
	{
		throw new NotImplementedException();
	}

	public float GetFloat(int i)
	{
		throw new NotImplementedException();
	}

	public Guid GetGuid(int i)
	{
		byte[] b = reader.ReadBytes(16);
		return new Guid(b);
	}

	public short GetInt16(int i)
	{
		return reader.ReadInt16();
	}

	public int GetInt32(int i)
	{
		return reader.ReadInt32();
	}

	public long GetInt64(int i)
	{
		return reader.ReadInt64();
	}

	public string GetName(int i)
	{
		throw new NotImplementedException();
	}

	public int GetOrdinal(string name)
	{
		bufferEntireRowOnRead = true;
		if (name == "$companyId")
		{
			return fields.Length;
		}
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].SqlColumnName == name)
			{
				return i;
			}
		}
		throw NavNCLFieldNotFoundException.Create(CultureInfo.CurrentCulture, name, table.TableCaptionSafe);
	}

	public string GetString(int i)
	{
		return reader.ReadString();
	}

	public object GetValue(int i)
	{
		if (bufferEntireRowOnRead)
		{
			if (bufferedValues == null)
			{
				bufferedValues = new object[FieldCount];
				for (int j = 0; j < FieldCount; j++)
				{
					bufferedValues[j] = InternalGetValue(j);
				}
			}
			return bufferedValues[i];
		}
		return InternalGetValue(i);
	}

	private object InternalGetValue(int i)
	{
		if (fields.Length == i)
		{
			if (!companyId.HasValue)
			{
				throw new InvalidOperationException("Trying to read company id field, but company id is null.");
			}
			return companyId.Value;
		}
		NCLMetaField nCLMetaField = fields[i];
		switch (nCLMetaField.FieldNclType)
		{
		case NavNclType.NavGuid:
			return GetGuid(i);
		case NavNclType.NavBigInteger:
			if (nCLMetaField.SqlDataType == SqlDataType.Timestamp)
			{
				return reader.ReadBytes(8);
			}
			return GetInt64(i);
		case NavNclType.NavDuration:
			return GetInt64(i);
		case NavNclType.NavBlob:
		{
			long num = reader.ReadInt64();
			if (num == 0L)
			{
				return DBNull.Value;
			}
			return reader.ReadBytes((int)num);
		}
		case NavNclType.NavBoolean:
		case NavNclType.NavByte:
			return GetByte(i);
		case NavNclType.NavDecimal:
			return GetDecimal(i);
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			return GetInt32(i);
		case NavNclType.NavRecordId:
			return reader.ReadBytes(448);
		case NavNclType.NavDate:
		case NavNclType.NavTime:
		case NavNclType.NavDateTime:
			return GetDateTime(i);
		case NavNclType.NavTableFilter:
			return reader.ReadBytes(504);
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
			switch (nCLMetaField.SqlDataType)
			{
			case SqlDataType.VarChar:
			case SqlDataType.Default:
				return GetString(i);
			case SqlDataType.BigInt:
				return GetInt64(i);
			case SqlDataType.Integer:
				return GetInt32(i);
			case SqlDataType.Variant:
				return (NavNclType)reader.ReadInt32() switch
				{
					NavNclType.NavInteger => (object)GetInt32(i), 
					NavNclType.NavBigInteger => GetInt64(i), 
					_ => GetString(i), 
				};
			default:
				throw new NotSupportedException();
			}
		case NavNclType.NavOemText:
		case NavNclType.NavText:
		case NavNclType.NavDateFormula:
			return GetString(i);
		default:
			throw new NotSupportedException();
		}
	}

	public int GetValues(object[] values)
	{
		throw new NotImplementedException();
	}

	public bool IsDBNull(int i)
	{
		return false;
	}
}
