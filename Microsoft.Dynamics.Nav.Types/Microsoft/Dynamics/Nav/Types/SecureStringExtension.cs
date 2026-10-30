using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Dynamics.Nav.Types;

public static class SecureStringExtension
{
	public static string ConvertToUnsecureString(this SecureString secureString)
	{
		if (secureString == null)
		{
			throw new ArgumentNullException("secureString");
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}

	public static SecureString ToSecureStringOrNull(this string value)
	{
		if (value == null)
		{
			return null;
		}
		return CreateFromUnsecureString(value);
	}

	public unsafe static SecureString CreateFromUnsecureString(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Length == 0)
		{
			return new SecureString();
		}
		char[] array = value.ToCharArray();
		SecureString result;
		fixed (char* value2 = array)
		{
			result = new SecureString(value2, array.Length);
		}
		return result;
	}
}
