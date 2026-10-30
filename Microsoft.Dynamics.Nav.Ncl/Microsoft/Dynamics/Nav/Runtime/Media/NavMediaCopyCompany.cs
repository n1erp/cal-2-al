using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal static class NavMediaCopyCompany
{
	internal struct MediaSetInfo
	{
		internal Guid MediaId;

		internal long SortKey;
	}

	internal struct MediaSetUpdateInfo
	{
		internal Guid UpdateValue;

		internal IList<MediaSetInfo> Children;
	}

	private const string LoadMediaDefinitionsInCompanyStatement = "SELECT [ID] FROM [{0}].[{1}].[Tenant Media] WITH(READUNCOMMITTED) where {2}[Company Name] = @0";

	private const string LoadMediaSetDefinitionsInCompanyStatement = "SELECT [ID],[Media ID],[Media Index] FROM [{0}].[{1}].[Tenant Media Set] WITH(READUNCOMMITTED) where {2}[Company Name] = @0";

	internal static Dictionary<Guid, Guid> LoadMediaDefinitionsInCompany(int companyToken)
	{
		NavDatabase mediaDatabase = NavCurrentThread.Session.Database;
		Dictionary<Guid, Guid> mediaDictionary = new Dictionary<Guid, Guid>();
		if (companyToken == -1)
		{
			return mediaDictionary;
		}
		string companyName = NavCurrentThread.Session.Database.GetCompanyNameFromToken(companyToken);
		string text = "LoadMediaDefinitionsInCompany";
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(mediaDatabase, TransactionType.Browse);
		string companyIdWhere = string.Empty;
		CompanyId? companyId = mediaDatabase.GetCompanyIdForSystemTables(2000000184);
		if (companyId.HasValue)
		{
			text += "ID";
			companyIdWhere = "\"$companyId\" = @1 AND ";
		}
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(text, mediaDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [ID] FROM [{0}].[{1}].[Tenant Media] WITH(READUNCOMMITTED) where {2}[Company Name] = @0", mediaDatabase.DatabaseName, "dbo", companyIdWhere), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 30));
			if (companyId.HasValue)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = companyName;
			if (companyId.HasValue)
			{
				cmd.Command.Parameters[1].Value = companyId.Value;
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			while (cmd.ReadNextRow())
			{
				Guid guid = cmd.Reader.GetGuid(0);
				if (guid != Guid.Empty)
				{
					mediaDictionary.Add(guid, Guid.Empty);
				}
			}
			return cmd;
		});
		return mediaDictionary;
	}

	internal static Dictionary<Guid, MediaSetUpdateInfo> LoadMediaSetDefinitionsInCompany(int companyToken)
	{
		NavDatabase mediaDatabase = NavCurrentThread.Session.Database;
		Dictionary<Guid, MediaSetUpdateInfo> mediaDictionary = new Dictionary<Guid, MediaSetUpdateInfo>();
		if (companyToken == -1)
		{
			return mediaDictionary;
		}
		string companyName = NavCurrentThread.Session.Database.GetCompanyNameFromToken(companyToken);
		string text = "LoadMediaSetDefinitionsInCompany";
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(mediaDatabase, TransactionType.Browse);
		string companyIdWhere = string.Empty;
		CompanyId? companyId = mediaDatabase.GetCompanyIdForSystemTables(2000000183);
		if (companyId.HasValue)
		{
			text += "ID";
			companyIdWhere = "\"$companyId\" = @1 AND ";
		}
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(text, mediaDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [ID],[Media ID],[Media Index] FROM [{0}].[{1}].[Tenant Media Set] WITH(READUNCOMMITTED) where {2}[Company Name] = @0", mediaDatabase.DatabaseName, "dbo", companyIdWhere), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 30));
			if (companyId.HasValue)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = companyName;
			if (companyId.HasValue)
			{
				cmd.Command.Parameters[1].Value = companyId.Value;
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			while (cmd.ReadNextRow())
			{
				Guid guid = cmd.Reader.GetGuid(0);
				Guid guid2 = cmd.Reader.GetGuid(1);
				long @int = cmd.Reader.GetInt64(2);
				if (guid != Guid.Empty)
				{
					MediaSetInfo item = new MediaSetInfo
					{
						MediaId = guid2,
						SortKey = @int
					};
					if (mediaDictionary.TryGetValue(guid, out var value))
					{
						value.Children.Add(item);
					}
					else
					{
						value = new MediaSetUpdateInfo
						{
							Children = new List<MediaSetInfo> { item }
						};
						mediaDictionary.Add(guid, value);
					}
				}
			}
			return cmd;
		});
		return mediaDictionary;
	}

	internal static void UpdateMediaReferencesInTables(NavDatabase database, IList<NCLMetaTable> tablesWithMediaContent, int fromCompanyToken, int toCompanyToken)
	{
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000QF", DiagnosticParameter.SystemMetadata("UpdateMediaReferences"), (MonitorType)720899, Category.Runtime))
		{
			UpdateMediaReferencesInTablesCore(database, tablesWithMediaContent, fromCompanyToken, toCompanyToken);
		}
	}

	private static void UpdateMediaReferencesInTablesCore(NavDatabase database, IList<NCLMetaTable> tablesWithMediaContent, int fromCompanyToken, int toCompanyToken)
	{
		if (tablesWithMediaContent == null || !tablesWithMediaContent.Any())
		{
			return;
		}
		Dictionary<Guid, Guid> dictionary = LoadMediaDefinitionsInCompany(fromCompanyToken);
		Dictionary<Guid, MediaSetUpdateInfo> dictionary2 = LoadMediaSetDefinitionsInCompany(fromCompanyToken);
		if (dictionary == null || (!dictionary.Any() && dictionary2 == null) || !dictionary2.Any())
		{
			return;
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QG", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Update media references for company copy, {0} media items, {1} media sets", dictionary.Count, dictionary2.Count);
		Dictionary<Guid, int> transferredObjects = new Dictionary<Guid, int>();
		foreach (NCLMetaTable item in tablesWithMediaContent.Where((NCLMetaTable table) => table.MediaFieldCount > 0))
		{
			CompanyId? companyId = database.GetCompanyId(item, toCompanyToken);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QH", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Update media references for table '{0}' (id={1})", item.TableName, item.TableId);
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
			IList<Guid> list = FindMediaItemsUsedInApplicationTable(database, toCompanyToken, conScope, item, companyId);
			if (list.Any())
			{
				UpdateMediaItemsInApplicationTable(database, item, conScope, toCompanyToken, list, dictionary, dictionary2, companyId);
				UpdateMediaSystemTables(database, toCompanyToken, dictionary2, transferredObjects, dictionary);
			}
		}
	}

	private static void UpdateMediaItemsInApplicationTable(NavDatabase database, NCLMetaTable table, NavSqlConnectionScope conScope, int toCompanyToken, IList<Guid> guidList, IDictionary<Guid, Guid> mediaRefDictionary, IDictionary<Guid, MediaSetUpdateInfo> mediaSetRefDictionary, CompanyId? companyId)
	{
		List<NavAppOrigin> list = (from f in table.MediaFields
			where f.IsCompanionTableField
			select f.AppOrigin).Distinct().Prepend(null).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NavAppOrigin item in list)
		{
			NavAppTableContext navAppTableContext = ((item == null) ? NavAppTableContext.Main : NavAppTableContext.Companion(item));
			StringBuilder stringBuilder2 = new StringBuilder("UPDATE ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder2, table, database, toCompanyToken, navAppTableContext);
			stringBuilder2.Append(" SET ");
			string text = stringBuilder2.ToString();
			int mediaFieldCount = table.MediaFieldCount;
			for (int num = 0; num < guidList.Count / mediaFieldCount; num++)
			{
				stringBuilder.Append(text);
				int num2 = 0;
				bool flag = false;
				NCLMetaField[] mediaFields = table.MediaFields;
				foreach (NCLMetaField nCLMetaField in mediaFields)
				{
					if (!nCLMetaField.OriginatingTableContext.Equals(navAppTableContext))
					{
						num2++;
						continue;
					}
					flag = true;
					NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, nCLMetaField.SqlColumnName);
					Guid value = Guid.Empty;
					Guid key = guidList[num * mediaFieldCount + num2++];
					MediaSetUpdateInfo value2;
					if (nCLMetaField.FieldNclType == NavNclType.NavMedia)
					{
						if (mediaRefDictionary.TryGetValue(key, out value) && value.Equals(Guid.Empty))
						{
							value = (mediaRefDictionary[key] = Guid.NewGuid());
						}
					}
					else if (mediaSetRefDictionary.TryGetValue(key, out value2))
					{
						if (value2.UpdateValue.Equals(Guid.Empty))
						{
							value2.UpdateValue = Guid.NewGuid();
							mediaSetRefDictionary[key] = value2;
						}
						value = value2.UpdateValue;
					}
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " = '{0}',", value);
				}
				if (!flag)
				{
					stringBuilder.Length -= text.Length;
					continue;
				}
				stringBuilder.Length--;
				stringBuilder.Append(" WHERE ");
				if (companyId.HasValue)
				{
					stringBuilder.Append(NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(0)).Append(" AND ");
				}
				num2 = 0;
				NCLMetaField[] mediaFields2 = table.MediaFields;
				foreach (NCLMetaField nCLMetaField2 in mediaFields2)
				{
					if (!nCLMetaField2.OriginatingTableContext.Equals(navAppTableContext))
					{
						num2++;
						continue;
					}
					NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, nCLMetaField2.SqlColumnName);
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " = '{0}' and ", guidList[num * mediaFieldCount + num2++]);
				}
				stringBuilder.Length -= 4;
				stringBuilder.Append(";");
			}
		}
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		if (companyId.HasValue)
		{
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(navSqlCommand.Command.Parameters, 0), companyId.Value);
		}
		navSqlCommand.ExecuteNonQuery();
	}

	private static void UpdateMediaSystemTables(NavDatabase database, int toCompanyToken, IDictionary<Guid, MediaSetUpdateInfo> mediaSetRefDictionary, IDictionary<Guid, int> transferredObjects, IDictionary<Guid, Guid> mediaRefDictionary)
	{
		foreach (KeyValuePair<Guid, MediaSetUpdateInfo> item in mediaSetRefDictionary)
		{
			if (!(item.Value.UpdateValue != Guid.Empty) || transferredObjects.ContainsKey(item.Key))
			{
				continue;
			}
			foreach (MediaSetInfo child in item.Value.Children)
			{
				if (mediaRefDictionary.TryGetValue(child.MediaId, out var value) && value == Guid.Empty)
				{
					value = Guid.NewGuid();
					mediaRefDictionary[child.MediaId] = value;
				}
			}
		}
		foreach (KeyValuePair<Guid, Guid> item2 in mediaRefDictionary)
		{
			if (item2.Value != Guid.Empty && !transferredObjects.ContainsKey(item2.Key))
			{
				UpdateMediaObjectAfterCompanyCopy(database, item2.Key, item2.Value, toCompanyToken);
				UpdateMediaThumbnailObjectAfterCompanyCopy(database, item2.Key, item2.Value, toCompanyToken);
				transferredObjects.Add(item2.Key, 1);
			}
		}
		foreach (KeyValuePair<Guid, MediaSetUpdateInfo> item3 in mediaSetRefDictionary)
		{
			if (item3.Value.UpdateValue != Guid.Empty && !transferredObjects.ContainsKey(item3.Key))
			{
				UpdateMediaSetIdAfterCompanyCopy(database, mediaRefDictionary, item3.Value, toCompanyToken);
				transferredObjects.Add(item3.Key, 1);
			}
		}
	}

	private static IList<Guid> FindMediaItemsUsedInApplicationTable(NavDatabase database, int toCompanyToken, NavSqlConnectionScope conScope, NCLMetaTable table, CompanyId? companyId)
	{
		List<Guid> list = new List<Guid>();
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		StringBuilder selectStmt = new StringBuilder("SELECT  ");
		string mainAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(table.TableId);
		NavSqlStatementHelper.AppendFieldList(selectStmt, table.MediaFields, NavSqlColumnListType.DataLengthInsteadOfBlobs, mainAlias);
		NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(selectStmt, table, database, toCompanyToken, " WITH(READUNCOMMITTED) ", table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, mainAlias);
		selectStmt.Append(" WHERE ");
		if (companyId.HasValue)
		{
			selectStmt.Append('"').Append(mainAlias).Append("\".");
			selectStmt.Append(NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(0)).Append(" AND ");
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(navSqlCommand.Command.Parameters, 0), companyId.Value);
		}
		table.MediaFields.ForEach((NCLMetaField field) =>
		{
			NavSqlStatementHelper.AppendSqlColumnName(selectStmt, field, mainAlias);
			selectStmt.Append(" != '00000000-0000-0000-0000-000000000000' or ");
		});
		selectStmt.Length -= 3;
		navSqlCommand.SetCommandText(selectStmt.ToString());
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				for (int num = 0; num < table.MediaFieldCount; num++)
				{
					list.Add(navSqlCommand.Reader.GetGuid(num));
				}
			}
			return list;
		}
	}

	private static void UpdateMediaSetIdAfterCompanyCopy(NavDatabase database, IDictionary<Guid, Guid> mediaRefDictionary, MediaSetUpdateInfo mediaSetUpdateInfo, int toCompanyToken)
	{
		CompanyId? companyId = database.GetCompanyIdForSystemTables(2000000184);
		string companyIdWhere = string.Empty;
		if (companyId.HasValue)
		{
			companyIdWhere = "\"$companyId\"";
		}
		string companyName = database.GetCompanyNameFromToken(toCompanyToken);
		string text = "UpdateMediaSetIdAfterCompanyCopy";
		if (companyId.HasValue)
		{
			text += "ID";
		}
		NCLMetaTable metaTable = NavGlobal.NCLMetadata.GetMetaTableById(2000000183);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(text, database), () =>
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("INSERT INTO ");
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(stringBuilder, metaTable, database, toCompanyToken, NavAppTableContext.Main);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "([ID], [Media ID], [Company Name], [Media Index]{0})", companyId.HasValue ? ("," + companyIdWhere) : string.Empty);
			stringBuilder.Append(" VALUES (@0, @1, @2, @3");
			stringBuilder.Append(companyId.HasValue ? ",@4) " : ")");
			return stringBuilder.ToString();
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 30));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.BigInt));
			if (companyId.HasValue)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = mediaSetUpdateInfo.UpdateValue;
			cmd.Command.Parameters[2].Value = companyName;
			if (companyId.HasValue)
			{
				cmd.Command.Parameters[4].Value = companyId.Value;
			}
			foreach (MediaSetInfo child in mediaSetUpdateInfo.Children)
			{
				if (mediaRefDictionary.TryGetValue(child.MediaId, out var value))
				{
					cmd.Command.Parameters[1].Value = value;
				}
				else
				{
					cmd.Command.Parameters[1].Value = child.MediaId;
				}
				cmd.Command.Parameters[3].Value = child.SortKey;
				cmd.ExecuteNonQuery();
			}
			return true;
		});
	}

	private static void UpdateMediaObjectAfterCompanyCopy(NavDatabase database, Guid oldMediaId, Guid newMediaId, int toCompanyToken)
	{
		CompanyId? companyId = database.GetCompanyIdForSystemTables(2000000184);
		string companyIdWhere = string.Empty;
		string text = "UpdateMediaObjectAfterCompanyCopy";
		if (companyId.HasValue)
		{
			companyIdWhere = "\"$companyId\" = @3 AND ";
			text += "ID";
		}
		string companyName = database.GetCompanyNameFromToken(toCompanyToken);
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000184);
		string sqlTableName = NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(new StringBuilder(), metaTableById, database, toCompanyToken, NavAppTableContext.Main).ToString();
		StringBuilder sqlFieldList = new StringBuilder();
		NavSqlStatementHelper.AppendFieldList(sqlFieldList, metaTableById.AllFields.Where((NCLMetaField f) => !f.FieldIsTimestamp), null, companyId.HasValue);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(text, database), () => FormattableString.Invariant($"\r\n                            SELECT * into #temp_table FROM {sqlTableName} WITH (READUNCOMMITTED) where {companyIdWhere}[ID] = @0;\r\n                            UPDATE #temp_table SET ID= @1, [Company Name] = @2 WHERE {companyIdWhere}[ID] = @0;\r\n                            INSERT INTO {sqlTableName} ({sqlFieldList}) SELECT {sqlFieldList} FROM #temp_table;\r\n                            DROP TABLE #temp_table;"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 30));
			if (companyId.HasValue)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = oldMediaId;
			cmd.Command.Parameters[1].Value = newMediaId;
			cmd.Command.Parameters[2].Value = companyName;
			if (companyId.HasValue)
			{
				cmd.Command.Parameters[3].Value = companyId.Value;
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteNonQuery();
			return true;
		});
	}

	private static void UpdateMediaThumbnailObjectAfterCompanyCopy(NavDatabase database, Guid oldMediaId, Guid newMediaId, int toCompanyToken)
	{
		CompanyId? companyId = database.GetCompanyIdForSystemTables(2000000184);
		string companyIdWhere = string.Empty;
		if (companyId.HasValue)
		{
			companyIdWhere = "\"$companyId\" = @3 AND ";
		}
		string companyName = database.GetCompanyNameFromToken(toCompanyToken);
		string key = "UpdateMediaThumbnailObjectAfterCompanyCopy";
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000185);
		string sqlTableName = NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(new StringBuilder(), metaTableById, database, toCompanyToken, NavAppTableContext.Main).ToString();
		StringBuilder sqlFieldList = new StringBuilder();
		NavSqlStatementHelper.AppendFieldList(sqlFieldList, metaTableById.AllFields.Where((NCLMetaField f) => !f.FieldIsTimestamp), null, companyId.HasValue);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(key, database), () => FormattableString.Invariant($"\r\n                        SELECT * into #temp_table FROM {sqlTableName} WITH (READUNCOMMITTED) where {companyIdWhere}[Media ID] = @0;\r\n                        UPDATE #temp_table SET [Media ID]= @1, [Company Name] = @2, [ID] = NEWID() WHERE {companyIdWhere}[Media ID] = @0;\r\n                        INSERT INTO {sqlTableName} ({sqlFieldList}) SELECT {sqlFieldList} FROM #temp_table;\r\n                        DROP TABLE #temp_table;"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.UniqueIdentifier));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 30));
			if (companyId.HasValue)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = oldMediaId;
			cmd.Command.Parameters[1].Value = newMediaId;
			cmd.Command.Parameters[2].Value = companyName;
			if (companyId.HasValue)
			{
				cmd.Command.Parameters[3].Value = companyId.Value;
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteNonQuery();
			return true;
		});
	}
}
