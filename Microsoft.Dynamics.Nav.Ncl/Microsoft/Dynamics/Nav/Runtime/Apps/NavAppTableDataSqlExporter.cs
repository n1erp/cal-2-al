using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppTableDataSqlExporter
{
	public static int Export(BinaryWriter writer, NavDatabase tenantDatabase, NavSqlConnectionScope conScope, NCLMetaTable table)
	{
		int num = 0;
		int companyIndex = 0;
		StringBuilder stringBuilder = new StringBuilder("SELECT ");
		NavSqlStatementHelper.AppendFieldList(stringBuilder, table.SqlReadableFields, null);
		stringBuilder.Append(" FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, table, tenantDatabase, companyIndex, NavAppTableContext.Main);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		using (navSqlCommand.ExecuteReader(stringBuilder.ToString()))
		{
			while (navSqlCommand.ReadNextRow())
			{
				num++;
				writer.Write(value: true);
				foreach (NCLMetaField item in table.SqlReadableFields.Where((NCLMetaField field) => !field.FieldIsTimestamp))
				{
					WriteField(writer, navSqlCommand.Reader, item);
				}
			}
			writer.Write(value: false);
			return num;
		}
	}

	private static void WriteField(BinaryWriter writer, SqlDataReader reader, NCLMetaField field)
	{
		switch (field.FieldNclType)
		{
		case NavNclType.NavGuid:
		{
			byte[] buffer2 = reader.GetGuid(field.SqlColumnIndex).ToByteArray();
			writer.Write(buffer2);
			break;
		}
		case NavNclType.NavBigInteger:
			if (field.SqlDataType == SqlDataType.Timestamp)
			{
				byte[] buffer = new byte[8];
				reader.GetBytes(field.SqlColumnIndex, 0L, buffer, 0, 8);
				writer.Write(buffer);
			}
			else
			{
				writer.Write(reader.GetInt64(field.SqlColumnIndex));
			}
			break;
		case NavNclType.NavDuration:
			writer.Write(reader.GetInt64(field.SqlColumnIndex));
			break;
		case NavNclType.NavBlob:
		{
			if (reader.IsDBNull(field.SqlColumnIndex))
			{
				writer.Write(0L);
				break;
			}
			BlobReaderStream blobReaderStream = new BlobReaderStream(reader, field.SqlColumnIndex);
			writer.Write(blobReaderStream.Length);
			blobReaderStream.CopyTo(writer.BaseStream);
			break;
		}
		case NavNclType.NavBoolean:
		case NavNclType.NavByte:
			writer.Write(reader.GetByte(field.SqlColumnIndex));
			break;
		case NavNclType.NavDecimal:
			writer.Write(NavSqlDecimalHelper.SqlDecimalToDecimal(reader.GetSqlDecimal(field.SqlColumnIndex)));
			break;
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			writer.Write(reader.GetInt32(field.SqlColumnIndex));
			break;
		case NavNclType.NavRecordId:
		{
			byte[] array2 = new byte[448];
			reader.GetBytes(field.SqlColumnIndex, 0L, array2, 0, array2.Length);
			writer.Write(array2);
			break;
		}
		case NavNclType.NavDate:
		case NavNclType.NavTime:
			writer.Write(reader.GetDateTime(field.SqlColumnIndex).ToBinary());
			break;
		case NavNclType.NavDateTime:
		{
			DateTime dateTime = new DateTime(reader.GetDateTime(field.SqlColumnIndex).Ticks, DateTimeKind.Utc);
			writer.Write(dateTime.ToBinary());
			break;
		}
		case NavNclType.NavTableFilter:
		{
			byte[] array = new byte[504];
			reader.GetBytes(field.SqlColumnIndex, 0L, array, 0, array.Length);
			writer.Write(array);
			break;
		}
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
			switch (field.SqlDataType)
			{
			case SqlDataType.VarChar:
			case SqlDataType.Default:
				writer.Write(reader.GetString(field.SqlColumnIndex));
				break;
			case SqlDataType.BigInt:
				writer.Write(reader.GetInt64(field.SqlColumnIndex));
				break;
			case SqlDataType.Integer:
				writer.Write(reader.GetInt32(field.SqlColumnIndex));
				break;
			case SqlDataType.Variant:
			{
				object value = reader.GetValue(field.SqlColumnIndex);
				if (value is int)
				{
					writer.Write(3);
					writer.Write((int)value);
				}
				else if (value is long)
				{
					writer.Write(4);
					writer.Write((long)value);
				}
				else
				{
					writer.Write(12);
					writer.Write((string)value);
				}
				break;
			}
			default:
				throw new NotSupportedException();
			}
			break;
		case NavNclType.NavOemText:
		case NavNclType.NavText:
		case NavNclType.NavDateFormula:
			writer.Write(reader.GetString(field.SqlColumnIndex));
			break;
		default:
			throw new NotSupportedException();
		}
	}
}
