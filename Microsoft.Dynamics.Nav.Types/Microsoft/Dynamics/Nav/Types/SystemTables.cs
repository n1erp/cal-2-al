using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types;

public static class SystemTables
{
	public const int Object = 2000000001;

	public const int PermissionSet = 2000000004;

	public const int Permission = 2000000005;

	public const int Company = 2000000006;

	public const int Date = 2000000007;

	public const int Session = 2000000009;

	public const int Drive = 2000000020;

	public const int File = 2000000022;

	public const int Monitor = 2000000024;

	public const int Integer = 2000000026;

	public const int TableInformation = 2000000028;

	public const int SystemObject = 2000000029;

	public const int AllObj = 2000000038;

	public const int Printer = 2000000039;

	public const int LicenseInformation = 2000000040;

	public const int Field = 2000000041;

	public const int OleControl = 2000000042;

	public const int LicensePermission = 2000000043;

	public const int PermissionRange = 2000000044;

	public const int WindowsLanguage = 2000000045;

	public const int AutomationServer = 2000000046;

	public const int Server = 2000000047;

	public const int Database = 2000000048;

	public const int CodeCoverage = 2000000049;

	public const int AccessControl = 2000000053;

	public const int SidAccountId = 2000000055;

	public const int AllObjWithCaption = 2000000058;

	public const int Key = 2000000063;

	public const int SendToProgram = 2000000065;

	public const int StyleSheet = 2000000066;

	public const int RecordLink = 2000000068;

	public const int AddIn = 2000000069;

	public const int ErrorList = 2000000070;

	public const int ObjectMetadata = 2000000071;

	public const int ProfileTable = 2000000072;

	public const int UserPersonalization = 2000000073;

	public const int ProfileMetadata = 2000000074;

	public const int UserMetadata = 2000000075;

	public const int WebService = 2000000076;

	public const int Assembly = 2000000077;

	public const int Chart = 2000000078;

	public const int ObjectTracking = 2000000079;

	public const int PageDataPersonalization = 2000000080;

	public const int UpgradeBlobStorage = 2000000081;

	public const int ReportLayout = 2000000082;

	public const int ImageLibrary = 2000000090;

	public const int ApiWebhookSubscription = 2000000095;

	public const int ApiWebhookNotification = 2000000096;

	public const int ApiWebhookEntity = 2000000097;

	public const int ApiWebhookNotificationAggregate = 2000000098;

	public const int DebuggerBreakpoint = 2000000100;

	public const int DebuggerCallStack = 2000000101;

	public const int DebuggerVariable = 2000000102;

	public const int DebuggerWatchValue = 2000000103;

	public const int DebuggerWatch = 2000000104;

	public const int IsolatedStorage = 2000000107;

	public const int ActiveSession = 2000000110;

	public const int SessionEvent = 2000000111;

	public const int ServerInstance = 2000000112;

	public const int DocumentService = 2000000114;

	public const int User = 2000000120;

	public const int UserProperty = 2000000121;

	public const int Device = 2000000130;

	public const int ObjectMetadataSnapshot = 2000001071;

	public const int TableSynchSetup = 2000000135;

	public const int TableMetadata = 2000000136;

	public const int CodeunitMetadata = 2000000137;

	public const int PageMetadata = 2000000138;

	public const int ReportMetadata = 2000000139;

	public const int EventSubscription = 2000000140;

	public const int TableRelationMetadata = 2000000141;

	public const int QueryMetadata = 2000000142;

	public const int PageAction = 2000000143;

	public const int PowerBIBlob = 2000000144;

	public const int PowerBIDefaultSelection = 2000000145;

	public const int IntelligentCloud = 2000000146;

	public const int NavAppObjectMetadata = 2000000150;

	public const int NavAppTenantApp = 2000000151;

	public const int NavAppDataArchive = 2000000152;

	public const int NavAppInstalledApp = 2000000153;

	public const int DatabaseLocks = 2000000154;

	public const int NavAppPublishReference = 2000000155;

	public const int NavAppPublishedApp = 2000000156;

	public const int NavAppExtra = 2000000157;

	public const int DataSensitivity = 2000000159;

	public const int NavApp = 2000000160;

	public const int NavAppDependencies = 2000000161;

	public const int NavAppCapabilities = 2000000162;

	public const int NavAppObjectPrerequisites = 2000000163;

	public const int TimeZone = 2000000164;

	public const int TenantPermissionSet = 2000000165;

	public const int TenantPermission = 2000000166;

	public const int AggregatePermissionSet = 2000000167;

	public const int TenantWebService = 2000000168;

	public const int NavAppTenantAddIn = 2000000169;

	public const int OldFinishDesignSaveMode = 2000000173;

	public const int NavAppResource = 2000000176;

	public const int TenantProfile = 2000000177;

	public const int AllProfile = 2000000178;

	public const int MediaSet = 2000000180;

	public const int Media = 2000000181;

	public const int MediaResources = 2000000182;

	public const int MediaSetTenant = 2000000183;

