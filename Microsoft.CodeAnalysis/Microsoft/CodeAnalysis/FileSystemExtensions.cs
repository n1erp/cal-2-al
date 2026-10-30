using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public static class FileSystemExtensions
{
	public static EmitResult Emit(this Compilation compilation, string outputPath, string pdbPath = null, string xmlDocPath = null, string win32ResourcesPath = null, IEnumerable<ResourceDescription> manifestResources = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		using Stream peStream = FileUtilities.CreateFileStreamChecked(File.Create, outputPath, "outputPath");
		using Stream pdbStream = ((pdbPath == null) ? null : FileUtilities.CreateFileStreamChecked(File.Create, pdbPath, "pdbPath"));
		using Stream xmlDocumentationStream = ((xmlDocPath == null) ? null : FileUtilities.CreateFileStreamChecked(File.Create, xmlDocPath, "xmlDocPath"));
		using Stream win32Resources = ((win32ResourcesPath == null) ? null : FileUtilities.CreateFileStreamChecked(File.OpenRead, win32ResourcesPath, "win32ResourcesPath"));
		return compilation.Emit(peStream, pdbStream, xmlDocumentationStream, win32Resources, manifestResources, new EmitOptions(metadataOnly: false, (DebugInformationFormat)0, pdbPath, null, 0, 0uL), cancellationToken);
	}
}
