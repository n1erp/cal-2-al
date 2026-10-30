using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class DigitalSignatureChecker
{
	private const string MicrosoftCertHashString = "9E95C625D81B2BA9C72FD70275C3699613AF61E3";

	private const string WinTrustVeriyGuidString = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

	internal static bool VerifyDigitalSignature(string fileName)
	{
		Guid structure = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");
		WINTRUST_FILE_INFO fileInfo = new WINTRUST_FILE_INFO(fileName, Guid.Empty);
		WINTRUST_DATA structure2 = new WINTRUST_DATA(fileInfo);
		uint num = 1u;
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid)));
		IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA)));
		bool flag = false;
		bool flag2 = false;
		try
		{
			Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
			flag = true;
			Marshal.StructureToPtr(structure2, intPtr2, fDeleteOld: false);
			flag2 = true;
			num = NativeMethods.WinVerifyTrust(IntPtr.Zero, intPtr, intPtr2);
		}
		finally
		{
			if (flag)
			{
				Marshal.DestroyStructure(intPtr, typeof(Guid));
			}
			Marshal.FreeHGlobal(intPtr);
			if (flag2)
			{
				Marshal.DestroyStructure(intPtr2, typeof(WINTRUST_DATA));
			}
			Marshal.FreeHGlobal(intPtr2);
		}
		return num == 0;
	}

	internal static bool DigitalSignatureBelongsToMicrosoft(string name)
	{
		X509Certificate x509Certificate;
		try
		{
			x509Certificate = X509Certificate.CreateFromSignedFile(name);
		}
		catch (CryptographicException)
		{
			return false;
		}
		return x509Certificate.GetCertHashString().Equals("9E95C625D81B2BA9C72FD70275C3699613AF61E3");
	}
}
