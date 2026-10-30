namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class LocalMethodSymbolReferenceFinder : AbstractMemberScopedReferenceFinder<IMethodSymbol, IApplicationObjectTypeSymbol>
{
	protected override bool CanFind(IMethodSymbol symbol)
	{
		if (symbol.IsLocal)
		{
			return symbol.MethodKind == MethodKind.Method;
		}
		return false;
	}
}
