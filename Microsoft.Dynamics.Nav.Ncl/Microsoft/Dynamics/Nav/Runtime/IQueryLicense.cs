namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IQueryLicense
{
	string GetLicenseInfoString(ushort objType, int objNo);

	bool GetLicenseInfoInt(ushort objType, int objNo, out int info);

	byte[] GetLicenseInfoData(ushort objType, int objNo, int offset, int count);

	bool NextType(LicenseSearchDirection direction, ref ushort objType);

	bool NextIndex(LicenseSearchDirection direction, ushort objType, ref short index);

	bool GetIndexData(ushort objType, short index, out uint from, out uint to);
}
