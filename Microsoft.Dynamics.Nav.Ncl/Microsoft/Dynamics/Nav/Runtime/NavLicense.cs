using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavLicense : INavLicense, IQueryLicense, IDisposable
{
	internal static class LicenseObjectType
	{
		internal const ushort TableData = 0;

		internal const ushort TableDesc = 1;

		internal const ushort Form = 2;

		internal const ushort Report = 3;

		internal const ushort Dataport = 4;

		internal const ushort Codeunit = 5;

		internal const ushort XMLPort = 6;

		internal const ushort MenuSuite = 7;

		internal const ushort MetaForm = 8;

		internal const ushort Query = 9;

		internal const ushort System = 10;

		internal const ushort FieldNumber = 11;

		internal const ushort LimitedUsageTableData = 12;

		internal const ushort TablePage = 13;

		internal const ushort Misc = 20;
	}

	internal static class LicenseObjectNo
	{
		internal const int MiscSerialNo = 1;

		internal const int MiscUserInfo = 20;

		internal const int MiscMaxSessionsTotal = 100;

		internal const int MiscMaxSessionsAny = 101;

		internal const int MiscMaxServersTotal = 120;

		internal const int MiscMaxServersAny = 121;

		internal const int MiscMaxServersMSSQL = 136;

		internal const int MiscMaxServersDesktopEngine = 138;

		internal const int MiscMaxCompanies = 201;

		internal const int MiscMinProgVersion = 250;

		internal const int MiscMaxProgVersion = 251;

		internal const int MiscProgramID = 253;

		internal const int MiscFreeCompanyNameFilter = 255;

		internal const int MiscCompanyNameFilter = 300;

		internal const int MiscValidationFilterDate = 305;

		internal const int MiscSubscriptionFullUserCAL = 440;

		internal const int MiscSubscriptionLimitedUserCAL = 441;

		internal const int MiscFullDeviceCAL = 442;

		internal const int MiscLimitedDeviceCAL = 443;

		internal const int MiscProductLineId = 444;

		internal const int MiscPerpetualFullUserCAL = 445;

		internal const int MiscPerpetualLimitedUserCAL = 446;

		internal const int MiscAccountNumber = 447;

		internal const int MiscISVDeviceCAL = 448;

		internal const int MiscISVFunctionalDeviceCAL = 449;

		internal const int SysObjFileDatabaseOptions = 1580;

		internal const int SysObjLicenseFile = 6000;

		internal const int SysObjAllowNS300 = 6002;

		internal const int SysObjLangGlobalStart = 1879048192;

		internal const int SysObjLangGlobalEnd = 1879113727;

		internal const int SysObjLangFormStart = 1879113728;

		internal const int SysObjLangFormEnd = 1879179263;

		internal const int SysObjLangReportStart = 1879179264;

		internal const int SysObjLangReportEnd = 1879244799;

		internal const int SysObjLangDataportStart = 1879244800;

		internal const int SysObjLangDataportEnd = 1879310335;

		internal const int SysObjLangXMLPortStart = 1879310336;

		internal const int SysObjLangXMLPortEnd = 1879375871;
	}

	internal static class LicenseProductLine
	{
		public const int Subscription = 49;

		public const int Perpetual = 50;

		public const int BusinessCentralOnPremisesDualRightsUse = 73;

		public const int BusinessCentralOnPremises = 74;

		public const int BusinessCentralOnPremisesSubscription = 75;

		public const int BusinessCentralBPOOnPremises = 76;
	}

	private static class NativeMethods
	{
		[DllImport("navlic.dll", EntryPoint = "NL_UnpackLicense", PreserveSig = false)]
		internal static extern IntPtr UnpackLicense(byte[] license, int licenseSize, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder header, int headerSize);

		[DllImport("navlic.dll", EntryPoint = "NL_FreeLicense", PreserveSig = false)]
		internal static extern void FreeLicense(IntPtr hLicense);

		[DllImport("navlic.dll", EntryPoint = "NL_GetLicenseInfoS32", PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetLicenseInfoInt(IntPtr hLicense, ushort objType, int objNo, out int info);

		[DllImport("navlic.dll", EntryPoint = "NL_GetLicenseInfoString")]
		internal static extern void GetLicenseInfoString(IntPtr hLicense, ushort objType, int objNo, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder dest, int destSize);

		[DllImport("navlic.dll", EntryPoint = "NL_GetLicenseInfoData")]
		internal static extern void GetLicenseInfoData(IntPtr hLicense, ushort objType, int objNo, int offset, byte[] dest, int destSize);

		[DllImport("navlic.dll", EntryPoint = "NL_NextType", PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool NextType(IntPtr hLicense, byte direction, ref ushort objType);

		[DllImport("navlic.dll", EntryPoint = "NL_NextIndex", PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool NextIndex(IntPtr hLicense, byte direction, ushort objType, ref short index);

		[DllImport("navlic.dll", EntryPoint = "NL_GetIndexData", PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetIndexData(IntPtr hLicense, ushort objType, short index, out uint from, out uint to);
	}

	private const int ProgramID = 0;

	private IntPtr licenseHandle;

	private string description;

	private readonly string header;

	private readonly LicenseExpiredHandler licenseExpiredHandler;

	private FilterExpression dateLicenseFilter;

	private FilterExpression companyNameLicenseFilter;

	private FilterExpression freeCompanyNameLicenseFilter;

	private static readonly object LicenseLoadingLock = new object();

	private IQueryLicense query;

	private static int ProgramVersion => VersionCompatibility.AssemblyFileVersion.Major * 100;

	public string LicenseUniqueIdentifier { get; private set; }

	public string SerialNumber => query.GetLicenseInfoString(20, 1);

	public int AccountNumber => GetLicenseInfoInt(20, 447);

	public string Description
	{
		get
		{
			if (description == null)
			{
				StringBuilder stringBuilder = new StringBuilder(1024);
				stringBuilder.AppendLine(SerialNumber);
				for (int i = 0; i < 5; i++)
				{
					stringBuilder.AppendLine(query.GetLicenseInfoString(20, 20 + i));
				}
				description = stringBuilder.ToString();
			}
			return description;
		}
	}

	public int ProductLine => GetLicenseInfoInt(20, 444);

	public int NumberOfFullDevices => GetLicenseInfoInt(20, 442);

	public int NumberOfLimitedDevices => GetLicenseInfoInt(20, 443);

	public int NumberOfIsvDevices => GetLicenseInfoInt(20, 448);

	public int NumberOfIsvFunctionalDevices => GetLicenseInfoInt(20, 449);

	public int NamedUsersNumberOfFullUsers => GetLicenseInfoInt(20, 440);

	public int NamedUsersNumberOfLimitedUsers => GetLicenseInfoInt(20, 441);

	public int ConcurrentConnectionsNumberOfFullUsers => GetLicenseInfoInt(20, 445);

	public int ConcurrentConnectionsNumberOfLimitedUsers => GetLicenseInfoInt(20, 446);

	public int MaxSessionsTotal => GetLicenseInfoInt(20, 120);

	public int MaxSessionsAny => GetLicenseInfoInt(20, 101);

	public int MaxServersTotal => GetLicenseInfoInt(20, 120);

	public int MaxServersAny => GetLicenseInfoInt(20, 121);

	public int MaxCompanies => GetLicenseInfoInt(20, 201);

	public int MaxServersMSSQL => GetLicenseInfoInt(20, 136);

	public int MaxServersDesktopEngine => GetLicenseInfoInt(20, 138);

	public LicenseModel LicenseModel
	{
		get
		{
			switch (ProductLine)
			{
			case 49:
			case 73:
			case 74:
			case 75:
			case 76:
				return LicenseModel.NamedUsers;
			case 50:
				return LicenseModel.ConcurrentConnections;
			default:
				throw new NavInvalidLicenseFormatException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidLicenseFormat));
			}
		}
	}

	public string DateValidationFilter => query.GetLicenseInfoString(20, 305);

	public string FreeCompanyNameFilter => query.GetLicenseInfoString(20, 255);

	public string CompanyNameFilter => query.GetLicenseInfoString(20, 300);

	public string Header => header;

	public bool IsDemoLicense
	{
		get
		{
			BuildLicenseFilter();
			FilterExpression expression = dateLicenseFilter;
			return !expression.IsNotNullAndConstantTrue();
		}
	}

	public bool IsProgramVersionValid
	{
		get
		{
			if (ProgramVersion >= GetLicenseInfoInt(20, 250) && ProgramVersion <= GetLicenseInfoInt(20, 251))
			{
				return 0 >= GetLicenseInfoInt(20, 253);
			}
			return false;
		}
	}

	string IQueryLicense.GetLicenseInfoString(ushort objType, int objNo)
	{
		CheckDisposed();
		StringBuilder stringBuilder = new StringBuilder(128);
		NativeMethods.GetLicenseInfoString(licenseHandle, objType, objNo, stringBuilder, stringBuilder.Capacity);
		return stringBuilder.ToString();
	}

	bool IQueryLicense.GetLicenseInfoInt(ushort objType, int objNo, out int info)
	{
		CheckDisposed();
		return NativeMethods.GetLicenseInfoInt(licenseHandle, objType, objNo, out info);
	}

	byte[] IQueryLicense.GetLicenseInfoData(ushort objType, int objNo, int offset, int count)
	{
		CheckDisposed();
		byte[] array = new byte[count];
		NativeMethods.GetLicenseInfoData(licenseHandle, objType, objNo, offset, array, count);
		return array;
	}

	bool IQueryLicense.NextType(LicenseSearchDirection direction, ref ushort objType)
	{
		CheckDisposed();
		return NativeMethods.NextType(licenseHandle, (byte)direction, ref objType);
	}

	bool IQueryLicense.NextIndex(LicenseSearchDirection direction, ushort objType, ref short index)
	{
		CheckDisposed();
		return NativeMethods.NextIndex(licenseHandle, (byte)direction, objType, ref index);
	}

	bool IQueryLicense.GetIndexData(ushort objType, short index, out uint from, out uint to)
	{
		CheckDisposed();
		return NativeMethods.GetIndexData(licenseHandle, objType, index, out from, out to);
	}

	private int GetLicenseInfoInt(ushort objType, int objNo)
	{
		int info = 0;
		query.GetLicenseInfoInt(objType, objNo, out info);
		return info;
	}

	public bool GetNextType(LicenseSearchDirection direction, ref ushort objType)
	{
		return query.NextType(direction, ref objType);
	}

	public bool GetNextIndex(LicenseSearchDirection direction, ushort objType, ref short index)
	{
		return query.NextIndex(direction, objType, ref index);
	}

	public bool GetIndexData(ushort objType, short index, out uint from, out uint to)
	{
		return query.GetIndexData(objType, index, out from, out to);
	}

	private static void HandleException(Exception e)
	{
		if (e is COMException ex)
		{
			throw new NavInvalidLicenseFormatException(string.Format(CultureInfo.CurrentCulture, Lang.LicenseFileIsCorrupt, ex.ErrorCode));
		}
		throw e;
	}

	private static NavDate ReadDate(byte[] buffer, int offset)
	{
		int num = BitConverter.ToInt32(buffer, offset);
		if (num != 0)
		{
			return NavDate.CreateDate(new DateTime((num >> 10) & 0x3FFFFF, (num >> 6) & 0xF, (num >> 1) & 0x1F, 0, 0, 0, 0, DateTimeKind.Local), (num & 1) == 1);
		}
		return NavDate.Default;
	}

	public void GetExpiryDates(ushort objType, int objNo, out NavDate expiryDate, out NavDate warningDate)
	{
		byte[] licenseInfoData = query.GetLicenseInfoData(objType, objNo, 4, 8);
		warningDate = ReadDate(licenseInfoData, 0);
		expiryDate = ReadDate(licenseInfoData, 4);
	}

	public PermissionMask GetLicensePermissionMask(ObjectType objType, int objNo)
	{
		return GetLicensePermissionMask(objType, objNo, null);
	}

	public PermissionMask GetLicensePermissionMask(ObjectType objType, int objNo, NavDate today)
	{
		ushort num = (ushort)objType;
		if (num == 0 && (uint)(objNo - 2000000110) <= 2u)
		{
			return PermissionMask.ReadDeleteInsertModify;
		}
		if (num == 10 && objNo == 1580)
		{
			return PermissionMask.ReadDeleteInsertModify;
		}
		if (num == 7 && objNo > 1000 && objNo < 2000)
		{
			objNo -= 1000;
		}
		if (!query.GetLicenseInfoInt(num, objNo, out var info))
		{
			return PermissionMask.None;
		}
		PermissionMask currentPermissions = (PermissionMask)info;
		currentPermissions = ApplyLimitedUsagePermissions(objNo, num, currentPermissions);
		if (num == 1 && objNo >= 2000000001)
		{
			currentPermissions &= ~PermissionMask.Delete;
			currentPermissions &= ~PermissionMask.IndirectDelete;
		}
		if (today != null && currentPermissions.HasFlag(PermissionMask.HasExpDate))
		{
			GetExpiryDates(num, objNo, out var expiryDate, out var warningDate);
			bool flag = today > expiryDate;
			if (flag)
			{
				currentPermissions = PermissionMask.None;
			}
			if (flag || (warningDate != NavDate.Default && today > warningDate))
			{
				licenseExpiredHandler(num, objNo, num == 10 && objNo == 6000, !flag, expiryDate, today, string.Empty);
			}
		}
		return currentPermissions;
	}

	public int GetMaxSessionCountByConnectionType(ConnectionType connectionType)
	{
		return GetLicenseInfoInt(20, (int)connectionType);
	}

	public bool HasLanguagePermission(int lcid, NavSession.LanguagePermission languagePermission)
	{
		if (lcid == 0)
		{
			return true;
		}
		int num;
		switch (languagePermission)
		{
		case NavSession.LanguagePermission.Global:
			num = 1879048192;
			break;
		case NavSession.LanguagePermission.Form:
			num = 1879113728;
			break;
		case NavSession.LanguagePermission.Report:
			num = 1879179264;
			break;
		case NavSession.LanguagePermission.XmlPort:
			num = 1879310336;
			break;
		default:
			return false;
		}
		return PermissionHelper.HasExecutePermission(GetLicensePermissionMask(ObjectType.System, num + lcid, NavDate.Create(DateTime.Today)));
	}

	public void CheckIsValid()
	{
		CheckIsValid(NavDate.Create(DateTime.Today));
	}

	public void CheckIsValid(NavDate today)
	{
		CheckExpired(today);
		if (!PermissionHelper.HasExecutePermission(GetLicensePermissionMask(ObjectType.System, 6002, today)))
		{
			throw new NavLicenseViolationException(string.Format(CultureInfo.CurrentCulture, Lang.LicenseNotValidForThisVersion, ProductNames.ProductNameClassicClient));
		}
	}

	private static PermissionMask ApplyLimitedUsagePermissions(PermissionMask currentPermissions, int limitedUsageTableInfo)
	{
		switch ((LimitedUsagePermissionMask)limitedUsageTableInfo)
		{
		case LimitedUsagePermissionMask.Excluded:
			currentPermissions |= PermissionMask.ExcludedLimitedUsage;
			break;
		case LimitedUsagePermissionMask.Included:
			currentPermissions |= PermissionMask.IncludedLimitedUsage;
			break;
		}
		return currentPermissions;
	}

	private PermissionMask ApplyLimitedUsagePermissions(int objNo, ushort permObjType, PermissionMask currentPermissions)
	{
		switch (permObjType)
		{
		case 0:
		{
			if (query.GetLicenseInfoInt(12, objNo, out var info))
			{
				return ApplyLimitedUsagePermissions(currentPermissions, info);
			}
			break;
		}
		case 12:
			return ApplyLimitedUsagePermissions(PermissionMask.None, (int)currentPermissions);
		}
		return currentPermissions;
	}

	private void CheckExpired(NavDate today)
	{
		if (!PermissionHelper.HasExecutePermission(GetLicensePermissionMask(ObjectType.System, 6000, today)))
		{
			throw new NavLicenseViolationException(Lang.ProgramLicenseMismatch);
		}
	}

	private NavLicense(IntPtr licenseHandle, string header, LicenseExpiredHandler licenseExpiredHandler, string licenseUniqueIdentifier)
	{
		if (licenseExpiredHandler == null)
		{
			throw new ArgumentNullException("licenseExpiredHandler");
		}
		this.licenseHandle = licenseHandle;
		this.header = header;
		query = this;
		this.licenseExpiredHandler = licenseExpiredHandler;
		LicenseUniqueIdentifier = licenseUniqueIdentifier;
	}

	~NavLicense()
	{
		Dispose(disposing: false);
	}

	public static NavLicense Create(byte[] license, LicenseExpiredHandler licenseExpiredHandler)
	{
		StringBuilder stringBuilder = new StringBuilder(license.Length);
		try
		{
			IntPtr intPtr;
			lock (LicenseLoadingLock)
			{
				intPtr = NativeMethods.UnpackLicense(license, license.Length, stringBuilder, stringBuilder.Capacity);
			}
			return new NavLicense(intPtr, stringBuilder.ToString(), licenseExpiredHandler, GetLicenseHash(license));
		}
		catch (COMException e)
		{
			HandleException(e);
		}
		return null;
	}

	public void EvaluateDateLicenseFilter(NavDate date)
	{
		BuildLicenseFilter();
		FilterExpression filterExpression = dateLicenseFilter;
		if (filterExpression.IsNotNullAndConstantTrue() || filterExpression.Evaluate(new NavText(10, date.ToDateTime().ToString("ddMMyy", CultureInfo.InvariantCulture))))
		{
			return;
		}
		throw new NavCSideDataException(string.Format(CultureInfo.CurrentCulture, Lang.DateOutsideAllowedInterval, filterExpression));
	}

	private void BuildLicenseFilter()
	{
		if (dateLicenseFilter == null)
		{
			dateLicenseFilter = FilterExpressionParser.Parse(DateValidationFilter, FilterExpressionContext.CreateDefaultForType(NavNclType.NavText));
		}
	}

	public bool IsFreeCompanyName(string companyName)
	{
		FilterExpression filterExpression = freeCompanyNameLicenseFilter;
		if (filterExpression == null)
		{
			filterExpression = (freeCompanyNameLicenseFilter = ((!string.IsNullOrEmpty(FreeCompanyNameFilter)) ? ((FilterExpression)FilterExpression.Wildcard(isNegated: false, new NavText(FreeCompanyNameFilter), caseAndAccentInsensitive: false, FilterExpressionContext.CreateForTextField(NavFieldMetadata.DefaultMetadata(NavValueMetadata.DefaultMetadata(NavNclType.NavText)), CultureInfo.InvariantCulture, CompareOptions.None, canBeOptimized: true))) : ((FilterExpression)FilterExpression.BooleanConstant(value: true))));
		}
		if (filterExpression.IsNotNullAndConstantTrue())
		{
			return false;
		}
		return filterExpression.Evaluate(new NavText(companyName.Length, companyName));
	}

	public bool CheckCompanyNameLicenseFilter(string companyName)
	{
		FilterExpression filterExpression = companyNameLicenseFilter;
		if (filterExpression == null)
		{
			filterExpression = (companyNameLicenseFilter = ((!string.IsNullOrEmpty(CompanyNameFilter)) ? ((FilterExpression)FilterExpression.Wildcard(isNegated: false, new NavText(CompanyNameFilter), caseAndAccentInsensitive: false, FilterExpressionContext.CreateForTextField(NavFieldMetadata.DefaultMetadata(NavValueMetadata.DefaultMetadata(NavNclType.NavText)), CultureInfo.InvariantCulture, CompareOptions.None, canBeOptimized: true))) : ((FilterExpression)FilterExpression.BooleanConstant(value: true))));
		}
		if (filterExpression.IsNotNullAndConstantTrue())
		{
			return true;
		}
		return filterExpression.Evaluate(new NavText(companyName.Length, companyName));
	}

	private static string GetLicenseHash(byte[] license)
	{
		using SHA256CryptoServiceProvider sHA256CryptoServiceProvider = new SHA256CryptoServiceProvider();
		return Convert.ToBase64String(sHA256CryptoServiceProvider.ComputeHash(license));
	}

	private void CheckDisposed()
	{
		if (licenseHandle == IntPtr.Zero)
		{
			throw new ObjectDisposedException("NavLicense");
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (licenseHandle != IntPtr.Zero)
		{
			NativeMethods.FreeLicense(licenseHandle);
			licenseHandle = IntPtr.Zero;
		}
	}
}
