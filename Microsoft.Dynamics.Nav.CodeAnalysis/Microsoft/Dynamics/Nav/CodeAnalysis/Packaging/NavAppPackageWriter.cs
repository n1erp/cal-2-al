using System;
using System.IO;
using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppPackageWriter : PackageWriter
{
	private static Version assemblyFileVersion;

	private static Version AssemblyFileVersion
	{
		get
		{
			if (assemblyFileVersion != null)
			{
				return assemblyFileVersion;
			}
			Assembly assembly = typeof(NavAppPackageWriter).Assembly;
			object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				assemblyFileVersion = new Version(((AssemblyFileVersionAttribute)customAttributes[0]).Version);
			}
			else
			{
				assemblyFileVersion = assembly.GetName().Version;
			}
			return assemblyFileVersion;
		}
	}

	internal NavAppPackage NavAppPackage { get; private set; }

	public NavAppPackageWriter(NavAppPackage appPackage)
		: base(appPackage.ContentStream)
	{
		NavAppPackage = appPackage;
	}

	public static NavAppPackageWriter Create(Stream stream)
	{
		return new NavAppPackageWriter(NavAppPackage.Create(stream));
	}

	public static NavAppPackageWriter Create(Stream stream, Guid guid)
	{
		return new NavAppPackageWriter(NavAppPackage.Create(stream, guid, neaContent: false));
	}

	public static NavAppPackageWriter CreateNeaPackage(Stream stream)
	{
		return new NavAppPackageWriter(NavAppPackage.CreateNeaPackage(stream));
	}

	public static void CreateSymbolPackage(Stream outputStream, ModuleDefinition module, string name, Version version)
	{
		Version appCompatibilityId = AssemblyFileVersion;
		Version version2 = new Version(1, 0, 0, 0);
		Guid appId = Guid.NewGuid();
		string text = "Microsoft";
		module.Name = name;
		module.AppId = appId;
		module.Publisher = text;
		module.Version = version.ToString();
		NavAppManifest navAppManifest = new NavAppManifest
		{
			AppId = appId,
			AppName = name,
			AppPublisher = text,
			AppDescription = name,
			AppCompatibilityId = appCompatibilityId,
			AppVersion = version,
			Platform = version2,
			AppBrief = "",
			AppEula = "",
			AppHelp = "",
			AppLogo = "",
			AppHelpBaseUrl = "",
			ContextSensitiveHelpUrl = "",
			AppSupportedLocales = new NavAppSupportedLocales(Array.Empty<string>()),
			AppPrivacyStatement = "",
			AppScreenshots = Array.Empty<string>(),
			AppUrl = "",
			Dependencies = Array.Empty<NavAppDependency>()
		};
		if (name == "Application")
		{
			module.AddBuiltInControlAddIns();
		}
		else if (name == "Test")
		{
			navAppManifest.Application = version2;
		}
		using NavAppPackageWriter navAppPackageWriter = Create(outputStream);
		navAppPackageWriter.WriteString(navAppManifest.ToXml(), "/NavxManifest.xml");
		using Stream stream = navAppPackageWriter.GetPartStream("/SymbolReference.json");
		SymbolReferenceJsonWriter.WriteModule(stream, module);
	}

	public static void CreateSymbolPackage(Stream inputStream, Stream symbolStream, NavAppManifest manifest)
	{
		using NavAppPackageWriter navAppPackageWriter = Create(inputStream);
		navAppPackageWriter.WriteString(manifest.ToXml(), "/NavxManifest.xml");
		navAppPackageWriter.WriteStream(symbolStream, "/SymbolReference.json");
	}

	protected override void Dispose(bool disposing)
	{
		bool disposed = Disposed;
		base.Dispose(disposing);
		if ((!disposed & disposing) && NavAppPackage != null)
		{
			NavAppPackage.Dispose();
			NavAppPackage = null;
		}
	}
}
