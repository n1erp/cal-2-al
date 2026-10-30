using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.TokenCache;

internal class PersistentNavTokenCache : NavTokenCacheBase
{
	private readonly INavTokenCacheSessionManager sessionManager;

	private readonly DateTimeNowProvider dateTimeNowProvider;

	private readonly NavRecordId cacheNavRecordId;

	internal PersistentNavTokenCache(INavTokenCacheSessionManager sessionManager, DateTimeNowProvider dateTimeNowProvider)
	{
		if (sessionManager == null)
		{
			throw new ArgumentNullException("sessionManager");
		}
		if (dateTimeNowProvider == null)
		{
			throw new ArgumentNullException("dateTimeNowProvider");
		}
		this.sessionManager = sessionManager;
		this.dateTimeNowProvider = dateTimeNowProvider;
		cacheNavRecordId = new NavRecordId(2000000197, new NavValue[1]
		{
			new NavGuid(this.sessionManager.NavUserSecurityId)
		});
	}

	public new void Clear()
	{
		base.Clear();
		sessionManager.RunTenantActionInSystemSession((NavTenant navTenant, NavSession navSession) =>
		{
			using (new MaximizedPermissionScope(navSession, new int[1] { 2000000197 }))
			{
				using NavRecord navRecord = new NavRecord(navSession, 2000000197, SecurityFiltering.Filtered);
				if (navRecord.Get(DataError.TrapError, cacheNavRecordId) && navRecord.Delete(DataError.TrapError, runApplicationTrigger: false, isCalledFromUI: false))
				{
					SendTraceTag("Deleted Token Cache row for SID {0}.", DiagnosticParameter.Eupi(sessionManager.NavUserSecurityId));
				}
			}
		});
	}

	internal override UserTokenIdentifiers GetCurrentIdentifiers()
	{
		UserTokenIdentifiers identifiers = base.GetCurrentIdentifiers();
		if (identifiers == null)
		{
			sessionManager.RunTenantActionInSystemSessionWithoutTransaction((NavTenant navTenant, NavSession navSession) =>
			{
				using (new MaximizedPermissionScope(navSession, new int[1] { 2000000197 }))
				{
					using NavRecord navRecord = new NavRecord(navSession, 2000000197, SecurityFiltering.Filtered);
					if (navRecord.Get(DataError.TrapError, cacheNavRecordId))
					{
						identifiers = new UserTokenIdentifiers
						{
							UserSecurityId = NavTokenCacheBase.ConvertGuidToString(sessionManager.NavUserSecurityId),
							AadUserUniqueId = navRecord.GetFieldValue(6).ToString(),
							AadTenantId = navRecord.GetFieldValue(7).ToString()
						};
					}
				}
			});
		}
		return identifiers;
	}

	internal override void StoreAadTenantId(string aadTenantId)
	{
		sessionManager.RunTenantActionInSystemSession((NavTenant navTenant, NavSession navSession) =>
		{
			using (new MaximizedPermissionScope(navSession, new int[1] { 2000000197 }))
			{
				using NavRecord navRecord = new NavRecord(navSession, 2000000197, SecurityFiltering.Filtered);
				if (navRecord.Get(DataError.TrapError, cacheNavRecordId))
				{
					if (!navRecord.GetFieldValue(7).ToString().Equals(aadTenantId))
					{
						navRecord.SetFieldValue(7, new NavText(aadTenantId));
						if (navRecord.Modify(DataError.TrapError, runApplicationTrigger: false, runGlobalTrigger: false))
						{
							SendTraceTag("Updated Token Cache row for SID {0} with tenant ID {1}.", DiagnosticParameter.Eupi(sessionManager.NavUserSecurityId), DiagnosticParameter.Oii(aadTenantId));
						}
					}
					return;
				}
				throw new NavTokenCacheException(string.Format(CultureInfo.CurrentCulture, Lang.FailedToStoreTenantIdForCacheRow, sessionManager.NavUserSecurityId, aadTenantId));
			}
		});
		base.StoreAadTenantId(aadTenantId);
	}

	internal override void PersistTokenCacheState(string aadTenantId, string aadUserUniqueId, byte[] tokenCacheState)
	{
		CurrentCacheMetadata = new NavTokenCacheMetadata
		{
			UserSecurityId = sessionManager.NavUserSecurityId,
			AadTenantId = aadTenantId,
			AadUserUniqueId = aadUserUniqueId,
			CacheWriteTime = DateTimeHelper.TruncateMilliseconds(dateTimeNowProvider.UtcNow)
		};
		Deserialize(tokenCacheState);
		UpdateRecordFromCacheData(tokenCacheState);
	}

	protected override void BeforeAccessNotification(NavTokenCacheNotificationArgs args)
	{
		sessionManager.RunTenantActionInSystemSessionWithoutTransaction((NavTenant navTenant, NavSession navSession) =>
		{
			using (new MaximizedPermissionScope(navSession, new int[1] { 2000000197 }))
			{
				using NavRecord navRecord = new NavRecord(navSession, 2000000197, SecurityFiltering.Filtered);
				if (navRecord.Get(DataError.TrapError, cacheNavRecordId) && (CurrentCacheMetadata == null || CurrentCacheMetadata.CacheWriteTime < navRecord.GetFieldValue(4).ToDateTime()))
				{
					navRecord.CalcFields(DataError.TrapError, new int[1] { 5 });
					RefreshCacheFromRecord(navRecord);
				}
			}
		});
	}

