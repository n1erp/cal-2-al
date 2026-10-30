using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.LicensingService.Model.V10;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class EntitlementProvider
{
	internal Func<IEnumerable<NavMembershipEntitlement>> fetchMembershipEntitlementsAction;

	internal const string DelegatedAdministratorsRoleId = "62e90394-69f5-4237-9190-012177145e10";

	internal const string DelegatedHelpdeskRoleId = "729827e3-9c14-49f7-bb1b-9608f156bbb8";

	internal static readonly string ConcurrentDeviceUserWhitelistGroup = "Dynamics 365 Business Central Device Users";

	private const string SelectMembershipEntitlements = "SELECT [Type],[ID],[Name],[Entitlement Set ID],[Is Evaluation] FROM {0}[Membership Entitlement]";

	private static List<NavMembershipEntitlement> membershipEntitlements;

	private readonly NavDatabase database;

	private static readonly Dictionary<string, string> delegatedRoleInternalIds = new Dictionary<string, string>
	{
		{ "62e90394-69f5-4237-9190-012177145e10", "00000000-0000-0000-0000-000000000007" },
		{ "729827e3-9c14-49f7-bb1b-9608f156bbb8", "00000000-0000-0000-0000-000000000008" }
	};

	public IEnumerable<NavMembershipEntitlement> MembershipEntitlements
	{
		get
		{
			if (fetchMembershipEntitlementsAction != null)
			{
				return fetchMembershipEntitlementsAction();
			}
			if (membershipEntitlements == null)
			{
				PreloadMembershipEntitlements();
			}
			return membershipEntitlements;
		}
	}

	private IEnumerable<NavMembershipEntitlement> ProdMembershipEntitlements => MembershipEntitlements.Where((NavMembershipEntitlement e) => !e.IsEvaluation);

	public EntitlementProvider(NavDatabase database)
	{
		this.database = database;
	}

	internal IEnumerable<NavMembershipEntitlement> SelectAzureADClaimRoleMembershipEntitlements(UserSecurityTokenHelper azureAdUserTokenInfo, NavDiagnostics diagnostics)
	{
		delegatedRoleInternalIds.TryGetValue(azureAdUserTokenInfo.DirectoryRoleId, out var directoryRoleInternalId);
		List<NavMembershipEntitlement> list = (from e in MembershipEntitlements
			where e.Type == MembershipEntitlementType.AzureADDelegatedRole && e.Id.Equals(directoryRoleInternalId, StringComparison.OrdinalIgnoreCase)
			select (e)).ToList();
		diagnostics.SendTraceTag("0000081", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Found {0} total membership entitlements from Azure AD group membership claim using internal id {1}", list.Count, directoryRoleInternalId);
		return list;
	}

	internal IEnumerable<NavMembershipEntitlement> SelectAzureADUserAndRolesMembershipEntitlements(LicenseDetails licenseDetails, NavDiagnostics diagnostics, out bool isAdminUser, out bool hasSubscribedSkus, out bool hasEnabledSubscribedSkus)
	{
		return SelectAzureADUserAndRolesMembershipEntitlements(() => licenseDetails, diagnostics, out isAdminUser, out hasSubscribedSkus, out hasEnabledSubscribedSkus);
	}

	internal IEnumerable<NavMembershipEntitlement> SelectAzureADUserAndRolesMembershipEntitlements(Func<LicenseDetails> licenseDetailsProvider, NavDiagnostics diagnostics, out bool isAdminUser, out bool hasSubscribedSkus, out bool hasEnabledSubscribedSkus, int numOfRetries = 5)
	{
		if (licenseDetailsProvider == null)
		{
			throw new ArgumentNullException("licenseDetailsProvider");
		}
		HashSet<NavMembershipEntitlement> hashSet = new HashSet<NavMembershipEntitlement>();
		bool flag = true;
		isAdminUser = false;
		hasSubscribedSkus = false;
		hasEnabledSubscribedSkus = false;
		for (int i = 0; i <= numOfRetries; i++)
		{
			LicenseDetails val = licenseDetailsProvider();
			if (val == null || val.User == null)
			{
				throw new ArgumentNullException("licenseDetailsProvider");
			}
			try
			{
				List<SkuInfo> source = new List<SkuInfo>(val.SubscribedSkus);
				List<SkuInfo> source2 = source.Where((SkuInfo sku) => sku.ServicePlans.Select((ServicePlanInfo sp) => sp.ServicePlanId.ToString()).Intersect(from e in ProdMembershipEntitlements.Where((NavMembershipEntitlement e) =>
					{
						string name = e.Name;
						return name != null && name.IndexOf("Invoic", StringComparison.OrdinalIgnoreCase) <= 0;
					})
					select e.Id).Any()).ToList();
				if (source2.Any())
				{
					hasSubscribedSkus = true;
					hasEnabledSubscribedSkus = source2.Any((SkuInfo sku) => sku.CapabilityStatus == "Enabled");
				}
				if (((UserInfo)val.User).AssignedPlans != null)
				{
					IEnumerable<ServicePlanInfo> inner = (from enabledSku in source
						where enabledSku.CapabilityStatus == "Enabled" || enabledSku.CapabilityStatus == "Warning"
						join assignedLicenses in ((UserInfo)val.User).AssignedLicenses on enabledSku.SkuId equals assignedLicenses.SkuId
						select enabledSku.ServicePlans).SelectMany((IList<ServicePlanInfo> sp) => sp).Distinct();
					IEnumerable<ServicePlanInfo> inner2 = from servicePlan in ((UserInfo)val.User).AssignedPlans
						where servicePlan.CapabilityStatus == "Enabled" || servicePlan.CapabilityStatus == "Warning"
						join licensedPlan in inner on servicePlan.ServicePlanId equals licensedPlan.ServicePlanId
						select servicePlan;
					hashSet.UnionWith(from e in MembershipEntitlements
						where e.Type == MembershipEntitlementType.AzureADPlan
						join servicePlans in inner2 on e.Id.ToUpperInvariant() equals servicePlans.ServicePlanId.ToString().ToUpperInvariant()
						select e);
				}
				if (((UserInfo)val.User).Roles != null)
				{
					List<NavMembershipEntitlement> list = (from e in MembershipEntitlements
						where e.Type == MembershipEntitlementType.AzureADRole
						join userRoles in from userAssignedRole in ((UserInfo)val.User).Roles
							where userAssignedRole.IsSystem
							select userAssignedRole on e.Id.ToUpperInvariant() equals userRoles.RoleTemplateId.ToUpperInvariant()
						select e).ToList();
					if (list.Any())
					{
						isAdminUser = true;
					}
					hashSet.UnionWith(list);
				}
				bool flag2 = val.User.Groups != null;
				if (flag2)
				{
					List<NavMembershipEntitlement> other = (from e in MembershipEntitlements
						where e.Type == MembershipEntitlementType.AzureADGroup
						join userGroup in val.User.Groups on e.Id.ToUpperInvariant() equals userGroup.ObjectId.ToUpperInvariant()
						select e).ToList();
					hashSet.UnionWith(other);
				}
				if (hashSet.Count > 0)
				{
					break;
				}
				LogWriterHelper.LogWith(NavServerEventSource.Log.WriteAuthenticationSubsystemInfoMessage, "The received security token was validated but it does not have any service plans or roles that can be mapped to entitlements." + Environment.NewLine + ((object)val).ToString());
				if (flag2 && IsWhitelisted(val))
				{
					break;
				}
				if (numOfRetries != i)
				{
					diagnostics.SendTraceTag("0000059", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Retrying Azure AD license lookup");
					Thread.Sleep(3000);
				}
				continue;
			}
			catch (NavLicenseServiceException exception)
			{
				diagnostics.SendExceptionTag("00000GY", Category.Security, exception, Verbosity.Warning);
				if (flag)
				{
					flag = false;
					continue;
				}
				throw;
			}
		}
		diagnostics.SendTraceTag("000005A", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Found {0} total membership entitlements from Azure AD licenses and groups", hashSet.Count);
		return hashSet;
	}

	internal IEnumerable<NavMembershipEntitlement> SelectNavUserMembershipEntitlements(NavUser user, NavDiagnostics diagnostics)
	{
		string userId = user.Id.ToString();
		List<NavMembershipEntitlement> list = (from e in MembershipEntitlements
			where e.Type == MembershipEntitlementType.UserSecurityID && e.Id.Equals(userId, StringComparison.OrdinalIgnoreCase)
			select (e)).ToList();
		diagnostics.SendTraceTag("000005B", Category.Security, Verbosity.Normal, "Found {0} total membership entitlements from NavUser ID: {1}", DiagnosticParameter.SystemMetadata(list.Count), DiagnosticParameter.Eupi(userId));
		return list;
	}

	internal IEnumerable<NavMembershipEntitlement> SelectMembershipEntitlementsByTypeAndId(MembershipEntitlementType membershipEntitlementType, string id, NavDiagnostics diagnostics)
	{
		List<NavMembershipEntitlement> list = (from e in MembershipEntitlements
			where e.Type == membershipEntitlementType && e.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
			select (e)).ToList();
		diagnostics.SendTraceTag("000005C", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Found {0} total membership entitlements from {2} ID: {1}", list.Count, id, membershipEntitlementType);
		return list;
	}

	internal IEnumerable<NavMembershipEntitlement> SelectMembershipEntitlementsByTypeAndIds(MembershipEntitlementType membershipEntitlementType, List<string> ids, NavDiagnostics diagnostics)
	{
		List<NavMembershipEntitlement> list = (from e in MembershipEntitlements
			where e.Type == membershipEntitlementType && ids.Contains(e.Id, StringComparer.OrdinalIgnoreCase)
			select (e)).ToList();
		diagnostics.SendTraceTag("00009F2", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Found {0} total membership entitlements from {2} ID's: {1}", list.Count, string.Join(", ", ids), membershipEntitlementType);
		return list;
	}

	public static void ImportMembershipEntitlement(ITreeObject session, byte[] entitlementsFile, string type, string id, string name, string entitlementSetId, string entitlementSetName, bool isEvaluation, bool force)
	{
		VerifyParametersNotNullOrWhiteSpace(session, entitlementsFile, type, id, name, entitlementSetId, entitlementSetName);
		using NavRecord navRecord = new NavRecord(session, 2000000195);
		NavOption navType = GetNavType(type, navRecord);
		NavText navId = new NavText(navRecord.GetFieldValue(2).NavDefinedLengthMetadata, id);
		NavText navName = new NavText(navRecord.GetFieldValue(3).NavDefinedLengthMetadata, name);
		NavCode navCode = new NavCode(navRecord.GetFieldValue(4).NavDefinedLengthMetadata, entitlementSetId);
		NavText navText = new NavText(navRecord.GetFieldValue(5).NavDefinedLengthMetadata, entitlementSetName);
		NavBoolean navIsEvaluation = NavBoolean.Create(isEvaluation);
		if (CheckIfMembershipEntitlementExists(navRecord, navType, navId, navCode))
		{
			if (!force)
			{
				throw NavMembershipEntitlementAlreadyExistsException.Create(type, id, entitlementSetId);
			}
			navRecord.Delete(DataError.ThrowError, runApplicationTrigger: false, isCalledFromUI: false, isBulkDelete: false);
		}
		DeleteAllEntitlements(session, navCode);
		DeleteEntitlementSet(session, navCode);
		InsertMembershipEntitlement(navRecord, navType, navId, navName, navCode, navText, navIsEvaluation);
		InsertEntitlementSet(session, navCode, navText);
		InsertEntitlements(session, entitlementsFile, navCode, navText);
	}

	internal void PreloadMembershipEntitlements()
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		mostSpecificInstance.SendTraceTag("0000027", Category.Operations, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading Membership Entitlements");
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		List<NavMembershipEntitlement> newMembershipEntitlementSets = new List<NavMembershipEntitlement>();
		string getEntitlementSetsQuery = GenerateSelectEntitlementSetsQuery(navSqlConnectionScope.Connection, database);
		try
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey(getEntitlementSetsQuery, database), () => getEntitlementSetsQuery, (NavSqlCommand command) =>
			{
				using (command.ExecuteReader())
				{
					while (command.ReadNextRow())
					{
						NavMembershipEntitlement item = new NavMembershipEntitlement
						{
							Type = (MembershipEntitlementType)command.Reader.GetInt32(0),
							Id = command.Reader.GetString(1),
							Name = command.Reader.GetString(2),
							EntitlementSetId = command.Reader.GetString(3),
							IsEvaluation = Convert.ToBoolean(command.Reader.GetByte(4))
						};
						newMembershipEntitlementSets.Add(item);
					}
					command.CloseReader(forceClose: false);
				}
				return 0;
			});
		}
		catch (NavSqlException ex)
		{
			mostSpecificInstance.SendExceptionTag("0000028", Category.Operations, ex);
			if (((SqlException)ex.InnerException).Number != 208)
			{
				throw;
			}
		}
		Interlocked.Exchange(ref membershipEntitlements, newMembershipEntitlementSets);
	}

	internal List<string> GetTenantDeviceServicePlans(LicenseDetails userLicenseDetails)
	{
		return GetTenantDeviceSkus(userLicenseDetails).SelectMany((SkuInfo sku) => sku.ServicePlans.Select((ServicePlanInfo sp) => sp.ServicePlanId.ToString())).ToList();
	}

	internal bool IsAzureADUserValidDeviceUser(LicenseDetails userLicenseDetails, NavTenant tenant)
	{
		if (userLicenseDetails == null || userLicenseDetails.User == null)
		{
			return false;
		}
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance(tenant);
		if (!FeatureSwitches.Instance.EnableConcurrentDeviceUserAuth)
		{
			mostSpecificInstance.SendTraceTag("00009F3", Category.Operations, Verbosity.Warning, PrivacyClassification.SystemMetadata, FormattableString.Invariant(FormattableStringFactory.Create("Feature switch {0} is not enabled.", "EnableConcurrentDeviceUserAuth")));
			return false;
		}
		if (!IsWhitelisted(userLicenseDetails))
		{
			mostSpecificInstance.SendTraceTag("00009F4", Category.Operations, Verbosity.Warning, "User with User ID:{0} belonging to Tenant ID:{1} does not belong to group: {2}", DiagnosticParameter.Eupi(((UserInfo)userLicenseDetails.User).ObjectId), DiagnosticParameter.Oii(tenant.Id), DiagnosticParameter.SystemMetadata(ConcurrentDeviceUserWhitelistGroup));
			return false;
		}
		ValidateConcurrentDeviceUserSignIn(userLicenseDetails, tenant);
		return true;
	}

	private static bool IsWhitelisted(LicenseDetails userLicenseDetails)
	{
		return userLicenseDetails.User.Groups.Any((GroupInfo group) => group.DisplayName.IndexOf(ConcurrentDeviceUserWhitelistGroup, StringComparison.OrdinalIgnoreCase) >= 0);
	}

	internal void ValidateAndUpdateConcurrentDeviceUserSignIn(LicenseDetails licenseDetails, NavTenant tenant)
	{
		if (licenseDetails == null || licenseDetails.User == null)
		{
			throw new ArgumentNullException("licenseDetails");
		}
		if (tenant == null)
		{
			throw new ArgumentNullException("tenant");
		}
		ValidateConcurrentDeviceUserSignIn(licenseDetails, tenant);
		tenant.ConcurrentDeviceUsers.AddOrUpdate(((UserInfo)licenseDetails.User).ObjectId, 1, (string k, int v) => v + 1);
	}

	private void ValidateConcurrentDeviceUserSignIn(LicenseDetails licenseDetails, NavTenant tenant)
	{
		NavDiagnostics diagnostics = tenant.Diagnostics;
		IEnumerable<SkuInfo> tenantDeviceSkus = GetTenantDeviceSkus(licenseDetails);
		int num = tenantDeviceSkus.Sum((SkuInfo sku) => sku.PrepaidUnits.Enabled + sku.PrepaidUnits.Warning);
		int count = tenant.ConcurrentDeviceUsers.Count;
		diagnostics.SendTraceTag("00009F5", Category.Operations, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"{count} out of {num} device users currently using the system."));
		if (count >= num)
		{
			throw tenant.Diagnostics.SendExceptionTag("00009F6", Category.Runtime, new NavLicenseViolationException(Lang.ConcurrentDeviceUserConnectionsLicenseViolation), Verbosity.Warning);
		}
	}

	private static string GenerateSelectEntitlementSetsQuery(NavSqlConnection connection, NavDatabase database)
	{
		StringBuilder stringBuilder = new StringBuilder("SELECT [Type],[ID],[Name],[Entitlement Set ID],[Is Evaluation] FROM {0}[Membership Entitlement]");
		stringBuilder.Append(NavSqlStatementHelper.GenerateSelectStatementHints(locking: false, noExpand: false, connection));
		StringBuilder stringBuilder2 = new StringBuilder();
		NavSqlStatementHelper.AppendDatabaseAndSchemaQualification(stringBuilder2, database, "dbo");
		return string.Format(CultureInfo.InvariantCulture, stringBuilder.ToString(), stringBuilder2);
	}

	private static void VerifyParametersNotNullOrWhiteSpace(ITreeObject session, byte[] entitlementsFile, string type, string id, string name, string entitlementSetId, string entitlementSetName)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (entitlementsFile == null || entitlementsFile.Length == 0)
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "entitlementsFile");
		}
		if (string.IsNullOrWhiteSpace(type))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "type");
		}
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "id");
		}
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "name");
		}
		if (string.IsNullOrWhiteSpace(entitlementSetId))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "entitlementSetId");
		}
		if (string.IsNullOrWhiteSpace(entitlementSetName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "entitlementSetName");
		}
	}

	private static NavOption GetNavType(string type, NavRecord membershipEntitlementRecord)
	{
		NCLOptionMetadata navOptionMetadata = membershipEntitlementRecord.GetFieldValue(1).NavOptionMetadata;
		int indexFromOption = navOptionMetadata.GetIndexFromOption(type);
		if (indexFromOption < 0 || indexFromOption > navOptionMetadata.Count)
		{
			throw NavNCLInvalidOptionStringException.Create(CultureInfo.CurrentCulture, type, navOptionMetadata.OptionString);
		}
		return NavOption.Create(navOptionMetadata, indexFromOption);
	}

	private static bool CheckIfMembershipEntitlementExists(NavRecord membershipEntitlementRecord, NavOption navType, NavText navId, NavCode navEntitlementSetId)
	{
		membershipEntitlementRecord.ALSetRange(1, navType, null);
		membershipEntitlementRecord.ALSetRange(2, navId, null);
		membershipEntitlementRecord.ALSetRange(4, navEntitlementSetId, null);
		return membershipEntitlementRecord.ALFindFirst(DataError.TrapError);
	}

	private static void DeleteAllEntitlements(ITreeObject session, NavCode entitlementSetId)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000191);
		navRecord.ALSetRange(1, entitlementSetId, null);
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			navRecord.ALDeleteAll(runTrigger: false);
		}
	}

	private static void DeleteEntitlementSet(ITreeObject session, NavCode entitlementSetId)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000190);
		navRecord.ALSetRange(1, entitlementSetId, null);
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			navRecord.Delete(DataError.ThrowError, runApplicationTrigger: false, isCalledFromUI: false, isBulkDelete: false);
		}
	}

	private static void InsertMembershipEntitlement(NavRecord membershipEntitlementRecord, NavOption navType, NavText navId, NavText navName, NavCode navEntitlementSetId, NavText navEntitlementSetName, NavBoolean navIsEvaluation)
	{
		membershipEntitlementRecord.ALInit();
		membershipEntitlementRecord.SetFieldValue(1, navType);
		membershipEntitlementRecord.SetFieldValue(2, navId);
		membershipEntitlementRecord.SetFieldValue(3, navName);
		membershipEntitlementRecord.SetFieldValue(4, navEntitlementSetId);
		membershipEntitlementRecord.SetFieldValue(5, navEntitlementSetName);
		membershipEntitlementRecord.SetFieldValue(6, navIsEvaluation);
		membershipEntitlementRecord.Insert(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false);
	}

	private static void InsertEntitlementSet(ITreeObject session, NavCode navEntitlementSetId, NavText navEntitlementSetName)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000190);
		navRecord.ALInit();
		navRecord.SetFieldValue(1, navEntitlementSetId);
		navRecord.SetFieldValue(2, navEntitlementSetName);
		navRecord.Insert(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false);
	}

	private static void InsertEntitlements(ITreeObject session, byte[] entitlementsFile, NavCode entitlementSetId, NavText entitlementSetName)
	{
		using MemoryStream stream = new MemoryStream(entitlementsFile, writable: false);
		using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 128, leaveOpen: true);
		string text = streamReader.ReadLine();
		if (text == null)
		{
			return;
		}
		if (!text.Equals("ObjectType,ObjectRangeStart,ObjectRangeEnd,Permission"))
		{
			throw NavEntitlementFileFormatException.CreateForWrongHeader("ObjectType,ObjectRangeStart,ObjectRangeEnd,Permission", text);
		}
		using NavRecord navRecord = new NavRecord(session, 2000000191);
		string line;
		while ((line = streamReader.ReadLine()) != null)
		{
			int[] entitlementValues = GetEntitlementValues(line, ',');
			NavOption objectType = GetObjectType(entitlementValues[0], navRecord);
			NavInteger navInteger = NavInteger.Create(entitlementValues[1]);
			NavInteger navInteger2 = NavInteger.Create(entitlementValues[2]);
			if ((int)navInteger > (int)navInteger2 || (int)navInteger < 0 || (navInteger == 0 && (int)navInteger2 > 0))
			{
				throw NavInvalidEntitlementObjectIdRangeException.Create(entitlementSetId, objectType.ToString(), navInteger, navInteger2);
			}
			ExtractPermissionOptionsFromFlags(entitlementValues[3], navRecord, out var readPermission, out var insertPermission, out var modifyPermission, out var deletePermission, out var executePermission);
			if (!(readPermission == 0) || !(insertPermission == 0) || !(modifyPermission == 0) || !(deletePermission == 0) || !(executePermission == 0))
			{
				navRecord.ALInit();
				navRecord.SetFieldValue(1, entitlementSetId);
				navRecord.SetFieldValue(2, entitlementSetName);
				navRecord.SetFieldValue(3, objectType);
				navRecord.SetFieldValue(4, navInteger);
				navRecord.SetFieldValue(5, navInteger2);
				navRecord.SetFieldValue(6, readPermission);
				navRecord.SetFieldValue(7, insertPermission);
				navRecord.SetFieldValue(8, modifyPermission);
				navRecord.SetFieldValue(9, deletePermission);
				navRecord.SetFieldValue(10, executePermission);
				navRecord.Insert(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false);
			}
		}
	}

	private static int[] GetEntitlementValues(string line, char separator)
	{
		string[] array = line.Split(separator);
		if (array.Length != 4)
		{
			throw NavEntitlementFileFormatException.CreateForWrongLine(line);
		}
		int[] array2 = new int[4];
		try
		{
			array2[0] = int.Parse(array[0], CultureInfo.InvariantCulture);
			array2[1] = int.Parse(array[1], CultureInfo.InvariantCulture);
			array2[2] = int.Parse(array[2], CultureInfo.InvariantCulture);
			array2[3] = int.Parse(array[3], CultureInfo.InvariantCulture);
			return array2;
		}
		catch (Exception ex) when (ex is FormatException || ex is OverflowException)
		{
			throw NavEntitlementFileFormatException.CreateForWrongLine(line, ex);
		}
	}

	private static NavOption GetObjectType(int type, NavRecord entitlementRecord)
	{
		NCLOptionMetadata navOptionMetadata = entitlementRecord.GetFieldValue(3).NavOptionMetadata;
		if (type < 0 || type > navOptionMetadata.Count)
		{
			throw NavNCLInvalidOptionStringException.Create(CultureInfo.CurrentCulture, type.ToString(CultureInfo.InvariantCulture), navOptionMetadata.OptionString);
		}
		return NavOption.Create(navOptionMetadata, type);
	}

	private static void ExtractPermissionOptionsFromFlags(int value, NavRecord entitlementRecord, out NavOption readPermission, out NavOption insertPermission, out NavOption modifyPermission, out NavOption deletePermission, out NavOption executePermission)
	{
		NCLOptionMetadata navOptionMetadata = entitlementRecord.GetFieldValue(6).NavOptionMetadata;
		readPermission = NavOption.Create(navOptionMetadata, ((value & 1) == 1) ? 1 : (((value & 0x20) == 32) ? 2 : 0));
		insertPermission = NavOption.Create(navOptionMetadata, ((value & 2) == 2) ? 1 : (((value & 0x40) == 64) ? 2 : 0));
		modifyPermission = NavOption.Create(navOptionMetadata, ((value & 4) == 4) ? 1 : (((value & 0x80) == 128) ? 2 : 0));
		deletePermission = NavOption.Create(navOptionMetadata, ((value & 8) == 8) ? 1 : (((value & 0x100) == 256) ? 2 : 0));
		executePermission = NavOption.Create(navOptionMetadata, ((value & 0x10) == 16) ? 1 : (((value & 0x200) == 512) ? 2 : 0));
	}

	private IEnumerable<SkuInfo> GetTenantDeviceSkus(LicenseDetails userLicenseDetails)
	{
		IEnumerable<SkuInfo> source = userLicenseDetails.SubscribedSkus.Where((SkuInfo sku) => sku.CapabilityStatus == "Enabled" || sku.CapabilityStatus == "Warning");
		IEnumerable<string> deviceServicePlans = (from e in ProdMembershipEntitlements
			where e.Type == MembershipEntitlementType.AzureADDevicePlan
			select e.Id).Distinct();
		return source.Where((SkuInfo sku) => sku.ServicePlans.Any((ServicePlanInfo plan) => deviceServicePlans.Any((string dsp) => dsp.Equals(plan.ServicePlanId.ToString(), StringComparison.OrdinalIgnoreCase))));
	}
}
