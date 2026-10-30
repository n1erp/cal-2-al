using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class CecilAssemblyResolver : IAssemblyResolver, IDisposable
{
	private readonly ConcurrentDictionary<string, AssemblyDefinition> filePathToAssemblyCache = new ConcurrentDictionary<string, AssemblyDefinition>();

	private readonly IAssemblyLocator locator;

	public CecilAssemblyResolver(IAssemblyLocator locator)
	{
		this.locator = locator;
	}

	public AssemblyDefinition Resolve(AssemblyNameReference name)
	{
		return Resolve(name, new ReaderParameters());
	}

	public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
	{
		string pathToAssembly = locator.GetPathToAssembly(name.FullName);
		if (pathToAssembly == null)
		{
			return null;
		}
		return filePathToAssemblyCache.GetOrAdd(pathToAssembly, (string path) =>
		{
			try
			{
				if (parameters.AssemblyResolver == null)
				{
					parameters.AssemblyResolver = this;
				}
				return ModuleDefinition.ReadModule(path, parameters).Assembly;
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (BadImageFormatException)
			{
			}
			catch (SecurityException)
			{
			}
			return (AssemblyDefinition)null;
		});
	}

	public void Dispose()
	{
		foreach (AssemblyDefinition value in filePathToAssemblyCache.Values)
		{
			value?.Dispose();
		}
		filePathToAssemblyCache.Clear();
	}
}
