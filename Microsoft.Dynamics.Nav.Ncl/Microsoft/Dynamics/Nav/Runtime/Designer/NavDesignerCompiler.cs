using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerCompiler : INavDesignerCompiler
{
	private const string ExtensionTargetNotFound = "AL0247";

	public IEnumerable<NavAppObjectMetadata> CompilePersonalizationObjects(IEnumerable<ObjectSyntax> objectSyntaxList, IEnumerable<SymbolReferenceSpecification> references = null, CompilationOptions compilationOptions = null, NavAppPackageMetadataOutputter outputter = null, IFileSystem fileSystem = null)
	{
		outputter = outputter ?? new NavAppPackageMetadataOutputter();
		outputter.PackageId = PackageId.op_Implicit(Guid.Empty);
		Compilation compilation = CreatePersonalizationCompilation(objectSyntaxList ?? Enumerable.Empty<ObjectSyntax>(), references ?? Enumerable.Empty<SymbolReferenceSpecification>(), compilationOptions ?? new CompilationOptions(concurrentBuild: true, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, CompilationTarget.Extension, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code), fileSystem);
		EmitResult emitResult = compilation.Emit(null, outputter);
		if (emitResult.Success)
		{
			return outputter.ObjectMetadata;
		}
		string emitResultErrorMessage = GetEmitResultErrorMessage(emitResult);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000OP", Category.Extensions, Verbosity.Warning, "Error(s) compiling designer extension -- Details:{0}", DiagnosticParameter.SystemMetadata(emitResultErrorMessage));
		throw new NavDesignerCompilerException(emitResultErrorMessage, emitResult);
	}

	public Stream BuildDesignerPackage(IEnumerable<ObjectSyntax> objectSyntaxList, NavAppManifest manifest, IFileSystem fileSystem = null, bool removeUnusedDependencies = false)
	{
		MemoryStream memoryStream = new MemoryStream();
		try
		{
			Compilation compilation = CreateDesignerCompilation(manifest, objectSyntaxList ?? Enumerable.Empty<ObjectSyntax>(), fileSystem);
			if (removeUnusedDependencies)
			{
				RemovedUnusedDependenciesFromManifest(manifest, compilation);
			}
			using (PackageModuleOutputter outputter = new PackageModuleOutputter(manifest, memoryStream, compilation, new EmitOptions()))
			{
				EmitResult emitResult = compilation.Emit(null, outputter);
				if (!emitResult.Success)
				{
					string emitResultErrorMessage = GetEmitResultErrorMessage(emitResult);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000UV", Category.Extensions, Verbosity.Warning, "Error(s) creating package -- Details:{0}", DiagnosticParameter.SystemMetadata(emitResultErrorMessage));
					throw new NavDesignerCompilerException(emitResultErrorMessage, emitResult);
				}
			}
			memoryStream.Flush();
			return memoryStream;
		}
		catch
		{
			memoryStream.Dispose();
			throw;
		}
	}

	private static Compilation CreateDesignerCompilation(NavAppManifest manifest, IEnumerable<ObjectSyntax> objectSyntaxList, IFileSystem fileSystem)
	{
		CompilationOptions options = new CompilationOptions(concurrentBuild: true, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, CompilationTarget.Extension, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code).WithManifestOptions(manifest);
		Compilation compilation = Compilation.Create(manifest.AppName, manifest.AppPublisher, appId: manifest.AppId, version: manifest.AppVersion, syntaxTrees: GetSyntaxTrees(objectSyntaxList), options: options, fileSystem: fileSystem, dotNetResolverFactory: NavGlobal.SystemTenant.DotNetResolverFactory);
		NavAppSymbolReferenceLoader referenceLoader = new NavAppSymbolReferenceLoader(compilation.ModuleInfo, NavGlobal.SystemTenant.SymbolReferenceCache, NavCurrentThread.Session.Tenant.Id);
		return compilation.WithReferenceLoader(referenceLoader).WithReferences(manifest.GetAllReferences());
	}

	private static Compilation CreatePersonalizationCompilation(IEnumerable<ObjectSyntax> objectSyntaxList, IEnumerable<SymbolReferenceSpecification> references, CompilationOptions options, IFileSystem fileSystem)
	{
		Compilation compilation = Compilation.Create("Designer", null, null, null, GetSyntaxTrees(objectSyntaxList), options, fileSystem, NavGlobal.SystemTenant.DotNetResolverFactory);
		return compilation.WithReferenceLoader(new NavAppSymbolReferenceLoader(compilation.ModuleInfo, NavGlobal.SystemTenant.SymbolReferenceCache, NavCurrentThread.Session.Tenant.Id)).WithReferences(references);
	}

	private static SyntaxTree[] GetSyntaxTrees(IEnumerable<ObjectSyntax> objectSyntaxList)
	{
		objectSyntaxList = objectSyntaxList ?? throw new ArgumentNullException("objectSyntaxList");
		ObjectCompilationUnitSyntax objectCompilationUnitSyntax = SyntaxFactory.ObjectCompilationUnit().WithObjects(default(SyntaxList<ObjectSyntax>).AddRange(objectSyntaxList)).NormalizeWhiteSpace();
		return new SyntaxTree[1] { objectCompilationUnitSyntax.SyntaxTree };
	}

	private static string GetEmitResultErrorMessage(EmitResult result)
	{
		if (result.Success)
		{
			return string.Empty;
		}
		string text = string.Join(Environment.NewLine, result.Diagnostics.Select((Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic x) => x.Severity.ToString() + " - " + x.Id + ": " + x.GetMessage()));
		if (result.Diagnostics.Any((Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic d) => d.Id.Equals("AL0247", StringComparison.OrdinalIgnoreCase)))
		{
			text = text.Insert(0, Lang.NavAppCompileSystemSymbolsMissing + Environment.NewLine);
		}
		return text;
	}

	private static void RemovedUnusedDependenciesFromManifest(NavAppManifest manifest, Compilation compilation)
	{
		ImmutableArray<IModuleSymbol> referenceModuleSymbolsUsed = SymbolReferenceUsageAnalyzer.GetReferenceModuleSymbolsUsed(compilation);
		ImmutableArray<SymbolReferenceSpecification> referenceSpecifications = (from x in NavDesignerUtil.GetSymbolReferenceSpecifications(referenceModuleSymbolsUsed)
			orderby x.Name, x.Publisher, x.AppId
			select x).ToImmutableArray();
		manifest.UpdateDependencies(referenceSpecifications);
	}
}
