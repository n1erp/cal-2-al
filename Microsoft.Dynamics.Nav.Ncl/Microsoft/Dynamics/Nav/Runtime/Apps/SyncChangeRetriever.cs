using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncChangeRetriever
{
	private class TemporaryChangeResult
	{
		public SyncTableBundle Bundle { get; set; }

		public SyncTableChanges TableChanges { get; set; }

		public MetaTable Snapshot { get; set; }

		public NavSqlTableAndChange TableAndChanges { get; set; }

		public List<string> ValidationErrors { get; set; }

		public TemporaryChangeResult()
		{
			ValidationErrors = new List<string>();
		}
	}

	public static SyncChangeResult GetChanges(AppSyncContext syncContext, SyncTableBundle bundle)
	{
		switch (syncContext.AppSyncMode)
		{
		case NavAppSyncMode.Add:
		case NavAppSyncMode.Development:
			return GetChangesForAdd(syncContext, bundle);
		case NavAppSyncMode.Clean:
			return GetChangesForClean(syncContext, bundle);
		case NavAppSyncMode.ForceSync:
			return GetChangesForForce(syncContext, bundle);
		default:
			throw new InvalidOperationException("Unsupported sync mode: " + syncContext.AppSyncMode);
		}
	}

	private static SyncChangeResult GetChangesForAdd(AppSyncContext syncContext, SyncTableBundle bundle)
	{
		TemporaryChangeResult temporaryChangeResult = new TemporaryChangeResult
		{
			Bundle = bundle
		};
		temporaryChangeResult.ValidationErrors = bundle.Validate(SyncValidatorFactory.AllValidators).ToList();
		if (!temporaryChangeResult.ValidationErrors.Any())
		{
			temporaryChangeResult.TableChanges = SyncChangeCalculator.CalculateChanges(syncContext, temporaryChangeResult.Bundle);
			temporaryChangeResult.Snapshot = (MetaTable)temporaryChangeResult.Bundle.Target.Clone();
			temporaryChangeResult.TableAndChanges = SyncTableAndChangeCreator.CreateFromLogicalChanges(syncContext, temporaryChangeResult.Bundle, temporaryChangeResult.TableChanges, temporaryChangeResult.Snapshot);
		}
		return CreateChangeResult(syncContext.AppId, temporaryChangeResult);
	}

	private static SyncChangeResult GetChangesForClean(AppSyncContext syncContext, SyncTableBundle bundle)
	{
		TemporaryChangeResult temporaryChangeResult = new TemporaryChangeResult
		{
			Bundle = bundle
		};
		temporaryChangeResult.TableChanges = SyncChangeCalculator.CalculateChanges(syncContext, temporaryChangeResult.Bundle);
		temporaryChangeResult.TableAndChanges = SyncTableAndChangeCreator.CreateFromLogicalChanges(syncContext, temporaryChangeResult.Bundle, temporaryChangeResult.TableChanges, null);
		return CreateChangeResult(syncContext.AppId, temporaryChangeResult);
	}

	private static SyncChangeResult GetChangesForForce(AppSyncContext syncContext, SyncTableBundle bundle)
	{
		TemporaryChangeResult temporaryChangeResult = new TemporaryChangeResult
		{
			Bundle = bundle
		};
		temporaryChangeResult.TableChanges = SyncChangeCalculator.CalculateChanges(syncContext, temporaryChangeResult.Bundle);
		temporaryChangeResult.Snapshot = (MetaTable)(temporaryChangeResult.Bundle.Target?.Clone());
		temporaryChangeResult.TableAndChanges = SyncTableAndChangeCreator.CreateFromLogicalChanges(syncContext, temporaryChangeResult.Bundle, temporaryChangeResult.TableChanges, temporaryChangeResult.Snapshot);
		return CreateChangeResult(syncContext.AppId, temporaryChangeResult);
	}

	private static SyncChangeResult CreateChangeResult(AppId appId, TemporaryChangeResult tempResult)
	{
		SyncTableBundle bundle = tempResult.Bundle;
		int targetTableId = bundle.TargetTableId;
		string targetTableName = bundle.TargetNCL?.TableName ?? bundle.OriginalSnapshotNCL?.TableName;
		int objectId = bundle.ObjectId;
		string objectName = bundle.ObjectName;
		bool isTableExtension = bundle.ObjectType == ObjectType.TableExtension;
		return new SyncChangeResult(appId, targetTableId, targetTableName, objectId, objectName, isTableExtension, tempResult.TableAndChanges, tempResult.Snapshot, tempResult.ValidationErrors);
	}
}
