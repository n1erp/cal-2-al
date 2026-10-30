using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class FeatureSwitches
{
	private static readonly Lazy<FeatureSwitches> instance = new Lazy<FeatureSwitches>(() => new FeatureSwitches(), LazyThreadSafetyMode.PublicationOnly);

	private readonly Dictionary<string, bool> featureSwitchValues;

	private static readonly string[] Keys = new string[34]
	{
		"disableAppDomainMonitoring", "disableAppIdEntitlementsCheck", "disableCacheMemoryFix", "disableCacheMetrics", "disableCompanionTableJoinOptimization", "disableDatabaseLocksTelemetry", "disableDropStatisticsOnSync", "disableExtensionLicenseCount", "disableFilePathLockdown", "disableKeepAliveOnAppDB",
		"disableOfficeEntitlements", "disablePageBackgroundTasksChildSessionAsync", "disableRecyclableMemoryStream", "disableReportSettingsEdit", "disableRetrieveOfSqlServerSessionId", "disableSmartListActions", "disableTableConnectionPerCompany", "disableTenantParallelMounting", "disableTenantStateCheckOnSessionCreate", "enableAppsOnMultitenantSharedDB",
		"enableCompactSerialization", "enableConcurrentDeviceUserAuth", "enableDirectoryACLinAzure", "enableExternalTaskScheduler", "enableSecurityTracing", "enableSendAllTagsToIfx", "enableStreamingReportDataSet", "enableTestCustomerExtensionLicenseCount", "enableWaitOnInitializeTenant", "entitlementByAadPresenceOnly",
		"reportUnhealthyStateForTenantConfigMismatch", "sharedDB", "terminateForTenantConfigMismatch", "testDummySwitch"
	};

	private readonly bool disableAppDomainMonitoring;

	private readonly bool disableAppIdEntitlementsCheck;

	private readonly bool disableCacheMemoryFix;

	private readonly bool disableCacheMetrics;

	private readonly bool disableCompanionTableJoinOptimization;

	private readonly bool disableDatabaseLocksTelemetry;

	private readonly bool disableDropStatisticsOnSync;

	private readonly bool disableExtensionLicenseCount;

	private readonly bool disableFilePathLockdown;

	private readonly bool disableKeepAliveOnAppDB;

	private readonly bool disableOfficeEntitlements;

	private readonly bool disablePageBackgroundTasksChildSessionAsync;

	private readonly bool disableRecyclableMemoryStream;

	private readonly bool disableReportSettingsEdit;

	private readonly bool disableRetrieveOfSqlServerSessionId;

	private readonly bool disableSmartListActions;

	private readonly bool disableTableConnectionPerCompany;

	private readonly bool disableTenantParallelMounting;

	private readonly bool disableTenantStateCheckOnSessionCreate;

	private readonly bool enableAppsOnMultitenantSharedDB;

	private readonly bool enableCompactSerialization;

	private readonly bool enableConcurrentDeviceUserAuth;

	private readonly bool enableDirectoryACLinAzure;

	private readonly bool enableExternalTaskScheduler;

	private readonly bool enableSecurityTracing;

	private readonly bool enableSendAllTagsToIfx;

	private readonly bool enableStreamingReportDataSet;

	private readonly bool enableTestCustomerExtensionLicenseCount;

	private readonly bool enableWaitOnInitializeTenant;

	private readonly bool entitlementByAadPresenceOnly;

	private readonly bool reportUnhealthyStateForTenantConfigMismatch;

	private readonly bool sharedDB;

	private readonly bool terminateForTenantConfigMismatch;

	private readonly bool testDummySwitch;

	internal IReadOnlyDictionary<string, bool> RawValues => featureSwitchValues;

	public static FeatureSwitches Instance => instance.Value;

	public bool DisableAppDomainMonitoring => disableAppDomainMonitoring;

	public bool DisableAppIdEntitlementsCheck => disableAppIdEntitlementsCheck;

	public bool DisableCacheMemoryFix => disableCacheMemoryFix;

	public bool DisableCacheMetrics => disableCacheMetrics;

	public bool DisableCompanionTableJoinOptimization => disableCompanionTableJoinOptimization;

	public bool DisableDatabaseLocksTelemetry => disableDatabaseLocksTelemetry;

	public bool DisableDropStatisticsOnSync => disableDropStatisticsOnSync;

	public bool DisableExtensionLicenseCount => disableExtensionLicenseCount;

	public bool DisableFilePathLockdown => disableFilePathLockdown;

	public bool DisableKeepAliveOnAppDB => disableKeepAliveOnAppDB;

	public bool DisableOfficeEntitlements => disableOfficeEntitlements;

	public bool DisablePageBackgroundTasksChildSessionAsync => disablePageBackgroundTasksChildSessionAsync;

	public bool DisableRecyclableMemoryStream => disableRecyclableMemoryStream;

	public bool DisableReportSettingsEdit => disableReportSettingsEdit;

	public bool DisableRetrieveOfSqlServerSessionId => disableRetrieveOfSqlServerSessionId;

	public bool DisableSmartListActions => disableSmartListActions;

	public bool DisableTableConnectionPerCompany => disableTableConnectionPerCompany;

	public bool DisableTenantParallelMounting => disableTenantParallelMounting;

	public bool DisableTenantStateCheckOnSessionCreate => disableTenantStateCheckOnSessionCreate;

	public bool EnableAppsOnMultitenantSharedDB => enableAppsOnMultitenantSharedDB;

	public bool EnableCompactSerialization => enableCompactSerialization;

	public bool EnableConcurrentDeviceUserAuth => enableConcurrentDeviceUserAuth;

	public bool EnableDirectoryACLinAzure => enableDirectoryACLinAzure;

	public bool EnableExternalTaskScheduler => enableExternalTaskScheduler;

	public bool EnableSecurityTracing => enableSecurityTracing;

	public bool EnableSendAllTagsToIfx => enableSendAllTagsToIfx;

	public bool EnableStreamingReportDataSet => enableStreamingReportDataSet;

	public bool EnableTestCustomerExtensionLicenseCount => enableTestCustomerExtensionLicenseCount;

	public bool EnableWaitOnInitializeTenant => enableWaitOnInitializeTenant;

	public bool EntitlementByAadPresenceOnly => entitlementByAadPresenceOnly;

	public bool ReportUnhealthyStateForTenantConfigMismatch => reportUnhealthyStateForTenantConfigMismatch;

	public bool SharedDB => sharedDB;

	public bool TerminateForTenantConfigMismatch => terminateForTenantConfigMismatch;

	public bool TestDummySwitch => testDummySwitch;

	private FeatureSwitches()
		: this(Keys)
	{
	}

	internal FeatureSwitches(IEnumerable<string> keys)
		: this(keys, FeatureSwitchOverrideProvider.OverrideValues)
	{
	}

	internal FeatureSwitches(IEnumerable<string> keys, IEnumerable<KeyValuePair<string, bool>> values)
		: this(MergeDictionaryWithKeys(values, keys))
	{
	}

	private static Dictionary<string, TValue> MergeDictionaryWithKeys<TValue>(IEnumerable<KeyValuePair<string, TValue>> dictionary, IEnumerable<string> keys)
	{
		Dictionary<string, TValue> dictionary2 = new Dictionary<string, TValue>();
		foreach (string key in keys)
		{
			dictionary2[key.ToUpperInvariant()] = default;
		}
		if (dictionary != null)
		{
			foreach (KeyValuePair<string, TValue> item in dictionary)
			{
				dictionary2[item.Key.ToUpperInvariant()] = item.Value;
			}
		}
		return dictionary2;
	}

	private FeatureSwitches(IDictionary<string, bool> switches)
	{
		featureSwitchValues = new Dictionary<string, bool>(switches);
		switches.TryGetValue("disableAppDomainMonitoring".ToUpperInvariant(), out disableAppDomainMonitoring);
		switches.TryGetValue("disableAppIdEntitlementsCheck".ToUpperInvariant(), out disableAppIdEntitlementsCheck);
		switches.TryGetValue("disableCacheMemoryFix".ToUpperInvariant(), out disableCacheMemoryFix);
		switches.TryGetValue("disableCacheMetrics".ToUpperInvariant(), out disableCacheMetrics);
		switches.TryGetValue("disableCompanionTableJoinOptimization".ToUpperInvariant(), out disableCompanionTableJoinOptimization);
		switches.TryGetValue("disableDatabaseLocksTelemetry".ToUpperInvariant(), out disableDatabaseLocksTelemetry);
		switches.TryGetValue("disableDropStatisticsOnSync".ToUpperInvariant(), out disableDropStatisticsOnSync);
		switches.TryGetValue("disableExtensionLicenseCount".ToUpperInvariant(), out disableExtensionLicenseCount);
		switches.TryGetValue("disableFilePathLockdown".ToUpperInvariant(), out disableFilePathLockdown);
		switches.TryGetValue("disableKeepAliveOnAppDB".ToUpperInvariant(), out disableKeepAliveOnAppDB);
		switches.TryGetValue("disableOfficeEntitlements".ToUpperInvariant(), out disableOfficeEntitlements);
		switches.TryGetValue("disablePageBackgroundTasksChildSessionAsync".ToUpperInvariant(), out disablePageBackgroundTasksChildSessionAsync);
		switches.TryGetValue("disableRecyclableMemoryStream".ToUpperInvariant(), out disableRecyclableMemoryStream);
		switches.TryGetValue("disableReportSettingsEdit".ToUpperInvariant(), out disableReportSettingsEdit);
		switches.TryGetValue("disableRetrieveOfSqlServerSessionId".ToUpperInvariant(), out disableRetrieveOfSqlServerSessionId);
		switches.TryGetValue("disableSmartListActions".ToUpperInvariant(), out disableSmartListActions);
		switches.TryGetValue("disableTableConnectionPerCompany".ToUpperInvariant(), out disableTableConnectionPerCompany);
		switches.TryGetValue("disableTenantParallelMounting".ToUpperInvariant(), out disableTenantParallelMounting);
		switches.TryGetValue("disableTenantStateCheckOnSessionCreate".ToUpperInvariant(), out disableTenantStateCheckOnSessionCreate);
		switches.TryGetValue("enableAppsOnMultitenantSharedDB".ToUpperInvariant(), out enableAppsOnMultitenantSharedDB);
		switches.TryGetValue("enableCompactSerialization".ToUpperInvariant(), out enableCompactSerialization);
		switches.TryGetValue("enableConcurrentDeviceUserAuth".ToUpperInvariant(), out enableConcurrentDeviceUserAuth);
		switches.TryGetValue("enableDirectoryACLinAzure".ToUpperInvariant(), out enableDirectoryACLinAzure);
		switches.TryGetValue("enableExternalTaskScheduler".ToUpperInvariant(), out enableExternalTaskScheduler);
		switches.TryGetValue("enableSecurityTracing".ToUpperInvariant(), out enableSecurityTracing);
		switches.TryGetValue("enableSendAllTagsToIfx".ToUpperInvariant(), out enableSendAllTagsToIfx);
		switches.TryGetValue("enableStreamingReportDataSet".ToUpperInvariant(), out enableStreamingReportDataSet);
		switches.TryGetValue("enableTestCustomerExtensionLicenseCount".ToUpperInvariant(), out enableTestCustomerExtensionLicenseCount);
		switches.TryGetValue("enableWaitOnInitializeTenant".ToUpperInvariant(), out enableWaitOnInitializeTenant);
		switches.TryGetValue("entitlementByAadPresenceOnly".ToUpperInvariant(), out entitlementByAadPresenceOnly);
		switches.TryGetValue("reportUnhealthyStateForTenantConfigMismatch".ToUpperInvariant(), out reportUnhealthyStateForTenantConfigMismatch);
		switches.TryGetValue("sharedDB".ToUpperInvariant(), out sharedDB);
		switches.TryGetValue("terminateForTenantConfigMismatch".ToUpperInvariant(), out terminateForTenantConfigMismatch);
		switches.TryGetValue("testDummySwitch".ToUpperInvariant(), out testDummySwitch);
	}
}
