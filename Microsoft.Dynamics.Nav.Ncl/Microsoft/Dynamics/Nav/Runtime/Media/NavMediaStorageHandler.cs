using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaStorageHandler : INavMediaStorageHandler
{
	void INavMediaStorageHandler.CleanupExpiredFiles(NavTenant tenant)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenant.Database, TransactionType.Update);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		CompanyId? companyIdForSystemTables = tenant.Database.GetCompanyIdForSystemTables(2000000184);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "DELETE FROM [Tenant Media] WHERE {0}[Security Token] <> '' AND [Expiration Date] < GETUTCDATE()", companyIdForSystemTables.HasValue ? (NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(0) + " AND ") : string.Empty));
		if (companyIdForSystemTables.HasValue)
		{
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(navSqlCommand.Command.Parameters, 0), companyIdForSystemTables.Value);
		}
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	Uri INavMediaStorageHandler.GetDocumentUrl(Guid documentId)
	{
		NavUrlAccessibleMedia urlAccessibleMediaById = NavMediaLinkHelper.GetUrlAccessibleMediaById(documentId, retrieveContent: false);
		return ALSystemOperatingSystem.GetOpenDynamicFileHandlerUrl(urlAccessibleMediaById.SecurityToken);
	}

	Guid INavMediaStorageHandler.ImportWithUrlAccess(NavStream stream, string fileName, int minutesToExpire)
	{
		return NavMediaLinkHelper.ImportMediaObjectForUseInPublicLink(NavCurrentThread.Session, stream.InternalStream, fileName, DateTime.UtcNow.AddMinutes(minutesToExpire), triggerCleanup: true);
	}
}
