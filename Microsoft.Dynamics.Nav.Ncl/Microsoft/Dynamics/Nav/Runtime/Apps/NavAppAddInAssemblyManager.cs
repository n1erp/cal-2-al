using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppAddInAssemblyManager
{
	private class AssemblyCache
	{
		private readonly Dictionary<NavAppAddInAssemblyName, Assembly> lookup = new Dictionary<NavAppAddInAssemblyName, Assembly>();

		public Assembly GetAssembly(NavAppAddInAssemblyName assemblyName)
		{
			Assembly value;
			return lookup.TryGetValue(assemblyName, out value) ? value : null;
		}

		public void StoreAssembly(NavAppAddInAssemblyName assemblyName, Assembly assembly)
		{
			lookup.Add(assemblyName, assembly);
		}
	}

	private readonly object serverAssemblyLockObj = new object();

	private readonly INavAppAddInRetriever addInRetriever;

	private readonly AssemblyCache allServerAssembliesCache = new AssemblyCache();

	private readonly Dictionary<int, AssemblyCache> appGroupServerAssemblyCaches = new Dictionary<int, AssemblyCache>();

	public NavAppAddInAssemblyManager(INavAppAddInRetriever addInRetriever)
	{
		this.addInRetriever = addInRetriever;
	}

	public Assembly GetNavAppServerAddInAssemblyUsingCurrentContext(string requestedAssemblyName)
	{
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
		if (navAppGroup != NavAppGroup.BaseGroup)
		{
			return GetNavAppServerAddInAssembly(NavCurrentThread.Session, navAppGroup.GroupId, requestedAssemblyName);
		}
		return null;
	}

	public Assembly GetNavAppServerAddInAssembly(NavSession session, int appGroupId, string requestedAssemblyName)
	{
		NavAppAddInAssemblyName assemblyName = NavAppAddInAssemblyName.CreateFromString(requestedAssemblyName);
		AssemblyCache value;
		lock (serverAssemblyLockObj)
		{
			if (!appGroupServerAssemblyCaches.TryGetValue(appGroupId, out value))
			{
				value = CreateFullyPopulatedServerAssemblyCacheForAppGroup(session, appGroupId);
				appGroupServerAssemblyCaches.Add(appGroupId, value);
			}
		}
		return value.GetAssembly(assemblyName);
	}

	public NavAppAddInAssemblyArchive GetArchiveContainingAllControlAddInsAndDependencies(NavSession session, int appGroupId, string appGroupCombinationId)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance(session);
		NavAppAddInAssemblyArchive result = null;
		MemoryStream memoryStream = CreateArchiveOfAllAppGroupControlAddInAssemblies(session, appGroupId);
		if (memoryStream != null)
		{
			MemoryStream stream = new MemoryStream(memoryStream.ToArray());
			string text = string.Format(CultureInfo.InvariantCulture, "{0}{1}", "AppsAddIns_Client_", appGroupCombinationId);
			mostSpecificInstance.SendTraceTag("00000AW", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Created extension assembly archive -- Group:{0}; Name:{1}", appGroupId, text);
			result = new NavAppAddInAssemblyArchive(text, stream);
		}
		return result;
	}

	private AssemblyCache CreateFullyPopulatedServerAssemblyCacheForAppGroup(NavSession session, int appGroupId)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance(session);
		string content = string.Format(CultureInfo.InvariantCulture, "Populating extension server assembly cache -- Group:{0}", appGroupId);
		using (DiagnosticsResolver.GetMostSpecificInstance(session)?.CreateTraceScope("00000AQ", DiagnosticParameter.SystemMetadata(content), (MonitorType)589827, Category.Extensions, createNewActivity: false))
		{
			AssemblyCache assemblyCache = new AssemblyCache();
			IEnumerable<NavAppAddInRecord> tenantAddInRecords = addInRetriever.GetTenantAddInRecords(session, AddInCategory.DotNetInteroperability);
			foreach (NavAppAddInRecord item in tenantAddInRecords)
			{
				NavAppAddInAssemblyName navAppAddInAssemblyName = NavAppAddInAssemblyName.CreateFromComponents(item.AddInName, item.PublicKeyToken, item.Version);
				mostSpecificInstance.SendTraceTag("00000AR", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Processing assembly -- Assembly:{0}", navAppAddInAssemblyName.ToString());
				Assembly assembly = allServerAssembliesCache.GetAssembly(navAppAddInAssemblyName);
				if (assembly == null)
				{
					mostSpecificInstance.SendTraceTag("00000AS", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Processing assembly :: cache miss -- Assembly:{0}", navAppAddInAssemblyName.ToString());
					byte[] bytes = addInRetriever.GetTenantAddInAssemblyContent(session, item)?.Content;
					Assembly assembly2 = LoadServerAssemblyFromBytes(session, navAppAddInAssemblyName.ToString(), bytes);
					if (assembly2 != null)
					{
						allServerAssembliesCache.StoreAssembly(navAppAddInAssemblyName, assembly2);
						assemblyCache.StoreAssembly(navAppAddInAssemblyName, assembly2);
					}
				}
				else
				{
					mostSpecificInstance.SendTraceTag("00000AT", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Processing assembly :: cache hit -- Assembly:{0}", navAppAddInAssemblyName.ToString());
					assemblyCache.StoreAssembly(navAppAddInAssemblyName, assembly);
				}
			}
			return assemblyCache;
		}
	}

	private MemoryStream CreateArchiveOfAllAppGroupControlAddInAssemblies(NavSession session, int appGroupId)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance(session);
		List<NavAppAddInRecord> list = new List<NavAppAddInRecord>();
		string content = string.Format(CultureInfo.InvariantCulture, "Creating control add-in assembly archive -- Group:{0}", appGroupId);
		using (mostSpecificInstance.CreateTraceScope("00000AX", DiagnosticParameter.SystemMetadata(content), (MonitorType)589827, Category.Extensions, createNewActivity: false))
		{
			IEnumerable<NavAppAddInRecord> tenantAddInRecords = addInRetriever.GetTenantAddInRecords(session, AddInCategory.DotNetControlAddIn);
			list.AddRange(tenantAddInRecords);
			if (!list.Any())
			{
				return null;
			}
			IEnumerable<NavAppAddInRecord> tenantAddInRecords2 = addInRetriever.GetTenantAddInRecords(session, AddInCategory.DotNetInteroperability);
			list.AddRange(tenantAddInRecords2);
			MemoryStream memoryStream = new MemoryStream();
			using ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create);
			foreach (NavAppAddInRecord item in list)
			{
				NavAppAddInAssemblyName navAppAddInAssemblyName = NavAppAddInAssemblyName.CreateFromComponents(item.AddInName, item.PublicKeyToken, item.Version);
				mostSpecificInstance.SendTraceTag("00000AY", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Putting extension assembly in archive -- Assembly:{0}", navAppAddInAssemblyName.ToString());
				NavAppAddInRawAssembly tenantAddInAssemblyContent = addInRetriever.GetTenantAddInAssemblyContent(session, item);
				using MemoryStream memoryStream2 = new MemoryStream(tenantAddInAssemblyContent.Content);
				ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(tenantAddInAssemblyContent.Name);
				using Stream destination = zipArchiveEntry.Open();
				memoryStream2.CopyTo(destination);
			}
			return memoryStream;
		}
	}

	private static Assembly LoadServerAssemblyFromBytes(NavSession session, string assemblyNameString, byte[] bytes)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance(session);
		if (bytes == null)
		{
			mostSpecificInstance.SendTraceTag("00000AU", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading assembly :: null bytes -- Assembly:{0}", assemblyNameString);
			return null;
		}
		Exception exception;
		try
		{
			return Assembly.Load(bytes);
		}
		catch (FileNotFoundException ex)
		{
			exception = ex;
		}
		catch (FileLoadException ex2)
		{
			exception = ex2;
		}
		catch (BadImageFormatException ex3)
		{
			exception = ex3;
		}
		catch (PathTooLongException ex4)
		{
			exception = ex4;
		}
		session?.Diagnostics.SendExceptionTag("00000AV", Category.Extensions, exception);
		return null;
	}
}
