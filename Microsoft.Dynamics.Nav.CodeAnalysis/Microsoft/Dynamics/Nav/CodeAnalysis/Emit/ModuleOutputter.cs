using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Intended spelling")]
public abstract class ModuleOutputter
{
	public abstract void InitializeModule(IModuleSymbol moduleSymbol);

	public abstract void AddApplicationObject(IApplicationObjectTypeSymbol symbol, CompilationUnitSyntax code, Stream metadata, string debugCode);

	public abstract void AddProfileObject(ISymbol symbol, CompilationUnitSyntax code, Stream metadata, string debugCode);

	public abstract void FinalizeModule();

	public abstract ImmutableArray<Diagnostic> GetDiagnostics();
}
