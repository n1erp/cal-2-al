using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public interface IDotNetTypeLoader : IDisposable
{
	IAssemblyLocator AssemblyLocator { get; }

	DotNetAssemblyInfo LoadAssembly(string fullAssemblyName);

	DotNetTypeInfo LoadType(DotNetAssemblyInfo assemblyInfo, string fullTypeName);
}
