using System;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

public sealed class NavClientExceptionHelper
{
	private static readonly PropertyInfo hresult;

	private const uint DiskFullHResult = 2147942512u;

	static NavClientExceptionHelper()
	{
		hresult = typeof(Exception).GetProperty("HResult", BindingFlags.Instance | BindingFlags.NonPublic);
		if (hresult == null)
		{
			hresult = typeof(Exception).GetProperty("HResult");
		}
	}

	private NavClientExceptionHelper()
	{
	}

	public static bool IsDiskFull(Exception exception)
	{
		return (int)hresult.GetValue(exception, null) == -2147024784;
	}

	public static bool IsExpectedErrorCode(Exception exception, int nativeErrorCode)
	{
		return (int)hresult.GetValue(exception, null) == nativeErrorCode;
	}
}
