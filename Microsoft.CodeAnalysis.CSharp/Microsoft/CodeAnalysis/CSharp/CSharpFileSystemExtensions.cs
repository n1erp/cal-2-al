using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp;

public static class CSharpFileSystemExtensions
{
	public static EmitResult Emit(this CSharpCompilation compilation, string outputPath, string pdbPath = null, string xmlDocumentationPath = null, string win32ResourcesPath = null, IEnumerable<ResourceDescription> manifestResources = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FileSystemExtensions.Emit(compilation, outputPath, pdbPath, xmlDocumentationPath, win32ResourcesPath, manifestResources, cancellationToken);
	}
}
