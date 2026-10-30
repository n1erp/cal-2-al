namespace Microsoft.Dynamics.Nav.Runtime;

public delegate void LicenseExpiredHandler(ushort objType, int objNo, bool isLicenseFile, bool isWarning, NavDate expiryDate, NavDate today, string expireText);
