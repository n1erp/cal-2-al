using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class V2UserDefinedTimestampFixer
{
	private readonly IAppSyncSnapshotRepository snapshotRepo;

	public V2UserDefinedTimestampFixer(IAppSyncSnapshotRepository snapshotRepo)
	{
		this.snapshotRepo = snapshotRepo;
	}

	public void RemoveUserDefinedTimestampFromSnapshot(IList<AppSyncSnapshotRecord> snapshots)
	{
		foreach (AppSyncSnapshotRecord item in snapshots.Where((AppSyncSnapshotRecord s) => s.TableSnapshot != null))
		{
			if (FixSingleSnapshot(item))
			{
				snapshotRepo.InsertOrUpdate(item);
			}
		}
	}

	private static bool FixSingleSnapshot(AppSyncSnapshotRecord s)
	{
		if (s.IsTableExtension)
		{
			return false;
		}
		NavAppOrigin origin = NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)s.AppId).Value);
		NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, s.TargetTableId, NavAppGroup.BaseGroup);
		nCLMetaTable.AssignFromMetaTable(s.TableSnapshot, databaseConvertedFromNav2013: false);
		return RemoveUserDefinedTimestampFromSnapshot(s, nCLMetaTable, origin);
	}

	private static bool RemoveUserDefinedTimestampFromSnapshot(AppSyncSnapshotRecord snapshotRecord, NCLMetaTable table, NavAppOrigin origin)
	{
		bool result = false;
		foreach (NCLMetaField item in from f in table.Fields
			where f.AppOrigin.Equals(origin)
			where f.FieldIsUserDefinedTimestamp
			select f)
		{
			MetaField fieldsById = snapshotRecord.TableSnapshot.GetFieldsById(item.FieldNo);
			if (fieldsById != null)
			{
				result = true;
				snapshotRecord.TableSnapshot.Fields.Remove(fieldsById);
				LogCommand(FormattableString.Invariant($"Removing user defined snapshot field '{item.FieldName}' from table {table.TableName}"));
			}
		}
		return result;
	}

	private static void LogCommand(string command)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007AA", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, command);
	}
}
