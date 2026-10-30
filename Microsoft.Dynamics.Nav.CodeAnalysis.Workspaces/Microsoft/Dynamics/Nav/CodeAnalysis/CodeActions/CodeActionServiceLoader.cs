using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

internal class CodeActionServiceLoader
{
	private readonly string[] defaultAssemblyNames = new string[1] { "Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces" };

	private readonly string analyzersFolderPath = Path.Combine(AppContextUtility.GetBaseDirectory(), "Analyzers");

	private readonly CompositionContext compositionContext;

	public CodeActionServiceLoader()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		HashSet<Assembly> hashSet = new HashSet<Assembly>();
		LoadDefaultAssemblies(hashSet);
		LoadAnalyzerAssemblies(hashSet);
		ContainerConfiguration val = new ContainerConfiguration().WithAssemblies((IEnumerable<Assembly>)hashSet);
		compositionContext = (CompositionContext)(object)val.CreateContainer();
	}

	public CodeActionService CreateCodeActionService()
	{
		CodeFixService codeFixService = new CodeFixService(TryGetExports<ICodeFixProvider, CodeActionProviderMetadata>()?.Select((Lazy<ICodeFixProvider, CodeActionProviderMetadata> export) => export.Value));
		CodeRefactoringService codeRefactoringService = new CodeRefactoringService(TryGetExports<ICodeRefactoringProvider, CodeActionProviderMetadata>()?.Select((Lazy<ICodeRefactoringProvider, CodeActionProviderMetadata> export) => export.Value));
		return new ALCodeActionService(codeFixService, codeRefactoringService);
	}

	private void LoadDefaultAssemblies(HashSet<Assembly> builder)
	{
		LoadNearbyAssemblies(defaultAssemblyNames, builder);
	}

	private void LoadNearbyAssemblies(IEnumerable<string> assemblyNames, HashSet<Assembly> builder)
	{
		foreach (string assemblyName in assemblyNames)
		{
			Assembly assembly = TryLoadNearbyAssembly(assemblyName);
			if (assembly != null)
			{
				builder.Add(assembly);
			}
		}
	}

	private Assembly TryLoadNearbyAssembly(string assemblySimpleName)
	{
		AssemblyName name = GetType().GetTypeInfo().Assembly.GetName();
		_ = name.Name;
		Version version = name.Version;
		string text = name.GetPublicKeyToken().Aggregate(string.Empty, (string s, byte b) => s + b.ToString("x2", CultureInfo.InvariantCulture));
		if (string.IsNullOrEmpty(text))
		{
			text = "null";
		}
		AssemblyName assemblyRef = new AssemblyName(string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, Culture=neutral, PublicKeyToken={2}", assemblySimpleName, version, text));
		try
		{
			return Assembly.Load(assemblyRef);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private void LoadAnalyzerAssemblies(HashSet<Assembly> builder)
	{
		ImmutableArray<string> assemblyPathsFromAnalyzerFolder = GetAssemblyPathsFromAnalyzerFolder();
		if (!assemblyPathsFromAnalyzerFolder.IsEmpty)
		{
			LoadExternalAssemblies(assemblyPathsFromAnalyzerFolder, builder);
		}
	}

	private void LoadExternalAssemblies(IEnumerable<string> analyzerFileNames, HashSet<Assembly> builder)
	{
		foreach (string analyzerFileName in analyzerFileNames)
		{
			if (File.Exists(analyzerFileName))
			{
				Assembly assembly = TryLoadAnalyzerAssembly(analyzerFileName);
				if (assembly != null)
				{
					builder.Add(assembly);
				}
			}
		}
	}

	private Assembly TryLoadAnalyzerAssembly(string assemblyFileName)
	{
		try
		{
			return Assembly.LoadFrom(assemblyFileName);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private ImmutableArray<string> GetAssemblyPathsFromAnalyzerFolder()
	{
		if (!Directory.Exists(analyzersFolderPath))
		{
			return ImmutableArray<string>.Empty;
		}
		return Directory.EnumerateFiles(analyzersFolderPath, "*.dll").ToImmutableArray();
	}

	private IEnumerable<Lazy<TExtension, TMetadata>> TryGetExports<TExtension, TMetadata>()
	{
		try
		{
			return compositionContext.GetExports<Lazy<TExtension, TMetadata>>();
		}
		catch (Exception)
		{
			return null;
		}
	}
}
