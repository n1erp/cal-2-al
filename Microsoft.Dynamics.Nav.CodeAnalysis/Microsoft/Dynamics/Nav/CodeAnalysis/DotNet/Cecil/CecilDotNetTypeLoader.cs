using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class CecilDotNetTypeLoader : IDotNetTypeLoader, IDisposable
{
	private readonly ConcurrentDictionary<string, AssemblyDefinition> assemblyCache = new ConcurrentDictionary<string, AssemblyDefinition>();

	private readonly ConcurrentDictionary<string, DotNetAssemblyInfo> assemblyInfoCache = new ConcurrentDictionary<string, DotNetAssemblyInfo>();

	private readonly ReaderParameters cecilReaderParameters;

	private readonly ConcurrentDictionary<string, TypeReference> typeCache = new ConcurrentDictionary<string, TypeReference>();

	private readonly ConcurrentDictionary<string, DotNetTypeInfo> typeInfoCache = new ConcurrentDictionary<string, DotNetTypeInfo>();

	private IDotNetTypeInfoProvider lazyTypeInfoProvider;

	private IDotNetTypeInfoProvider TypeInfoProvider
	{
		get
		{
			if (lazyTypeInfoProvider != null)
			{
				return lazyTypeInfoProvider;
			}
			Interlocked.CompareExchange(ref lazyTypeInfoProvider, CreateTypeInfoProvider(), null);
			return lazyTypeInfoProvider;
		}
	}

	public IAssemblyLocator AssemblyLocator { get; }

	public CecilDotNetTypeLoader(IAssemblyLocator locator)
	{
		AssemblyLocator = locator;
		cecilReaderParameters = new ReaderParameters
		{
			AssemblyResolver = new CecilAssemblyResolver(locator)
		};
	}

	public void Dispose()
	{
	}

	public DotNetAssemblyInfo LoadAssembly(string fullAssemblyName)
	{
		if (string.IsNullOrWhiteSpace(fullAssemblyName))
		{
			return null;
		}
		return assemblyInfoCache.GetOrAdd(fullAssemblyName, (string assemblyName) =>
		{
			string pathToAssembly = AssemblyLocator.GetPathToAssembly(assemblyName);
			AssemblyDefinition assembly = LoadAssemblyFromFile(pathToAssembly);
			return TypeInfoProvider.GetAssemblyInfo(assembly);
		});
	}

	public DotNetTypeInfo LoadType(DotNetAssemblyInfo assemblyInfo, string fullTypeName)
	{
		DebugAssertHelper.Assert(assemblyInfo != null);
		if (string.IsNullOrWhiteSpace(fullTypeName))
		{
			return null;
		}
		string assemblyQualifiedTypeName = DotNetUtilities.GetAssemblyQualifiedTypeName(assemblyInfo, fullTypeName);
		return typeInfoCache.GetOrAdd(assemblyQualifiedTypeName, (string _) =>
		{
			AssemblyDefinition assembly = LoadAssemblyFromFile(assemblyInfo.Location);
			TypeReference type = LoadTypeFromAssembly(assembly, fullTypeName);
			return TypeInfoProvider.GetTypeInfo(type);
		});
	}

	private TypeReference LoadTypeFromAssembly(AssemblyDefinition assembly, string fullTypeName)
	{
		string assemblyQualifiedTypeName = DotNetUtilities.GetAssemblyQualifiedTypeName(assembly, fullTypeName);
		return typeCache.GetOrAdd(assemblyQualifiedTypeName, (string _) =>
		{
			try
			{
				TypeReference type = assembly.MainModule.GetType(fullTypeName, runtimeName: true);
				if (string.IsNullOrEmpty(type.Name))
				{
					return (TypeReference)null;
				}
				return type;
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
			catch (FormatException)
			{
			}
			return (TypeReference)null;
		});
	}

	private AssemblyDefinition LoadAssemblyFromFile(string pathToAssembly)
	{
		if (pathToAssembly == null)
		{
			return null;
		}
		return assemblyCache.GetOrAdd(pathToAssembly, (string path) =>
		{
			try
			{
				return AssemblyDefinition.ReadAssembly(path, cecilReaderParameters);
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

	private IDotNetTypeInfoProvider CreateTypeInfoProvider()
	{
		return new CecilDotNetTypeInfoProvider(cecilReaderParameters);
	}
}
