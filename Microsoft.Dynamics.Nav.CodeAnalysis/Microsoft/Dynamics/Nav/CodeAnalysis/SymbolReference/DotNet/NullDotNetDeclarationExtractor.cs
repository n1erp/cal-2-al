using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;

public class NullDotNetDeclarationExtractor : IDotNetDeclarationExtractor
{
	private readonly bool quoteName;

	public bool HasDeclarations => false;

	public NullDotNetDeclarationExtractor(bool quoteName = false)
	{
		this.quoteName = quoteName;
	}

	public string AddTypeDeclarationAndGetAlias(string assemblyQualifiedName)
	{
		if (!quoteName)
		{
			return assemblyQualifiedName;
		}
		return FormattableString.Invariant($"\"{assemblyQualifiedName}\"");
	}

	public DotNetPackageDefinition GetDeclaration()
	{
		return null;
	}
}
