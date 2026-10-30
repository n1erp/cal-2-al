using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class NullDotNetTypeInfoProvider : IDotNetTypeInfoProvider
{
	public DotNetAssemblyInfo GetAssemblyInfo(AssemblyDefinition assembly)
	{
		return null;
	}

	public DotNetTypeInfo GetTypeInfo(TypeReference type)
	{
		return null;
	}
}
