using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class WebServiceHelper
{
	private class ApiVersionComparer : IComparer<string>
	{
		public int Compare(string s1, string s2)
		{
			Version version = GetVersion(s1);
			Version version2 = GetVersion(s2);
			return version.CompareTo(version2);
		}

		private static Version GetVersion(string value)
		{
			if (value != "beta" && (value ?? string.Empty).StartsWith("v", StringComparison.Ordinal) && Version.TryParse(value.Substring(1), out var result))
			{
				return result;
			}
			return new Version();
		}
	}

	internal delegate string PublishedServiceNameFinder(NavObjectType objectType, int objectId, NavRecord record);

	internal delegate string ODataFilterConverter(NavRecord record);

	internal delegate string CompanyNameGenerator(string companyName);

	private static readonly Regex ApiVersionRegex = new Regex("^v[0-9]+[.][0-9]+$", RegexOptions.Compiled);

	internal static CompanyNameGenerator GenerateApiCompanySegment => ApiCompanyUrlSegmentGenerator;

	internal static PublishedServiceNameFinder FindApiServiceName => FindPublishedApiObject;

	internal static ODataFilterConverter FilterNotSupportedConverter => (NavRecord x) =>
	{
		throw new NavNotSupportedException("Filters are not supported for this client type.");
	};

	internal static PublishedServiceNameFinder FindWebServiceName => FindPublishedObject;

	internal static CompanyNameGenerator GenerateODataCompanySegment => ODataCompanyUrlSegmentGenerator;

	internal static ODataFilterConverter ODataV3FilterConverter => (NavRecord x) => ODataFilterGenerator.CreateDefault().ConvertRecordToODataV3Filter(x);

	internal static ODataFilterConverter ODataV4FilterConverter => (NavRecord x) => ODataFilterGenerator.CreateDefault().ConvertRecordToODataV4Filter(x);

	public static IEnumerable<ApiWebServiceEntry> GetApiWebServicesBaseApp()
	{
		HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		return ApiWebServiceEntryHelper.GetEntriesFromBaseApp(paths);
	}

	public static IEnumerable<ApiWebServiceEntry> GetApiWebServices(NavTenant tenant)
	{
		HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<ApiWebServiceEntry> list = new List<ApiWebServiceEntry>();
		list.AddRange(ApiWebServiceEntryHelper.GetEntriesFromBaseApp(paths));
		list.AddRange(ApiWebServiceEntryHelper.GetEntriesFromNavApp(tenant, paths));
		return list;
	}

	public static IEnumerable<ApiWebServiceEntry> GetApiWebServices(NavAppGroup group)
	{
		HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<ApiWebServiceEntry> list = new List<ApiWebServiceEntry>();
		list.AddRange(ApiWebServiceEntryHelper.GetEntriesFromBaseApp(paths));
		list.AddRange(ApiWebServiceEntryHelper.GetEntriesFromNavApp(group, paths));
		return list;
	}

	private static string FindPublishedApiObject(NavObjectType objectType, int objectId, NavRecord record)
	{
		string text = null;
		switch (record?.TableID)
		{
		case 2000000193:
			text = record.GetFieldValue(9).ToString();
			break;
		case 2000000097:
			text = record.GetFieldValue(4).ToString();
			break;
		}
		string text2 = ((!string.IsNullOrWhiteSpace(text)) ? text : GetApiWebServicesInstalled((ObjectType)objectType, objectId).FirstOrDefault()?.Name);
		if (string.IsNullOrWhiteSpace(text2))
		{
			throw new NavNCLGetUrlException(string.Format(CultureInfo.CurrentCulture, Lang.GetUrlObjectIdNotPublished, objectType.ToString(), objectId.ToString(CultureInfo.InvariantCulture)));
		}
		return text2;
	}

	public static IEnumerable<ApiWebServiceEntry> GetApiWebServicesInstalled()
	{
		NavTenantCollection tenants = NavEnvironment.Instance.Tenants;
		NavTenant tenant = (tenants.IsSingleTenancyMode ? tenants.DefaultTenant : tenants.GetTenantById(NavEnvironment.CurrentTenantId));
		return GetApiWebServicesInstalled(tenant);
	}

	public static IEnumerable<ApiWebServiceEntry> GetApiWebServicesInstalled(NavTenant tenant)
	{
		List<ApiWebServiceEntry> installedServices = ApiWebServiceCache.GetInstalledServices(tenant);
		if (installedServices != null)
		{
			return installedServices;
		}
		HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		installedServices = new List<ApiWebServiceEntry>();
		installedServices.AddRange(ApiWebServiceEntryHelper.GetEntriesFromBaseApp(paths));
		installedServices.AddRange(ApiWebServiceEntryHelper.GetEntriesFromNavApp(tenant, paths));
		ApiWebServiceCache.SetInstalledServices(tenant, installedServices);
		return installedServices;
	}

	public static IEnumerable<string> GetApiWebServiceApiVersions(string apiPublisher, string apiGroup, bool sort = false)
	{
		HashSet<string> hashSet = new HashSet<string>();
		IEnumerable<ApiWebServiceEntry> enumerable = from s in GetApiWebServicesInstalled()
			where s.Publisher.Equals(apiPublisher ?? string.Empty, StringComparison.OrdinalIgnoreCase) && s.Group.Equals(apiGroup ?? string.Empty, StringComparison.OrdinalIgnoreCase)
			select s;
		foreach (ApiWebServiceEntry item in enumerable)
		{
			hashSet.Add(item.Version);
		}
		if (sort)
		{
			return hashSet.OrderBy((string v) => v, new ApiVersionComparer());
		}
		return hashSet;
	}

	public static void GetApiInformationFromRoute(string route, out string apiPublisher, out string apiGroup, out string apiVersion)
	{
		apiVersion = string.Empty;
		apiPublisher = string.Empty;
		apiGroup = string.Empty;
		if (string.IsNullOrEmpty(route))
		{
			throw new ArgumentNullException("route");
		}
		string[] array = route.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		apiVersion = array[array.Length - 1];
		if (array.Length != 1)
		{
			apiGroup = array[array.Length - 2];
			apiPublisher = array[array.Length - 3];
		}
	}

	public static string GetApiWebServiceLatestVersionPath(ObjectType objectType, int objectId)
	{
		ApiWebServiceEntry latestEntryVersion = ApiWebServiceEntryHelper.GetLatestEntryVersion(objectType, objectId, new ApiVersionComparer());
		return (latestEntryVersion != null) ? GetApiPath(latestEntryVersion.Publisher, latestEntryVersion.Group, latestEntryVersion.Version) : null;
	}

	internal static string GetApiPath(string apiPublisher, string apiGroup, string apiVersion)
	{
		return GetApiPath(apiPublisher, apiGroup, apiVersion, string.Empty);
	}

	internal static string GetApiPath(string apiPublisher, string apiGroup, string apiVersion, string entitySetName)
	{
		string text = ((string.IsNullOrEmpty(apiPublisher) || string.IsNullOrEmpty(apiGroup)) ? apiVersion : string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", apiPublisher, apiGroup, apiVersion));
		if (!string.IsNullOrWhiteSpace(entitySetName))
		{
			text = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", text, entitySetName);
		}
		return text;
	}

	internal static ReadOnlyCollection<string> ParseApiVersion(string apiVersionRaw, string name, int objectId, string apiPublisher, string apiGroup)
	{
		List<string> list = new List<string>();
		if (apiVersionRaw != null)
		{
			string[] array = apiVersionRaw.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				if (IsValidApiVersion(text))
				{
					list.Add(text);
					continue;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000019K", Category.Api, Verbosity.Warning, PrivacyClassification.SystemMetadata, "APIVersion {0} is not correct for {1} {2} {3}/{4}. The APIVersion should be beta or vX.Y format.", text, name, objectId, apiPublisher, apiGroup);
			}
		}
		return list.AsReadOnly();
	}

	internal static IEnumerable<ApiWebServiceEntry> GetApiWebServicesInstalled(ObjectType objectType, int objectId)
	{
		return from s in GetApiWebServicesInstalled()
			where s.ObjectType == objectType && s.ObjectId == objectId
			select s;
	}

	internal static bool IsValidApiVersion(string apiVersion)
	{
		if (apiVersion != null)
		{
			if (!apiVersion.Equals("beta"))
			{
				return ApiVersionRegex.IsMatch(apiVersion);
			}
			return true;
		}
		return false;
	}

	private static string ApiCompanyUrlSegmentGenerator(string companyName)
	{
		using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000006))
		{
			if (navRecord.ALGet(DataError.TrapError, NavText.Create(companyName)))
			{
				return "/companies(" + navRecord.GetFieldValue(8000).ToGuid().ToString() + ")";
			}
		}
		throw new NavNCLArgumentException("companyName");
	}

	internal static string GetServiceNameForPageId(NavSession session, int pageToExport)
	{
		return session.MetadataProvider.GetMasterPageWithoutConfiguration(pageToExport).Caption;
	}

	internal static void PublishTenantWebService(NavSession session, string serviceName, int pageToExport, SecurityFiltering securityFilter)
	{
		session.TransactionScope(() =>
		{
			using NavRecord navRecord = new NavRecord(session, 2000000168, securityFilter);
			navRecord.ALSetFilter(9, NavText.Create(serviceName));
			if (navRecord.ALCount == 0)
			{
				navRecord.ALInit();
				navRecord.ALValidate(3, NavInteger.Create(8), null);
				navRecord.ALValidate(6, NavInteger.Create(pageToExport), null);
				navRecord.ALValidate(9, NavText.Create(serviceName), null);
				navRecord.ALValidate(12, NavBoolean.Create(value: true), null);
				navRecord.ALInsert(runApplicationTrigger: true);
			}
		});
	}

	private static string FindPublishedObject(NavObjectType objectType, int objectId, NavRecord record)
	{
		string text = FindPublishedServiceName(serviceName: record?.TableID switch
		{
			2000000076 => record.GetFieldValue(9).ToString(), 
			2000000168 => record.GetFieldValue(9).ToString(), 
			_ => string.Empty, 
		}, objectType: objectType, objectId: objectId);
		if (string.IsNullOrWhiteSpace(text))
		{
			throw new NavNCLGetUrlException(string.Format(CultureInfo.CurrentCulture, Lang.GetUrlObjectIdNotPublished, objectType.ToString(), objectId.ToString(CultureInfo.InvariantCulture)));
		}
		return text;
	}

	private static string FindPublishedServiceName(NavObjectType objectType, int objectId, string serviceName)
	{
		string result = null;
		if (NavCurrentThread.Session == null)
		{
			return null;
		}
		if (!string.IsNullOrWhiteSpace(serviceName))
		{
			WebServiceSystemTenantView webServiceSystemTenantView = WebServiceSystemTenantView.Create(objectType, serviceName);
			if (webServiceSystemTenantView.Count != 0)
			{
				result = webServiceSystemTenantView.FirstOrDefault((IWebServiceRecord item) => item.ServiceName == serviceName && item.ObjectId == objectId && item.Published)?.ServiceName;
			}
		}
		else
		{
			WebServiceSystemTenantView webServiceSystemTenantView2 = WebServiceSystemTenantView.Create(objectType, objectId);
			if (webServiceSystemTenantView2.Count != 0)
			{
				result = webServiceSystemTenantView2.FirstOrDefault((IWebServiceRecord item) => item.Published)?.ServiceName;
			}
		}
		return result;
	}

	private static string ODataCompanyUrlSegmentGenerator(string companyName)
	{
		companyName = Uri.EscapeDataString(companyName);
		if (!companyName.StartsWith("'", StringComparison.Ordinal) && !companyName.EndsWith("'", StringComparison.Ordinal))
		{
			companyName = "'" + companyName + "'";
		}
		return "/Company(" + companyName + ")";
	}
}
