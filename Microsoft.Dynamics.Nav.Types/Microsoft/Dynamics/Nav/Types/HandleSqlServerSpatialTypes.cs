using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

internal class HandleSqlServerSpatialTypes
{
	private static class NativeMethods
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr LoadLibrary(string libraryName);
	}

	private const string TypeFolder = "SqlServerNativeBinaries";

	public static void LoadNativeBinaries(string rootApplicationPath)
	{
		string path = ((IntPtr.Size > 4) ? "x64" : "x86");
		string text = Path.Combine(rootApplicationPath, "SqlServerNativeBinaries", path);
		if (!Directory.Exists(text))
		{
			return;
		}
		try
		{
			LoadNativeAssembly(text, "msvcr120.dll");
			LoadNativeAssembly(text, "SqlServerSpatial140.dll");
		}
		catch (FileNotFoundException ex)
		{
			NavDiagnostics.GetMostSpecificInstance.SendTraceTag("00001GP", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SQL Server native binary object could not be found: {0}", ex.FileName);
		}
		catch (BadImageFormatException ex2)
		{
			NavDiagnostics.GetMostSpecificInstance.SendTraceTag("00001GQ", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, ex2.Message);
		}
	}

	private static void LoadNativeAssembly(string nativeBinaryPath, string assemblyName)
	{
		string text = Path.Combine(nativeBinaryPath, assemblyName);
		if (!File.Exists(text))
		{
			throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "The native assembly {0} does not exist", text), text);
		}
		if (NativeMethods.LoadLibrary(text) == IntPtr.Zero)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			throw new BadImageFormatException(string.Format(CultureInfo.InvariantCulture, "The native image {0} is an incorrect file type and cannot load. Error code {1},", assemblyName, lastWin32Error));
		}
	}
}
