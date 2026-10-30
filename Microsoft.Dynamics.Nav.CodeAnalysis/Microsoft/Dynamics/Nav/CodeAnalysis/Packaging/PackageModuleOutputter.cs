using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Outputter", Justification = "Intended spelling")]
public sealed class PackageModuleOutputter : PackageModuleOutputterBase
{
	public PackageModuleOutputter(NavAppManifest manifest, Stream outStream, Compilation compilation, EmitOptions options, string projectRoot = null)
		: this(manifest, NavAppPackageWriter.Create(outStream), compilation, options, projectRoot)
	{
	}

	public PackageModuleOutputter(NavAppManifest manifest, NavAppPackageWriter writer, Compilation compilation, EmitOptions options, string projectRoot = null)
		: base(manifest, writer, compilation, options, projectRoot)
	{
	}

	public override void FinalizeModule()
	{
		base.FinalizeModule();
		AddLayoutDocuments();
	}

	private void AddLayoutDocuments()
	{
		foreach (SourceReportTypeSymbol item in Compilation.CompiledModule.GetDeclaredObjectSymbols().OfType<SourceReportTypeSymbol>())
		{
			string filePath = item.GetProperty(PropertyKind.RDLCLayout)?.ValueText;
			string filePath2 = item.GetProperty(PropertyKind.WordLayout)?.ValueText;
			AddLayoutDocument(WrittenFiles, PackageWriter, Compilation.FileSystem, filePath);
			AddLayoutDocument(WrittenFiles, PackageWriter, Compilation.FileSystem, filePath2);
		}
	}

	internal static void AddLayoutDocument(HashSet<string> writtenFiles, IPackageWriter writer, IFileSystem fileSystem, string filePath)
	{
		if (filePath != null)
		{
			string text = Path.Combine("/layout/", filePath);
			if (!writtenFiles.Contains(text))
			{
				writtenFiles.Add(text);
				writer.WriteBytes(fileSystem.ReadBytes(filePath), text);
			}
		}
	}
}
