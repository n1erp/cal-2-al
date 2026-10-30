using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class TenantNavAppsAddInRetriever
{
	private static readonly Dictionary<string, Type> typeNameToTypeMap = new Dictionary<string, Type>();

	private static readonly object typeCacheLock = new object();

	public static object CreateDotNetObject(string assemblyFullName, string typeName, params object[] arguments)
	{
		Type typeFromAssembly = GetTypeFromAssembly(assemblyFullName, typeName);
		if (typeFromAssembly == null)
		{
			throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, null);
		}
		return NavAutomationHelper.CreateObjectTypeInstance(typeFromAssembly, assemblyFullName, typeName, arguments);
	}

	public static Type GetInterfaceType(string assemblyFullName, string typeName, Type otherType)
	{
		if (string.IsNullOrEmpty(assemblyFullName) || string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		Type typeFromAssembly = GetTypeFromAssembly(assemblyFullName, typeName);
		return NavAutomationHelper.ConvertGenericInterfaceType(typeFromAssembly, otherType);
	}

	public static Type GetTypeFromAssembly(string assemblyFullName, string typeName)
	{
		NavSession session = NavCurrentThread.Session;
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
		Type value = null;
		Assembly navAppServerAddInAssembly = NavGlobal.SystemTenant.NavAppAddInAssemblyManager.GetNavAppServerAddInAssembly(session, navAppGroup.GroupId, assemblyFullName);
		if (navAppServerAddInAssembly != null)
		{
			lock (typeCacheLock)
			{
				string key = assemblyFullName + "@" + typeName;
				if (!typeNameToTypeMap.TryGetValue(key, out value))
				{
					try
					{
						value = navAppServerAddInAssembly.GetType(typeName);
						typeNameToTypeMap[key] = value;
					}
					catch (FileNotFoundException innerException)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException);
					}
					catch (FileLoadException innerException2)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException2);
					}
					catch (BadImageFormatException innerException3)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException3);
					}
					catch (PathTooLongException innerException4)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException4);
					}
				}
			}
		}
		return value;
	}

	public static Stream GetControlAddInAssembliesForAppGroup(out string archiveFileName)
	{
		NavSession session = NavCurrentThread.Session;
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
		int groupId = navAppGroup.GroupId;
		string stableCombinationId = navAppGroup.StableCombinationId;
		NavAppAddInAssemblyArchive archiveContainingAllControlAddInsAndDependencies = NavGlobal.SystemTenant.NavAppAddInAssemblyManager.GetArchiveContainingAllControlAddInsAndDependencies(session, groupId, stableCombinationId);
		if (archiveContainingAllControlAddInsAndDependencies == null)
		{
			archiveFileName = null;
			return null;
		}
		archiveFileName = archiveContainingAllControlAddInsAndDependencies.Name;
		return archiveContainingAllControlAddInsAndDependencies.Stream;
	}

	public static Stream GetDotNetAddInResourceStreamFromDatabase(string databaseServer, string databaseName, string userName, string password, string assemblyFullName, out string ideName)
	{
		ideName = null;
		NavDatabaseServer databaseServer2 = NavGlobal.SystemTenant.Database.DatabaseServer;
		if (!string.IsNullOrEmpty(databaseServer))
		{
			databaseServer2 = ((string.IsNullOrEmpty(userName) || password == null) ? new NavDatabaseServer(databaseServer, null, null) : new NavDatabaseServer(databaseServer, userName, ProtectedDatabasePassword.Create(SecureStringExtension.CreateFromUnsecureString(password))));
		}
		NavDatabase database = new NavDatabase(NavGlobal.SystemTenant, databaseServer2, databaseName, NavDatabaseType.Application);
		try
		{
			AssemblyName assemblyName = new AssemblyName(assemblyFullName);
			string value = assemblyName.FullName.Split(',', '=').Last();
			string commandText = "SELECT [Resource] FROM [Add-in] WHERE UPPER([Add-in Name]) = UPPER(@0) AND [Public Key Token] = @1" + ((assemblyName.Version == null) ? string.Empty : " AND [Version] = @2");
			ideName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", "AppsAddIns_IDE_", assemblyFullName);
			NavBLOB navBLOB = new NavBLOB();
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse))
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				navSqlCommand.SetCommandText(commandText);
				navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 220, assemblyName.Name);
				navSqlCommand.Command.Parameters.Add(1, SqlDbType.NVarChar, 20, value);
				if (assemblyName.Version != null)
				{
					navSqlCommand.Command.Parameters.Add(2, SqlDbType.NVarChar, 25, (assemblyName.Version == null) ? string.Empty : assemblyName.Version.ToString());
				}
				navSqlCommand.PrepareStatement();
				using (navSqlCommand.ExecuteReader())
				{
					while (navSqlCommand.ReadNextRow())
					{
						navSqlCommand.GetBlobDataFromReader(isCompressed: true, 0, navBLOB);
					}
				}
			}
			if (navBLOB.IsZeroOrEmpty)
			{
				assemblyFullName = NavAutomationHelper.MapOldAssemblyFullName(assemblyFullName);
				if (assemblyFullName == null)
				{
					return null;
				}
				return GetDotNetAddInResourceStreamFromDatabase(databaseServer, databaseName, userName, password, assemblyFullName, out ideName);
			}
			return navBLOB.GetStream();
		}
		catch (NavBaseException)
		{
			return null;
		}
	}
}
