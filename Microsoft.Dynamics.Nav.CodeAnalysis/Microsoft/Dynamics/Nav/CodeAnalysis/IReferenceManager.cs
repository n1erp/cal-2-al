using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IReferenceManager : IReferenceResolver
{
	ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken);

	ImmutableArray<IModuleSymbol> GetLoadedModules();
}
