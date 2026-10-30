using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.AppXmlObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppReportLayoutManagement
{
	public const int CustomReportLayoutTableId = 9650;

	public static void AddCustomReportLayouts(NavTenant tenant, Stream packageStream, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000758", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Adding report layouts -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<DataTable> list = ExtractLayouts(packageStream);
		if (list.Any())
		{
			InsertLayouts(list, appId);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000759", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Added report layouts -- Tenant:{0}; Layouts:{1}; Elapsed:{2}", tenant.Id, list.Count, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public static void RemoveCustomReportLayouts(NavTenant tenant, NavSqlConnectionScope conScope, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000075A", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing report layouts -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		NavAppManagementUtility.DeleteRecordWhereAppId(tenant, conScope, appId, "Custom Report Layout", 9650);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000075B", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removed report layouts -- Tenant:{0}; AppId:{1}; Elapsed:{2}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static void InsertLayouts(IEnumerable<DataTable> layouts, AppId appId)
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 9650 }))
		{
			using NavRecord obj = NavCurrentThread.Session.GetGlobalRecordInstance(9650);
			foreach (DataTable layout in layouts)
			{
				NavApplicationMethod.InvokeMethod(obj, "NewExtensionLayout", new object[2]
				{
					((WrappedValue<Guid>)(object)appId).Value,
					layout
				});
			}
		}
	}

	private static List<DataTable> ExtractLayouts(Stream packageStream)
	{
		List<DataTable> layouts = new List<DataTable>();
		ResultExtensions.OnSuccess<List<XDocument>>(NavAppManagementUtility.GetXDocumentsFromPackage(packageStream, (NavAppPackageReader r) => r.ReadCustomReportLayoutPaths()), (Action<List<XDocument>>)((List<XDocument> documents) =>
		{
			documents.ForEach((XDocument doc) =>
			{
				layouts.AddRange(NavAppCustomReportLayoutCollection.FromXml(doc));
			});
		}));
		return layouts;
	}
}
