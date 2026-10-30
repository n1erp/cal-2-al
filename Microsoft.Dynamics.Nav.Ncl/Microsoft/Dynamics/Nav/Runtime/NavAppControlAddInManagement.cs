using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.AppXmlObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppControlAddInManagement
{
	public static void AddControlAddIns(NavTenant tenant, NavSqlConnectionScope conScope, Stream packageStream, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074R", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Adding tenant control addins -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		IList<ControlAddInEntry> list = ExtractAddIns(packageStream);
		InsertAddins(tenant, conScope, list, appId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074S", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Added tenant control addins -- Tenant:{0}; AppId:{1}; AddIns:{2}; Elapsed:{3}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, list.Count, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public static void RemoveControlAddins(NavTenant tenant, NavSqlConnectionScope conScope, AppId appId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074T", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing tenant control addins -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		NavAppManagementUtility.DeleteRecordWhereAppId(tenant, conScope, appId, "NAV App Tenant Add-In", 2000000169);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074U", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removed tenant control addins -- Tenant:{0}; AppId:{1}; Elapsed:{2}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static void InsertAddins(NavTenant tenant, NavSqlConnectionScope conScope, IEnumerable<ControlAddInEntry> addIns, AppId appId)
	{
		foreach (ControlAddInEntry addIn in addIns)
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "INSERT INTO [NAV App Tenant Add-In] ([App ID], [Add-In Name], [Public Key Token], [Version], [Category], [Description], [Resource]{0}) VALUES (@0, @1, @2, @3, @4, @5, @6{1})", tenant.Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, tenant.Database.IsSharedTenantSchema ? ", @7" : string.Empty));
			navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)appId).Value);
			navSqlCommand.Command.Parameters.Add(1, SqlDbType.NVarChar, 220, addIn.Name);
			navSqlCommand.Command.Parameters.Add(2, SqlDbType.NVarChar, 20, addIn.Token);
			navSqlCommand.Command.Parameters.Add(3, SqlDbType.NVarChar, 25, addIn.Version);
			navSqlCommand.Command.Parameters.Add(4, SqlDbType.Int, addIn.Category);
			navSqlCommand.Command.Parameters.Add(5, SqlDbType.NVarChar, 250, string.Empty);
			navSqlCommand.Command.Parameters.Add(6, SqlDbType.Binary, int.MaxValue, new NavBLOB(addIn.Resource).GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true));
			if (tenant.Database.IsSharedTenantSchema)
			{
				navSqlCommand.Command.Parameters.Add(7, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000169));
			}
			navSqlCommand.PrepareStatement();
			navSqlCommand.ExecuteNonQuery();
		}
	}

	private static IList<ControlAddInEntry> ExtractAddIns(Stream packageStream)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected Obj, but got Unknown
		Dictionary<string, byte[]> resources = new Dictionary<string, byte[]>();
		ControlAddInDocket val = new ControlAddInDocket();
		NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(packageStream);
		IEnumerable<string> enumerable = navAppPackageReader.ReadControlAddinPaths();
		foreach (string item in enumerable)
		{
			using Stream stream = navAppPackageReader.ReadFile(item);
			using Stream stream2 = (NeaStreamReader.IsSupported(stream) ? new NeaStreamReader(stream, leaveOpen: true) : stream);
			if (item.EndsWith(".dock", StringComparison.OrdinalIgnoreCase))
			{
				using XmlReader reader = XmlHelper.SafeXmlReader(stream2);
				val = ControlAddInDocket.FromXml((XContainer)XDocument.Load(reader).Root);
			}
			else
			{
				byte[] array = new byte[stream2.Length];
				stream2.Read(array, 0, array.Length);
				resources.Add(Path.GetFileName(item), array);
			}
		}
		if (val.ControlAddIns.Any())
		{
			val.ControlAddIns.ForEach((ControlAddInEntry c) =>
			{
				c.Resource = resources[c.FileName];
			});
		}
		return val.ControlAddIns.ToList();
	}
}
