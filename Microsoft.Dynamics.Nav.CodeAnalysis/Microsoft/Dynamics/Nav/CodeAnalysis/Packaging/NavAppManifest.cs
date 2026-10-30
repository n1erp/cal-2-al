using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppManifest
{
	private static readonly XNamespace Namespace = "http://schemas.microsoft.com/navx/2015/manifest";

	private static readonly List<NavAppDependency> NoDependencies = new List<NavAppDependency>();

	private Version application;

	private Version platform;

	private Version test;

	private IEnumerable<NavAppDependency> dependencies;

	public Guid AppId { get; set; }

	public string AppName { get; set; }

	public string AppPublisher { get; set; }

	public string AppDescription { get; set; }

	public string AppBrief { get; set; }

	public Version AppVersion { get; set; }

	public string AppPrivacyStatement { get; set; }

	public string AppEula { get; set; }

	[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
	public string AppUrl { get; set; }

	public string AppHelp { get; set; }

	public string AppLogo { get; set; }

	public IEnumerable<string> AppScreenshots { get; set; }

	public bool ShowMyCode { get; set; }

	[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
	public string AppHelpBaseUrl { get; set; }

	[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
	public string ContextSensitiveHelpUrl { get; set; }

	public NavAppSupportedLocales AppSupportedLocales { get; set; }

	public IEnumerable<NavAppDependency> Dependencies
	{
		get
		{
			return dependencies;
		}
		set
		{
			dependencies = value;
			if (dependencies != null)
			{
				DependencyReferences = dependencies.Select((NavAppDependency x) => new SymbolReferenceSpecification(x.Publisher, x.Name, x.MinVersion, exact: false, x.AppId)).ToImmutableArray();
			}
			else
			{
				DependencyReferences = null;
			}
		}
	}

	public Version Application
	{
		get
		{
			return application;
		}
		set
		{
			application = value;
			if (application != null)
			{
				AppReference = SymbolReferenceSpecification.ApplicationReference(application);
			}
			else
			{
				AppReference = null;
			}
		}
	}

	public Version Test
	{
		get
		{
			return test;
		}
		set
		{
			test = value;
			if (test != null)
			{
				TestReference = SymbolReferenceSpecification.TestReference(test);
			}
			else
			{
				TestReference = null;
			}
		}
	}

	public Version Platform
	{
		get
		{
			return platform;
		}
		set
		{
			platform = value;
			if (platform != null)
			{
				PlatformReference = SymbolReferenceSpecification.PlatformReference(platform);
			}
			else
			{
				PlatformReference = null;
			}
		}
	}

	public Version Runtime { get; set; } = RuntimeVersion.CurrentRelease;

	public CompilationTarget Target { get; set; } = CompilationTarget.Extension;

	public IdSpaces IdSpaces { get; set; } = IdSpaces.BaseAppDevelopmentIdSpaces;

	public CompilerFeatures CompilerFeatures { get; set; }

	public Version AppCompatibilityId { get; set; }

	public SymbolReferenceSpecification AppReference { get; private set; }

	public SymbolReferenceSpecification PlatformReference { get; private set; }

	public SymbolReferenceSpecification TestReference { get; private set; }

	public IEnumerable<SymbolReferenceSpecification> DependencyReferences { get; private set; }

	internal bool ReadonlyErrorsAsWarnings { get; set; }

	internal Dictionary<string, Location> PropertyLocations { get; } = new Dictionary<string, Location>();

	internal string ManifestLocation { get; set; }

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public IEnumerable<SymbolReferenceSpecification> GetAllReferences()
	{
		if (AppReference != null)
		{
			yield return AppReference;
		}
		if (PlatformReference != null)
		{
			yield return PlatformReference;
		}
		if (TestReference != null)
		{
			yield return TestReference;
		}
		if (DependencyReferences == null)
		{
			yield break;
		}
		foreach (SymbolReferenceSpecification dependencyReference in DependencyReferences)
		{
			yield return dependencyReference;
		}
	}

	public void UpdateDependencies(ImmutableArray<SymbolReferenceSpecification> referenceSpecifications)
	{
		ArrayBuilder<NavAppDependency> instance = ArrayBuilder<NavAppDependency>.GetInstance(referenceSpecifications.Length);
		try
		{
			Application = null;
			Platform = null;
			Test = null;
			AppReference = null;
			PlatformReference = null;
			TestReference = null;
			foreach (SymbolReferenceSpecification item in referenceSpecifications)
			{
				if (item.IsApplicationSymbolReference())
				{
					AppReference = item;
					Application = item.Version;
					continue;
				}
				if (item.IsPlatformSymbolReference())
				{
					PlatformReference = item;
					Platform = item.Version;
					continue;
				}
				if (item.IsTestSymbolReference())
				{
					TestReference = item;
					Test = item.Version;
					continue;
				}
				instance.Add(new NavAppDependency
				{
					AppId = item.AppId,
					MinVersion = item.Version,
					Name = item.Name,
					Publisher = item.Publisher,
					CompatibilityId = new Version(1, 0, 0, 0)
				});
			}
			Dependencies = instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	public void Validate(string projectManifestPath, IList<Diagnostic> diagnostics)
	{
		string directoryName = Path.GetDirectoryName(projectManifestPath);
		ValidateProperties(directoryName, diagnostics);
	}

	public void ValidateDependencies(IList<Diagnostic> diagnostics)
	{
		ValidateManifestDependencies(diagnostics);
	}

	public string DefaultPackageName()
	{
		return FileNameUtilities.SanitizeFilename(string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", AppPublisher, AppName, AppVersion));
	}

	internal void SetPropertyLocations(Dictionary<string, Location> propertyLocations)
	{
		PropertyLocations.Clear();
		foreach (KeyValuePair<string, Location> propertyLocation in propertyLocations)
		{
			PropertyLocations.Add(propertyLocation.Key, propertyLocation.Value);
		}
	}

	public Location GetDiagnosticLocation(string propertyName)
	{
		if (PropertyLocations.TryGetValue(propertyName, out var value))
		{
			return value;
		}
		if (string.IsNullOrEmpty(ManifestLocation))
		{
			return Location.None;
		}
		TextSpan textSpan = new TextSpan(0, 1);
		LinePosition start = new LinePosition(0, 0);
		LinePosition end = new LinePosition(0, 0);
		return Location.Create(lineSpan: new LinePositionSpan(start, end), filePath: ManifestLocation, textSpan: textSpan);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static NavAppManifest FromXml(string document)
	{
		if (string.IsNullOrWhiteSpace(document))
		{
			return null;
		}
		NavAppManifest navAppManifest = new NavAppManifest();
		XElement xElement = XElement.Parse(document);
		XElement xElement2 = xElement.Element(Namespace + "App");
		if (xElement2 != null)
		{
			XAttribute xAttribute = xElement2.Attribute("Id");
			XAttribute xAttribute2 = xElement2.Attribute("Name");
			XAttribute xAttribute3 = xElement2.Attribute("Publisher");
			XAttribute xAttribute4 = xElement2.Attribute("Brief");
			XAttribute xAttribute5 = xElement2.Attribute("Description");
			XAttribute xAttribute6 = xElement2.Attribute("Version");
			XAttribute xAttribute7 = xElement2.Attribute("CompatibilityId");
			XAttribute xAttribute8 = xElement2.Attribute("PrivacyStatement");
			XAttribute xAttribute9 = xElement2.Attribute("EULA");
			XAttribute xAttribute10 = xElement2.Attribute("Url");
			XAttribute xAttribute11 = xElement2.Attribute("Logo");
			XAttribute xAttribute12 = xElement2.Attribute("Help");
			XAttribute xAttribute13 = xElement2.Attribute("HelpBaseUrl");
			XAttribute xAttribute14 = xElement2.Attribute("ContextSensitiveHelpUrl");
			XAttribute xAttribute15 = xElement2.Attribute("Platform");
			XAttribute xAttribute16 = xElement2.Attribute("Application");
			XAttribute xAttribute17 = xElement2.Attribute("Test");
			XAttribute xAttribute18 = xElement2.Attribute("Target");
			XAttribute xAttribute19 = xElement2.Attribute("Runtime");
			XAttribute xAttribute20 = xElement2.Attribute("ShowMyCode");
			navAppManifest.AppId = new Guid(xAttribute.Value);
			navAppManifest.AppName = xAttribute2?.Value;
			navAppManifest.AppPublisher = xAttribute3?.Value;
			navAppManifest.AppBrief = xAttribute4?.Value ?? string.Empty;
			navAppManifest.AppDescription = xAttribute5?.Value ?? string.Empty;
			navAppManifest.AppVersion = ((xAttribute6 == null) ? null : new Version(xAttribute6.Value));
			navAppManifest.AppCompatibilityId = ((xAttribute7 == null) ? null : new Version(xAttribute7.Value));
			navAppManifest.AppPrivacyStatement = xAttribute8?.Value ?? string.Empty;
			navAppManifest.AppEula = xAttribute9?.Value ?? string.Empty;
			navAppManifest.AppUrl = xAttribute10?.Value ?? string.Empty;
			navAppManifest.AppHelp = xAttribute12?.Value ?? string.Empty;
			navAppManifest.AppHelpBaseUrl = xAttribute13?.Value ?? string.Empty;
			navAppManifest.ContextSensitiveHelpUrl = xAttribute14?.Value;
			navAppManifest.AppLogo = xAttribute11?.Value;
			navAppManifest.Platform = ((xAttribute15 == null) ? null : new Version(xAttribute15.Value));
			navAppManifest.Application = ((xAttribute16 == null) ? null : new Version(xAttribute16.Value));
			navAppManifest.Test = ((xAttribute17 == null) ? null : new Version(xAttribute17.Value));
			navAppManifest.Runtime = ((xAttribute19 == null) ? null : new Version(xAttribute19.Value));
			navAppManifest.Target = ((xAttribute18 == null) ? CompilationTarget.Extension : CompilationTargetHelpers.ParseSafe(xAttribute18.Value));
			navAppManifest.ShowMyCode = xAttribute20 != null && bool.Parse(xAttribute20.Value);
		}
		navAppManifest.IdSpaces = GetIdSpaces(xElement);
		XElement xElement3 = xElement.Element(Namespace + "Dependencies");
		if (xElement3 != null)
		{
			IEnumerable<NavAppDependency> source = from x in xElement3.Elements(Namespace + "Dependency").Select((XElement element) =>
				{
					XAttribute xAttribute21 = element.Attribute("Id");
					XAttribute xAttribute22 = element.Attribute("Name");
					XAttribute xAttribute23 = element.Attribute("Publisher");
					XAttribute xAttribute24 = element.Attribute("MinVersion");
					XAttribute xAttribute25 = element.Attribute("CompatibilityId");
					Guid result;
					Version result2;
					Version result3;
					return (xAttribute21 == null || xAttribute22 == null || xAttribute23 == null || xAttribute24 == null || xAttribute25 == null || !Guid.TryParse(xAttribute21.Value, out result) || !Version.TryParse(xAttribute24.Value, out result2) || !Version.TryParse(xAttribute25.Value, out result3)) ? null : new NavAppDependency
					{
						CompatibilityId = result3,
						MinVersion = result2,
						AppId = result,
						Name = xAttribute22.Value,
						Publisher = xAttribute23.Value
					};
				})
				where x != null
				select x;
			navAppManifest.Dependencies = source.ToList();
		}
		else
		{
			navAppManifest.Dependencies = NoDependencies;
		}
		XElement xElement4 = xElement.Element(Namespace + "ScreenShots");
		if (xElement4 != null)
		{
			List<string> list = new List<string>();
			foreach (string item in from element in xElement4.Elements(Namespace + "ScreenShot")
				select element.Attribute("Path") into attribute
				where attribute != null
				select attribute.Value)
			{
				if (!string.IsNullOrWhiteSpace(item))
				{
					list.Add(item);
				}
			}
			navAppManifest.AppScreenshots = list;
		}
		XElement xElement5 = xElement.Element(Namespace + "SupportedLocales");
		if (xElement5 != null)
		{
			ArrayBuilder<string> arrayBuilder = new ArrayBuilder<string>();
			foreach (string item2 in from element in xElement5.Elements(Namespace + "SupportedLocale")
				select element.Attribute("Local") into attribute
				where attribute != null
				select attribute.Value)
			{
				if (!string.IsNullOrWhiteSpace(item2))
				{
					arrayBuilder.Add(item2);
				}
			}
			navAppManifest.AppSupportedLocales = new NavAppSupportedLocales(arrayBuilder.ToArrayAndFree());
		}
		return navAppManifest;
	}

	private static IdSpaces GetIdSpaces(XElement root)
	{
		(bool, IdSpaces) idSpacesFromRange = GetIdSpacesFromRange(root);
		if (idSpacesFromRange.Item1)
		{
			return idSpacesFromRange.Item2;
		}
		idSpacesFromRange = GetIdSpacesFromRanges(root);
		if (idSpacesFromRange.Item1)
		{
			return idSpacesFromRange.Item2;
		}
		return IdSpaces.BaseAppDevelopmentIdSpaces;
	}

	private static (bool result, IdSpaces idSpaces) GetIdSpacesFromRanges(XElement root)
	{
		XElement xElement = root.Element(Namespace + "IdRanges");
		if (xElement != null)
		{
			IEnumerable<IdSpace> source = from x in xElement.Elements(Namespace + "IdRange").Select((XElement element) =>
				{
					XAttribute xAttribute = element.Attribute("MinObjectId");
					XAttribute xAttribute2 = element.Attribute("MaxObjectId");
					int result;
					int result2;
					return (xAttribute == null || xAttribute2 == null || !int.TryParse(xAttribute.Value, out result) || !int.TryParse(xAttribute2.Value, out result2)) ? null : new IdSpace(result, result2);
				})
				where x != null
				select x;
			return (result: true, idSpaces: new IdSpaces(source.ToArray()));
		}
		return (result: false, idSpaces: null);
	}

	private static (bool result, IdSpaces idSpaces) GetIdSpacesFromRange(XElement root)
	{
		XElement xElement = root.Element(Namespace + "IdRange");
		if (xElement != null)
		{
			XAttribute xAttribute = xElement.Attribute("MinObjectId");
			XAttribute xAttribute2 = xElement.Attribute("MaxObjectId");
			IdSpace idSpace = ((xAttribute != null && xAttribute2 != null && int.TryParse(xAttribute.Value, out var result) && int.TryParse(xAttribute2.Value, out var result2)) ? new IdSpace(result, result2) : IdSpace.BaseAppDevelopmentIdSpace);
			return (result: true, idSpaces: new IdSpaces(new IdSpace[1] { idSpace }));
		}
		return (result: false, idSpaces: null);
	}

	public string ToXml()
	{
		XElement xElement = new XElement(Namespace + "App", new XAttribute("Id", AppId.ToString()), new XAttribute("Name", AppName ?? string.Empty), new XAttribute("Publisher", AppPublisher ?? string.Empty), new XAttribute("Brief", AppBrief ?? string.Empty), new XAttribute("Description", AppDescription ?? string.Empty), new XAttribute("Version", (AppVersion != null) ? AppVersion.ToString() : string.Empty), new XAttribute("CompatibilityId", (AppCompatibilityId != null) ? AppCompatibilityId.ToString() : string.Empty), new XAttribute("PrivacyStatement", AppPrivacyStatement ?? string.Empty), new XAttribute("EULA", AppEula ?? string.Empty), new XAttribute("Help", AppHelp ?? string.Empty), new XAttribute("HelpBaseUrl", AppHelpBaseUrl ?? string.Empty), (ContextSensitiveHelpUrl == null) ? null : new XAttribute("ContextSensitiveHelpUrl", ContextSensitiveHelpUrl ?? string.Empty), new XAttribute("Url", AppUrl ?? string.Empty), new XAttribute("Logo", string.IsNullOrEmpty(AppLogo) ? string.Empty : Path.Combine("/logo/", Path.GetFileName(AppLogo))), (Platform == null) ? null : new XAttribute("Platform", Platform.ToString()), (Application == null) ? null : new XAttribute("Application", Application.ToString()), (Test == null) ? null : new XAttribute("Test", Test.ToString()), (Runtime == null) ? null : new XAttribute("Runtime", Runtime.ToString()), new XAttribute("Target", Target.ToString()), new XAttribute("ShowMyCode", ShowMyCode.ToString()));
		XElement xElement2 = ((!IsCompatible("IdRanges")) ? new XElement(Namespace + "IdRange", new XAttribute("MinObjectId", IdSpaces.ApplicationObjectIdRanges.FirstOrDefault()?.ApplicationObjectIdRangeStart), new XAttribute("MaxObjectId", IdSpaces.ApplicationObjectIdRanges.LastOrDefault()?.ApplicationObjectIdRangeEnd)) : new XElement(Namespace + "IdRanges", (IdSpaces == null) ? null : IdSpaces.ApplicationObjectIdRanges.Select((IdSpace space) => new XElement(Namespace + "IdRange", new XAttribute("MinObjectId", space.ApplicationObjectIdRangeStart), new XAttribute("MaxObjectId", space.ApplicationObjectIdRangeEnd))).Cast<object>().ToArray()));
		XElement xElement3 = new XElement(Namespace + "Dependencies", (Dependencies == null) ? null : Dependencies.Select((NavAppDependency dependency) => new XElement(Namespace + "Dependency", new XAttribute("Id", dependency.AppId), new XAttribute("Name", dependency.Name), new XAttribute("Publisher", dependency.Publisher), new XAttribute("MinVersion", dependency.MinVersion), new XAttribute("CompatibilityId", dependency.CompatibilityId))).Cast<object>().ToArray());
		XElement xElement4 = new XElement(Namespace + "ScreenShots", (AppScreenshots == null) ? null : AppScreenshots.Select((string screenShot) => new XElement(Namespace + "ScreenShot", new XAttribute("Path", Path.Combine("/media/", Path.GetFileName(screenShot))))).Cast<object>().ToArray());
		XElement xElement5 = new XElement(Namespace + "SupportedLocales", (AppSupportedLocales == null) ? null : AppSupportedLocales.Locales.Select((string local) => new XElement(Namespace + "SupportedLocale", new XAttribute("Local", local))).Cast<object>().ToArray());
		return new XDocument(new XElement(Namespace + "Package", new XAttribute("xmlns", Namespace), xElement, xElement2, xElement3, xElement4, xElement5)).ToString();
	}

	private bool IsCompatible(string key)
	{
		return GetCompatibleVersion(key)?.IsSupported(Runtime ?? RuntimeVersion.CurrentRelease) ?? true;
	}

	private VersionCompatibility GetCompatibleVersion(string key)
	{
		if (NavAppManifestCompatibility.ManifestPropertiesInfos.TryGetValue(key.ToUpperInvariant(), out var value))
		{
			return value.Compatibility;
		}
		return null;
	}

	internal static bool TryParse(string document, List<Diagnostic> diagnostics, out NavAppManifest manifest)
	{
		manifest = null;
		try
		{
			manifest = FromXml(document);
		}
		catch (XmlException ex)
		{
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1017, ex.Message));
		}
		return manifest != null;
	}

	private void ValidateProperties(string searchPath, IList<Diagnostic> diagnostics)
	{
		if (!string.IsNullOrEmpty(AppPrivacyStatement))
		{
			ValidateHttpUri(AppPrivacyStatement, "privacyStatement", diagnostics);
		}
		if (!string.IsNullOrEmpty(AppEula))
		{
			ValidateHttpUri(AppEula, "EULA", diagnostics);
		}
		if (!string.IsNullOrEmpty(AppHelp))
		{
			ValidateHttpUri(AppHelp, "help", diagnostics);
		}
		if (!string.IsNullOrEmpty(AppUrl))
		{
			ValidateHttpUri(AppUrl, "url", diagnostics);
		}
		if (!string.IsNullOrEmpty(AppLogo))
		{
			string text = PathUtilities.ResolveRelativePath(AppLogo, searchPath);
			if (!FileUtilities.ValidateFileExists(text))
			{
				AddDiagnostic("logo", diagnostics, ErrorCode.ERR_FileNotFound, AppLogo);
			}
			AppLogo = text;
		}
		if (AppScreenshots != null)
		{
			List<string> list = new List<string>();
			foreach (string appScreenshot in AppScreenshots)
			{
				string text2 = PathUtilities.ResolveRelativePath(appScreenshot, searchPath);
				if (!FileUtilities.ValidateFileExists(text2))
				{
					AddDiagnostic("screenshots", diagnostics, ErrorCode.ERR_FileNotFound, appScreenshot);
				}
				else
				{
					list.Add(text2);
				}
			}
			AppScreenshots = list;
		}
		ValidateHelpDocumentationProperties(diagnostics);
	}

	private void ValidateHelpDocumentationProperties(IList<Diagnostic> diagnostics)
	{
		bool flag = ValidateHelpBaseUrl("helpBaseUrl", AppHelpBaseUrl, mandatoryLocalePlaceholder: true, diagnostics);
		bool flag2 = ValidateHelpBaseUrl("contextSensitiveHelpUrl", ContextSensitiveHelpUrl, mandatoryLocalePlaceholder: false, diagnostics);
		bool flag3 = false;
		if (AppSupportedLocales != null)
		{
			if (AppSupportedLocales.Locales.Length == 0)
			{
				AddDiagnostic("supportedLocales", diagnostics, ErrorCode.ERR_PropertyValueCannotBeEmpty, "supportedLocales");
			}
			else
			{
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				string[] locales = AppSupportedLocales.Locales;
				foreach (string text in locales)
				{
					if (hashSet.Contains(text, StringComparer.OrdinalIgnoreCase))
					{
						AddDiagnostic("supportedLocales", diagnostics, ErrorCode.ERR_DuplicateLocale, text);
					}
					flag3 = true;
					if (!IsValidCulture(text))
					{
						AddDiagnostic("supportedLocales", diagnostics, ErrorCode.ERR_InvalidTranslationLocaleCulture, text);
					}
					hashSet.Add(text);
				}
			}
		}
		if (!flag3)
		{
			if (flag)
			{
				AddDiagnostic("helpBaseUrl", diagnostics, ErrorCode.ERR_MissingPropertyForTranslationApp);
			}
			if (flag2 && ContextSensitiveHelpUrl.Contains("{0}"))
			{
				AddDiagnostic("contextSensitiveHelpUrl", diagnostics, ErrorCode.ERR_MissingSupportedLocalesManifestProperty, "contextSensitiveHelpUrl");
			}
		}
	}

	private bool ValidateHelpBaseUrl(string propertyName, string propertyValue, bool mandatoryLocalePlaceholder, IList<Diagnostic> diagnostics)
	{
		bool flag = propertyValue != null;
		if (flag)
		{
			if (string.IsNullOrEmpty(propertyValue))
			{
				AddDiagnostic(propertyName, diagnostics, ErrorCode.ERR_PropertyValueCannotBeEmpty, propertyName);
			}
			else
			{
				ValidateHttpUri(propertyValue, propertyName, diagnostics);
				if (mandatoryLocalePlaceholder && !propertyValue.Contains("{0}"))
				{
					AddDiagnostic(propertyName, diagnostics, ErrorCode.ERR_HelpUrlWithMissingLocalePlaceHolders, propertyValue);
				}
				if (propertyValue.Contains("{0}") && !ValidateHelpUrlPlaceholders(propertyValue))
				{
					AddDiagnostic(propertyName, diagnostics, ErrorCode.ERR_HelpUrlWithTooManyPlaceHolders, propertyValue);
				}
			}
		}
		return flag;
	}

	internal static bool ValidateHelpUrlPlaceholders(string url)
	{
		try
		{
			string.Format(CultureInfo.InvariantCulture, url, "en-US");
			return true;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	private static bool IsValidCulture(string cultureName)
	{
		if (string.IsNullOrEmpty(cultureName))
		{
			return false;
		}
		try
		{
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureName);
			return !MultiLanguageHelper.IsCustomCulture(cultureInfo) && MultiLanguageHelper.HasCountryRegion(cultureInfo);
		}
		catch (CultureNotFoundException)
		{
			return false;
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private void ValidateHttpUri(string url, string propertyName, IList<Diagnostic> diagnostics)
	{
		try
		{
			Uri uri = new Uri(url);
			if (!uri.IsAbsoluteUri || (!(uri.Scheme == "http") && !(uri.Scheme == "https")))
			{
				AddDiagnostic(propertyName, diagnostics, ErrorCode.ERR_InvalidLink, propertyName, url, CodeAnalysisResources.HttpUriLinkSpecification);
			}
		}
		catch (Exception ex)
		{
			if (!url.StartsWith("http:", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
			{
				AddDiagnostic(propertyName, diagnostics, ErrorCode.ERR_InvalidLink, propertyName, url, CodeAnalysisResources.HttpUriLinkSpecification);
			}
			else
			{
				AddDiagnostic(propertyName, diagnostics, ErrorCode.ERR_InvalidLink, propertyName, url, ex.Message);
			}
		}
	}

	private void ValidateManifestDependencies(ICollection<Diagnostic> diagnostics)
	{
		if (Dependencies == null)
		{
			return;
		}
		foreach (NavAppDependency dependency in Dependencies)
		{
			if (dependency.AppId == Guid.Empty || dependency.MinVersion == null || dependency.Name == null || dependency.Publisher == null)
			{
				AddDiagnostic("dependencies", diagnostics, ErrorCode.ERR_IncompleteDependency);
			}
			if (string.Equals(dependency.Name, AppName, StringComparison.OrdinalIgnoreCase) && string.Equals(dependency.Publisher, AppPublisher, StringComparison.OrdinalIgnoreCase))
			{
				AddDiagnostic("dependencies", diagnostics, ErrorCode.ERR_SelfReferenceDependency, AppName);
			}
		}
	}

	private void AddDiagnostic(string propertyName, ICollection<Diagnostic> diagnostics, ErrorCode errorCode, params object[] messageArgs)
	{
		DiagnosticInfo info = new DiagnosticInfo(MessageProvider.Instance, (int)errorCode, messageArgs);
		diagnostics.Add(new DiagnosticWithInfo(info, GetDiagnosticLocation(propertyName)));
	}
}
