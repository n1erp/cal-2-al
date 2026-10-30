using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public abstract class PackageModuleOutputterBase : ModuleOutputter, IDisposable
{
	private IModuleSymbol packageModuleSymbol;

	internal DiagnosticBag DiagnosticBag { get; set; }

	protected internal NavAppPackageWriter PackageWriter { get; }

	protected internal NavAppManifest Manifest { get; }

	protected internal Compilation Compilation { get; }

	protected internal FilePackageWriter FilePackageWriter { get; }

	protected internal Uri ProjectRootUri { get; }

	internal HashSet<string> WrittenFiles { get; }

	protected internal int MetadataVersion { get; }

	protected internal EmitOptions EmitOptions { get; }

	protected internal int NameIndex { get; protected set; } = 1;

	protected bool IsDisposed { get; private set; }

	protected internal IModuleSymbol PackageModuleSymbol => packageModuleSymbol;

	private bool UseFilePackageWriter
	{
		get
		{
			if (EmitOptions.ConcurrentEmit)
			{
				return FilePackageWriter != null;
			}
			return false;
		}
	}

	public Guid PackageId
	{
		get
		{
			if (PackageWriter == null)
			{
				throw new ObjectDisposedException(CodeAnalysisResources.ObjectDisposedError);
			}
			return PackageWriter.NavAppPackage.PackageId;
		}
	}

	protected PackageModuleOutputterBase(NavAppManifest manifest, NavAppPackageWriter packageWriter, Compilation compilation, EmitOptions options, string projectRoot = null)
	{
		if (manifest == null)
		{
			throw new ArgumentNullException("manifest");
		}
		if (packageWriter == null)
		{
			throw new ArgumentNullException("packageWriter");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		PackageWriter = packageWriter;
		Manifest = manifest;
		Compilation = compilation;
		string text;
		if (!string.IsNullOrEmpty(projectRoot))
		{
			text = PathUtilities.EnsureEndingInDirectorySeperatorChar(projectRoot);
			ProjectRootUri = new Uri(text);
		}
		else
		{
			text = Path.GetTempPath();
		}
		WrittenFiles = new HashSet<string>(PackagePathComparer.Instance);
		MetadataVersion = options.RuntimeMetadataVersion;
		EmitOptions = options;
		if (EmitOptions.ConcurrentEmit)
		{
			FilePackageWriter = new FilePackageWriter(Path.Combine(text, ".bin"));
		}
	}

	public override void InitializeModule(IModuleSymbol moduleSymbol)
	{
		packageModuleSymbol = moduleSymbol;
		PackageWriter.WriteString(Manifest.ToXml(), "/NavxManifest.xml");
		AddSourceCode();
	}

	public override void AddApplicationObject(IApplicationObjectTypeSymbol symbol, CompilationUnitSyntax code, Stream metadata, string debugCode)
	{
		if (!string.IsNullOrEmpty(debugCode))
		{
			AddEmittedFiles(symbol, code, metadata, debugCode, isProfile: false);
		}
	}

	public override void AddProfileObject(ISymbol symbol, CompilationUnitSyntax code, Stream metadata, string debugCode)
	{
		if (!string.IsNullOrEmpty(debugCode))
		{
			AddEmittedFiles(symbol, code, metadata, debugCode, isProfile: true);
		}
	}

	public override void FinalizeModule()
	{
		FilePackageWriter?.Flush();
		AddVariousValidFiles(Compilation.FileSystem, DiagnosticBag, WrittenFiles, PackageWriter);
		AddSymbolReference(PackageWriter, PackageModuleSymbol);
		AddMediaFiles(Compilation.FileSystem, Manifest, PackageWriter, WrittenFiles);
		AddTranslationsV2Files(Compilation.FileSystem, PackageWriter);
		AddControlAddIns();
		AddNavigationFiles();
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics()
	{
		return DiagnosticBag?.ToReadOnly() ?? ImmutableArray<Diagnostic>.Empty;
	}

	[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<FilePackageWriter>k__BackingField", Justification = "Known bug: https://github.com/dotnet/roslyn/issues/4077")]
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	internal static void AddMediaFilesStatic(IFileSystem fileSystem, NavAppManifest manifest, IPackageWriter packageWriter, HashSet<string> writtenFiles)
	{
		if (fileSystem == null || (string.IsNullOrEmpty(manifest.AppLogo) && manifest.AppScreenshots == null))
		{
			return;
		}
		NavAppMediaEntry logo = ((!string.IsNullOrEmpty(manifest.AppLogo)) ? new NavAppMediaEntry
		{
			FileName = Path.GetFileName(manifest.AppLogo),
			Id = Guid.NewGuid(),
			IsLogo = true
		} : null);
		List<NavAppMediaSetEntry> list = new List<NavAppMediaSetEntry>();
		if (manifest.AppScreenshots != null && !manifest.AppScreenshots.IsEmpty())
		{
			List<NavAppMediaEntry> list2 = new List<NavAppMediaEntry>();
			foreach (string appScreenshot in manifest.AppScreenshots)
			{
				string filename = Path.GetFileName(appScreenshot);
				if (!list2.Exists((NavAppMediaEntry addedScreenshot) => string.Equals(addedScreenshot.FileName, filename, StringComparison.OrdinalIgnoreCase)))
				{
					list2.Add(new NavAppMediaEntry
					{
						FileName = filename,
						Id = Guid.NewGuid()
					});
				}
			}
			list.Add(new NavAppMediaSetEntry
			{
				Name = "screenshots",
				SetId = Guid.NewGuid(),
				Media = list2
			});
		}
		MemoryStream memoryStream = new MemoryStream();
		using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true))
		{
			streamWriter.Write(new NavAppMediaIdListing(logo, list).ToXml());
		}
		memoryStream.Seek(0L, SeekOrigin.Begin);
		packageWriter.WriteStream(memoryStream, "/MediaIdListing.xml");
		if (!string.IsNullOrEmpty(manifest.AppLogo) && fileSystem.Exists(manifest.AppLogo))
		{
			packageWriter.WriteBytes(fileSystem.ReadBytes(manifest.AppLogo), Path.Combine("/logo/", Path.GetFileName(manifest.AppLogo)));
		}
		if (manifest.AppScreenshots == null)
		{
			return;
		}
		foreach (string appScreenshot2 in manifest.AppScreenshots)
		{
			if (appScreenshot2 != null && fileSystem.Exists(appScreenshot2))
			{
				string text = Path.Combine("/media/", Path.GetFileName(appScreenshot2));
				if (!writtenFiles.Contains(text))
				{
					packageWriter.WriteBytes(fileSystem.ReadBytes(appScreenshot2), text);
					writtenFiles.Add(text);
				}
			}
		}
	}

	internal static void AddSymbolReferenceStatic(IPackageWriter writer, IModuleSymbol moduleSymbol)
	{
		ModuleDefinition module = SerializableSymbolModelConverter.ConvertModuleToSerializableSymbolModel(moduleSymbol);
		IEnumerable<ModuleDefinition> modelsWithProfilesAndTheirCustomizations = SerializableSymbolModelConverter.GetModelsWithProfilesAndTheirCustomizations(module, moduleSymbol.GetObjectSymbols(SymbolKind.Profile).Cast<ProfileTypeSymbol>());
		module = SerializableSymbolModelConverter.FilterOutPageCustomizations(module, modelsWithProfilesAndTheirCustomizations);
		writer.WithStream("/SymbolReference.json", (Stream partStream) =>
		{
			SymbolReferenceJsonWriter.WriteModule(partStream, module);
		});
		foreach (ModuleDefinition moduleForProfile in modelsWithProfilesAndTheirCustomizations)
		{
			string arg = moduleForProfile.Profiles.Single().Name.MangleUnquotedIdentifierName();
			string path = string.Format(CultureInfo.InvariantCulture, "/ProfileSymbolReferences/{0}.json", arg);
			writer.WithStream(path, (Stream partStream) =>
			{
				SymbolReferenceJsonWriter.WriteModule(partStream, moduleForProfile);
			});
		}
	}

	internal static void AddVariousValidFilesStatic(IFileSystem fileSystem, DiagnosticBag diagnostics, HashSet<string> writtenFiles, IPackageWriter writer)
	{
		if (fileSystem == null)
		{
			return;
		}
		GlobalLogger.LogVerbose("Starting process to add various files. (Report Layouts, Permission Sets, etc.).");
		NavAppCheckerResult navAppCheckerResult = new NavAppCheckerResult(fileSystem.GetFiles("*.xml").Concat(fileSystem.GetFiles("*.layoutdata")).Concat(fileSystem.GetFiles("*.navxdata"))
			.Concat(fileSystem.GetFiles("*.txt"))).VerifyAgainst(new NavAppPermissionSetChecker(fileSystem)).VerifyAgainst(new NavAppTenantWebServiceChecker(fileSystem)).VerifyAgainst(new NavAppCustomReportV2Checker(fileSystem))
			.VerifyAgainst(new NavAppTableDataChecker(fileSystem))
			.VerifyAgainst(new NavAppTranslationFileChecker(fileSystem));
		NavAppSourceFile[] array = navAppCheckerResult.SourceFiles.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			NavAppSourceFile navAppSourceFile = array[i];
			if (navAppSourceFile.FileTypes != NavAppFileTypes.None && navAppSourceFile.PassedCheck)
			{
				writtenFiles.Add(navAppSourceFile.FileLocation);
				string text = "file" + i + "_" + Path.GetFileNameWithoutExtension(navAppSourceFile.FileLocation);
				string fileExtensionFromFileType = GetFileExtensionFromFileType(navAppSourceFile.FileTypes);
				string path = Path.Combine(GetFolderPathFromXmlType(navAppSourceFile.FileTypes), text + fileExtensionFromFileType);
				writer.WriteBytes(fileSystem.ReadBytes(navAppSourceFile.FileLocation), path);
			}
		}
		if (diagnostics == null)
		{
			return;
		}
		NavAppSourceFile[] array2 = array;
		foreach (NavAppSourceFile navAppSourceFile2 in array2)
		{
			Location location = (string.IsNullOrEmpty(navAppSourceFile2.FileLocation) ? Location.None : Location.Create(navAppSourceFile2.FileLocation, TextSpan.FromBounds(0, 0), new LinePositionSpan(LinePosition.Zero, LinePosition.Zero)));
			if (navAppSourceFile2.FileTypes != NavAppFileTypes.None)
			{
				diagnostics.Add(ErrorCode.INF_FileMatchesADefinition, location, navAppSourceFile2.FileLocation, navAppSourceFile2.FileTypes);
			}
			else
			{
				diagnostics.Add(ErrorCode.WRN_FileDoesNotMatchAnyFormat, location, navAppSourceFile2.FileLocation);
			}
		}
		foreach (string error in navAppCheckerResult.Errors)
		{
			diagnostics.Add(ErrorCode.WRN_XmlValidationErrorOccurred, Location.None, error);
		}
	}

	protected internal static string CreateAlSourceOutputPath(string filepath, Uri projectRootUri = null)
	{
		string result = null;
		try
		{
			if (!string.IsNullOrWhiteSpace(filepath))
			{
				result = ((!(projectRootUri != null)) ? ("/src/" + new Uri(filepath, UriKind.RelativeOrAbsolute)) : ("/src/" + projectRootUri.MakeRelativeUri(new Uri(filepath))));
			}
		}
		catch (UriFormatException)
		{
		}
		return result;
	}

	internal virtual void AddVariousValidFiles(IFileSystem fileSystem, DiagnosticBag diagnostics, HashSet<string> writtenFilesToPackage, IPackageWriter writer)
	{
		AddVariousValidFilesStatic(fileSystem, diagnostics, writtenFilesToPackage, writer);
	}

	internal virtual void AddMediaFiles(IFileSystem fileSystem, NavAppManifest navManifest, IPackageWriter writer, HashSet<string> writtenFilesToPackage)
	{
		AddMediaFilesStatic(fileSystem, navManifest, writer, writtenFilesToPackage);
	}

	internal virtual void AddSymbolReference(IPackageWriter writer, IModuleSymbol moduleSymbol)
	{
		AddSymbolReferenceStatic(writer, moduleSymbol);
	}

	internal virtual void AddTranslationsV2Files(IFileSystem fileSystem, PackageWriter writer)
	{
		AddTranslationsV2FilesStatic(fileSystem, writer);
	}

	protected static string GetObjectIdOrName(ISymbol symbol)
	{
		if (symbol is IApplicationObjectTypeSymbol applicationObjectTypeSymbol && symbol.Kind != SymbolKind.PageCustomization)
		{
			return applicationObjectTypeSymbol.Id.ToString(CultureInfo.InvariantCulture);
		}
		return FileNameUtilities.SanitizeFilename(symbol.Name);
	}

	protected static string GetObjectPrefix(SymbolKind kind)
	{
		switch (kind)
		{
		case SymbolKind.Codeunit:
			return "COD";
		case SymbolKind.Table:
			return "TAB";
		case SymbolKind.TableExtension:
			return "TABX";
		case SymbolKind.Page:
			return "PAG";
		case SymbolKind.PageExtension:
		case SymbolKind.PageCustomization:
			return "PAGX";
		case SymbolKind.Report:
			return "REP";
		case SymbolKind.XmlPort:
			return "XML";
		case SymbolKind.Query:
			return "QUE";
		case SymbolKind.Profile:
			return "PROFILE";
		case SymbolKind.DotNetPackage:
			return "DOTNET";
		case SymbolKind.Enum:
			return "ENUM";
		case SymbolKind.EnumExtension:
			return "ENUMX";
		default:
			return string.Empty;
		}
	}

	[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<FilePackageWriter>k__BackingField", Justification = "Known bug: https://github.com/dotnet/roslyn/issues/4077")]
	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			PackageWriter?.Dispose();
			FilePackageWriter?.Dispose();
			IsDisposed = true;
		}
	}

	protected virtual void AddAdditionalEmittedFiles(ISymbol symbol, bool isProfile, string codeFilename, string metadataFilename, string alCode)
	{
	}

	protected internal virtual void AddSourceCode()
	{
		for (int i = 0; i < Compilation.SyntaxTrees.Length; i++)
		{
			string path = CreateSourceUri(Compilation.SyntaxTrees[i].FilePath);
			PackageWriter.WriteString(Compilation.SyntaxTrees[i].GetText().ToString(), path);
		}
	}

	protected string CreateSourceUri(string filepath)
	{
		string text = CreateAlSourceOutputPath(filepath, ProjectRootUri);
		if (text == null || WrittenFiles.Contains(text))
		{
			do
			{
				text = "/src/" + NameIndex + ".al";
				NameIndex++;
			}
			while (WrittenFiles.Contains(text));
		}
		return text;
	}

	private static string GetFileExtensionFromFileType(NavAppFileTypes sourceFileTypes)
	{
		if ((sourceFileTypes & NavAppFileTypes.PermissionSet) != 0 || (sourceFileTypes & NavAppFileTypes.TenantWebService) != 0)
		{
			return ".xml";
		}
		if ((sourceFileTypes & NavAppFileTypes.CustomReportLayout) != 0)
		{
			return ".layoutdata";
		}
		if ((sourceFileTypes & NavAppFileTypes.TableData) != 0)
		{
			return ".navxdata";
		}
		if ((sourceFileTypes & NavAppFileTypes.TranslationFile) != 0)
		{
			return ".txt";
		}
		return string.Empty;
	}

	private static string GetFolderPathFromXmlType(NavAppFileTypes sourceFileTypes)
	{
		if ((sourceFileTypes & NavAppFileTypes.PermissionSet) != 0)
		{
			return "perm";
		}
		if ((sourceFileTypes & NavAppFileTypes.TenantWebService) != 0)
		{
			return "serv";
		}
		if ((sourceFileTypes & NavAppFileTypes.CustomReportLayout) != 0)
		{
			return "replay";
		}
		if ((sourceFileTypes & NavAppFileTypes.TableData) != 0)
		{
			return "tabledata";
		}
		if ((sourceFileTypes & NavAppFileTypes.TranslationFile) != 0)
		{
			return "trans";
		}
		if ((sourceFileTypes & NavAppFileTypes.Navigation) != 0)
		{
			return "/navigation.xml";
		}
		return string.Empty;
	}

	private static void AddTranslationsV2FilesStatic(IFileSystem fileSystem, PackageWriter writer)
	{
		if (fileSystem == null)
		{
			return;
		}
		foreach (string item in fileSystem.GetFiles("/Translations/*.lcl").Concat(fileSystem.GetFiles("/Translations/*.xlf")).Concat(fileSystem.GetFiles("/Translations/*.xliff")))
		{
			string path = "/Translations/" + Path.GetFileName(item);
			writer.WriteBytes(fileSystem.ReadBytes(item), path);
		}
	}

	private static string GetControlAddInEntryName(SourceControlAddInTypeSymbol controlAddIn)
	{
		if (controlAddIn.GetFirstSyntax().SyntaxTree.Options?.RuntimeVersion == RuntimeVersion.Spring2018)
		{
			return controlAddIn.Name;
		}
		return controlAddIn.MetadataName;
	}

	private void WriteControlAddInResources(IEnumerable<string> resources, PackageWriter addInWriter)
	{
		foreach (string resource in resources)
		{
			byte[] array = Compilation.FileSystem.ReadBytes(resource);
			string path = PackageCommon.NormalizeRelativeUrlPath(resource);
			addInWriter.WriteBytes(array, path);
			WriteToPackageSafe("/addin/src/" + resource, array);
		}
	}

	private void WriteToPackageSafe(string path, byte[] bytes)
	{
		if (!WrittenFiles.Contains(path))
		{
			PackageWriter.WriteBytes(bytes, path);
			WrittenFiles.Add(path);
		}
	}

	private void AddEmittedFiles(ISymbol symbol, CompilationUnitSyntax cSharpCode, Stream metadata, string alCode, bool isProfile)
	{
		bool flag = Compilation.Options.CompilationGenerateOptions.HasFlag(CompilationGenerationOptions.Code);
		string text = null;
		string text2 = null;
		if ((cSharpCode != null) & flag)
		{
			if (EmitOptions.EmitCodeAsAst)
			{
				text = CreateEmittedCodeUri(symbol, ".ast");
				using MemoryStream memoryStream = new MemoryStream();
				cSharpCode.SerializeTo(memoryStream);
				byte[] buffer = memoryStream.GetBuffer();
				if (UseFilePackageWriter)
				{
					FilePackageWriter.WriteBytes(buffer, text);
				}
				else
				{
					PackageWriter.WriteBytes(buffer, text);
				}
			}
			else
			{
				text = CreateEmittedCodeUri(symbol, ".cs");
				string contents = cSharpCode.ToFullString();
				if (UseFilePackageWriter)
				{
					FilePackageWriter.WriteString(contents, text);
				}
				else
				{
					PackageWriter.WriteString(contents, text);
				}
			}
		}
		if ((metadata != null) & flag)
		{
			text2 = CreateEmittedCodeUri(symbol, ".xml");
			if (UseFilePackageWriter)
			{
				FilePackageWriter.WriteStream(metadata, text2);
			}
			else
			{
				PackageWriter.WriteStream(metadata, text2);
			}
		}
		AddAdditionalEmittedFiles(symbol, isProfile, text, text2, alCode);
	}

	private ControlAddInManifest CreateControlAddInManifest(SourceControlAddInTypeSymbol controlAddIn)
	{
		ControlAddInManifest controlAddInManifest = new ControlAddInManifest();
		foreach (string localScript in controlAddIn.GetLocalScripts())
		{
			controlAddInManifest.ScriptResourceNames.Add(PackageCommon.NormalizeRelativeUrlPath(localScript));
		}
		foreach (string localStyleSheet in controlAddIn.GetLocalStyleSheets())
		{
			controlAddInManifest.StyleSheetResourceNames.Add(PackageCommon.NormalizeRelativeUrlPath(localStyleSheet));
		}
		foreach (string allImage in controlAddIn.GetAllImages())
		{
			controlAddInManifest.ImageResourceNames.Add(PackageCommon.NormalizeRelativeUrlPath(allImage));
		}
		ExtractScriptContent(controlAddIn, PropertyKind.StartupScript, (string x) =>
		{
			controlAddInManifest.Script = x;
		});
		ExtractScriptContent(controlAddIn, PropertyKind.RecreateScript, (string x) =>
		{
			controlAddInManifest.RecreateScript = x;
		});
		ExtractScriptContent(controlAddIn, PropertyKind.RefreshScript, (string x) =>
		{
			controlAddInManifest.RefreshScript = x;
		});
		controlAddInManifest.ScriptUrls.AddRange(controlAddIn.GetExternalScripts());
		controlAddInManifest.StyleSheetUrls.AddRange(controlAddIn.GetExternalStyleSheets());
		controlAddInManifest.RequestedHeight = controlAddIn.GetInt32PropertyValue(PropertyKind.RequestedHeight).GetValueOrDefault();
		controlAddInManifest.RequestedWidth = controlAddIn.GetInt32PropertyValue(PropertyKind.RequestedWidth).GetValueOrDefault();
		controlAddInManifest.MinimumWidth = controlAddIn.GetInt32PropertyValue(PropertyKind.MinimumWidth).GetValueOrDefault();
		controlAddInManifest.MinimumHeight = controlAddIn.GetInt32PropertyValue(PropertyKind.MinimumHeight).GetValueOrDefault();
		controlAddInManifest.MaximumWidth = controlAddIn.GetInt32PropertyValue(PropertyKind.MaximumWidth).GetValueOrDefault();
		controlAddInManifest.MaximumHeight = controlAddIn.GetInt32PropertyValue(PropertyKind.MaximumHeight).GetValueOrDefault();
		controlAddInManifest.VerticalShrink = controlAddIn.GetBooleanPropertyValue(PropertyKind.VerticalShrink) == true;
		controlAddInManifest.VerticalStretch = controlAddIn.GetBooleanPropertyValue(PropertyKind.VerticalStretch) == true;
		controlAddInManifest.HorizontalShrink = controlAddIn.GetBooleanPropertyValue(PropertyKind.HorizontalShrink) == true;
		controlAddInManifest.HorizontalStretch = controlAddIn.GetBooleanPropertyValue(PropertyKind.HorizontalStretch) == true;
		return controlAddInManifest;
	}

	private void ExtractScriptContent(SourceControlAddInTypeSymbol controlAddIn, PropertyKind propertyKind, Action<string> action)
	{
		string text = controlAddIn.GetProperty(propertyKind)?.ValueText;
		if (!string.IsNullOrEmpty(text))
		{
			byte[] bytes = Compilation.FileSystem.ReadBytes(text);
			WriteToPackageSafe("/addin/src/" + text, bytes);
			action(ControlAddInManifest.ScriptEncoding.GetString(bytes));
		}
	}

	private void AddControlAddIns()
	{
		IEnumerable<SourceControlAddInTypeSymbol> enumerable = Compilation.CompiledModule.GetDeclaredObjectSymbols().OfType<SourceControlAddInTypeSymbol>();
		if (!enumerable.Any())
		{
			return;
		}
		ControlAddInDocket docket = new ControlAddInDocket();
		foreach (SourceControlAddInTypeSymbol item in enumerable)
		{
			AddControlAddIn(docket, item);
		}
		AddControlAddInDocket(docket);
	}

	private void AddControlAddIn(ControlAddInDocket docket, SourceControlAddInTypeSymbol controlAddIn)
	{
		ControlAddInEntry controlAddInEntry = new ControlAddInEntry
		{
			Name = GetControlAddInEntryName(controlAddIn),
			Category = ControlAddInCategory.JavaScriptControlAddIn,
			FileName = controlAddIn.MetadataName + ".zip",
			Token = controlAddIn.PublicKeyToken,
			Version = string.Empty
		};
		docket.ControlAddIns.Add(controlAddInEntry);
		using MemoryStream memoryStream = new MemoryStream();
		using (PackageWriter packageWriter = new PackageWriter(memoryStream))
		{
			WriteControlAddInResources(controlAddIn.GetLocalScripts(), packageWriter);
			WriteControlAddInResources(controlAddIn.GetLocalStyleSheets(), packageWriter);
			WriteControlAddInResources(controlAddIn.GetAllImages(), packageWriter);
			ControlAddInManifest controlAddInManifest = CreateControlAddInManifest(controlAddIn);
			packageWriter.WriteString(controlAddInManifest.ToXml(), "manifest.xml");
		}
		memoryStream.Position = 0L;
		PackageWriter.WriteStream(memoryStream, Path.Combine("addin", controlAddInEntry.FileName));
	}

	private void AddControlAddInDocket(ControlAddInDocket docket)
	{
		string contents = docket.ToXml().ToString();
		PackageWriter.WriteString(contents, Path.Combine("addin", "controladdins.dock"));
	}

	private void AddNavigationFiles()
	{
		string text = NavigationMetadataEmitter.ToXmlString(Compilation.CompiledModule.GetDeclaredObjectSymbols().OfType<ApplicationObjectTypeSymbol>());
		if (!string.IsNullOrEmpty(text))
		{
			PackageWriter.WriteString(text, "/navigation.xml");
		}
	}

	private string CreateEmittedCodeUri(ISymbol symbol, string extension)
	{
		return ((FilePackageWriter != null) ? string.Empty : "/bin/") + GetObjectPrefix(symbol.Kind) + GetObjectIdOrName(symbol) + extension;
	}
}
