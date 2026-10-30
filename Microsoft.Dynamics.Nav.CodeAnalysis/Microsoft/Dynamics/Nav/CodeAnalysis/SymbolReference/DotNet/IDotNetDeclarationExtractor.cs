namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;

public interface IDotNetDeclarationExtractor
{
	bool HasDeclarations { get; }

	string AddTypeDeclarationAndGetAlias(string assemblyQualifiedName);

	DotNetPackageDefinition GetDeclaration();
}
