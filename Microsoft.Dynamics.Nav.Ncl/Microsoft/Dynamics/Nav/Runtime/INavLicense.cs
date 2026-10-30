using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INavLicense : IQueryLicense, IDisposable
{
	int AccountNumber { get; }

	string CompanyNameFilter { get; }

	int ConcurrentConnectionsNumberOfFullUsers { get; }

	int ConcurrentConnectionsNumberOfLimitedUsers { get; }

	string DateValidationFilter { get; }

	string Description { get; }

	string FreeCompanyNameFilter { get; }

	string Header { get; }

	bool IsDemoLicense { get; }

	LicenseModel LicenseModel { get; }

	string LicenseUniqueIdentifier { get; }

	int MaxCompanies { get; }

	int MaxServersAny { get; }

	int MaxServersDesktopEngine { get; }

	int MaxServersMSSQL { get; }

	int MaxServersTotal { get; }

	int MaxSessionsAny { get; }

	int MaxSessionsTotal { get; }

	int NamedUsersNumberOfFullUsers { get; }

	int NamedUsersNumberOfLimitedUsers { get; }

	int NumberOfFullDevices { get; }

	int NumberOfIsvDevices { get; }

	int NumberOfIsvFunctionalDevices { get; }

	int NumberOfLimitedDevices { get; }

	int ProductLine { get; }

	string SerialNumber { get; }

	bool IsProgramVersionValid { get; }

	bool CheckCompanyNameLicenseFilter(string companyName);

	void EvaluateDateLicenseFilter(NavDate date);

	void GetExpiryDates(ushort objType, int objNo, out NavDate expiryDate, out NavDate warningDate);

	PermissionMask GetLicensePermissionMask(ObjectType objType, int objNo);

	PermissionMask GetLicensePermissionMask(ObjectType objType, int objNo, NavDate today);

	int GetMaxSessionCountByConnectionType(ConnectionType connectionType);

	bool IsFreeCompanyName(string companyName);

	bool HasLanguagePermission(int lcid, NavSession.LanguagePermission languagePermission);

	void CheckIsValid();

	void CheckIsValid(NavDate date);

	bool GetNextType(LicenseSearchDirection direction, ref ushort objType);

	bool GetNextIndex(LicenseSearchDirection direction, ushort objType, ref short index);
}
