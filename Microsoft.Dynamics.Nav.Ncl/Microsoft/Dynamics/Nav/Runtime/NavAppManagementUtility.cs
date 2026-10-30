using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppManagementUtility
{
	public static Result<List<XDocument>> GetXDocumentsFromPackage(Stream appPackage, Func<NavAppPackageReader, IEnumerable<string>> sourceFinder)
	{
		NavAppPackageReader reader = NavAppPackageReader.Create(appPackage);
		IEnumerable<string> source = sourceFinder(reader);
		return Result.Ok<List<XDocument>>(source.Select((string path) =>
		{
			using Stream stream = reader.ReadFile(path);
			using Stream documentStream = (NeaStreamReader.IsSupported(stream) ? new NeaStreamReader(stream) : stream);
			return XDocument.Load(XmlHelper.SafeXmlReader(documentStream));
		}).ToList());
	}

	public static void DeleteRecordWhereAppId(NavTenant tenant, NavSqlConnectionScope conScope, AppId id, string tableName, int tableId)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF(SELECT OBJECTPROPERTY(OBJECT_ID(N'" + tableName + "'), N'IsTable')) = 1 DELETE FROM [" + tableName + "] WHERE {0}[App ID] = @0", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @1 AND " : string.Empty));
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)id).Value);
		if (tenant.Database.IsSharedTenantSchema)
		{
			navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(tableId));
		}
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}
}
