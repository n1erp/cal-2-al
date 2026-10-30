using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppPackageCompiler
{
	public static Result<CompilationOutput> Recompile(Stream packageStream, NavAppPackageType packageType, bool isDeveloperExtension = false, Compilation compilation = null, ObjectChangeModelDefinition radChanges = null)
	{
		try
		{
			packageStream.Seek(0L, SeekOrigin.Begin);
			using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(packageStream, leaveOpen: true);
			NavAppManifest navAppManifest = navAppPackageReader.ReadNavAppManifest();
			if (navAppPackageReader.MetadataVersion < 2)
			{
				throw new InvalidOperationException(Lang.V1ExtensionUnsupportedAction);
			}
			if (packageType == NavAppPackageType.Extension && navAppManifest.Runtime != null && navAppManifest.Runtime > RuntimeVersion.Latest)
			{
				return Result.Fail<CompilationOutput>(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, "The runtime version of the extension package is currently set to '{0}'. The runtime version must be set to '{1}' or earlier in the app.json file in order to install the extension package on this server.", navAppManifest.Runtime, RuntimeVersion.Latest), ErrorType.InvalidOperation));
			}
			if (packageType == NavAppPackageType.SymbolsOnly && !ApplicationIdTypes.IsOnPrem(NavCurrentThread.Session.AppId))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000CZ6", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Trying to publish a not on prem app with symbols only. App details: Name: {navAppManifest.AppName}; Publisher: {navAppManifest.AppPublisher}; Version: {navAppManifest.AppVersion}; AppId: {navAppManifest.AppId}"));
				return Result.Fail<CompilationOutput>(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, "The package {0} by {1} {2} failed to publish. Packages that contain just symbols can only be published in on-premise scenarios.", navAppManifest.AppName, navAppManifest.AppPublisher, navAppManifest.AppVersion), ErrorType.InvalidOperation));
			}
			PackageId val = PackageId.op_Implicit(navAppPackageReader.PackageId);
			string text = string.Format(CultureInfo.InvariantCulture, "Extension Compilation Request -- PackageId:{0}; AppId:{1}; Name:{2}; Publisher:{3}; Version:{4}; Target:{5}; Type:{6}, Size:{7}, IsRad:{8}", val, navAppManifest.AppId.ToString(), navAppManifest.AppName, navAppManifest.AppPublisher, navAppManifest.AppVersion, navAppManifest.Target, packageType, packageStream.Length, radChanges != null);
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001YJ", DiagnosticParameter.SystemMetadata(text), (MonitorType)196609, Category.Extensions, createNewActivity: false))
			{
				ExtensionEmitValues extensionEmitValues = new ExtensionEmitValues(navAppManifest.AppId, navAppManifest.AppVersion);
				EmitOptions emitOptions = new EmitOptions(130000, null, skipStmtHit: false, extensionEmitValues, emitCodeAsAst: false, concurrentEmit: true, !(navAppManifest.ShowMyCode | isDeveloperExtension));
				NavAppPackageMetadataOutputter navAppPackageMetadataOutputter = new NavAppPackageMetadataOutputter
				{
					PackageId = val,
					Manifest = navAppManifest,
					MediaIdListing = navAppPackageReader.MediaIdListing,
					Media = navAppPackageReader.ResolvePackageMedia(),
					PackageType = packageType,
					EmitOptions = emitOptions
				};
				CompilationOutput compilationOutput = new CompilationOutput(compilation, navAppPackageMetadataOutputter);
				if (packageType == NavAppPackageType.SymbolsOnly)
				{
					navAppPackageMetadataOutputter.Symbols = navAppPackageReader.ResolvePackageSymbols();
					if (navAppPackageMetadataOutputter.Symbols == null || navAppPackageMetadataOutputter.Symbols.Length == 0)
					{
						return Result.Fail<CompilationOutput>(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, "Attempting to publish a package as SymbolsOnly, but symbols are missing. Package: {0} version {1} by {2}", navAppManifest.AppName, navAppManifest.AppVersion, navAppManifest.AppPublisher), ErrorType.InvalidOperation));
					}
				}
				else if (radChanges == null || radChanges.Added.Length != 0 || radChanges.Modified.Length != 0)
				{
					Stopwatch stopwatch = Stopwatch.StartNew();
					Result<CompilationOutput> val2 = RecompileFullPackage(navAppPackageReader, navAppManifest, navAppPackageMetadataOutputter, compilation, radChanges, navAppPackageMetadataOutputter.Symbols);
					string message = string.Format(CultureInfo.InvariantCulture, "{0}; Success:{1}; ElapsedMs:{2}", text, ((Result)val2).Success, stopwatch.ElapsedMilliseconds);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000210", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, message);
					if (((Result)val2).Failure)
					{
						return val2;
					}
					compilationOutput = val2.Value;
				}
				InvalidateReferenceLoaderCache(navAppManifest);
				return Result.Ok<CompilationOutput>(compilationOutput);
			}
		}
		catch (Exception ex)
		{
			return Result.Fail<CompilationOutput>(ex);
		}
	}

	private static void InvalidateReferenceLoaderCache(NavAppManifest manifest)
	{
		NavGlobal.SystemTenant.SymbolReferenceCache?.InvalidateModuleDefinition(manifest.AppName, manifest.AppPublisher);
	}

	public static Result<CompilationOutput> RecompileFullPackage(NavAppPackageReader reader, NavAppManifest manifest, CodeModuleOutputter outputter, Compilation compilation = null, ObjectChangeModelDefinition radChanges = null, byte[] packagedSymbolBytes = null)
	{
		try
		{
			ServerUserSettings instance = ServerUserSettings.Instance;
			Microsoft.Dynamics.Nav.Types.CompilationTarget value = instance.ExtensionAllowedTargetLevel.Value;
			if (manifest.Target > CompilationTargetHelper.CompilationTargetMapping(value))
			{
				return Result.Fail<CompilationOutput>(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, Lang.NavAppsHigherThanAllowedCompilationTarget), ErrorType.InvalidOperation));
			}
			IEnumerable<SyntaxTree> enumerable = from filename in reader.ReadSourceCodeFilePaths()
				select SyntaxTree.ParseObjectText(reader.ReadFileAsString(filename), RemoveFirstDirectory(filename));
			IFileSystem fileSystem = new MemoryFileSystem(reader.MakeDictionaryForPackageRecompilation());
			compilation = ((compilation != null) ? compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(enumerable).WithModuleName(manifest.AppName) : CreateCompilation(reader, manifest, enumerable, fileSystem, radChanges, packagedSymbolBytes).Value);
			EmitOptions options = outputter.EmitOptions ?? new EmitOptions(130000, null, skipStmtHit: false, new ExtensionEmitValues(manifest.AppId, manifest.AppVersion));
			EmitResult emitResult = compilation.Emit(options, outputter);
			if (!emitResult.Success)
			{
				IEnumerable<Error> enumerable2 = emitResult.Diagnostics.Select((Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic x) => Error.FromMessage(x.ToString(), ErrorType.InvalidOperation));
				return Result.Fail<CompilationOutput>(Error.FromMultipleErrors("Extension compilation failed", enumerable2));
			}
		}
		catch (InvalidOperationException ex)
		{
			return Result.Fail<CompilationOutput>((Exception)ex);
		}
		return Result.Ok<CompilationOutput>(new CompilationOutput(compilation, outputter));
	}

	public static Result<INavAppPackageMetadata> ExtractEmittedContent(Stream packageStream)
	{
		packageStream.Seek(0L, SeekOrigin.Begin);
		using NavAppPackageReader reader = NavAppPackageReader.Create(packageStream, leaveOpen: true);
		return ResultExtensions.OnSuccess<NavAppPackageMetadata, INavAppPackageMetadata>(NavAppPackageMetadata.Create(reader, NavAppPackageType.Extension), (Func<NavAppPackageMetadata, INavAppPackageMetadata>)((NavAppPackageMetadata package) =>
		{
			InvalidateReferenceLoaderCache(package.Manifest);
			return package;
		}));
	}

	private static string RemoveFirstDirectory(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		int num = path.IndexOf("/", 1, StringComparison.Ordinal);
		if (num < 0)
		{
			return path;
		}
		return path.Substring(num + 1);
	}

	private static Result<Compilation> CreateCompilation(NavAppPackageReader reader, NavAppManifest manifest, IEnumerable<SyntaxTree> syntaxTrees, IFileSystem fileSystem, ObjectChangeModelDefinition changes = null, byte[] packagedSymbolBytes = null)
	{
		if (manifest == null)
		{
			return Result.Fail<Compilation>((Exception)new NavNCLArgumentException("manifest"));
		}
		bool value = ServerUserSettings.Instance.EnableMultithreadedCompilation.Value;
		CompilationOptions options = new CompilationOptions(value, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Extension, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code, null, CodeGenerationOption.Text, value).WithManifestOptions(manifest);
		Compilation compilation;
		if (changes == null)
		{
			string appName = manifest.AppName;
			Guid? guid = manifest.AppId;
			string appPublisher = manifest.AppPublisher;
			Version appVersion = manifest.AppVersion;
			Guid? appId = guid;
			IFileSystem fileSystem2 = fileSystem;
			compilation = Compilation.Create(appName, appPublisher, appVersion, appId, syntaxTrees, options, fileSystem2, NavGlobal.SystemTenant.DotNetResolverFactory);
			NavAppSymbolReferenceLoader referenceLoader = new NavAppSymbolReferenceLoader(compilation.ModuleInfo, NavGlobal.SystemTenant.SymbolReferenceCache, NavCurrentThread.Session.Tenant.Id);
			compilation = compilation.AddReferences(manifest.GetAllReferences()).WithReferenceLoader(referenceLoader);
		}
		else
		{
			packagedSymbolBytes = reader.ResolvePackageSymbols();
			ModuleDefinition moduleDefinition = null;
			if (packagedSymbolBytes != null)
			{
				using MemoryStream stream = new MemoryStream(packagedSymbolBytes);
				moduleDefinition = SymbolReferenceJsonReader.ReadModule(stream);
			}
			NavAppSymbolReferenceLoader symbolReferenceLoader = new NavAppSymbolReferenceLoader(new SymbolReferenceSpecification(moduleDefinition.Publisher, moduleDefinition.Name, Version.Parse(moduleDefinition.Version), exact: false, moduleDefinition.AppId), NavGlobal.SystemTenant.SymbolReferenceCache, NavCurrentThread.Session.Tenant.Id);
			string appName2 = manifest.AppName;
			ModuleDefinition packagedModuleDefinition = moduleDefinition;
			IEnumerable<SymbolReferenceSpecification> allReferences = manifest.GetAllReferences();
			Guid? guid = manifest.AppId;
			string appPublisher2 = manifest.AppPublisher;
			Version appVersion2 = manifest.AppVersion;
			Guid? appId2 = guid;
			IFileSystem fileSystem2 = fileSystem;
			compilation = Compilation.CreateForRad(appName2, changes, packagedModuleDefinition, symbolReferenceLoader, allReferences, appPublisher2, appVersion2, appId2, syntaxTrees, options, fileSystem2, NavGlobal.SystemTenant.DotNetResolverFactory);
		}
		return Result.Ok<Compilation>(compilation);
	}
}
