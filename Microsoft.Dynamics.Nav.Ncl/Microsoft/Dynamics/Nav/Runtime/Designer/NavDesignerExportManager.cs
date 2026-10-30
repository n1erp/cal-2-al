using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public static class NavDesignerExportManager
{
	public static void GenerateZipStream(Stream outputStream, string publisher = null, string name = null, string description = null, string eula = null, Uri url = null)
	{
		GenerateZipStream(outputStream, publisher, name, description, eula, url?.AbsoluteUri);
	}

	public static void GenerateZipStream(Stream outputStream, string publisher = null, string name = null, string description = null, string eula = null, string url = null)
	{
		NavAppRuntimeMetadata val = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(NavCurrentThread.Session.Tenant.Id).SingleOrDefault((NavAppRuntimeMetadata x) => x.Name == name && x.Publisher == publisher);
		AppId val2 = ((val != null) ? val.AppId : null);
		if ((WrappedComparable<Guid>)(object)val2 != (WrappedComparable<Guid>)null)
		{
			NavDesignerExtension navDesignerExtension = GetNavDesignerExtension(val2);
			GenerateZipStream(outputStream, navDesignerExtension, publisher, name, description, eula, url);
		}
	}

	public static void GenerateZipStream(Stream outputStream, AppId appId, Version appVersion = null)
	{
		NavDesignerExtension extension = new NavDesignerExtensionFactory().CreateDevelopmentDesignerExtension(appId, appVersion);
		GenerateZipStream(outputStream, extension, null, null, null, null, null, generateDesignerManifest: false);
	}

	private static void GenerateZipStream(Stream outputStream, NavDesignerExtension extension, string publisher = null, string appName = null, string description = null, string eula = null, string url = null, bool generateDesignerManifest = true)
	{
		if (extension == null)
		{
			return;
		}
		NavAppManifest manifest = ((!generateDesignerManifest) ? extension.Manifest.WithSystemReferences() : GenerateDesignerManifest(extension.Manifest.WithSystemReferences(), publisher, appName, description, eula, url));
		using ZipArchive package = new ZipArchive(outputStream, ZipArchiveMode.Create);
		AddNavObjects(package, extension);
		AddMediaFiles(package, extension);
		AddAppJson(package, manifest);
		AddLaunchJson(package);
	}

	private static NavDesignerExtension GetNavDesignerExtension(AppId appid)
	{
		return NavCurrentThread.Session.CurrentDesignerExtension ?? new NavDesignerExtensionFactory().CreateDevelopmentDesignerExtension(appid);
	}

	private static void AddNavObjects(ZipArchive package, NavDesignerExtension extension)
	{
		AddExtensionObjectGeneric(package, extension?.GetNavObjectsSyntax());
	}

	private static void AddMediaFiles(ZipArchive package, NavDesignerExtension extension)
	{
		if (extension?.PackageFiles == null)
		{
			return;
		}
		foreach (KeyValuePair<string, byte[]> packageFile in extension.PackageFiles)
		{
			AddArchiveEntryWithPath(package, packageFile.Key, packageFile.Value);
		}
	}

	private static void AddAppJson(ZipArchive package, NavAppManifest manifest)
	{
		AddArchiveEntry(package, "app.json", GenerateAppJsonContent(manifest));
	}

	private static void AddLaunchJson(ZipArchive package)
	{
		string formattedServerUrl = GetFormattedServerUrl(ServerUserSettings.Instance.PublicWebBaseUrl?.Value);
		AddArchiveEntryWithPath(package, ".vscode\\launch.json", GenerateLaunchJsonContent(formattedServerUrl, ServerUserSettings.Instance.ServerInstance));
	}

	private static void AddExtensionObjectGeneric(ZipArchive package, IEnumerable<ObjectSyntax> navObjects)
	{
		foreach (ObjectSyntax navObject in navObjects)
		{
			string fileName = navObject.Name?.ToString() + ".al";
			string content = navObject.NormalizeWhiteSpace().ToString();
			AddArchiveEntry(package, fileName, content);
		}
	}

	private static void AddArchiveEntry(ZipArchive archive, string fileName, string content)
	{
		fileName = NavDesignerALFunctions.SanitizeDesignerFileName(fileName);
		ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(fileName);
		using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
		streamWriter.WriteLine(content ?? string.Empty);
	}

	private static void AddArchiveEntryWithPath(ZipArchive archive, string filePath, string content)
	{
		filePath = NavDesignerALFunctions.SanitizeDesignerPathName(filePath);
		ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(filePath);
		using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
		streamWriter.WriteLine(content ?? string.Empty);
	}

	private static void AddArchiveEntryWithPath(ZipArchive archive, string filePath, byte[] buffer)
	{
		filePath = NavDesignerALFunctions.SanitizeDesignerPathName(filePath);
		ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(filePath);
		using Stream stream = zipArchiveEntry.Open();
		stream.Write(buffer, 0, buffer.Length);
	}

	private static NavAppManifest GenerateDesignerManifest(NavAppManifest sessionManifest, string designerPublisher, string designerAppName, string designerAppDescription, string designerEula, string designerUrl)
	{
		try
		{
			NavAppManifest navAppManifest = NavAppManifest.FromXml(sessionManifest.ToXml());
			navAppManifest.AppPublisher = (string.IsNullOrWhiteSpace(designerPublisher) ? sessionManifest.AppPublisher : designerPublisher);
			navAppManifest.AppName = (string.IsNullOrWhiteSpace(designerAppName) ? sessionManifest.AppName : designerAppName);
			navAppManifest.AppDescription = (string.IsNullOrWhiteSpace(designerAppDescription) ? sessionManifest.AppDescription : designerAppDescription);
			navAppManifest.AppEula = (string.IsNullOrWhiteSpace(designerEula) ? sessionManifest.AppEula : designerEula);
			navAppManifest.AppUrl = (string.IsNullOrWhiteSpace(designerUrl) ? sessionManifest.AppUrl : designerUrl);
			return navAppManifest;
		}
		catch (NullReferenceException ex)
		{
			Exception innerException = ex;
			throw NavAppObjectMetadataException.Create(ex.Message, innerException);
		}
	}

	private static string GenerateAppJsonContent(NavAppManifest manifest)
	{
		if (manifest == null)
		{
			return string.Empty;
		}
		JObject jObject = new JObject(new JProperty("id", manifest.AppId), new JProperty("name", manifest.AppName), new JProperty("publisher", manifest.AppPublisher), new JProperty("brief", manifest.AppBrief), new JProperty("description", manifest.AppDescription), new JProperty("version", manifest.AppVersion.ToString()), new JProperty("privacyStatement", manifest.AppPrivacyStatement), new JProperty("EULA", manifest.AppEula), new JProperty("help", manifest.AppHelp), new JProperty("url", manifest.AppUrl), new JProperty("logo", manifest.AppLogo), new JProperty("capabilities", new JArray()), new JProperty("dependencies", GenerateDependencySetting(manifest.Dependencies)), new JProperty("screenshots", new JArray()), new JProperty("platform", manifest.Platform?.ToString()), new JProperty("application", manifest.Application.ToString()), new JProperty("target", manifest.Target.ToString()), new JProperty("showMyCode", manifest.ShowMyCode), new JProperty("idRanges", GenerateIdRangesSetting(manifest.IdSpaces)));
		return jObject.ToString();
	}

	private static JArray GenerateDependencySetting(IEnumerable<NavAppDependency> dependencyList)
	{
		JArray jArray = new JArray();
		foreach (NavAppDependency dependency in dependencyList)
		{
			jArray.Add(new JObject(new JProperty("name", dependency.Name), new JProperty("publisher", dependency.Publisher), new JProperty("version", dependency.MinVersion.ToString()), new JProperty("appId", dependency.AppId)));
		}
		return jArray;
	}

	private static JArray GenerateIdRangesSetting(IdSpaces idSpaces)
	{
		JArray jArray = new JArray();
		foreach (IdSpace applicationObjectIdRange in idSpaces.ApplicationObjectIdRanges)
		{
			jArray.Add(new JObject(new JProperty("from", applicationObjectIdRange.ApplicationObjectIdRangeStart), new JProperty("to", applicationObjectIdRange.ApplicationObjectIdRangeEnd)));
		}
		return jArray;
	}

	private static string GenerateLaunchJsonContent(string serverUrl, string serverInstance)
	{
		JObject jObject = new JObject(new JProperty("version", "0.2.0"), new JProperty("configurations", new JArray(GetLaunchConfiguration("Publish", serverUrl, serverInstance))));
		return jObject.ToString();
	}

	private static string GetFormattedServerUrl(string url)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return url;
		}
		string text = (ServerUserSettings.Instance.DeveloperServicesSSLEnabled.Value ? "https://" : "http://");
		string text2 = text + new Uri(url).Host;
		return text2 + ":" + ServerUserSettings.Instance.DeveloperServicesPort;
	}

	private static JObject GetLaunchConfiguration(string name, string serverUrl, string serverInstance)
	{
		JObject jObject = new JObject
		{
			new JProperty("type", "al"),
			new JProperty("request", "launch"),
			new JProperty("name", name)
		};
		NavSession session = NavCurrentThread.Session;
		if (session.Authenticator.AuthenticationMethod != NavClientCredentialType.AccessControlService)
		{
			NavClientCredentialType value = ServerUserSettings.Instance.ClientServicesCredentialType.Value;
			jObject.Add(new JProperty("server", serverUrl));
			jObject.Add(new JProperty("serverInstance", serverInstance));
			jObject.Add(new JProperty("authentication", (value == NavClientCredentialType.Windows) ? "Windows" : "UserPassword"));
		}
		else
		{
			string initialDomainNameSafe = GetInitialDomainNameSafe(session);
			if (!string.IsNullOrEmpty(initialDomainNameSafe))
			{
				jObject.Add(new JProperty("tenant", initialDomainNameSafe));
			}
		}
		return jObject;
	}

	private static string GetInitialDomainNameSafe(NavSession session)
	{
		try
		{
			return GraphTableConnection.GetInitialDomainFromAad(session);
		}
		catch (NavBaseException exception)
		{
			session.Diagnostics.SendExceptionTag("00001UK", Category.Extensions, exception);
			return string.Empty;
		}
	}
}
