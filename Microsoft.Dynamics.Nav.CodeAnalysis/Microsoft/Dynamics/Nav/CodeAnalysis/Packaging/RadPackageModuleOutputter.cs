using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal sealed class RadPackageModuleOutputter : PackageModuleOutputterBase
{
	private readonly IObjectChangeManager objectChangeManager;

	public RadPackageModuleOutputter(IObjectChangeManager applicationObjectChangeManager, NavAppManifest manifest, Compilation compilation, EmitOptions options, string projectRoot, NavAppPackageWriter writer)
		: base(manifest, writer, compilation, options, projectRoot)
	{
		objectChangeManager = applicationObjectChangeManager;
	}

	internal override void AddVariousValidFiles(IFileSystem fileSystem, DiagnosticBag diagnostics, HashSet<string> writtenFilesToPackage, IPackageWriter writer)
	{
	}

	internal override void AddTranslationsV2Files(IFileSystem fileSystem, PackageWriter writer)
	{
	}

	protected internal override void AddSourceCode()
	{
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		try
		{
			foreach (ObjectChangeContext change in objectChangeManager.Changes)
			{
				Symbol symbol = Compilation.CompiledModule.GetSymbol(change.ObjectChangeElement);
				if (!(symbol == null))
				{
					SyntaxTree syntaxTree = symbol.GetFirstSyntax().SyntaxTree;
					string text = CreateSourceUri(syntaxTree.FilePath);
					string item = text.ToLowerInvariant();
					if (instance.Add(item))
					{
						string contents = syntaxTree.GetText().ToString();
						PackageWriter.WriteString(contents, text);
					}
				}
			}
		}
		finally
		{
			instance.Free();
		}
	}

	public override void FinalizeModule()
	{
		AddRadFile();
		objectChangeManager.Clear();
		base.FinalizeModule();
	}

	private void AddRadFile()
	{
		string path;
		if (ProjectRootUri != null)
		{
			path = Path.Combine(Path.Combine(ProjectRootUri.LocalPath, ".vscode"), "rad.json");
			if (!Compilation.FileSystem.DirectoryExistsForFile(path))
			{
				Compilation.FileSystem.CreateDirectoryForFile(path);
			}
		}
		else
		{
			path = "/rad.json";
		}
		if (Compilation.FileSystem.Exists(path))
		{
			byte[] content = Compilation.FileSystem.ReadBytes(path);
			PackageWriter.WriteBytes(content, "/rad.json");
		}
	}
}
