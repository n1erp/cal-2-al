using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

public sealed class NullDotNetAddInsInjector : IDotNetAddInsInjector
{
	public static IDotNetAddInsInjector Instance { get; } = new NullDotNetAddInsInjector();

	private NullDotNetAddInsInjector()
	{
	}

	public DotNetPackageDefinition InjectAddIns(DotNetPackageDefinition package)
	{
		return package;
	}
}
