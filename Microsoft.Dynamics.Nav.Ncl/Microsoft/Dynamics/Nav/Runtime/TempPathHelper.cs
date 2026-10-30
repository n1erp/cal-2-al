using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TempPathHelper : IDisposable
{
	private readonly bool instanceFolderNameBasedOnProcessId;

	private readonly string instanceBasePath;

	private readonly string configurableBasePath;

	private string instanceFolderName;

	private const string NavTempFolderName = "temp";

	private const string NavUsersFolderName = "users";

	private const string TranslationMasterFileName = "TranslationMaster.lang";

	private const string BaseFolderName = "base";

	private const string StaticSourceFolderName = "staticsource";

	private const string StaticAssemblyFolderName = "staticassembly";

	private const string StaticSubscribersFolderName = "subscribers";

	private const string StaticPublisherFolderName = "publishers";

	private const string BaseSourceFolderName = "source";

	private const string BaseAssemblyFolderName = "assembly";

	private const string AppsFolderName = "apps";

	private const string MetadataFolderName = "metadata";

	private const string AssemblyFolderName = "assembly";

	private bool disposed;

	private List<string> classSpecificSourceFolders = new List<string>();

	public string TempPath => Path.Combine(instanceBasePath, "temp");

	public string UserPath => Path.Combine(instanceBasePath, "users");

	public string StaticSourcePath => Path.Combine(configurableBasePath, "base", "staticsource");

	public string StaticAssemblyPath => Path.Combine(configurableBasePath, "base", "staticassembly");

	public string BaseSourcePath => Path.Combine(configurableBasePath, "base", "source");

	public string BaseAssemblyPath => Path.Combine(configurableBasePath, "base", "assembly");

	public string StaticPublisherAssemblyPath => Path.Combine(configurableBasePath, "base", "staticassembly", "publishers");

	public string StaticSubscriberAssemblyPath => Path.Combine(configurableBasePath, "base", "staticassembly", "subscribers");

	public string AppMetadataPath => Path.Combine(configurableBasePath, "apps", "metadata");

	public string AppAssemblyPath => Path.Combine(configurableBasePath, "apps", "assembly");

	public string TranslationMasterFile => Path.Combine(configurableBasePath, "TranslationMaster.lang");

	public TempPathHelper(string folderName)
	{
		if (string.IsNullOrEmpty(folderName))
		{
			instanceFolderName = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
			instanceFolderNameBasedOnProcessId = true;
		}
		else
		{
			instanceFolderName = folderName;
		}
		instanceBasePath = Path.Combine(ProductApplicationData.ServerPath, instanceFolderName);
		configurableBasePath = ServerUserSettings.Instance.ServerFileCacheDirectory.Value;
		if (string.IsNullOrEmpty(configurableBasePath) || !ShouldLeaveSourceFilesOnDiskOnShutdown())
		{
			configurableBasePath = instanceBasePath;
		}
		InitializeFolders();
	}

	~TempPathHelper()
	{
		Dispose(disposing: false);
	}

	private void InitializeFolders()
	{
		DirectorySecurity directorySecurity = NavDirectorySecurity.CreateSecurityForDomainDirectory();
		CreatePathIfNonExistent(ProductApplicationData.ServerPath, directorySecurity);
		CreatePathIfNonExistent(instanceBasePath, directorySecurity);
		CreatePathIfNonExistent(configurableBasePath, directorySecurity);
		CreateInstanceSubfolders(directorySecurity);
	}

	private void CreateInstanceSubfolders(DirectorySecurity directorySecurity)
	{
		CreatePathIfNonExistent(StaticSourcePath, directorySecurity);
		CreatePathIfNonExistent(StaticAssemblyPath, directorySecurity);
		CreatePathIfNonExistent(StaticPublisherAssemblyPath, directorySecurity);
		CreatePathIfNonExistent(StaticSubscriberAssemblyPath, directorySecurity);
		CreatePathIfNonExistent(BaseSourcePath, directorySecurity);
		CreatePathIfNonExistent(BaseAssemblyPath, directorySecurity);
		CreatePathIfNonExistent(AppMetadataPath, directorySecurity);
		CreatePathIfNonExistent(AppAssemblyPath, directorySecurity);
		ObjectType[] array = new ObjectType[9]
		{
			ObjectType.Table,
			ObjectType.Page,
			ObjectType.Report,
			ObjectType.CodeUnit,
			ObjectType.XmlPort,
			ObjectType.Query,
			ObjectType.MenuSuite,
			ObjectType.PageExtension,
			ObjectType.TableExtension
		};
		ObjectType[] array2 = array;
		foreach (ObjectType objectType in array2)
		{
			CreateClassSpecificSourceFolder(Path.Combine(StaticSourcePath, ObjectTypeExtensions.ToUniqueShortString(objectType)), directorySecurity);
			CreateClassSpecificSourceFolder(Path.Combine(BaseSourcePath, ObjectTypeExtensions.ToUniqueShortString(objectType)), directorySecurity);
			CreateClassSpecificSourceFolder(Path.Combine(BaseAssemblyPath, ObjectTypeExtensions.ToUniqueShortString(objectType)), directorySecurity);
		}
		CreatePathIfNonExistent(TempPath, directorySecurity);
		CreatePathIfNonExistent(UserPath, directorySecurity);
	}

	private void CreateClassSpecificSourceFolder(string path, DirectorySecurity directorySecurity)
	{
		CreatePathIfNonExistent(path, directorySecurity);
		classSpecificSourceFolders.Add(path);
	}

	private static void RemovePath(string path)
	{
		try
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		catch (IOException exception)
		{
			LogWriterHelper.LogExceptionEvent(exception);
		}
		catch (UnauthorizedAccessException exception2)
		{
			LogWriterHelper.LogExceptionEvent(exception2);
		}
		catch (ArgumentException exception3)
		{
			LogWriterHelper.LogExceptionEvent(exception3);
		}
	}

	private static void CreatePathIfNonExistent(string path, DirectorySecurity directorySecurity)
	{
		if (!Directory.Exists(path))
		{
			try
			{
				NavDirectorySecurity.CreateDirectoryWithFolderSecurity(path, directorySecurity, null);
			}
			catch (IOException innerException)
			{
				throw new NavNCLDirectoryCreationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToCreateServiceInstancePath, path), innerException);
			}
			catch (UnauthorizedAccessException innerException2)
			{
				throw new NavNCLDirectoryCreationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToCreateServiceInstancePath, path), innerException2);
			}
			catch (ArgumentException innerException3)
			{
				throw new NavNCLDirectoryCreationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToCreateServiceInstancePath, path), innerException3);
			}
			catch (NotSupportedException innerException4)
			{
				throw new NavNCLDirectoryCreationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToCreateServiceInstancePath, path), innerException4);
			}
		}
	}

	private void Dispose(bool disposing)
	{
		if (disposed || instanceBasePath == null)
		{
			return;
		}
		RemovePath(TempPath);
		if (!ServerUserSettings.Instance.CompileBusinessApplicationAtStartup.Value)
		{
			RemovePath(StaticAssemblyPath);
		}
		RemovePath(UserPath);
		if (!ShouldLeaveSourceFilesOnDiskOnShutdown())
		{
			RemovePath(StaticSourcePath);
			DirectoryInfo directoryInfo = new DirectoryInfo(instanceBasePath);
			if (directoryInfo.GetFiles().Length == 0)
			{
				RemovePath(instanceBasePath);
			}
		}
		disposed = true;
	}

	private bool ShouldLeaveSourceFilesOnDiskOnShutdown()
	{
		if ((ServerUserSettings.Instance.CompileBusinessApplicationAtStartup.Value || ServerUserSettings.Instance.EnableDebugging.Value) && !instanceFolderNameBasedOnProcessId)
		{
			return !NavEnvironment.Instance.StandaloneInstance;
		}
		return false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
