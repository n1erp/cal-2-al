using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncRepositoryUpdater
{
	private readonly IAppSyncSnapshotRepository snapshotRepository;

	private readonly IAppSyncTrackingRepository trackingRepository;

	public SyncRepositoryUpdater(IAppSyncSnapshotRepository snapshotRepository, IAppSyncTrackingRepository trackingRepository)
	{
		this.snapshotRepository = snapshotRepository;
		this.trackingRepository = trackingRepository;
	}

	public void UpdateTrackingRepository(AppSyncContext syncContext)
	{
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000018B", DiagnosticParameter.SystemMetadata("Updating tracking repository"), MonitorType.SqlWriteCount, Category.Extensions, createNewActivity: false))
		{
			if (syncContext.AppSyncMode == NavAppSyncMode.Add || syncContext.AppSyncMode == NavAppSyncMode.Development || syncContext.AppSyncMode == NavAppSyncMode.ForceSync)
			{
				bool forceUpdateBaseline = syncContext.AppSyncMode == NavAppSyncMode.ForceSync;
				trackingRepository.InsertOrUpdate(new AppSyncTrackingRecord
				{
					AppId = syncContext.AppForAdd.AppId,
					Version = syncContext.AppForAdd.Version,
					Name = syncContext.AppForAdd.Name,
					Publisher = syncContext.AppForAdd.Publisher,
					BaselineVersion = syncContext.AppForAdd.Version
				}, forceUpdateBaseline);
			}
			else if (syncContext.AppSyncMode == NavAppSyncMode.Clean)
			{
				trackingRepository.DeleteForApp(syncContext.AppId);
			}
		}
	}

	public void UpdateSnapshotRepository(AppSyncContext syncContext, IEnumerable<SyncChangeResult> changeResults)
	{
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000018C", DiagnosticParameter.SystemMetadata("Updating snapshot repository"), MonitorType.SqlWriteCount, Category.Extensions, createNewActivity: false))
		{
			if (syncContext.AppSyncMode == NavAppSyncMode.Add || syncContext.AppSyncMode == NavAppSyncMode.Development || syncContext.AppSyncMode == NavAppSyncMode.ForceSync)
			{
				snapshotRepository.DeleteAllForApp(syncContext.AppId);
				{
					foreach (SyncChangeResult changeResult in changeResults)
					{
						if (changeResult.UpdatedSnapshot != null)
						{
							NavSqlTableAndChange tableAndChange = changeResult.TableAndChange;
							if (tableAndChange == null || tableAndChange.ChangeType != TableChangeType.Excessive)
							{
								snapshotRepository.InsertOrUpdate(new AppSyncSnapshotRecord
								{
									AppId = changeResult.AppId,
									TargetTableId = changeResult.TargetTableId,
									TargetTableName = changeResult.TargetTableName,
									OriginalObjectId = changeResult.OriginalObjectId,
									OriginalObjectName = changeResult.OriginalObjectName,
									IsTableExtension = changeResult.IsTableExtension,
									TableSnapshot = changeResult.UpdatedSnapshot
								});
							}
						}
					}
					return;
				}
			}
			if (syncContext.AppSyncMode == NavAppSyncMode.Clean)
			{
				snapshotRepository.DeleteAllForApp(syncContext.AppId);
			}
		}
	}
}
