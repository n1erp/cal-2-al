using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class NavAppRuntimePackageGenerator
{
	public static Result<Stream> GenerateRuntimePackage(GetNavAppRuntimePackageArguments arguments)
	{
		INavAppMetadataRetriever navAppMetadataRetriever = NavEnvironment.Instance.NavAppMetadataRetriever;
		return GenerateRuntimePackage(arguments, navAppMetadataRetriever);
	}

	internal static Result<Stream> GenerateRuntimePackage(GetNavAppRuntimePackageArguments arguments, INavAppMetadataRetriever metadataRetriever)
	{
		Stream stream = FindPackage(metadataRetriever, arguments);
		if (stream == null)
		{
			return Result.Fail<Stream>(Error.FromMessage("No published package matches the provided arguments.", ErrorType.Argument));
		}
		using Stream stream2 = stream;
		NavAppPackageReader packageReader = NavAppPackageReader.Create(stream2);
		return MakeRuntimePackage(packageReader, arguments.Tenant, arguments.ShowMyCode);
	}

	private static Stream FindPackage(INavAppMetadataRetriever metadataRetriever, GetNavAppRuntimePackageArguments arguments)
	{
		IEnumerable<NavAppRuntimeMetadata> source = ((string.IsNullOrEmpty(arguments.Tenant) || string.Equals("nav-systemapplication", arguments.Tenant)) ? metadataRetriever.GetAllMetadata() : metadataRetriever.GetAllMetadataVisibleToTenant(arguments.Tenant));
		List<NavAppRuntimeMetadata> list = source.Where((NavAppRuntimeMetadata x) =>
		{
			if (arguments.AppId.HasValue)
			{
				Guid value = ((WrappedValue<Guid>)(object)x.AppId).Value;
				Guid? appId = arguments.AppId;
				if (!(value == appId))
				{
					goto IL_00cd;
				}
			}
			if ((string.IsNullOrEmpty(arguments.Name) || string.Equals(x.Name, arguments.Name, StringComparison.Ordinal)) && (string.IsNullOrEmpty(arguments.Publisher) || string.Equals(x.Publisher, arguments.Publisher, StringComparison.Ordinal)))
			{
				if (!(arguments.Version == null))
				{
					return x.Version == arguments.Version;
				}
				return true;
			}
			goto IL_00cd;
			IL_00cd:
			return false;
		}).ToList();
		if (list.Count != 1)
		{
			return null;
		}
		NavAppRuntimeMetadata val = list.Single();
		return metadataRetriever.GetPackageBlob(val.PackageId);
	}

	internal static Result<Stream> MakeRuntimePackage(NavAppPackageReader packageReader, string tenant, bool? showMyCode = null)
	{
		NavAppManifest navAppManifest = packageReader.ReadNavAppManifest();
		if (showMyCode.HasValue)
		{
			navAppManifest.ShowMyCode = showMyCode.Value;
		}
		IDictionary<string, byte[]> data = NavDesignerUtil.MakePackageFilesDictionary(packageReader, navAppManifest);
		IEnumerable<SyntaxTree> syntaxTrees = from s in packageReader.ReadSourceContents()
			select SyntaxTree.ParseObjectText(s);
		bool value = ServerUserSettings.Instance.EnableMultithreadedCompilation.Value;
		CompilationOptions options = new CompilationOptions(value, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Extension, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code, null, CodeGenerationOption.Text, value).WithManifestOptions(navAppManifest);
		Compilation compilation = Compilation.Create(navAppManifest.AppName, appId: navAppManifest.AppId, publisher: navAppManifest.AppPublisher, version: navAppManifest.AppVersion, syntaxTrees: syntaxTrees, options: options, fileSystem: new MemoryFileSystem(data), dotNetResolverFactory: NavGlobal.SystemTenant.DotNetResolverFactory);
		compilation = compilation.WithReferenceLoader(new NavAppSymbolReferenceLoader(compilation.ModuleInfo, NavGlobal.SystemTenant.SymbolReferenceCache, tenant)).WithReferences(navAppManifest.GetAllReferences());
		ExtensionEmitValues extensionEmitValues = new ExtensionEmitValues(navAppManifest.AppId, navAppManifest.AppVersion);
		Stream stream = new MemoryStream();
		EmitOptions options2 = new EmitOptions(130000, null, skipStmtHit: false, extensionEmitValues);
		RuntimeBuildOptions runtimeBuildOptions = new RuntimeBuildOptions(navAppManifest.ShowMyCode);
		using (RuntimePackageModuleOutputter outputter = new RuntimePackageModuleOutputter(navAppManifest, stream, compilation, options2, runtimeBuildOptions))
		{
			EmitResult emitResult = compilation.Emit(options2, outputter);
			if (!emitResult.Success)
			{
				IEnumerable<Error> enumerable = emitResult.Diagnostics.Select((Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic x) => Error.FromMessage(x.ToString(), ErrorType.InvalidOperation));
				return Result.Fail<Stream>(Error.FromMultipleErrors("Extension compilation failed", enumerable));
			}
		}
		stream.Seek(0L, SeekOrigin.Begin);
		return Result.Ok<Stream>(stream);
	}
}
