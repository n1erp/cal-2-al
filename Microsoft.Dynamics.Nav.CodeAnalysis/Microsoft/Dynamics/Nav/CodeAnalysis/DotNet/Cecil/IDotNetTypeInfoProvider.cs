using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal interface IDotNetTypeInfoProvider
{
	DotNetAssemblyInfo GetAssemblyInfo(AssemblyDefinition assembly);

	DotNetTypeInfo GetTypeInfo(TypeReference type);
}
