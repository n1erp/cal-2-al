using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlStreamReaderWriter
{
	internal static void WriteFieldToStream(SqlDataReader reader, NCLMetaField field, BinaryWriter writer, int curSqlColumnIndex)
	{
		switch (field.FieldNclType)
		{
		case NavNclType.NavMedia:
		case NavNclType.NavMediaSet:
		case NavNclType.NavGuid:
		{
			byte[] buffer2 = reader.GetGuid(curSqlColumnIndex).ToByteArray();
			writer.Write(buffer2);
			break;
		}
		case NavNclType.NavBigInteger:
			if (field.SqlDataType == SqlDataType.Timestamp)
			{
				byte[] buffer = new byte[8];
				reader.GetBytes(curSqlColumnIndex, 0L, buffer, 0, 8);
				writer.Write(buffer);
			}
			else
			{
				writer.Write(reader.GetInt64(curSqlColumnIndex));
			}
			break;
		case NavNclType.NavDuration:
			writer.Write(reader.GetInt64(curSqlColumnIndex));
			break;
		case NavNclType.NavBlob:
		{
			if (reader.IsDBNull(curSqlColumnIndex))
			{
				writer.Write(0L);
				break;
			}
			BlobReaderStream blobReaderStream = new BlobReaderStream(reader, curSqlColumnIndex);
			writer.Write(blobReaderStream.Length);
			blobReaderStream.CopyTo(writer.BaseStream);
			break;
		}
		case NavNclType.NavBoolean:
		case NavNclType.NavByte:
			writer.Write(reader.GetByte(curSqlColumnIndex));
			break;
		case NavNclType.NavDecimal:
			writer.Write(NavSqlDecimalHelper.SqlDecimalToDecimal(reader.GetSqlDecimal(curSqlColumnIndex)));
			break;
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			writer.Write(reader.GetInt32(curSqlColumnIndex));
			break;
		case NavNclType.NavRecordId:
		{
			byte[] array2 = new byte[448];
			short num = (short)reader.GetBytes(curSqlColumnIndex, 0L, array2, 0, array2.Length);
			writer.Write(num);
			writer.Write(array2, 0, num);
			break;
		}
		case NavNclType.NavDate:
		case NavNclType.NavTime:
			writer.Write(reader.GetDateTime(curSqlColumnIndex).ToBinary());
			break;
		case NavNclType.NavDateTime:
		{
			DateTime dateTime = new DateTime(reader.GetDateTime(curSqlColumnIndex).Ticks, DateTimeKind.Utc);
			writer.Write(dateTime.ToBinary());
			break;
		}
		case NavNclType.NavTableFilter:
		{
			byte[] array = new byte[504];
			reader.GetBytes(curSqlColumnIndex, 0L, array, 0, array.Length);
			writer.Write(array);
			break;
		}
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
			switch (field.SqlDataType)
			{
			case SqlDataType.VarChar:
			case SqlDataType.Default:
				writer.Write(reader.GetString(curSqlColumnIndex));
				break;
			case SqlDataType.BigInt:
				writer.Write(reader.GetInt64(curSqlColumnIndex));
				break;
			case SqlDataType.Integer:
				writer.Write(reader.GetInt32(curSqlColumnIndex));
				break;
			case SqlDataType.Variant:
			{
				object value = reader.GetValue(curSqlColumnIndex);
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
			writer.Write(reader.GetString(curSqlColumnIndex));
			break;
		default:
			throw new NotSupportedException();
		}
	}

	public static int WriteTableContentToStream(BinaryWriter writer, NavDatabase database, NCLMetaTable table, NavAppTableContext tableContext, int companyToken, string whereClause = null, IList<SqlParameter> parameters = null)
	{
		int num = 0;
		parameters = parameters ?? new List<SqlParameter>();
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database);
		List<NCLMetaField> list = NormalFieldsWithPrimaryKeyNoTimestamp(table, tableContext);
		using NavSqlCommand navSqlCommand = BuildTableQueryCommand(conScope, companyToken, table, list, whereClause, parameters);
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				num++;
				writer.Write(value: true);
				for (int i = 0; i < list.Count; i++)
				{
					NCLMetaField field = list[i];
					WriteFieldToStream(navSqlCommand.Reader, field, writer, i);
				}
			}
			writer.Write(value: false);
			return num;
		}
	}

	private static NavSqlCommand BuildTableQueryCommand(NavSqlConnectionScope conScope, int companyToken, NCLMetaTable table, List<NCLMetaField> fields, string whereClause, IList<SqlParameter> parameters)
	{
		StringBuilder stringBuilder = new StringBuilder("SELECT ");
		string defaultMainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(table.TableId);
		NavSqlStatementHelper.AppendFieldList(stringBuilder, fields, defaultMainTableAlias);
		NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, table, conScope.Connection.Database, companyToken, string.Empty, table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, defaultMainTableAlias);
		CompanyId? companyId = conScope.Connection.Database.GetCompanyId(table, companyToken);
		if (companyId.HasValue || !string.IsNullOrEmpty(whereClause))
		{
			stringBuilder.Append(" WHERE ");
			if (companyId.HasValue)
			{
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, defaultMainTableAlias);
				int count = parameters.Count;
				stringBuilder.Append('.').Append(NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(count));
				parameters.Add(NavSqlStatementHelper.CreateSqlParameter(NavSqlStatementHelper.ParameterName(count), companyId.Value));
			}
			if (!string.IsNullOrEmpty(whereClause))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0} ({1})", companyId.HasValue ? "AND" : string.Empty, whereClause);
			}
		}
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		navSqlCommand.Command.Parameters.AddRange(parameters.ToArray());
		return navSqlCommand;
	}

	public static void BulkCopyFromStreamIntoTable(NavDatabase database, NCLMetaTable table, NavAppTableContext tableContext, int companyToken, BinaryReader reader, SqlBulkCopy sqlBulkCopy, int exportVersion, Func<NavSqlStreamDataReader, bool> predicate = null)
	{
		sqlBulkCopy.DestinationTableName = NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(new StringBuilder(), table, database, companyToken, tableContext).ToString();
		sqlBulkCopy.EnableStreaming = true;
		sqlBulkCopy.BulkCopyTimeout = 0;
		sqlBulkCopy.ColumnMappings.Clear();
		CompanyId? companyId = database.GetCompanyId(table, companyToken);
		SqlFieldsMappings(sqlBulkCopy.ColumnMappings, from field in table.SqlReadableFieldsForContext(tableContext)
			where !field.FieldIsTimestamp
			select field, companyId);
		SetSqlBulkCopyBatchSize(table, sqlBulkCopy);
		sqlBulkCopy.WriteToServer(new NavSqlStreamDataReader(table, companyId, reader, tableContext, database.IsSharedTenantSchema, exportVersion, predicate));
	}

	internal static void SetSqlBulkCopyBatchSize(NCLMetaTable table, SqlBulkCopy sqlBulkCopy)
	{
		int num = 8060 / table.ApproximateRowByteSize;
		if (num == 0)
		{
			num = 1;
		}
		sqlBulkCopy.BatchSize = num * ServerUserSettings.Instance.SqlBulkImportBatchSize.Value;
	}

	private static List<NCLMetaField> NormalFieldsWithPrimaryKeyNoTimestamp(NCLMetaTable table, NavAppTableContext tableContext)
	{
		List<NCLMetaField> list = new List<NCLMetaField>();
		IEnumerable<NCLMetaField> collection = from field in table.SqlReadableFieldsForContext(tableContext)
			where !field.FieldIsTimestamp
			select field;
		list.AddRange(collection);
		return list;
	}

	private static void SqlFieldsMappings(SqlBulkCopyColumnMappingCollection ColumnMappings, IEnumerable<NCLMetaField> exportFields, CompanyId? companyId)
	{
		foreach (NCLMetaField exportField in exportFields)
		{
			ColumnMappings.Add(exportField.SqlColumnName, exportField.SqlColumnName);
		}
		if (companyId.HasValue)
		{
			ColumnMappings.Add("$companyId", "$companyId");
		}
	}
}
