using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public static class NavAppPackageConstants
{
	public const string PackageExtension = ".app";

	public const string SourcesDirectoryName = "src";

	public const string SourcesInternalPackagePathPart = "/src/";

	public const string ProfilesDirectoryName = "profile";

	public const string ProfilesInternalPackagePathPart = "/profile/";

	public const string OutputCodePath = ".bin";

	public const string EmittedFilesInternalPackagePathPart = "/bin/";

	public const string EmittedFilesContentFilePath = "/bin/EmittedContent.json";

	public const string MediaDirectoryName = "media";

	public const string MediaInternalPackagePartPath = "/media/";

	public const string LayoutDirectoryName = "layout";

	public const string LayoutInternalPackagePartPath = "/layout/";

	public const string ManifestFilename = "manifest.xml";

	public const string InternalManifestFilename = "NavxManifest.xml";

	public const string ManifestInternalPackagePartPath = "/NavxManifest.xml";

	public const string SymbolReferenceInternalPath = "/SymbolReference.json";

	public const string ProfileSymbolReferencesInternalPath = "/ProfileSymbolReferences/";

	public const string ProfileSymbolReferencesFileInternalPath = "/ProfileSymbolReferences/{0}.json";

	public const string MediaIdListingFilename = "MediaIdListing.xml";

	public const string MediaIdListingInternalPackagePartPath = "/MediaIdListing.xml";

	public const string LogoDirectoryName = "logo";

	public const string LogoInternalPackagePartPath = "/logo/";

	public const string TranslationDirectoryName = "trans";

	public const string TranslationInternalPackagePartPath = "/trans/";

	public const string NavigationInternalPackagePath = "/navigation.xml";

	public const string RadFileName = "rad.json";

	public const string RadInternalPackagePath = "/rad.json";

	public const string RadRelativePackagePath = ".vscode";

	public const string TranslationsV2DirectoryName = "Translations";

	public const string TranslationsV2InternalPackagePartPath = "/Translations/";

	public const string TranslationsV2ExtensionNameLcl = ".lcl";

	public const string TranslationsV2ExtensionNameLcg = ".lcg";

	public const string TranslationsV2ExtensionNameXliff = ".xlf";

	public const string TranslationsV2ExtensionNameXliffLong = ".xliff";

	public const string TranslationsV2SearchPatternLcl = "/Translations/*.lcl";

	public const string TranslationsV2SearchPatternXliff = "/Translations/*.xlf";

	public const string TranslationsV2SearchPatternXliffLong = "/Translations/*.xliff";

	public const string PermissionSetsFileName = "allPermissions.xml";

	public const string SourceExtension = ".al";

	public const string MetadataExtension = ".xml";

	public const string EmittedCodeExtension = ".cs";

	public const string EmittedSerializedAstExtension = ".ast";

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ScreenShot")]
	public const string ScreenShotMediaSetName = "screenshots";

	public const string WebServiceDirectoryName = "serv";

	public const string WebServiceInternalPackagePartPath = "/serv/";

	public const string PermissionSetsDirectoryName = "perm";

	public const string PermissionSetInternalPackagePathPart = "/perm/";

	public const string TableDataFileExtension = ".navxdata";

	public const string ControlAddInDocketExtension = ".dock";

	public const string ControlAddInDocket = "controladdins.dock";

	public const string ControlAddInPackageExtension = ".zip";

	public const string ControlAddInManifestFile = "manifest.xml";

	public const string ControlAddInDirectoryName = "addin";

	public const string ControlAddInInternalPackagePathPart = "/addin/";

	public const string ControlAddInSourcePackagePathPart = "/addin/src/";

	public const string TableDataDirectoryName = "tabledata";

	public const string TableDataInternalPackagePartPath = "/tabledata/";

	public const string CustomReportLayoutDirectoryName = "replay";

	public const string CustomReportLayoutInternalPackagePathPart = "/replay/";

	public const string RdlExtension = ".rdl";

	public const string RdlcExtension = ".rdlc";

	public const string WordExtension = ".docx";

	public const string TextFileExtension = ".txt";

	public const string TextFileSearchPattern = "*.txt";

	public const string XmlExtension = ".xml";

	public const string XmlSearchPattern = "*.xml";

	public const string CustomReportLayoutV2Extension = ".layoutdata";

	public const string CustomReportLayoutV2SearchPattern = "*.layoutdata";

	public const string TableDataSearchPattern = "*.navxdata";

	public const string WebServiceClassification = "web service";

	public const string PermissionSetsClassification = "permission set";

	public const string CustomReportLayoutClassification = "custom report layout";

	public const string XmlPermissionSetSchemaPath = "XML.PermissionSetsV2.xsd";

	public const string XmlTenantWebServicesSchemaPath = "XML.TenantWebServicesV1.xsd";

	public const string XmlCustomReportLayoutSchemaPath = "XML.CustomReportLayoutsV2.xsd";
}