	protected override void AfterAccessNotification(NavTokenCacheNotificationArgs args)
	{
		if (HasStateChanged)
		{
			CurrentCacheMetadata = new NavTokenCacheMetadata
			{
				UserSecurityId = sessionManager.NavUserSecurityId,
				AadUserUniqueId = (string.IsNullOrEmpty(args.UniqueId) ? args.ClientId : args.UniqueId),
				CacheWriteTime = DateTimeHelper.TruncateMilliseconds(dateTimeNowProvider.UtcNow)
			};
			UpdateRecordFromCacheData(Serialize());
			HasStateChanged = false;
		}
	}

	private void UpdateRecordFromCacheData(byte[] cacheData)
	{
		cacheData = cacheData ?? NavTokenCacheBase.EmptyData;
		sessionManager.RunTenantActionInSystemSession((NavTenant navTenant, NavSession navSession) =>
		{
			try
			{
				using (new MaximizedPermissionScope(navSession, new int[1] { 2000000197 }))
				{
					using NavRecord navRecord = new NavRecord(navSession, 2000000197, SecurityFiltering.Filtered);
					if (navRecord.Get(DataError.TrapError, cacheNavRecordId))
					{
						if (HasStateChanged)
						{
							CurrentCacheMetadata.AadTenantId = navRecord.GetFieldValue(7).ToString();
						}
						else
						{
							navRecord.SetFieldValue(7, new NavText(CurrentCacheMetadata.AadTenantId));
						}
						navRecord.SetFieldValue(6, new NavText(CurrentCacheMetadata.AadUserUniqueId));
						navRecord.SetFieldValue(4, NavDateTime.Create(CurrentCacheMetadata.CacheWriteTime, DateTimeReferenceFrame.Server));
						navRecord.SetFieldValue(5, new NavBLOB(cacheData));
						if (navRecord.Modify(DataError.TrapError, runApplicationTrigger: false, runGlobalTrigger: false))
						{
							SendTraceTag("Updated Token Cache row for SID {0} with AAD user unique ID {1}, write time {2:O}, and cache BLOB of {3} bytes.", DiagnosticParameter.Eupi(sessionManager.NavUserSecurityId), DiagnosticParameter.Eupi(CurrentCacheMetadata.AadUserUniqueId), DiagnosticParameter.SystemMetadata(CurrentCacheMetadata.CacheWriteTime), DiagnosticParameter.SystemMetadata(cacheData.Length));
						}
					}
					else
					{
						if (!HasStateChanged)
						{
							navRecord.SetFieldValue(7, new NavText(CurrentCacheMetadata.AadTenantId));
						}
						navRecord.SetFieldValue(1, new NavGuid(CurrentCacheMetadata.UserSecurityId));
						navRecord.SetFieldValue(6, new NavText(CurrentCacheMetadata.AadUserUniqueId));
						navRecord.SetFieldValue(4, NavDateTime.Create(CurrentCacheMetadata.CacheWriteTime, DateTimeReferenceFrame.Server));
						navRecord.SetFieldValue(5, new NavBLOB(cacheData ?? Serialize()));
						if (navRecord.Insert(DataError.TrapError, runApplicationTrigger: false, runGlobalTrigger: false))
						{
							SendTraceTag("Inserted Token Cache row for SID {0} on AAD tenant {1} with AAD user unique ID {2}, write time {3:O}, and cache BLOB of {4} bytes.", DiagnosticParameter.Eupi(sessionManager.NavUserSecurityId), DiagnosticParameter.Eupi(CurrentCacheMetadata.AadTenantId), DiagnosticParameter.Eupi(CurrentCacheMetadata.AadUserUniqueId), DiagnosticParameter.SystemMetadata(CurrentCacheMetadata.CacheWriteTime), DiagnosticParameter.SystemMetadata(cacheData.Length));
						}
					}
				}
			}
			catch (NavCSideException ex) when (ex.ErrorCode == 85132286)
			{
			}
		});
	}

	private void RefreshCacheFromRecord(NavRecord record)
	{
		CurrentCacheMetadata = new NavTokenCacheMetadata
		{
			UserSecurityId = record.GetFieldValue(1).ToGuid(),
			AadUserUniqueId = record.GetFieldValue(6).ToString(),
			AadTenantId = record.GetFieldValue(7).ToString(),
			CacheWriteTime = record.GetFieldValue(4).ToDateTime()
		};
		byte[] bytes = record.GetFieldValue(5).GetBytes();
		Deserialize(NavTokenCacheBase.IsEmptyData(bytes) ? null : bytes);
	}

	private void SendTraceTag(string messageFormat, DiagnosticParameter firstParameter, params DiagnosticParameter[] parameters)
	{
		sessionManager.GetMostSpecificDiagnosticInstance().SendTraceTag("00000DD", Category.Security, Verbosity.Verbose, messageFormat, firstParameter, parameters);
	}
}