	public const int MediaTenant = 2000000184;

	public const int MediaThumbnailsTenant = 2000000185;

	public const int ProfilePageMetadata = 2000000186;

	public const int TenantProfilePageMetadata = 2000000187;

	public const int UserPageMetadata = 2000000188;

	public const int ConfigurationPackageFile = 2000000170;

	public const int PageTableField = 2000000171;

	public const int FieldTypeDesign = 2000000172;

	public const int IntelligentCloudStatus = 2000000173;

	public const int ScheduledTask = 2000000175;

	public const int ODataEdmType = 2000000179;

	public const int TenantLicenseState = 2000000189;

	public const int EntitlementSet = 2000000190;

	public const int Entitlement = 2000000191;

	public const int ApiWebService = 2000000193;

	public const int WebhookNotification = 2000000194;

	public const int MembershipEntitlement = 2000000195;

	public const int ObjectOptions = 2000000196;

	public const int TokenCache = 2000000197;

	public const int PageDocumentation = 2000000198;

	public const int WebhookSubscription = 2000000199;

	public const int PageControlField = 2000000192;

	public const int NavAppTenantOperation = 2000000200;

	public const int NavAppSetting = 2000000201;

	public const int AllControlFields = 2000000202;

	public const int ReportDataItems = 2000000203;

	public const int PageInfoAndFields = 2000000204;

	private static readonly HashSet<int> applicationDatabaseTables = new HashSet<int>(new int[36]
	{
		2000000001, 2000000071, 2000000069, 2000000004, 2000000005, 2000000112, 2000000100, 2000000104, 2000000065, 2000000066,
		2000000072, 2000000074, 2000000186, 2000000076, 2000000081, 2000000082, 2000000078, 2000000150, 2000000151, 2000000160,
		2000000161, 2000000162, 2000000163, 2000000181, 2000000180, 2000000182, 2000000170, 2000000190, 2000000191, 2000000195,
		2000000198, 2000000179, 2000000176, 2000000155, 2000000144, 2000000145
	});

	private static readonly Lazy<HashSet<int>> virtualTables = new Lazy<HashSet<int>>(() => new HashSet<int>(BuildVirtualTableList()));

	private static readonly HashSet<int> tenantDatabaseCrossTenantTables = new HashSet<int>(new int[1] { 2000001071 });

	private static readonly HashSet<int> internalTables = new HashSet<int>(new int[59]
	{
		2000000170, 2000000159, 2000000100, 2000000104, 2000000130, 2000000114, 2000000190, 2000000191, 2000000107, 2000000180,
		2000000181, 2000000195, 2000000162, 2000000152, 2000000161, 2000000150, 2000000163, 2000000176, 2000000151, 2000000160,
		2000000071, 2000000079, 2000000001, 2000000198, 2000000186, 2000000082, 2000000065, 2000000112, 2000000066, 2000000197,
		2000000081, 2000000121, 2000000076, 2000000194, 2000000199, 2000000146, 2000000173, 2000000046, 2000000154, 2000000048,
		2000000101, 2000000102, 2000000103, 2000000020, 2000000140, 2000000022, 2000000040, 2000000024, 2000000042, 2000000171,
		2000000044, 2000000039, 2000000055, 2000000029, 2000000172, 2000000028, 2000000141, 2000000135, 2000000204
	});

	public static IReadOnlyDictionary<int, IReadOnlyCollection<int>> TablesWithIntellectualProperty { get; } = new Dictionary<int, IReadOnlyCollection<int>>
	{
		{
			2000000160,
			new List<int>(new int[2] { 21, 26 }).AsReadOnly()
		},
		{
			2000000150,
			new List<int>(new int[2] { 6, 7 }).AsReadOnly()
		},
		{
			2000000176,
			new List<int>(new int[1] { 4 }).AsReadOnly()
		}
	};

	public static ICollection<int> ApplicationDatabaseTables => applicationDatabaseTables;

	public static ICollection<int> VirtualTables => virtualTables.Value;

	public static ICollection<int> TenantDatabaseCrossTenantTables => tenantDatabaseCrossTenantTables;

	public static ICollection<int> InternalTables => internalTables;

	private static int[] BuildVirtualTableList()
	{
		List<int> list = new List<int>
		{
			2000000007, 2000000026, 2000000049, 2000000101, 2000000102, 2000000103, 2000000020, 2000000055, 2000000009, 2000000029,
			2000000038, 2000000058, 2000000041, 2000000028, 2000000063, 2000000022, 2000000048, 2000000040, 2000000043, 2000000044,
			2000000039, 2000000045, 2000000135, 2000000136, 2000000137, 2000000138, 2000000139, 2000000140, 2000000164, 2000000141,
			2000000167, 2000000171, 2000000172, 2000000154, 2000000193, 2000000097, 2000000178, 2000000192, 2000000142, 2000000143,
			2000000157, 2000000202, 2000000203, 2000000204
		};
		if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
		{
			list.Add(2000000110);
		}
		return list.ToArray();
	}
}
