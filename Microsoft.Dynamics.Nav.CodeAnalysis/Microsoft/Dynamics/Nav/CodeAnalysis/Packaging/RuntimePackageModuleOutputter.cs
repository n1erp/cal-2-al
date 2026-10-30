using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public sealed class RuntimePackageModuleOutputter : PackageModuleOutputterBase
{
	private readonly RuntimeBuildOptions runtimeBuildOptions;

	private readonly IList<PackageEmittedContent> emittedContent;

	public RuntimePackageModuleOutputter(NavAppManifest manifest, Stream outStream, Compilation compilation, EmitOptions options, RuntimeBuildOptions runtimeBuildOptions = default(RuntimeBuildOptions), string projectRoot = null)
		: this(manifest, NavAppPackageWriter.CreateNeaPackage(outStream), compilation, options, runtimeBuildOptions, projectRoot)
	{
	}

	public RuntimePackageModuleOutputter(NavAppManifest manifest, NavAppPackageWriter writer, Compilation compilation, EmitOptions options, RuntimeBuildOptions runtimeBuildOptions = default(RuntimeBuildOptions), string projectRoot = null)
		: base(manifest, writer, compilation, options, projectRoot)
	{
		this.runtimeBuildOptions = runtimeBuildOptions;
		emittedContent = new List<PackageEmittedContent>();
	}

	private static string CreateRuntimeSourceCodeUri(ISymbol symbol, string extension)
	{
		return "/src/" + PackageModuleOutputterBase.GetObjectPrefix(symbol.Kind) + PackageModuleOutputterBase.GetObjectIdOrName(symbol) + extension;
	}

	protected override void AddAdditionalEmittedFiles(ISymbol symbol, bool isProfile, string codeFilename, string metadataFilename, string alCode)
	{
		string text = null;
		if (runtimeBuildOptions.IncludeAlCode && !string.IsNullOrEmpty(alCode))
		{
			text = CreateRuntimeSourceCodeUri(symbol, ".al");
			PackageWriter.WriteString(alCode, text);
		}
		IApplicationObjectTypeSymbol applicationObjectTypeSymbol = symbol as IApplicationObjectTypeSymbol;
		string applicationObjectSubtype = applicationObjectTypeSymbol.GetApplicationObjectSubtype();
		if (EmitOptions.ConcurrentEmit)
		{
			lock (emittedContent)
			{
				emittedContent.Add(new PackageEmittedContent
				{
					IsProfileObject = isProfile,
					Id = applicationObjectTypeSymbol?.Id,
					Name = symbol.Name,
					TypeKind = symbol.GetNavTypeKind(),
					ApplicationObjectSubtype = applicationObjectSubtype,
					CodeFileName = codeFilename,
					MetadataFileName = metadataFilename,
					MetadataVersion = MetadataVersion,
					SourceFileName = text
				});
				return;
			}
		}
		emittedContent.Add(new PackageEmittedContent
		{
			IsProfileObject = isProfile,
			Id = applicationObjectTypeSymbol?.Id,
			Name = symbol.Name,
			TypeKind = symbol.GetNavTypeKind(),
			ApplicationObjectSubtype = applicationObjectSubtype,
			CodeFileName = codeFilename,
			MetadataFileName = metadataFilename,
			MetadataVersion = MetadataVersion,
			SourceFileName = text
		});
	}

	public override void FinalizeModule()
	{
		base.FinalizeModule();
		if (emittedContent.Count > 0)
		{
			PackageWriter.WithStream("/bin/EmittedContent.json", (Stream partStream) =>
			{
				PackageEmittedContentWriter.WriteContent(partStream, emittedContent);
			});
		}
	}

	protected internal override void AddSourceCode()
	{
	}
}
