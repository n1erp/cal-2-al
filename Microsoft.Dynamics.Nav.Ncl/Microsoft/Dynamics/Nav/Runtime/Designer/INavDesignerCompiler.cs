using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal interface INavDesignerCompiler
{
	IEnumerable<NavAppObjectMetadata> CompilePersonalizationObjects(IEnumerable<ObjectSyntax> objectSyntaxList, IEnumerable<SymbolReferenceSpecification> references = null, CompilationOptions compilationOptions = null, NavAppPackageMetadataOutputter outputter = null, IFileSystem fileSystem = null);

	Stream BuildDesignerPackage(IEnumerable<ObjectSyntax> objectSyntaxList, NavAppManifest manifest, IFileSystem fileSystem = null, bool removeUnusedDependencies = false);
}
