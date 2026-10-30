using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SystemTableTriggers
{
	private readonly NavSession session;

	private bool shouldCheckSuperUserOnCommit;

	private bool shouldCheckUserLicenseTypeOnCommit;

	private bool shouldCheckDeviceLicenseTypeOnCommit;

	private bool shouldFlushUserCacheOnCommit;

	private static readonly ConcurrentDictionary<Action<string, int[]>, int[]> tableChangeSubscriptions = new ConcurrentDictionary<Action<string, int[]>, int[]>();

	private static List<int> monitoredTables = new List<int>();

	private static readonly object monitoredTablesLock = new object();

	private readonly HashSet<int> changedTables = new HashSet<int>();

	private static bool isSaas = ServerUserSettings.Instance.EnableMembershipEntitlement.Value;

	private (NavText oldName, NavText newName)? allowedCompanyNameChange;

	public SystemTableTriggers(NavSession session)
	{
		this.session = session;
	}

	public static bool TableHasSystemModifyTrigger(int tableId)
	{
		switch (tableId)
		{
		case 2000000001:
		case 2000000004:
		case 2000000005:
		case 2000000006:
		case 2000000053:
		case 2000000071:
		case 2000000072:
		case 2000000074:
		case 2000000075:
		case 2000000120:
		case 2000000121:
		case 2000000130:
		case 2000000146:
		case 2000000150:
		case 2000000175:
		case 2000000177:
		case 2000000186:
		case 2000000187:
		case 2000000188:
		case 2000000189:
		case 2000000190:
		case 2000000191:
		case 2000000195:
		case 2000000199:
			return true;
		default:
			return false;
		}
	}

	public static bool TableHasSystemDeleteTrigger(int tableId)
	{
		switch (tableId)
		{
		case 2000000001:
		case 2000000004:
		case 2000000005:
		case 2000000006:
		case 2000000053:
		case 2000000071:
		case 2000000072:
		case 2000000074:
		case 2000000075:
		case 2000000120:
		case 2000000146:
		case 2000000150:
		case 2000000165:
		case 2000000177:
		case 2000000181:
		case 2000000184:
		case 2000000186:
		case 2000000187:
		case 2000000188:
		case 2000000189:
		case 2000000190:
		case 2000000191:
		case 2000000195:
			return true;
		default:
			return false;
		}
	}

	public static bool TableHasSystemInsertTrigger(int tableId)
	{
		switch (tableId)
		{
		case 2000000001:
		case 2000000005:
		case 2000000006:
		case 2000000053:
		case 2000000071:
		case 2000000074:
		case 2000000075:
		case 2000000120:
		case 2000000130:
		case 2000000146:
		case 2000000150:
		case 2000000175:
		case 2000000186:
		case 2000000187:
		case 2000000188:
		case 2000000189:
		case 2000000190:
		case 2000000191:
		case 2000000195:
		case 2000000199:
			return true;
		default:
			return false;
		}
	}

	public void OnAfterModify(MutableRecordBuffer inputRecordBuffer, ReadOnlyRecordBuffer outputRecordBuffer)
	{
		switch (inputRecordBuffer.MetaTable.ObjectId)
		{
		case 2000000120:
		{
			NavValue navValue3 = inputRecordBuffer[inputRecordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			NavValue originalValue = inputRecordBuffer.GetOriginalValue(1);
			NavValue changedFieldValue = inputRecordBuffer.GetChangedFieldValue(1);
			using (new MaximizedPermissionScope(session, new int[1] { 2000000107 }))
			{
				ModifyAllInTable(session, 2000000107, 4, originalValue, changedFieldValue);
			}
			session.Tenant.AuthenticationCache.ExpireUser(navValue3.ToGuid());
			break;
		}
		case 2000000121:
		{
			NavValue navValue2 = inputRecordBuffer[inputRecordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			session.Tenant.AuthenticationCache.ExpireUser(navValue2.ToGuid());
			break;
		}
		case 2000000006:
			OnAfterModifyCompany(inputRecordBuffer);
			break;
		case 2000000146:
		{
			NavValue navValue = inputRecordBuffer[inputRecordBuffer.MetaTable.GetFieldIndexByNo(2)];
			session.Tenant.IsIntelligentCloudReplicationEnabled = navValue.ToBoolean();
			break;
		}
		}
	}

	private void OnAfterModifyCompany(MutableRecordBuffer inputRecordBuffer)
	{
		NCLMetaField fieldByNo = inputRecordBuffer.MetaTable.GetFieldByNo(1);
		NavText navText = inputRecordBuffer.GetOriginalValue(fieldByNo.FieldIndex) as NavText;
		NavText navText2 = inputRecordBuffer.GetChangedFieldValue(fieldByNo.FieldIndex) as NavText;
		if (navText2 != null)
		{
			session.Diagnostics.SendTraceTag("00000KP", Category.Runtime, Verbosity.Verbose, PrivacyClassification.OrganizationIdentifiableInformation, "The company name has been changed from {0} to {1}", navText, navText2);
			if (navText != navText2)
			{
				using (new MaximizedPermissionScope(session, new int[1] { 2000000107 }))
				{
					ModifyAllInTable(session, 2000000107, 3, navText, navText2);
				}
			}
		}
		NCLMetaField fieldByNo2 = inputRecordBuffer.MetaTable.GetFieldByNo(2);
		NavBoolean navBoolean = inputRecordBuffer.GetOriginalValue(fieldByNo2.FieldIndex) as NavBoolean;
		if (inputRecordBuffer.GetChangedFieldValue(fieldByNo2.FieldIndex) is NavBoolean navBoolean2)
		{
			session.Diagnostics.SendTraceTag("00000EG", Category.Runtime, Verbosity.Warning, PrivacyClassification.OrganizationIdentifiableInformation, "The evaluation flag of company {0} has been changed to from {1} to {2}", navText2 ?? navText, navBoolean, navBoolean2);
		}
		NCLMetaField fieldByNo3 = inputRecordBuffer.MetaTable.GetFieldByNo(3);
		NavText navText3 = inputRecordBuffer.GetOriginalValue(fieldByNo3.FieldIndex) as NavText;
		NavText navText4 = inputRecordBuffer.GetChangedFieldValue(fieldByNo3.FieldIndex) as NavText;
		if (navText4 != null)
		{
			session.Diagnostics.SendTraceTag("00000KQ", Category.Runtime, Verbosity.Verbose, PrivacyClassification.OrganizationIdentifiableInformation, "The company display name has been changed from {0} to {1}", navText3, navText4);
			NotifyClientWhenDisplayNameChanged(navText4, navText2 ?? navText);
		}
	}

	private static void ModifyAllInTable(NavSession currentSession, int tableNo, int fieldNo, NavValue fieldOldValue, NavValue fieldNewValue)
	{
		currentSession.Diagnostics.SendTraceTag("00007TD", Category.Runtime, Verbosity.Verbose, "ModifyAllInTable {0} using field {1} with filter {2} to {3}", DiagnosticParameter.SystemMetadata(tableNo), DiagnosticParameter.SystemMetadata(fieldNo), DiagnosticParameter.CustomerContent(fieldOldValue), DiagnosticParameter.CustomerContent(fieldNewValue));
		NCLMetaTable metaTableById = currentSession.NCLMetadata.GetMetaTableById(tableNo);
		NCLMetaField nCLMetaField = metaTableById.Fields.First((NCLMetaField fld) => fld.FieldNo == fieldNo);
		using NavRecord navRecord = metaTableById.CreateObjectInstance(currentSession);
		navRecord.ALSetRange(fieldNo, fieldOldValue);
		if (nCLMetaField.FieldIsPartOfPrimaryKey)
		{
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					navRecord.SetFieldValue(fieldNo, NavValue.CreateNavValueFromObject(nCLMetaField, fieldNewValue));
					navRecord.ALModify();
				}
				while (navRecord.ALNext() == 0);
			}
		}
		else
		{
			navRecord.ALModifyAll(fieldNo, fieldNewValue);
		}
	}

	private void NotifyClientWhenDisplayNameChanged(string displayName, string companyName)
	{
		IClientCallback clientCallbackOrNull = session.ClientCallbackOrNull;
		if (clientCallbackOrNull != null && session.Company.CompanyNameToken == session.Database.GetCompanyNameToken(companyName))
		{
			clientCallbackOrNull.CompanyInformationChanged(new CompanyInformationChanges
			{
				CompanyDisplayName = NavCompany.GetCompanyDisplayNameDefaulted(displayName, companyName)
			});
		}
	}

	public void OnAfterDelete(MutableRecordBuffer recordBuffer)
	{
		switch (recordBuffer.MetaTable.ObjectId)
		{
		case 2000000001:
		{
			using NavRecord navRecord = new NavRecord(session, 2000000071);
			navRecord.SetFieldValue(3, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex]);
			navRecord.SetFieldValue(6, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex]);
			navRecord.Delete(DataError.TrapError, runApplicationTrigger: true, isCalledFromUI: false);
			break;
		}
		case 2000000004:
		{
			NavValue roleId = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			NCLMetaTable metaTableById2 = session.NCLMetadata.GetMetaTableById(2000000053);
			NCLMetaField scopeField = metaTableById2.GetFieldByNo(8);
			DeleteAllFromTable(session, 2000000005, 1, roleId);
			Action<NavTenant, NavSession> deleteFromAccessControlAction = (NavTenant navTenant, NavSession navSession) =>
			{
				DeleteAllFromTable(navSession, 2000000053, new Tuple<int, NavValue>(8, NavOption.Create(scopeField.FieldOptionMetadata, 0)), new Tuple<int, NavValue>(9, new NavGuid(((WrappedValue<Guid>)(object)AppId.None).Value)), new Tuple<int, NavValue>(2, roleId));
			};
			if (session.Tenant.TenantType != NavTenantType.System)
			{
				deleteFromAccessControlAction(session.Tenant, session);
			}
			NavEnvironment.Instance.Tenants.AddTenantAction((NavTenant tenant) =>
			{
				((ITenantSessionHandler)tenant).RunTenantActionInSystemSession(deleteFromAccessControlAction, false, false, false, (LanguageSetting)null);
			}, session.Tenant);
			break;
		}
		case 2000000072:
		{
			NavValue navValue4 = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex];
			NavProfileKey navProfileKey = new NavProfileKey(navValue4.ToString());
			DeleteAllFromTable(session, 2000000074, 3, navValue4);
			DeleteAllFromTable(session, 2000000186, 1, navValue4);
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(navProfileKey);
			NavEnvironment.Instance.NavProfilePageMetadataCacheV1.Remove(navProfileKey);
			NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
			break;
		}
		case 2000000177:
		{
			NavValue navValue = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex];
			NavGuid navGuid = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex] as NavGuid;
			NavProfileKey profileKey2 = new NavProfileKey(navValue.ToString(), navGuid, ProfileScope.Tenant);
			DeleteAllFromTable(session, 2000000187, new Tuple<int, NavValue>(2, navValue), new Tuple<int, NavValue>(1, navGuid));
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(profileKey2);
			NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
			break;
		}
		case 2000000165:
		{
			NavValue navValue3 = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			if (navValue3.ToGuid().Equals(Guid.Empty))
			{
				NavValue item = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex];
				NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(2000000053);
				NCLMetaField fieldByNo = metaTableById.GetFieldByNo(8);
				DeleteAllFromTable(session, 2000000166, new Tuple<int, NavValue>(1, navValue3), new Tuple<int, NavValue>(2, item));
				DeleteAllFromTable(session, 2000000053, new Tuple<int, NavValue>(8, NavOption.Create(fieldByNo.FieldOptionMetadata, 1)), new Tuple<int, NavValue>(9, navValue3), new Tuple<int, NavValue>(2, item));
			}
			break;
		}
		case 2000000120:
		{
			NavValue navValue5 = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			DeleteAllFromTable(session, 2000000053, 1, navValue5);
			DeleteAllFromTable(session, 2000000121, 1, navValue5);
			using (new MaximizedPermissionScope(session, new int[1] { 2000000107 }))
			{
				DeleteAllFromTable(session, 2000000107, 4, navValue5);
			}
			session.Tenant.AuthenticationCache.ExpireUser(navValue5.ToGuid());
			break;
		}
		case 2000000006:
		{
			NavValue navValue2 = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			DeleteAllFromTable(session, 2000000053, 3, navValue2);
			DeleteAllFromTable(session, 2000000073, 15, navValue2);
			DeleteAllFromTable(session, 2000000068, 12, navValue2);
			DeleteAllFromTable(session, 2000000159, 1, navValue2);
			using (new MaximizedPermissionScope(session, new int[1] { 2000000107 }))
			{
				DeleteAllFromTable(session, 2000000107, 3, navValue2);
			}
			using (new MaximizedPermissionScope(session, new int[1] { 2000000196 }))
			{
				DeleteAllFromTable(session, 2000000196, 4, navValue2);
			}
			using (new MaximizedPermissionScope(session, new int[3] { 2000000183, 2000000184, 2000000185 }))
			{
				DeleteAllFromTable(session, 2000000183, 3, navValue2);
				DeleteAllFromTable(session, 2000000184, 7, navValue2);
				DeleteAllFromTable(session, 2000000185, 7, navValue2);
			}
			NavTaskSchedulerUtility.CancelAllTasksForCompany(NavCurrentThread.Session.Tenant.Id, navValue2.ToString());
			if (!IsTenantDataModifiedInSql())
			{
				break;
			}
			if (session.CompanyDeletionMode == NavSession.CompanyDeletionType.Incremental || session.CompanyDeletionMode == NavSession.CompanyDeletionType.IncrementalNoSystemTask)
			{
				session.Database.TenantCompanies.SetState(navValue2.ToString(), CompanyState.Deleting);
				if (session.CompanyDeletionMode == NavSession.CompanyDeletionType.Incremental)
				{
					NavTaskSchedulerUtility.ScheduleSystemTask(SystemTaskActionId.DeleteCompany, new DeleteCompanySystemTask.DeleteCompanySystemTaskParameters
					{
						CompanyName = navValue2.ToString()
					}, DateTime.UtcNow);
				}
			}
			else
			{
				session.Database.TenantCompanies.Delete(navValue2.ToString());
			}
			break;
		}
		case 2000000186:
		{
			NavProfileKey profileKey = new NavProfileKey(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex].ToString());
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(profileKey, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToInt32(), ProfilePageMetadataOwner.SystemOrApp);
			NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
			break;
		}
		case 2000000187:
		{
			string profileId = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToString();
			NavGuid appId = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex] as NavGuid;
			NavProfileKey navProfileKey = new NavProfileKey(profileId, appId, ProfileScope.Tenant);
			if (!navProfileKey.ProfileExists())
			{
				navProfileKey = new NavProfileKey(profileId, appId);
			}
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(navProfileKey, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex].ToInt32());
			NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
			break;
		}
		case 2000000188:
			session.UserPageMetadataCache.Remove(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToInt32());
			NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
			break;
		case 2000000074:
		{
			NavProfileKey profileKey = new NavProfileKey(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex].ToString());
			NavEnvironment.Instance.NavProfilePageMetadataCacheV1.Remove(profileKey, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(6).FieldIndex].ToInt32(), recordBuffer[recordBuffer.MetaTable.GetFieldByNo(18).FieldIndex].ToString());
			NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
			break;
		}
		case 2000000075:
			NavCurrentThread.Session.UserPageMetadataCacheV1.Remove(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(9).FieldIndex].ToInt32(), recordBuffer[recordBuffer.MetaTable.GetFieldByNo(21).FieldIndex].ToString());
			NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
			break;
		case 2000000146:
			session.Tenant.IsIntelligentCloudReplicationEnabled = false;
			break;
		}
	}

	public void OnBeforeInsert(MutableRecordBuffer recordBuffer)
	{
		int objectId = recordBuffer.MetaTable.ObjectId;
		switch (objectId)
		{
		case 2000000005:
			ValidateNotSuper(recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)]);
			break;
		case 2000000053:
			shouldCheckSuperUserOnCommit = true;
			ValidateAccessControlForSuperAndSecurityRoles(recordBuffer);
			break;
		case 2000000121:
			ValidateUniqueNameIdentifier(recordBuffer);
			break;
		case 2000000120:
		{
			NavText navText = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)] as NavText;
			NavGuid navGuid = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)] as NavGuid;
			if (!CheckInvalidUserSecurityIds(recordBuffer))
			{
				throw NavNCLUserTableInvalidUserSidException.Create(navGuid.ToString(), navText);
			}
			if (!IsUserFieldUnique(recordBuffer, 2, insert: true))
			{
				throw NavNCLUserTableUserNameMustBeUniqueException.Create();
			}
			ValidateAuthenticationEmail(recordBuffer, insert: true);
			ValidateApplicationId(recordBuffer, insert: true);
			NavText navText2 = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(7)] as NavText;
			if (navText2 != null && !navText2.IsZeroOrEmpty && !IsUserFieldUnique(recordBuffer, 7, insert: true))
			{
				throw NavNCLUserTableUserWindowsSidMustBeUniqueException.Create();
			}
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000121);
			NavRecord navRecord = metaTableById.CreateObjectInstance(NavCurrentThread.Session, isTemporary: false, null, string.Empty, SecurityFiltering.Ignored);
			navRecord.SetFieldValue(1, navGuid);
			navRecord.Insert(DataError.TrapError, runApplicationTrigger: false, runGlobalTrigger: false);
			shouldFlushUserCacheOnCommit = true;
			NavOption navOption = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)] as NavOption;
			NavOption navOption2 = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(10)] as NavOption;
			if (navOption2 == null || (navOption2.Value != 2 && navOption2.Value != 4))
			{
				shouldCheckUserLicenseTypeOnCommit = navOption == null || navOption.Value != 1 || shouldCheckUserLicenseTypeOnCommit;
			}
			if (isSaas && !NavLicenseValidator.IsFullOrFullLikeUser((LicenseType)navOption2.Value))
			{
				session.Diagnostics.SendExceptionTag("00008E9", Category.Runtime, new NavCSideException(string.Format(CultureInfo.CurrentCulture, "LicenseType set to {0}.", navOption2)), Verbosity.Normal);
			}
			break;
		}
		case 2000000001:
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableInsertNotSupported, ProductNames.ProductNameClassicClient));
		case 2000000071:
			if (!NavEnvironment.Instance.DevelopmentMode)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectMetadataTableCannotBeModified, ProductNames.ProductNameClassicClient));
			}
			break;
		case 2000000150:
			throw new NavCSideException(Lang.NavAppObjectMetadataTableCannotBeModified);
		case 2000000130:
			shouldCheckDeviceLicenseTypeOnCommit = true;
			break;
		case 2000000006:
		{
			session.GetPermissionSet(null, session.Company.CompanyNameToken, new ApplicationObjectId[1]
			{
				new ApplicationObjectId(ObjectType.System, 1610)
			}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
			NCLMetaField fieldByNo2 = recordBuffer.MetaTable.GetFieldByNo(8000);
			recordBuffer[fieldByNo2.FieldIndex] = new NavGuid(Guid.NewGuid());
			break;
		}
		case 2000000190:
		case 2000000191:
		case 2000000195:
			HandleOnBeforeEntitlementModification(recordBuffer.MetaTable);
			break;
		case 2000000189:
			OnBeforeInsertTenantLicenseState(recordBuffer);
			break;
		case 2000000199:
		{
			NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(7);
			recordBuffer[fieldByNo.FieldIndex] = new NavText(fieldByNo.FieldDefinedLength, session.AppId);
			break;
		}
		case 2000000186:
		{
			NavProfileKey profileKey = new NavProfileKey(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex].ToString());
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(profileKey, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToInt32(), ProfilePageMetadataOwner.SystemOrApp);
			break;
		}
		case 2000000187:
		{
			string profileId = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToString();
			NavGuid appId = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex] as NavGuid;
			NavProfileKey navProfileKey = new NavProfileKey(profileId, appId, ProfileScope.Tenant);
			if (!navProfileKey.ProfileExists())
			{
				navProfileKey = new NavProfileKey(profileId, appId);
			}
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(navProfileKey, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex].ToInt32());
			break;
		}
		case 2000000188:
			session.UserPageMetadataCache.Remove(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToInt32());
			break;
		case 2000000074:
		{
			NavProfileKey profileKey = new NavProfileKey(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex].ToString());
			NavEnvironment.Instance.NavProfilePageMetadataCacheV1.Remove(profileKey, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(6).FieldIndex].ToInt32(), recordBuffer[recordBuffer.MetaTable.GetFieldByNo(18).FieldIndex].ToString());
			break;
		}
		case 2000000075:
			NavCurrentThread.Session.UserPageMetadataCacheV1.Remove(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(9).FieldIndex].ToInt32(), recordBuffer[recordBuffer.MetaTable.GetFieldByNo(21).FieldIndex].ToString());
			break;
		case 2000000175:
			ValidateUserInfoHasNotBeenChanged(recordBuffer);
			break;
		}
		if (objectId >= 2000000001)
		{
			changedTables.Add(objectId);
		}
	}

	public void OnAfterInsert(MutableRecordBuffer recordBuffer)
	{
		switch (recordBuffer.MetaTable.ObjectId)
		{
		case 2000000006:
			OnAfterInsertCompany(recordBuffer);
			break;
		case 2000000189:
			OnAfterInsertTenantLicenseState(recordBuffer);
			break;
		case 2000000146:
		{
			NavValue navValue = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)];
			session.Tenant.IsIntelligentCloudReplicationEnabled = navValue.ToBoolean();
			break;
		}
		}
	}

	private void ValidateUserInfoHasNotBeenChanged(IRecordBuffer recordBuffer)
	{
		if (ServerUserSettings.Instance.EnableUserConsistencyValidationOnTasks.Value)
		{
			bool flag = !recordBuffer[3].IsZeroOrEmpty && !recordBuffer[3].ValueAsObject.Equals(session.User.Name);
			bool flag2 = !recordBuffer[2].IsZeroOrEmpty && !recordBuffer[2].ValueAsObject.Equals(session.User.Id);
			if (flag | flag2)
			{
				throw new NavCSideException(Lang.TaskHadUserInfoChanged);
			}
		}
	}

	private void OnAfterInsertCompany(MutableRecordBuffer recordBuffer)
	{
		NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(1);
		NavText navText = (NavText)recordBuffer.GetChangedFieldValue(fieldByNo.FieldIndex);
		if (navText == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(navText))
		{
			throw new NavCSideException(Lang.NavNCLCompanyNameEmptyException);
		}
		if (IsTenantDataModifiedInSql())
		{
			NavDatabase database = session.Database;
			int companyNameToken = database.GetCompanyNameToken(navText);
			CleanupDeletedCompanyData(session.Tenant, navText);
			database.TenantCompanies.Insert(navText);
			if (session.CopyFromCompany == 0)
			{
				if (!database.IsSharedTenantSchema)
				{
					database.DatabaseSync.CreateAllTablesInCompany(companyNameToken);
				}
				if (database.IsSharedTenantSchema)
				{
					database.DatabaseSync.CreateAllSequencesInCompany(companyNameToken);
				}
			}
			else
			{
				database.DatabaseSync.CopyAllTablesInCompany(session.CopyFromCompany, companyNameToken);
			}
		}
		OnWriteToCompanyTable();
	}

	internal static void CleanupDeletedCompanyData(NavTenant tenant, string companyName)
	{
		if (tenant.Database.TenantCompanies.CompanyExists(companyName))
		{
			tenant.Diagnostics.SendTraceTag("00001IO", Category.Operations, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Deleting partially deleted company before inserting company of the same name. Company name: {companyName}."));
			DeleteCompanySystemTask.DeleteCompanyData(tenant, companyName, commitIndividualDeleteOperations: false);
			if (tenant.Database.TenantCompanies.CompanyExists(companyName))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CompanyWithSameNameAlreadyExists, companyName));
			}
		}
	}

	private void OnAfterInsertTenantLicenseState(MutableRecordBuffer recordBuffer)
	{
		NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(1);
		NCLMetaField fieldByNo2 = recordBuffer.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo3 = recordBuffer.MetaTable.GetFieldByNo(3);
		TenantLicenseState tenantLicenseState = (TenantLicenseState)recordBuffer[fieldByNo3.FieldIndex].ToInt32();
		DateTime dateTime = recordBuffer[fieldByNo.FieldIndex].ToDateTime();
		DateTime dateTime2 = recordBuffer[fieldByNo2.FieldIndex].ToDateTime();
		session.Diagnostics.SendTraceTag("00000EH", Category.License, Verbosity.Normal, "Tenant license state has been updated. New state: {0} - New Start Date: {1} - New End Date: {2}, NAV Tenant ID : {3},  AAD Tenant ID: {4}", DiagnosticParameter.SystemMetadata(tenantLicenseState), DiagnosticParameter.SystemMetadata(dateTime.ToString(CultureInfo.InvariantCulture)), DiagnosticParameter.SystemMetadata(dateTime2.ToString(CultureInfo.InvariantCulture)), DiagnosticParameter.Oii(session.Tenant?.TenantSettings?.Id), DiagnosticParameter.Oii(session.Tenant?.TenantSettings?.AadTenantId));
	}

	private static bool CheckInvalidUserSecurityIds(MutableRecordBuffer recordBuffer)
	{
		NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(1);
		NavGuid navGuid = recordBuffer[fieldByNo.FieldIndex] as NavGuid;
		if (navGuid.Value.Equals(Guid.Empty) || navGuid.Value.Equals(NavUser.DefaultSuperGuid))
		{
			return false;
		}
		return true;
	}

	private bool IsUserFieldUnique(MutableRecordBuffer recordBuffer, int fieldNo, bool insert, bool enabledUsersOnly = false)
	{
		return FindUserRecordWithFieldValue(recordBuffer, recordBuffer.MetaTable, fieldNo, insert, enabledUsersOnly) == null;
	}

	private MutableRecordBuffer FindUserRecordWithFieldValue(IRecordBuffer recordBuffer, NCLMetaTable metaTable, int fieldNo, bool insert, bool enabledUsersOnly = false)
	{
		NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(metaTable.TableId);
		NCLMetaField fieldByNo = metaTable.GetFieldByNo(fieldNo);
		NCLMetaField fieldByNo2 = metaTable.GetFieldByNo(1);
		IList<Tuple<INavFieldMetadata, FilterExpression>> list = (insert ? EqualsFilter(metaTableById, fieldNo, recordBuffer[fieldByNo.FieldIndex]) : UserTableFilter(metaTableById, recordBuffer[fieldByNo2.FieldIndex], fieldNo, recordBuffer[fieldByNo.FieldIndex]));
		if (enabledUsersOnly)
		{
			AddEnabledUsersFilter(metaTableById, list);
		}
		FiltersAndMarks filtersAndMarks = new FiltersAndMarks(new FilterFieldDictionary(list));
		FindCacheRequest request = new FindCacheRequest(0, metaTableById, cacheable: true, filtersAndMarks, null, SecurityFiltering.Ignored, FieldList.Empty, SortingFieldList.Create(new SortingField[1]
		{
			new SortingField(fieldByNo, SortOrder.Ascending)
		}), FindType.FirstOnly);
		DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(metaTableById);
		ResultSetEnumerator resultSetEnumerator = dataAccessForTable.Find(request);
		if (resultSetEnumerator.TryMoveNext(out var recordBuffer2) != ResultSetMoveResult.Ok)
		{
			return null;
		}
		return recordBuffer2;
	}

	public void OnBeforeModify(MutableRecordBuffer recordBuffer, int companyToken)
	{
		int objectId = recordBuffer.MetaTable.ObjectId;
		switch (objectId)
		{
		case 2000000005:
			ValidateNotSuper(recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)]);
			break;
		case 2000000053:
			shouldCheckSuperUserOnCommit = true;
			ValidateAccessControlForSuperAndSecurityRoles(recordBuffer);
			break;
		case 2000000121:
			OnBeforeModifyUserProperty(recordBuffer);
			shouldFlushUserCacheOnCommit = true;
			break;
		case 2000000120:
			OnBeforeModifyUser(recordBuffer);
			break;
		case 2000000004:
			if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(1)))
			{
				throw new NavCSideException(Lang.PermissionSetCannotRenamed);
			}
			break;
		case 2000000001:
			OnBeforeModifyObject(recordBuffer, companyToken);
			break;
		case 2000000150:
			throw new NavCSideException(Lang.NavAppObjectMetadataTableCannotBeModified);
		case 2000000071:
		{
			ObjectType objectType = (ObjectType)recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)].ToInt32();
			if (!NavEnvironment.Instance.DevelopmentMode && (objectType == ObjectType.Table || objectType == ObjectType.TableData))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectMetadataTableCannotBeModified, ProductNames.ProductNameClassicClient));
			}
			break;
		}
		case 2000000130:
		{
			NavBoolean navBoolean = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)] as NavBoolean;
			shouldCheckDeviceLicenseTypeOnCommit = navBoolean == null || navBoolean.Value == (bool)NavBoolean.True || shouldCheckDeviceLicenseTypeOnCommit;
			break;
		}
		case 2000000006:
			OnBeforeModifyCompany(recordBuffer);
			break;
		case 2000000072:
		{
			NavValue originalValue = recordBuffer.GetOriginalValue(recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex);
			NavValue navValue = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex];
			if (originalValue != navValue)
			{
				NavProfileKey profileKey = new NavProfileKey(originalValue.ToString());
				UpdateProfileIdForTable(session, 2000000074, 3, originalValue, navValue);
				UpdateProfileIdForTable(session, 2000000073, 9, originalValue, navValue);
				NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(profileKey);
				NavEnvironment.Instance.NavProfilePageMetadataCacheV1.Remove(profileKey);
			}
			break;
		}
		case 2000000177:
		{
			NavValue originalValue2 = recordBuffer.GetOriginalValue(recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex);
			NavValue navValue2 = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex];
			NavValue originalValue3 = recordBuffer.GetOriginalValue(recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex);
			NavValue item = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			if (originalValue2 != navValue2)
			{
				NavProfileKey profileKey2 = new NavProfileKey(originalValue2.ToString(), originalValue3 as NavGuid, ProfileScope.Tenant);
				UpdateFieldsForTable(session, 2000000187, new Tuple<int, Tuple<NavValue, NavValue>>(2, new Tuple<NavValue, NavValue>(originalValue2, navValue2)), new Tuple<int, Tuple<NavValue, NavValue>>(1, new Tuple<NavValue, NavValue>(originalValue3, item)));
				UpdateFieldsForTable(session, 2000000073, new Tuple<int, Tuple<NavValue, NavValue>>(9, new Tuple<NavValue, NavValue>(originalValue2, navValue2)), new Tuple<int, Tuple<NavValue, NavValue>>(10, new Tuple<NavValue, NavValue>(originalValue3, item)));
				NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(profileKey2);
			}
			break;
		}
		case 2000000190:
		case 2000000191:
		case 2000000195:
			HandleOnBeforeEntitlementModification(recordBuffer.MetaTable);
			break;
		case 2000000189:
			if (!session.IsSystemSession || !session.Tenant.IsInExclusiveAccessMode)
			{
				throw new NavCSideException(Lang.CannotModifyTenantLicenseStateTable);
			}
			break;
		case 2000000199:
		{
			NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(7);
			if (recordBuffer.GetChangedFieldValue(fieldByNo.FieldIndex) != null)
			{
				throw new NavCSideException(Lang.CannotModifyApplicationIdFieldInWebhookSubscriptionTable);
			}
			break;
		}
		case 2000000186:
		{
			NavProfileKey profileKey3 = new NavProfileKey(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex].ToString());
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(profileKey3, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToInt32());
			break;
		}
		case 2000000187:
		{
			string profileId = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex].ToString();
			NavGuid appId = recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex] as NavGuid;
			NavProfileKey navProfileKey = new NavProfileKey(profileId, appId, ProfileScope.Tenant);
			if (!navProfileKey.ProfileExists())
			{
				navProfileKey = new NavProfileKey(profileId, appId);
			}
			NavEnvironment.Instance.NavProfilePageMetadataCache.Remove(navProfileKey, recordBuffer.GetOriginalValue(recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex).ToInt32());
			break;
		}
		case 2000000188:
			session.UserPageMetadataCache.Remove(recordBuffer.GetOriginalValue(recordBuffer.MetaTable.GetFieldByNo(2).FieldIndex).ToInt32());
			break;
		case 2000000074:
		{
			NavProfileKey profileKey3 = new NavProfileKey(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(3).FieldIndex].ToString());
			NavEnvironment.Instance.NavProfilePageMetadataCacheV1.Remove(profileKey3, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(6).FieldIndex].ToInt32(), recordBuffer[recordBuffer.MetaTable.GetFieldByNo(18).FieldIndex].ToString());
			break;
		}
		case 2000000075:
			NavCurrentThread.Session.UserPageMetadataCacheV1.Remove(recordBuffer[recordBuffer.MetaTable.GetFieldByNo(9).FieldIndex].ToInt32(), recordBuffer[recordBuffer.MetaTable.GetFieldByNo(21).FieldIndex].ToString());
			break;
		case 2000000175:
		{
			bool flag = false;
			if (allowedCompanyNameChange.HasValue)
			{
				int fieldIndexByNo = recordBuffer.MetaTable.GetFieldIndexByNo(10);
				NavText navText = (NavText)recordBuffer.GetOriginalValue(fieldIndexByNo);
				NavText navText2 = (NavText)recordBuffer.GetChangedFieldValue(fieldIndexByNo);
				flag = navText == allowedCompanyNameChange.Value.oldName && navText2 == allowedCompanyNameChange.Value.newName && OnlySpecifiedFieldChanged(recordBuffer, fieldIndexByNo);
			}
			if (!flag)
			{
				ValidateUserInfoHasNotBeenChanged(recordBuffer);
			}
			break;
		}
		}
		if (objectId >= 2000000001)
		{
			changedTables.Add(objectId);
		}
	}

	private void OnBeforeModifyUserProperty(MutableRecordBuffer recordBuffer)
	{
		ValidateUniqueNameIdentifier(recordBuffer);
	}

	private void ValidateUniqueNameIdentifier(MutableRecordBuffer recordBuffer)
	{
		int fieldIndexByNo = recordBuffer.MetaTable.GetFieldIndexByNo(3);
		NavText navText = (NavText)recordBuffer[fieldIndexByNo];
		NavText navText2 = (NavText)recordBuffer.GetOriginalValue(fieldIndexByNo);
		if (!navText.IsZeroOrEmpty && !navText.Equals(navText2) && !IsUserFieldUnique(recordBuffer, 3, insert: false))
		{
			throw new NavNCLUserPropertyTableNameIdentifierMustBeUniqueException(Lang.AppIdIsUsedByAnotherUser);
		}
	}

	private void OnBeforeModifyUser(MutableRecordBuffer recordBuffer)
	{
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(1)))
		{
			throw NavNCLUserTableUserSidCannotChangeException.Create();
		}
		MutableRecordBuffer mutableRecordBuffer = FindUserRecordWithFieldValue(recordBuffer, recordBuffer.MetaTable, 2, insert: false);
		if (mutableRecordBuffer != null)
		{
			NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo2 = recordBuffer.MetaTable.GetFieldByNo(1);
			session.Diagnostics.SendTraceTag("000011K", Category.Security, Verbosity.Normal, "User modification failed: UserName is not unique (former value is {0}), another user with SID {1} has the same Name: {2}. ", DiagnosticParameter.Euii(recordBuffer.ReadOnlyBuffer[fieldByNo.FieldIndex].ToString()), DiagnosticParameter.Euii(mutableRecordBuffer[fieldByNo2.FieldIndex]), DiagnosticParameter.Euii(mutableRecordBuffer[fieldByNo.FieldIndex].ToString()));
			throw NavNCLUserTableUserNameMustBeUniqueException.Create();
		}
		NavText navText = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(7)] as NavText;
		if (navText != null && !navText.IsZeroOrEmpty && !IsUserFieldUnique(recordBuffer, 7, insert: false))
		{
			throw NavNCLUserTableUserWindowsSidMustBeUniqueException.Create();
		}
		shouldFlushUserCacheOnCommit = true;
		shouldCheckSuperUserOnCommit = true;
		NavOption navOption = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)] as NavOption;
		NavOption navOption2 = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(10)] as NavOption;
		if (navOption2 == null || (navOption2.Value != 2 && navOption2.Value != 4))
		{
			shouldCheckUserLicenseTypeOnCommit = navOption == null || navOption.Value != 1 || shouldCheckUserLicenseTypeOnCommit;
		}
		NavGuid userSid = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)] as NavGuid;
		ValidateOnlySuperCanModifySuper(userSid);
		ValidateApplicationId(recordBuffer, insert: false);
		ValidateAuthenticationEmail(recordBuffer, insert: false);
		if (isSaas && !NavLicenseValidator.IsFullOrFullLikeUser((LicenseType)navOption2.Value))
		{
			session.Diagnostics.SendExceptionTag("00008EA", Category.Runtime, new NavCSideException(string.Format(CultureInfo.CurrentCulture, "LicenseType set to {0} from old value {1}", navOption2, recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(10)] as NavOption)), Verbosity.Normal);
		}
	}

	private void OnBeforeModifyObject(MutableRecordBuffer recordBuffer, int companyToken)
	{
		if (recordBuffer.GetOriginalValue(1).ToInt32() == 0)
		{
			throw new NavCSideException(Lang.ObjectTableDataIsReadOnly);
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(1)))
		{
			throw new NavCSideException(18023770, Lang.ObjectTableCannotChangeType);
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(2)))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeField, recordBuffer.MetaTable.GetFieldByNo(2).FieldName));
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(3)))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeField, recordBuffer.MetaTable.GetFieldByNo(3).FieldName));
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(7)) || !recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(8)))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeBlob, ProductNames.ProductNameClassicClient));
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(6)))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeCompiledFlag, ProductNames.ProductNameClassicClient));
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(50)))
		{
			NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(50);
			NavOemText navOemText = recordBuffer[fieldByNo.FieldIndex] as NavOemText;
			if (!navOemText.IsZeroOrEmpty)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeField, fieldByNo.FieldName));
			}
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(40)))
		{
			ValidateObjectLockStatus(recordBuffer);
		}
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(recordBuffer.MetaTable.GetFieldIndexByNo(4)))
		{
			NCLMetaField fieldByNo2 = recordBuffer.MetaTable.GetFieldByNo(4);
			ExistsCacheRequest request = new ExistsCacheRequest(companyToken, recordBuffer.MetaTable, cacheable: true, new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo2, new UnaryFilterExpression(FilterExpressionType.Equal, recordBuffer[recordBuffer.MetaTable.GetFieldByNo(4).FieldIndex], fieldByNo2.ExpressionContext)))), SecurityFiltering.Ignored, forceNonLockingProviderRequest: true);
			DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(recordBuffer.MetaTable);
			if (dataAccessForTable.Exists(request))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableNameNotUnique, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)], recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)]));
			}
		}
	}

	private void ValidateObjectLockStatus(MutableRecordBuffer recordBuffer)
	{
		NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(50);
		if (recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(40)].Equals(NavBoolean.True))
		{
			recordBuffer[fieldByNo.FieldIndex] = new NavOemText(fieldByNo.FieldDefinedLength, session.User.Name);
			return;
		}
		NavOemText navOemText = recordBuffer.GetOriginalValue(fieldByNo.FieldIndex) as NavOemText;
		if (string.Compare(navOemText.Value, session.User.Name, StringComparison.OrdinalIgnoreCase) != 0 && !session.User.IsDefaultSuper && !session.Permissions.HasRole(0, "SUPER", PermissionScope.System, AppId.None))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		recordBuffer[fieldByNo.FieldIndex] = NavOemText.Default(0);
	}

	private void OnBeforeInsertTenantLicenseState(MutableRecordBuffer recordBuffer)
	{
		NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(1);
		NCLMetaField fieldByNo2 = recordBuffer.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo3 = recordBuffer.MetaTable.GetFieldByNo(3);
		int num = recordBuffer[fieldByNo3.FieldIndex].ToInt32();
		if (!Enum.IsDefined(typeof(TenantLicenseState), num))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidTenantLicenseState, num));
		}
		TenantLicenseState tenantLicenseState = (TenantLicenseState)num;
		int period = TenantLicenseStatePeriodProvider.GetPeriod(tenantLicenseState);
		recordBuffer[fieldByNo2.FieldIndex] = ((period == -1) ? NavDateTime.Undefined : NavDateTime.Create(recordBuffer[fieldByNo.FieldIndex].ToDateTime().AddDays(period)));
		using NavRecord navRecord = new NavRecord(session, 2000000189);
		if (!navRecord.ALFindLast(DataError.TrapError))
		{
			return;
		}
		try
		{
			if (recordBuffer[fieldByNo.FieldIndex].CompareTo(navRecord.GetFieldValue(fieldByNo)) <= 0)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.TenantLicenseStateStartDateIsEarlierThanCurrentState, navRecord.GetFieldValue(fieldByNo).ToDateTime().ToString(CultureInfo.CurrentCulture), recordBuffer[fieldByNo.FieldIndex].ToDateTime().ToString(CultureInfo.CurrentCulture)));
			}
			TenantLicenseState stateFromRecord = TenantLicenseStateHelper.GetStateFromRecord(navRecord);
			TenantLicenseStateHelper.ValidateStateTransition(stateFromRecord, tenantLicenseState);
		}
		catch (NavCSideException exception)
		{
			string content = string.Format(CultureInfo.InvariantCulture, "An attempt of inserting an invalid tenant state license record has been made.\r\nCurrent state: {0} - New state: {1}\r\nCurrent start date: {2} - New start date: {3}\r\nCurrent end state: {4} - New end state: {5} ", TenantLicenseStateHelper.GetStateFromRecord(navRecord), (TenantLicenseState)recordBuffer[fieldByNo3.FieldIndex].ToInt32(), navRecord.GetFieldValue(fieldByNo).ToDateTime().ToString(CultureInfo.InvariantCulture), recordBuffer[fieldByNo.FieldIndex].ToDateTime().ToString(CultureInfo.InvariantCulture), navRecord.GetFieldValue(fieldByNo2).ToDateTime().ToString(CultureInfo.InvariantCulture), recordBuffer[fieldByNo2.FieldIndex].ToDateTime().ToString(CultureInfo.InvariantCulture));
			NavCurrentThread.Session.Diagnostics.SendExceptionTag("00000EI", Category.License, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata(content));
			throw;
		}
	}

	private void OnBeforeModifyCompany(MutableRecordBuffer recordBuffer)
	{
		allowedCompanyNameChange = null;
		NCLMetaField fieldByNo = recordBuffer.MetaTable.GetFieldByNo(8000);
		if (!recordBuffer.IsChangedValueSameAsOriginalValue(fieldByNo.FieldIndex))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotChangeCompanyId));
		}
		NCLMetaField fieldByNo2 = recordBuffer.MetaTable.GetFieldByNo(1);
		NavText navText = (NavText)recordBuffer.GetChangedFieldValue(fieldByNo2.FieldIndex);
		if (!(navText == null))
		{
			if (string.IsNullOrEmpty(navText))
			{
				throw new NavCSideException(Lang.NavNCLCompanyNameEmptyException);
			}
			NavDatabase database = session.Database;
			NavText navText2 = (NavText)recordBuffer.GetOriginalValue(fieldByNo2.FieldIndex);
			int companyNameToken = database.GetCompanyNameToken(navText2);
			if (session.ClientConnectionType != ConnectionType.ManagementClient && navText != navText2 && companyNameToken == session.Company.CompanyNameToken)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotRenameCurrentCompany, navText2));
			}
			session.GetPermissionSet(null, companyNameToken, new ApplicationObjectId[1]
			{
				new ApplicationObjectId(ObjectType.System, 1630)
			}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
			if (!navText2.Value.Equals(navText.Value, StringComparison.CurrentCultureIgnoreCase))
			{
				CleanupDeletedCompanyData(session.Tenant, navText);
			}
			if (IsTenantDataModifiedInSql())
			{
				database.TenantCompanies.Rename(navText2, navText);
			}
			session.DataAccessSource.TenantDataAccess.DataProvider.FlushBufferedWrites();
			if (!database.IsSharedTenantSchema && IsTenantDataModifiedInSql())
			{
				int companyNameToken2 = database.GetCompanyNameToken(navText);
				database.DatabaseSync.RenameAllTablesInCompany(companyNameToken, companyNameToken2);
			}
			database.DatabaseState.ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted();
			OnWriteToCompanyTable();
			allowedCompanyNameChange = (navText2, navText);
		}
	}

	private void ValidateOnlySuperCanModifySuper(NavGuid userSid)
	{
		if (session.AdminSession || session.Permissions.IsSuperForAllCompanies)
		{
			return;
		}
		using NavRecord navRecord = new NavRecord(session, 2000000053);
		NCLOptionMetadata fieldOptionMetadata = navRecord.MetaTable.GetFieldByNo(8).FieldOptionMetadata;
		navRecord.ALSetRange(1, userSid);
		navRecord.ALSetRange(2, new NavCode("SUPER".Length, "SUPER"));
		navRecord.ALSetRange(8, NavOption.Create(fieldOptionMetadata, 0));
		navRecord.ALSetRange(9, new NavGuid(((WrappedValue<Guid>)(object)AppId.None).Value));
		if (!navRecord.ALFind(DataError.TrapError, "-"))
		{
			return;
		}
		do
		{
			int companyNameToken = session.Database.GetCompanyNameToken(navRecord.GetFieldValue(3).ToString());
			if (!session.Permissions.HasRole(companyNameToken, "SUPER", PermissionScope.System, AppId.None))
			{
				throw NavNCLSuperRoleRequiredException.Create();
			}
		}
		while (navRecord.ALNext() != 0);
	}

	private void ValidateAccessControlForSuperAndSecurityRoles(MutableRecordBuffer recordBuffer)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		if (session.AdminSession || session.Permissions.IsSuperForAllCompanies || session.Permissions.HasMaximizedPermissions(2000000053) || (session.TestExecution.EffectivePermissionsInUse && session.TestExecution.EffectivePermissionSets.Contains("SECURITY")))
		{
			return;
		}
		int fieldIndexByNo = recordBuffer.MetaTable.GetFieldIndexByNo(2);
		NavCode navCode = recordBuffer.GetChangedFieldValue(fieldIndexByNo) as NavCode;
		NavCode navCode2 = recordBuffer.GetOriginalValue(fieldIndexByNo) as NavCode;
		int fieldIndexByNo2 = recordBuffer.MetaTable.GetFieldIndexByNo(3);
		NavText navText = recordBuffer.GetChangedFieldValue(fieldIndexByNo2) as NavText;
		NavText navText2 = recordBuffer.GetOriginalValue(fieldIndexByNo2) as NavText;
		int fieldIndexByNo3 = recordBuffer.MetaTable.GetFieldIndexByNo(8);
		PermissionScope scope = (PermissionScope)(recordBuffer.GetChangedFieldValue(fieldIndexByNo3)?.ToInt32() ?? 0);
		PermissionScope scope2 = (PermissionScope)(recordBuffer.GetOriginalValue(fieldIndexByNo3)?.ToInt32() ?? 0);
		int fieldIndexByNo4 = recordBuffer.MetaTable.GetFieldIndexByNo(9);
		NavValue changedFieldValue = recordBuffer.GetChangedFieldValue(fieldIndexByNo4);
		AppId appId = ((changedFieldValue == null) ? AppId.None : new AppId(changedFieldValue.ToGuid()));
		NavValue changedFieldValue2 = recordBuffer.GetChangedFieldValue(fieldIndexByNo4);
		AppId appId2 = ((changedFieldValue2 == null) ? AppId.None : new AppId(changedFieldValue2.ToGuid()));
		bool num;
		if (!(navText != null))
		{
			if (!(navText2 != null))
			{
				return;
			}
			num = HasRole(session, navText2, "SUPER", PermissionScope.System, AppId.None);
		}
		else
		{
			num = HasRole(session, navText, "SUPER", PermissionScope.System, AppId.None);
		}
		if (num)
		{
			return;
		}
		bool num2;
		if (!(navText != null))
		{
			if (!(navText2 != null))
			{
				return;
			}
			if (HasRole(session, navText2, "SECURITY", PermissionScope.System, AppId.None))
			{
				num2 = HasRole(session, navText2, navCode2, scope2, appId2);
				goto IL_0239;
			}
		}
		else if (HasRole(session, navText, "SECURITY", PermissionScope.System, AppId.None))
		{
			num2 = HasRole(session, navText, navCode, scope, appId);
			goto IL_0239;
		}
		goto IL_023c;
		IL_023c:
		throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.OnlySuperOrSecurityCanManageAccessConrol));
		IL_0239:
		if (num2)
		{
			return;
		}
		goto IL_023c;
	}

	private static bool HasRole(NavSession session, string companyName, string roleId, PermissionScope scope, AppId appId)
	{
		int companyNameToken = session.Database.GetCompanyNameToken(companyName);
		return session.Permissions.HasRole(companyNameToken, roleId, scope, appId);
	}

	public void OnBeforeDelete(MutableRecordBuffer recordBuffer)
	{
		int objectId = recordBuffer.MetaTable.ObjectId;
		switch (objectId)
		{
		case 2000000005:
			ValidateNotSuper(recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)]);
			break;
		case 2000000053:
			shouldCheckSuperUserOnCommit = true;
			ValidateAccessControlForSuperAndSecurityRoles(recordBuffer);
			break;
		case 2000000001:
		{
			ObjectType objectType2 = (ObjectType)recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)].ToInt32();
			if (objectType2 == ObjectType.Table || objectType2 == ObjectType.TableData)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableOperationsOnTablesNotSupported, ProductNames.ProductNameClassicClient));
			}
			break;
		}
		case 2000000150:
			throw new NavCSideException(Lang.NavAppObjectMetadataTableCannotBeModified);
		case 2000000071:
		{
			ObjectType objectType = (ObjectType)recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)].ToInt32();
			if (!NavEnvironment.Instance.DevelopmentMode && (objectType == ObjectType.Table || objectType == ObjectType.TableData))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectMetadataTableCannotBeModified, ProductNames.ProductNameClassicClient));
			}
			break;
		}
		case 2000000120:
		{
			NavGuid navGuid = (NavGuid)recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)];
			NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(2000000073);
			NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
			ExistsCacheRequest request = new ExistsCacheRequest(0, metaTableById, cacheable: false, new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, new UnaryFilterExpression(FilterExpressionType.Equal, navGuid, fieldByNo.ExpressionContext)))), SecurityFiltering.Ignored, forceNonLockingProviderRequest: false);
			DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(metaTableById);
			if (dataAccessForTable.Exists(request))
			{
				throw NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException.Create((NavCode)recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)]);
			}
			ValidateOnlySuperCanModifySuper(navGuid);
			shouldFlushUserCacheOnCommit = true;
			shouldCheckSuperUserOnCommit = true;
			break;
		}
		case 2000000006:
		{
			NavDatabase database = session.Database;
			NavText navText = (NavText)recordBuffer[recordBuffer.MetaTable.GetFieldByNo(1).FieldIndex];
			int companyNameToken = database.GetCompanyNameToken(navText);
			if (companyNameToken == session.Company.CompanyNameToken)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotDeleteCurrentCompany, navText));
			}
			session.GetPermissionSet(null, companyNameToken, new ApplicationObjectId[1]
			{
				new ApplicationObjectId(ObjectType.System, 1640)
			}).VerifyPermissions(null, PermissionMask.Execute, session.LimitedUserPermissionChecker);
			session.DataAccessSource.TenantDataAccess.DataProvider.FlushBufferedWrites();
			if (IsTenantDataModifiedInSql())
			{
				if (session.CompanyDeletionMode == NavSession.CompanyDeletionType.Complete)
				{
					database.DeleteCompanyData(companyNameToken, commitIndividualDeleteOperations: false);
				}
				else
				{
					database.DatabaseSync.ValidateAllTablesSynchronizedBeforeCompanyDelete();
				}
			}
			database.DatabaseState.ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted();
			OnWriteToCompanyTable();
			break;
		}
		case 2000000190:
		case 2000000191:
		case 2000000195:
			HandleOnBeforeEntitlementModification(recordBuffer.MetaTable);
			break;
		case 2000000189:
			if (!session.IsSystemSession || !session.Tenant.IsInExclusiveAccessMode)
			{
				throw new NavCSideException(Lang.CannotDeleteTenantLicenseStateTable);
			}
			break;
		}
		if (objectId >= 2000000001)
		{
			changedTables.Add(objectId);
		}
	}

	public void OnBeforeModifyAll(NCLMetaTable table, int companyToken, FiltersAndMarks filtersAndMarks, FieldDictionary<NavValue> newFieldValues)
	{
		int objectId = table.ObjectId;
		switch (objectId)
		{
		case 2000000005:
			ValidateSuperNotAffected(table, filtersAndMarks);
			break;
		case 2000000053:
			shouldCheckSuperUserOnCommit = true;
			break;
		case 2000000120:
			HandleUserBeforeModifyAll(table, newFieldValues);
			break;
		case 2000000001:
			HandleObjectBeforeModifyAll(table, companyToken, filtersAndMarks, newFieldValues);
			break;
		case 2000000130:
			HandleDeviceBeforeModifyAll(table, newFieldValues);
			break;
		case 2000000190:
		case 2000000191:
		case 2000000195:
			HandleOnBeforeEntitlementModification(table);
			break;
		}
		if (objectId >= 2000000001)
		{
			changedTables.Add(objectId);
		}
	}

	private void HandleDeviceBeforeModifyAll(NCLMetaTable table, FieldDictionary<NavValue> newFieldValues)
	{
		if (newFieldValues.TryGetValue(table.GetFieldByNo(4), out var value))
		{
			if (!(value is NavBoolean navBoolean) || navBoolean.Value == (bool)NavBoolean.True)
			{
				shouldCheckDeviceLicenseTypeOnCommit = true;
			}
		}
		else
		{
			shouldCheckDeviceLicenseTypeOnCommit = true;
		}
	}

	private void HandleObjectBeforeModifyAll(NCLMetaTable table, int companyToken, FiltersAndMarks filtersAndMarks, FieldDictionary<NavValue> newFieldValues)
	{
		VerifyNoTableObjectsIncludedInFilter(table, filtersAndMarks, companyToken, tableDataOnly: true);
		if (newFieldValues.TryGetValue(table.GetFieldByNo(3), out var value))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeFieldByModifyAll, table.GetFieldByNo(3).FieldName));
		}
		if (newFieldValues.TryGetValue(table.GetFieldByNo(1), out value))
		{
			throw new NavCSideException(18023770, Lang.ObjectTableCannotChangeType);
		}
		if (newFieldValues.TryGetValue(table.GetFieldByNo(7), out value) || newFieldValues.TryGetValue(table.GetFieldByNo(8), out value))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeBlob, ProductNames.ProductNameClassicClient));
		}
		if (newFieldValues.TryGetValue(table.GetFieldByNo(6), out value))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeCompiledFlag, ProductNames.ProductNameClassicClient));
		}
		if (newFieldValues.TryGetValue(table.GetFieldByNo(40), out value))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeFieldByModifyAll, table.GetFieldByNo(40).FieldName));
		}
		if (newFieldValues.TryGetValue(table.GetFieldByNo(50), out value))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeFieldByModifyAll, table.GetFieldByNo(50).FieldName));
		}
		if (newFieldValues.TryGetValue(table.GetFieldByNo(2), out value))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeField, table.GetFieldByNo(2).FieldName));
		}
		if (newFieldValues.TryGetValue(table.GetFieldByNo(4), out value))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableCannotChangeFieldByModifyAll, table.GetFieldByNo(4).FieldName));
		}
	}

	private void HandleUserBeforeModifyAll(NCLMetaTable table, FieldDictionary<NavValue> newFieldValues)
	{
		if (newFieldValues.TryGetValue(table.GetFieldByNo(2), out var value))
		{
			throw NavNCLUserTableCannotBatchModifyFieldException.Create(table.GetFieldByNo(2).FieldName);
		}
		shouldFlushUserCacheOnCommit = true;
		if (newFieldValues.TryGetValue(table.GetFieldByNo(4), out value))
		{
			NavOption navOption = value as NavOption;
			if (navOption == null || navOption.Value != 1)
			{
				shouldCheckUserLicenseTypeOnCommit = true;
			}
		}
		else
		{
			shouldCheckUserLicenseTypeOnCommit = true;
		}
	}

	public void OnBeforeDeleteAll(NCLMetaTable table, FiltersAndMarks filtersAndMarks)
	{
		int objectId = table.ObjectId;
		switch (objectId)
		{
		case 2000000005:
			ValidateSuperNotAffected(table, filtersAndMarks);
			break;
		case 2000000053:
			shouldCheckSuperUserOnCommit = true;
			break;
		case 2000000001:
			throw new NavCSideException(Lang.ObjectTableDeleteAllNotSupported);
		case 2000000120:
			shouldFlushUserCacheOnCommit = true;
			shouldCheckSuperUserOnCommit = true;
			break;
		case 2000000190:
		case 2000000191:
		case 2000000195:
			HandleOnBeforeEntitlementModification(table);
			break;
		}
		if (objectId >= 2000000001)
		{
			changedTables.Add(objectId);
		}
	}

	public void OnTransactionEnding(bool commit)
	{
		try
		{
			CheckForExistenceOfSuperUserIfNecessary(commit);
			CheckUserLicenseTypeViolationsIfNecessary(commit);
			CheckDeviceLicenceTypeViolationsIfNecessary(commit);
			session.UncommittedTaskScheduleActions.OnTransactionEnding(commit);
		}
		finally
		{
			shouldCheckSuperUserOnCommit = false;
			shouldCheckUserLicenseTypeOnCommit = false;
			shouldCheckDeviceLicenseTypeOnCommit = false;
		}
	}

	public void OnTransactionEnded(bool commit)
	{
		FlushUserCacheIfNecessary(commit);
		session.UncommittedTaskScheduleActions.OnTransactionEnded(commit);
		if (changedTables.Count == 0)
		{
			return;
		}
		try
		{
			NotifyTableChanges(session.Tenant.Id, changedTables);
		}
		finally
		{
			changedTables.Clear();
			allowedCompanyNameChange = null;
		}
	}

	public static void TableChangeSubscribe(int[] tableIds, Action<string, int[]> notification)
	{
		tableChangeSubscriptions.AddOrUpdate(notification, tableIds, (Action<string, int[]> a, int[] i) => tableIds);
		UpdateMonitoredTableIds();
	}

	public static void TableChangeUnsubscribe(Action<string, int[]> notification)
	{
		tableChangeSubscriptions.TryRemove(notification, out var _);
		UpdateMonitoredTableIds();
	}

	private static void UpdateMonitoredTableIds()
	{
		List<int> list = new List<int>();
		lock (monitoredTablesLock)
		{
			list.AddRange(tableChangeSubscriptions.Values.SelectMany((int[] v) => v));
			monitoredTables = list;
		}
	}

	private static void NotifyTableChanges(string tenantId, HashSet<int> changedTables)
	{
		foreach (int monitoredTable in monitoredTables)
		{
			if (changedTables.Contains(monitoredTable))
			{
				DoNotifyTableChanges(tenantId, changedTables.ToArray());
				break;
			}
		}
	}

	private static void DoNotifyTableChanges(string tenantId, int[] changedTables)
	{
		foreach (KeyValuePair<Action<string, int[]>, int[]> item in tableChangeSubscriptions.Where((KeyValuePair<Action<string, int[]>, int[]> subscription) => subscription.Value.Any(((IEnumerable<int>)changedTables).Contains<int>)))
		{
			item.Key(tenantId, changedTables);
		}
	}

	private static void ValidateNotSuper(NavValue roleId)
	{
		if (roleId != null && roleId.ToString() == "SUPER")
		{
			throw new NavCSideException(Lang.PermissionsOfSuperGroupCannotBeChanged);
		}
	}

	private void ValidateSuperNotAffected(NCLMetaTable permissionTable, FiltersAndMarks filtersAndMarks)
	{
		NCLMetaField fieldByNo = permissionTable.GetFieldByNo(1);
		NavCode value = new NavCode(20, "SUPER");
		UnaryFilterExpression item = FilterExpression.Equal(value, fieldByNo.ExpressionContext);
		FilterFieldDictionary filterFieldDictionary = new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, item));
		FilterFieldDictionary filters = DataProviderHelper.MergeFilterFieldDictionaries(filtersAndMarks.Filters, filterFieldDictionary);
		FiltersAndMarks filtersAndMarks2 = new FiltersAndMarks(filters, filtersAndMarks.MarkedRecords);
		ExistsCacheRequest request = new ExistsCacheRequest(0, permissionTable, cacheable: true, filtersAndMarks2, SecurityFiltering.Ignored, forceNonLockingProviderRequest: false);
		DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(permissionTable);
		if (dataAccessForTable.Exists(request))
		{
			throw new NavCSideException(Lang.PermissionsOfSuperGroupCannotBeChanged);
		}
	}

	private void CheckForExistenceOfSuperUserIfNecessary(bool transactionCommitting)
	{
		if (!session.AdminSession && shouldCheckSuperUserOnCommit && transactionCommitting && !isSaas)
		{
			NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(2000000053);
			ExistsCacheRequest request = new ExistsCacheRequest(0, metaTableById, cacheable: false, FiltersAndMarks.Empty, SecurityFiltering.Ignored, forceNonLockingProviderRequest: false);
			DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(metaTableById);
			if (dataAccessForTable.Exists(request))
			{
				NCLMetaTable metaTableById2 = session.NCLMetadata.GetMetaTableById(2000000120);
				NCLMetaQueryDefinition queryDefinition = GenerateSuperUserAuthenticationMethodQuery(metaTableById, metaTableById2);
				NavClientCredentialType authenticationMethod = ((!session.User.IsNavAdminUser) ? session.Authenticator.AuthenticationMethod : ServerUserSettings.Instance.ClientServicesCredentialType.Value);
				DataProvider dataProvider = session.DataAccessSource.GetDataAccessForQuery(queryDefinition).DataProvider;
				IEnumerator<ReadOnlyRecordBuffer> enumerator = dataProvider.GetQueryResult(queryDefinition, 0, 0, 0, FiltersAndMarks.Empty, null, null, SecurityFiltering.Ignored).GetEnumerator();
				CheckSuperUserAuthenticationMethodQuery(enumerator, authenticationMethod);
			}
		}
	}

	private static NCLMetaQueryDefinition GenerateSuperUserAuthenticationMethodQuery(NCLMetaTable accessControlTable, NCLMetaTable userTable)
	{
		NCLMetaField roleIdField = accessControlTable.GetFieldByNo(2);
		NCLMetaField scopeField = accessControlTable.GetFieldByNo(8);
		NCLMetaField appIdField = accessControlTable.GetFieldByNo(9);
		NCLMetaField stateField = userTable.GetFieldByNo(4);
		NCLMetaField expiryDateField = userTable.GetFieldByNo(5);
		NCLMetaField licenseTypeField = userTable.GetFieldByNo(10);
		return NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, 2000000053, DataLockState.None, "AccessControl", (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(dataItemBeingCreated, 1, "1", 1, filterOnly: false), NCLMetaQueryColumn.Create(dataItemBeingCreated, 3, "2", 2, filterOnly: false)), (NCLMetaQueryDataItem dataItemBeingCreated) => new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(roleIdField, FilterExpression.Equal(new NavCode("SUPER".Length, "SUPER"), roleIdField.ExpressionContext)), new Tuple<INavFieldMetadata, FilterExpression>(scopeField, FilterExpression.Equal(NavOption.Create(scopeField.FieldOptionMetadata, 0), scopeField.ExpressionContext)), new Tuple<INavFieldMetadata, FilterExpression>(appIdField, FilterExpression.Equal(new NavGuid(((WrappedValue<Guid>)(object)AppId.None).Value), appIdField.ExpressionContext)))), null).InnerJoin(queryBeingCreated, 2000000120, DataLockState.None, "User", (NCLMetaQueryDataItem subDataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(subDataItemBeingCreated, 4, "3", 3, filterOnly: false), NCLMetaQueryColumn.Create(subDataItemBeingCreated, 7, "4", 4, filterOnly: false), NCLMetaQueryColumn.Create(subDataItemBeingCreated, 11, "5", 5, filterOnly: false)), (NCLMetaQueryDataItem dataitemBeingCreated) => new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(stateField, FilterExpression.Equal(NavOption.Create(NCLOptionMetadata.Default, 0), stateField.ExpressionContext)), new Tuple<INavFieldMetadata, FilterExpression>(expiryDateField, FilterExpression.Equal(NavDateTime.Undefined, expiryDateField.ExpressionContext)), new Tuple<INavFieldMetadata, FilterExpression>(licenseTypeField, FilterExpression.NotEqual(NavOption.Create(NCLOptionMetadata.Default, 3), licenseTypeField.ExpressionContext)))), (NCLMetaQueryDataItem dataItemBeingCreated) => new ReadOnlyCollection<NCLMetaDataItemLink>(new NCLMetaDataItemLink[1] { NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, "AccessControl", 1, 1, NCLMetaFilterModes.None) })).InnerJoin(queryBeingCreated, 2000000121, DataLockState.None, "UserProperty", (NCLMetaQueryDataItem subDataItemBeingCreated2) => new FieldList(NCLMetaQueryColumn.Create(subDataItemBeingCreated2, 2, "6", 6, filterOnly: false), NCLMetaQueryColumn.Create(subDataItemBeingCreated2, 3, "7", 7, filterOnly: false), NCLMetaQueryColumn.Create(subDataItemBeingCreated2, 7, "8", 8, filterOnly: false)), null, (NCLMetaQueryDataItem dataItemBeingCreated) => new ReadOnlyCollection<NCLMetaDataItemLink>(new NCLMetaDataItemLink[1] { NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, "AccessControl", 1, 1, NCLMetaFilterModes.None) })), (NCLMetaQueryDefinition queryBeingCreated) => new ReadOnlyCollection<NCLMetaQueryOrderBy>(new NCLMetaQueryOrderBy[1] { NCLMetaQueryOrderBy.Create(SortOrder.Ascending, queryBeingCreated.DataItems[0].DefinedColumns[1], extraColumnAutomaticallyAddedForUniqueness: false) }));
	}

	private static void CheckSuperUserAuthenticationMethodQuery(IEnumerator<ReadOnlyRecordBuffer> result, NavClientCredentialType authenticationMethod)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		while (result.MoveNext())
		{
			flag = true;
			string value = result.Current[1].ToString();
			if (string.IsNullOrEmpty(value))
			{
				flag2 = true;
			}
			_ = string.Empty;
			string value2;
			switch (authenticationMethod)
			{
			case NavClientCredentialType.Windows:
			case NavClientCredentialType.UserName:
			case NavClientCredentialType.TaskScheduler:
				value2 = result.Current[3].ToString();
				break;
			case NavClientCredentialType.NavUserPassword:
			case NavClientCredentialType.AccessControlService:
				value2 = result.Current[4].ToString() + result.Current[5].ToString() + result.Current[6].ToString() + result.Current[7].ToString();
				break;
			default:
				value2 = string.Empty;
				break;
			}
			if (!string.IsNullOrEmpty(value2))
			{
				flag3 = true;
			}
		}
		if (!flag)
		{
			throw new NavCSideException(Lang.MustBeASuperUser);
		}
		if (!flag2)
		{
			throw new NavCSideException(Lang.MustBeASuperUserInAllCompanies);
		}
		if (!flag3)
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.MustBeASuperWithCurrentAuthenticationType, ServerUserSettings.Instance.ClientServicesCredentialType.Value.ToString()));
		}
	}

	private void CheckUserLicenseTypeViolationsIfNecessary(bool transactionCommitting)
	{
		if (shouldCheckUserLicenseTypeOnCommit && transactionCommitting)
		{
			INavLicense license = session.License;
			if (license.LicenseModel == LicenseModel.NamedUsers && !session.Tenant.LicenseValidator.CheckIfLicenseNamedUserLimitsAreOk(session))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.UserLicenseTypeCountViolation, license.NamedUsersNumberOfFullUsers, license.NamedUsersNumberOfLimitedUsers));
			}
		}
	}

	private void CheckDeviceLicenceTypeViolationsIfNecessary(bool transactionCommitting)
	{
		if (shouldCheckDeviceLicenseTypeOnCommit && transactionCommitting)
		{
			INavLicense license = session.License;
			if (!session.Tenant.LicenseValidator.CheckIfLicenseDeviceLimitsAreOk(session))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.DeviceLicenseTypeCountViolation, license.NumberOfFullDevices, license.NumberOfLimitedDevices, license.NumberOfIsvDevices, license.NumberOfIsvFunctionalDevices));
			}
		}
	}

	private void FlushUserCacheIfNecessary(bool transactionCommitted)
	{
		if (!shouldFlushUserCacheOnCommit)
		{
			return;
		}
		shouldFlushUserCacheOnCommit = false;
		if (transactionCommitted)
		{
			session.Database.NavUsers.RefreshList();
			NavUser user = session.User;
			if (user.IsDefaultSuper && !user.IsNavAdminUser && session.Database.NavUsers.TryGetNavUserFromSid(user.Sid, out var user2) && !user2.IsDefaultSuper)
			{
				session.User.ReplaceDefaultSuperGuid(user2.Id);
			}
		}
	}

	private void VerifyNoTableObjectsIncludedInFilter(NCLMetaTable table, FiltersAndMarks filtersAndMarks, int companyToken, bool tableDataOnly)
	{
		NCLMetaField fieldByNo = table.GetFieldByNo(1);
		FilterExpression filterExpression = new UnaryFilterExpression(FilterExpressionType.Equal, NavOption.Create(fieldByNo.FieldOptionMetadata, 0), fieldByNo.ExpressionContext);
		if (!tableDataOnly)
		{
			filterExpression = new BinaryFilterExpression(FilterExpressionType.Or, filterExpression, new UnaryFilterExpression(FilterExpressionType.Equal, NavOption.Create(fieldByNo.FieldOptionMetadata, 1), fieldByNo.ExpressionContext));
		}
		FiltersAndMarks filtersAndMarks2 = new FiltersAndMarks(DataProviderHelper.MergeFilterFieldDictionaries(filtersAndMarks.Filters, new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, filterExpression))), filtersAndMarks.MarkedRecords);
		ExistsCacheRequest request = new ExistsCacheRequest(companyToken, table, cacheable: true, filtersAndMarks2, SecurityFiltering.Ignored, forceNonLockingProviderRequest: true);
		DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(table);
		if (dataAccessForTable.Exists(request))
		{
			throw new NavCSideException(tableDataOnly ? Lang.ObjectTableDataIsReadOnly : string.Format(CultureInfo.CurrentCulture, Lang.ObjectTableOperationsOnTablesNotSupported, ProductNames.ProductNameClassicClient));
		}
	}

	private static void DeleteAllFromTable(NavSession session, int tableNo, int fieldNo, NavValue fieldValue)
	{
		session.Diagnostics.SendTraceTag("00000WT", Category.Runtime, Verbosity.Verbose, "DeleteAllFromTable {0} using field {1} with filter {2}", DiagnosticParameter.SystemMetadata(tableNo), DiagnosticParameter.SystemMetadata(fieldNo), DiagnosticParameter.CustomerContent(fieldValue));
		NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(tableNo);
		using NavRecord navRecord = metaTableById.CreateObjectInstance(session);
		navRecord.ALSetRange(fieldNo, fieldValue);
		navRecord.ALDeleteAll();
	}

	private static void DeleteAllFromTable(NavSession session, int tableNo, params Tuple<int, NavValue>[] fieldNoValues)
	{
		session.Diagnostics.SendTraceTag("00000WU", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "DeleteAllFromTable {0} using multiple fields", tableNo);
		NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(tableNo);
		using NavRecord navRecord = metaTableById.CreateObjectInstance(session);
		foreach (Tuple<int, NavValue> tuple in fieldNoValues)
		{
			navRecord.ALSetRange(tuple.Item1, tuple.Item2);
		}
		navRecord.ALDeleteAll();
	}

	private static void UpdateProfileIdForTable(NavSession session, int tableId, int profileIdFieldNo, NavValue oldProfileId, NavValue newProfileId)
	{
		NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(tableId);
		using NavRecord navRecord = metaTableById.CreateObjectInstance(session);
		navRecord.SetRange(navRecord.MetaTable.GetFieldByNo(profileIdFieldNo), oldProfileId, oldProfileId);
		if (navRecord.ALFindSet(DataError.TrapError))
		{
			Tuple<INavFieldMetadata, NavValue> tuple = new Tuple<INavFieldMetadata, NavValue>(navRecord.MetaTable.GetFieldByNo(profileIdFieldNo), newProfileId);
			navRecord.RecordImplementation.ModifyOrRenameAllRecords(new FieldDictionary<NavValue>(tuple));
		}
	}

	private static void UpdateFieldsForTable(NavSession session, int tableId, params Tuple<int, Tuple<NavValue, NavValue>>[] fieldNoValues)
	{
		NCLMetaTable metaTableById = session.NCLMetadata.GetMetaTableById(tableId);
		using NavRecord navRecord = metaTableById.CreateObjectInstance(session);
		foreach (Tuple<int, Tuple<NavValue, NavValue>> tuple in fieldNoValues)
		{
			navRecord.ALSetRange(tuple.Item1, tuple.Item2.Item1);
		}
		if (navRecord.ALFindSet(DataError.TrapError))
		{
			foreach (Tuple<int, Tuple<NavValue, NavValue>> tuple2 in fieldNoValues)
			{
				Tuple<INavFieldMetadata, NavValue> tuple3 = new Tuple<INavFieldMetadata, NavValue>(navRecord.MetaTable.GetFieldByNo(tuple2.Item1), tuple2.Item2.Item1);
				navRecord.RecordImplementation.ModifyOrRenameAllRecords(new FieldDictionary<NavValue>(tuple3));
			}
		}
	}

	private static IList<Tuple<INavFieldMetadata, FilterExpression>> UserTableFilter(NCLMetaTable userTable, NavValue sid, int fieldNo, NavValue fieldValue)
	{
		NCLMetaField fieldByNo = userTable.GetFieldByNo(fieldNo);
		NCLMetaField fieldByNo2 = userTable.GetFieldByNo(1);
		return new List<Tuple<INavFieldMetadata, FilterExpression>>
		{
			new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, FilterExpression.Equal(fieldValue, fieldByNo.ExpressionContext)),
			new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo2, FilterExpression.NotEqual(sid, fieldByNo2.ExpressionContext))
		};
	}

	private static IList<Tuple<INavFieldMetadata, FilterExpression>> EqualsFilter(NCLMetaTable userTable, int fieldNo, NavValue value)
	{
		NCLMetaField fieldByNo = userTable.GetFieldByNo(fieldNo);
		return new List<Tuple<INavFieldMetadata, FilterExpression>>
		{
			new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, FilterExpression.Equal(NavValue.CreateNavValueFromObject(fieldByNo, value), fieldByNo.ExpressionContext))
		};
	}

	private static void AddEnabledUsersFilter(NCLMetaTable userTable, ICollection<Tuple<INavFieldMetadata, FilterExpression>> filterItems)
	{
		NCLMetaField fieldByNo = userTable.GetFieldByNo(4);
		NavOption value = NavOption.Create(fieldByNo.FieldOptionMetadata, 0);
		filterItems.Add(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, FilterExpression.Equal(value, fieldByNo.ExpressionContext)));
	}

	private void ValidateAuthenticationEmail(MutableRecordBuffer recordBuffer, bool insert)
	{
		if (!NavEnvironment.Topology.IsUniqueAuthenticationEmailRequired)
		{
			return;
		}
		int fieldIndexByNo = recordBuffer.MetaTable.GetFieldIndexByNo(11);
		NavText navText = (NavText)recordBuffer[fieldIndexByNo];
		NavText navText2 = (NavText)recordBuffer.GetOriginalValue(fieldIndexByNo);
		string text = TrimAndAnalyzeAuthenticationEmail(navText.Value);
		if (text != navText.Value)
		{
			recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(11)] = new NavText(text);
		}
		if (!string.IsNullOrEmpty(text))
		{
			NavOption navOption = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)] as NavOption;
			if (navOption != null && navOption.Value == 0 && !IsUserFieldUnique(recordBuffer, 11, insert, enabledUsersOnly: true))
			{
				throw NavNclUserTableAuthEmailMustBeUniqueException.Create(text);
			}
		}
		if (!insert && (string.IsNullOrEmpty(text) || navText2 == null || navText2.Value != text))
		{
			Guid userSecurityId = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)].ToGuid();
			UserManagement.SetAuthenticationObjectId(session, userSecurityId, string.Empty);
		}
	}

	private void ValidateApplicationId(MutableRecordBuffer recordBuffer, bool insert)
	{
		int fieldIndexByNo = recordBuffer.MetaTable.GetFieldIndexByNo(16);
		NavGuid navGuid = (NavGuid)recordBuffer[fieldIndexByNo];
		NavGuid navGuid2 = (NavGuid)recordBuffer.GetOriginalValue(fieldIndexByNo);
		if (!navGuid.IsZeroOrEmpty)
		{
			NavOption navOption = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(10)] as NavOption;
			if (navOption == null || navOption.Value != 4)
			{
				throw new NavNCLUserTableAppIdRequiresLicensedAsExternalException(Lang.AppIdRequiresLicensedAsExternal);
			}
			if (!navGuid.Equals(navGuid2) && !IsUserFieldUnique(recordBuffer, 16, insert))
			{
				throw new NavNCLUserTableAppIdMustBeUniqueException(Lang.AppIdIsUsedByAnotherUser);
			}
			NavText navText = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(11)] as NavText;
			if (!string.IsNullOrEmpty(navText.Value))
			{
				throw new NavNCLUserTableCannotSetBothAuthEmailAndAppIdException(Lang.UserAuthEmailAndAppIDBothSet);
			}
		}
		NavValue content = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)];
		if (!insert && !string.IsNullOrEmpty(navGuid2?.ToString()) && !navGuid.Equals(navGuid2))
		{
			Guid userSecurityId = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(1)].ToGuid();
			session.Diagnostics.SendTraceTag("00001CD", Category.License, Verbosity.Normal, "Clearing Authentication Object ID in User Property table for User-> UserName:{0}, UserSid:{1}, OldAppID:{2}, NewAppID:{3}, SessionID:{4}", DiagnosticParameter.Euii(content), DiagnosticParameter.Eupi(userSecurityId.ToString()), DiagnosticParameter.SystemMetadata(navGuid2), DiagnosticParameter.SystemMetadata(navGuid), DiagnosticParameter.SystemMetadata(session.UniqueSessionId));
			UserManagement.SetAuthenticationObjectId(session, userSecurityId, string.Empty);
		}
	}

	private static string TrimAndAnalyzeAuthenticationEmail(string authenticationEmail)
	{
		if (authenticationEmail == null)
		{
			throw new ArgumentNullException("authenticationEmail");
		}
		authenticationEmail = authenticationEmail.Trim();
		if (!string.IsNullOrEmpty(authenticationEmail))
		{
			try
			{
				MailAddress mailAddress = new MailAddress(authenticationEmail);
				authenticationEmail = mailAddress.Address;
			}
			catch (FormatException)
			{
				throw new NavCannotSpecifyAuthenticationEmailException(Lang.AuthenticationEmailNotValid);
			}
		}
		return authenticationEmail;
	}

	private void OnWriteToCompanyTable()
	{
		session.Database.PermissionSetupMonitor.ResetPermissionSetup(clearLicenseInfo: false);
	}

	private void HandleOnBeforeEntitlementModification(NCLMetaTable entitlementMetaTable)
	{
		if (session.Permissions.HasMaximizedPermissions(entitlementMetaTable.TableId))
		{
			return;
		}
		throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.EntitlementTablesDataIsReadOnly, entitlementMetaTable.TableName, ProductNames.ProductNameShort));
	}

	private bool IsTenantDataModifiedInSql()
	{
		return session.DataAccessSource.TenantDataAccess.DataProvider.IsDataModifiedInSqlServer;
	}

	private static bool OnlySpecifiedFieldChanged(MutableRecordBuffer recBuffer, int fieldIndex)
	{
		foreach (NCLMetaField normalField in recBuffer.MetaTable.NormalFields)
		{
			if (!normalField.FieldIsTimestamp && !recBuffer.IsChangedValueSameAsOriginalValue(normalField.FieldIndex) && normalField.FieldIndex != fieldIndex)
			{
				return false;
			}
		}
		return true;
	}
}
