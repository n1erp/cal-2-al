using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public static class SystemObjects
{
	public const int FileImportBinary = 1310;

	public const int FileImportText = 1320;

	public const int FileExportBinary = 1330;

	public const int FileExportText = 1340;

	public const int RunTable = 1350;

	public const int FileDatabaseTest = 1530;

	public const int FileDatabaseExpand = 1540;

	public const int FileDatabaseDelete = 1550;

	public const int FileDatabaseInfo = 1570;

	public const int FileDatabaseOptions = 1580;

	public const int FileCompanyNew = 1610;

	public const int FileCompanyRename = 1630;

	public const int FileCompanyDelete = 1640;

	public const int FileForceUnlock = 1650;

	public const int EditFind = 2510;

	public const int EditReplace = 2520;

	public const int ViewTableFilter = 3220;

	public const int ViewFlowFilter = 3230;

	public const int ViewSort = 3410;

	public const int ViewDesign = 3510;

	public const int ToolsObjectDesigner = 5210;

	public const int ToolsDebugger = 5310;

	public const int ToolsCoverage = 5315;

	public const int ToolsClientMonitor = 5320;

	public const int ToolsZoom = 5330;

	public const int ToolsBackup = 5410;

	public const int ToolsRestore = 5420;

	public const int ToolsClearOldVersions = 5510;

	public const int ToolsRenumber = 5620;

	public const int ToolsCrossReference = 5630;

	public const int ToolsTranslate = 5710;

	public const int ToolsSecurityRoles = 5810;

	public const int ToolsSecurityNavisionLogins = 5820;

	public const int ToolsSecurityWindowsLogins = 5821;

	public const int ToolsSecurityPassword = 5830;

	public const int ToolsLicenseInfo = 5910;

	public const int PerDatabaseLicense = 6300;

	public const int DesignTableBase = 9010;

	public const int DesignTableAdvanced = 9015;

	public const int DesignPageBase = 9020;

	public const int DesignPageAdvanced = 9025;

	public const int DesignReportBase = 9030;

	public const int DesignReportAdvanced = 9035;

	public const int DesignDataportBase = 9040;

	public const int DesignDataportAdvanced = 9045;

	public const int DesignCodeunitBase = 9050;

	public const int DesignCodeunitAdvanced = 9055;

	public const int DesignXmlPortBase = 9060;

	public const int DesignXmlPortAdvanced = 9065;

	public const int DesignMenuSuiteBase = 9070;

	public const int DesignMenuSuiteAdvanced = 9075;

	public const int DesignQueryBase = 9090;

	public const int DesignQueryAdvanced = 9095;

	public const int AllowDynamicsNavServer = 9100;

	public const int TestPartnerIntegration = 9500;

	private static readonly Dictionary<int, string> systemObjectAndResourceKeys = (from f in typeof(SystemObjects).GetFields()
		where f.IsLiteral && f.FieldType == typeof(int)
		select f).ToDictionary((FieldInfo f) => (int)f.GetValue(null), (FieldInfo f) => f.Name + "SystemObjectCaption");

	public static SortedList<int, string> CreateSystemObjectAndNamesList()
	{
		return new SortedList<int, string>(systemObjectAndResourceKeys.ToDictionary((KeyValuePair<int, string> p) => p.Key, (KeyValuePair<int, string> p) => Lang.ResourceManager.GetString(p.Value, CultureInfo.InvariantCulture)));
	}

	public static string GetSystemObjectCaption(int systemObjectNumber)
	{
		if (!systemObjectAndResourceKeys.TryGetValue(systemObjectNumber, out var value))
		{
			throw new NavMetadataNotFoundException(ObjectType.System, systemObjectNumber);
		}
		return Lang.ResourceManager.GetString(value);
	}

	public static bool TryGetSystemObjectCaption(int systemObjectNumber, out string caption)
	{
		caption = string.Empty;
		if (!systemObjectAndResourceKeys.TryGetValue(systemObjectNumber, out var value))
		{
			return false;
		}
		caption = Lang.ResourceManager.GetString(value);
		return true;
	}
}
