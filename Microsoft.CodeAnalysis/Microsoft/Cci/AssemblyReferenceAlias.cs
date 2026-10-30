namespace Microsoft.Cci;

internal struct AssemblyReferenceAlias
{
	public readonly string Name;

	public readonly IAssemblyReference Assembly;

	internal AssemblyReferenceAlias(string name, IAssemblyReference assembly)
	{
		Name = name;
		Assembly = assembly;
	}
}
