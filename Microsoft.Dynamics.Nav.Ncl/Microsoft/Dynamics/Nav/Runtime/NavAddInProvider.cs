using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavAddInProvider : INavAddInProvider
{
	private object lockObject = new object();

	public bool ExtractAddInResourceFromAddInTable(string assemblyFullName, bool includeJavaScript = false)
	{
		AssemblyName assemblyName = new AssemblyName(assemblyFullName);
		if (NavCurrentThread.Session != null)
		{
			return ExtractAddInResourceFromAddInTableWithinSession(assemblyFullName, includeJavaScript, ref assemblyName, NavCurrentThread.Session);
		}
		bool foundAddIn = false;
		((ITenantSessionHandler)NavGlobal.SystemTenant).RunTenantActionInSystemSessionWithoutTransaction((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
		{
			foundAddIn = ExtractAddInResourceFromAddInTableWithinSession(assemblyFullName, includeJavaScript, ref assemblyName, systemSession);
		}), true, false, (LanguageSetting)null);
		return foundAddIn;
	}

	public Assembly ResolveAddInFromAddInTable(string assemblyFullName)
	{
		if (!IsExecutingInSession())
		{
			return null;
		}
		try
		{
			if (ExtractAddInResourceFromAddInTable(assemblyFullName))
			{
				string addInAssemblyLocationFromAssemblyName = NavAutomationHelper.GetAddInAssemblyLocationFromAssemblyName(assemblyFullName, !Environment.Is64BitProcess);
				return NavAutomationHelper.LoadAssemblyFile(addInAssemblyLocationFromAssemblyName);
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000228", Category.Runtime, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata("ResolveAddInFromAddInTable failed to resolve assembly " + assemblyFullName));
		}
		return null;
	}

	private static bool IsExecutingInSession()
	{
		return NavCurrentThread.Session != null;
	}

	private bool ExtractAddInResourceFromAddInTableWithinSession(string assemblyFullName, bool includeJavaScript, ref AssemblyName assemblyName, NavSession session)
	{
		using (NavRecord navRecord = FindDotNetAddInRecord(ref assemblyName, includeJavaScript, session))
		{
			if (navRecord == null)
			{
				return false;
			}
			lock (lockObject)
			{
				string path = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", assemblyName.Name, assemblyName.Version.ToString());
				string text = Path.Combine(NavAutomationHelper.GetTempAddInDirectory(), path);
				if (Directory.Exists(text) && Directory.GetFiles(text).Length != 0)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001VD", Category.Runtime, Verbosity.Normal, "Assembly was already extracted hinting another thread extracted the resource just before. Assembly: {0}", DiagnosticParameter.SystemMetadata(assemblyFullName));
					return true;
				}
				using Stream stream = GetAddInStream(navRecord);
				using ControlAddInZipArchiveReader controlAddInZipArchiveReader = new ControlAddInZipArchiveReader(stream);
				try
				{
					controlAddInZipArchiveReader.ExtractToDirectory(text);
					return true;
				}
				catch (IOException)
				{
				}
				catch (InvalidDataException innerException)
				{
					NavNCLDotNetCreateException.Create(CultureInfo.CurrentCulture, assemblyName.FullName, innerException);
				}
			}
		}
		return false;
	}

	private static NavRecord FindDotNetAddInRecord(ref AssemblyName assemblyName, bool includeJavaScript, NavSession session)
	{
		using (new MaximizedPermissionScope(session, new int[1] { 2000000069 }))
		{
			NavRecord navRecord = new NavRecord(session, 2000000069, SecurityFiltering.Ignored);
			navRecord.ALInit();
			navRecord.ALSetFilter(1, "%1", new NavText(assemblyName.Name));
			byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
			if (publicKeyToken != null && publicKeyToken.Length != 0)
			{
				navRecord.ALSetFilter(5, "%1", new NavText(EncodeHexString(publicKeyToken)));
			}
			Version version = assemblyName.Version;
			if (version != null)
			{
				navRecord.ALSetFilter(8, "%1", new NavText(version.ToString()));
			}
			else
			{
				version = new Version();
				assemblyName.Version = version;
			}
			if (!includeJavaScript)
			{
				navRecord.ALSetFilter(15, "<>%1", NavInteger.Create(0));
			}
			navRecord.ALSetFilter(25, "*");
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				return navRecord;
			}
		}
		return null;
	}

	private static Stream GetAddInStream(NavRecord record)
	{
		if (!record.GetFieldValue(25).IsZeroOrEmpty && record.CalcFields(DataError.TrapError, new int[1] { 25 }))
		{
			return ((NavBLOB)record.GetFieldValue(25)).GetStream();
		}
		return null;
	}

	private static string EncodeHexString(byte[] value)
	{
		return BitConverter.ToString(value).Replace("-", string.Empty).ToLowerInvariant();
	}
}
