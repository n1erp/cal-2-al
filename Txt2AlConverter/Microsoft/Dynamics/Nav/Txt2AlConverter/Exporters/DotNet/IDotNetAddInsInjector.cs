using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

public interface IDotNetAddInsInjector
{
	DotNetPackageDefinition InjectAddIns(DotNetPackageDefinition package);
}
