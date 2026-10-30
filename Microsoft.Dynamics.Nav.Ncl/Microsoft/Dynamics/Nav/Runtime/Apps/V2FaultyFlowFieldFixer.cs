using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class V2FaultyFlowFieldFixer
{
	private readonly NavDatabase tenantDatabase;

	private readonly NavSqlConnectionScope conScope;

	private readonly IAppSyncSnapshotRepository snapshotRepo;

	public V2FaultyFlowFieldFixer(NavDatabase tenantDatabase, NavSqlConnectionScope conScope, IAppSyncSnapshotRepository snapshotRepo)
	{
		this.tenantDatabase = tenantDatabase;
		this.conScope = conScope;
		this.snapshotRepo = snapshotRepo;
	}

	public void RemoveFlowFieldsFromSchemaAndSnapshots(IList<AppSyncSnapshotRecord> snapshots)
	{
		if (!snapshots.Any())
		{
			return;
		}
		int[] companyTokens = (tenantDatabase.TenantDatabaseProperties.PerCompanySchema ? tenantDatabase.GetCompanyTokens() : new int[1] { -2147483648 });
		foreach (AppSyncSnapshotRecord snapshot in snapshots)
		{
			if (snapshot.TableSnapshot != null && FixSingleSnapshot(snapshot, companyTokens))
			{
				snapshotRepo.InsertOrUpdate(snapshot);
			}
		}
	}

	private bool FixSingleSnapshot(AppSyncSnapshotRecord s, int[] companyTokens)
	{
		NavAppOrigin navAppOrigin = NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)s.AppId).Value);
		NavAppTableContext context = (s.IsTableExtension ? NavAppTableContext.Companion(navAppOrigin) : NavAppTableContext.Main);
		NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, s.TargetTableId, NavAppGroup.BaseGroup);
		nCLMetaTable.AssignFromMetaTable(s.TableSnapshot, databaseConvertedFromNav2013: false);
		bool flag = false;
		flag |= DropAllSiftIndexesReferencingFlowFields(s, nCLMetaTable, navAppOrigin, context, companyTokens);
		return flag | DropAllColumnsBasedOnFlowFields(s, nCLMetaTable, navAppOrigin, context, companyTokens);
	}

	private bool DropAllSiftIndexesReferencingFlowFields(AppSyncSnapshotRecord s, NCLMetaTable table, NavAppOrigin origin, NavAppTableContext context, int[] companyTokens)
	{
		bool result = false;
		foreach (NCLMetaKey siftToDrop in from k in table.Keys
			where k.AppOrigin.Equals(origin)
			where k.MaintainSumIndex
			where k.SumFieldsList != null && k.SumFieldsList.Any((INavFieldMetadata f) => f.FieldClass != FieldClass.Normal)
			select k)
		{
			try
			{
				MetaKey metaKey = s.TableSnapshot.Keys.FirstOrDefault((MetaKey k) => k.KeyName == siftToDrop.KeyName);
				if (metaKey == null)
				{
					continue;
				}
				result = true;
				s.TableSnapshot.Keys.Remove(metaKey);
				int[] array = (table.DataPerCompany ? companyTokens : new int[1]);
				int[] array2 = array;
				foreach (int companyToken in array2)
				{
					string text = CreateSafeDropSiftCommand(siftToDrop, companyToken, context);
					LogCommand(text);
					using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
					navSqlCommand.SetCommandText(text);
					navSqlCommand.ExecuteNonQuery();
				}
			}
			catch (NavSqlException ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001WZ", Category.Extensions, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, "Faulty flow field fixer exception (SIFT DROP) -- {0}", ex.ToString());
				throw;
			}
		}
		return result;
	}

	private bool DropAllColumnsBasedOnFlowFields(AppSyncSnapshotRecord s, NCLMetaTable table, NavAppOrigin origin, NavAppTableContext context, int[] companyTokens)
	{
		bool result = false;
		foreach (NCLMetaField item in from f in table.Fields
			where f.AppOrigin.Equals(origin)
			where f.FieldClass != FieldClass.Normal
			select f)
		{
			try
			{
				MetaField fieldsById = s.TableSnapshot.GetFieldsById(item.FieldNo);
				if (fieldsById == null)
				{
					continue;
				}
				result = true;
				s.TableSnapshot.Fields.Remove(fieldsById);
				int[] array = (table.DataPerCompany ? companyTokens : new int[1]);
				int[] array2 = array;
				foreach (int companyToken in array2)
				{
					string text = CreateSafeDropColumnCommand(table, item, companyToken, context);
					LogCommand(text);
					using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
					navSqlCommand.SetCommandText(text);
					navSqlCommand.ExecuteNonQuery();
				}
			}
			catch (NavSqlException ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001WW", Category.Extensions, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, "Faulty flow field fixer exception (COLUMN DROP) -- {0}", ex.ToString());
				throw;
			}
		}
		return result;
	}

	private string CreateSafeDropColumnCommand(NCLMetaTable table, NCLMetaField field, int companyToken, NavAppTableContext context)
	{
		string text = BuildTableName(table, companyToken, context);
		string text2 = NavSqlStatementHelper.BuildAndFormatConstraintName(table, field, tenantDatabase, companyToken, context);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001WV", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Creating safe drop command (COLUMN) -- Table:{0}; Field:{1}; CompanyToken:{2}", text, field.SqlColumnName, companyToken);
		return string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}\"'), N'IsTable'))=1 BEGIN IF(SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{3}\"'), N'IsDefaultCnst')) = 1 BEGIN ALTER TABLE {0}.\"{1}\" DROP CONSTRAINT \"{3}\" END IF EXISTS(SELECT TOP 1 * FROM sys.columns c WHERE c.name = '{2}' AND c.object_id = Object_ID('{1}')) BEGIN ALTER TABLE {0}.\"{1}\" DROP COLUMN \"{2}\" END END", "dbo", text, field.SqlColumnName, text2);
	}

	private string CreateSafeDropSiftCommand(NCLMetaKey sift, int companyToken, NavAppTableContext context)
	{
		string text = BuildSiftName(sift, companyToken, context);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001X0", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Creating safe drop command (SIFT) -- View:{0}; CompanyToken:{1}", text, companyToken);
		return string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}\"'), N'IsView'))=1 BEGIN DROP VIEW {0}.\"{1}\" END ", "dbo", text);
	}

	private string BuildTableName(NCLMetaTable table, int companyToken, NavAppTableContext context)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendTableNameCompanyQualified(stringBuilder, table, tenantDatabase, companyToken, context);
		return stringBuilder.ToString();
	}

	private string BuildSiftName(NCLMetaKey sift, int companyToken, NavAppTableContext context)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendSiftIndexName(stringBuilder, sift, tenantDatabase, companyToken, context);
		return stringBuilder.ToString();
	}

	private static void LogCommand(string command)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001WU", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, command);
	}
}
