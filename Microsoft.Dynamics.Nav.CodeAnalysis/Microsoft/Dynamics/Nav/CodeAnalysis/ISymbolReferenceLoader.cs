using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ISymbolReferenceLoader
{
	ModuleDefinition LoadModule(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics);

	IEnumerable<SymbolReferenceSpecification> GetDependencies(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics);
}
