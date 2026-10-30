using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal sealed class NavAppCompilationAssemblyLocator : AssemblyLocatorBase
{
	private readonly INavAddInProvider addInProvider;

	internal NavAppCompilationAssemblyLocator(INavAddInProvider addInProvider)
		: base(GetAssemblyProbingPaths(), GetLocationOfAssembliesLoadedInServerAppDomain())
	{
		this.addInProvider = addInProvider;
	}

	protected override bool TryRetrieveAssembly(AssemblyNameAdapter searchName)
	{
		return addInProvider.ExtractAddInResourceFromAddInTable(searchName.FullName, includeJavaScript: true);
	}

	private static IEnumerable<string> GetAssemblyProbingPaths()
	{
		List<string> list = new List<string>
		{
			NavAutomationHelper.GetAddInDirectory(),
			NavAutomationHelper.GetTempAddInDirectory(),
			AppDomain.CurrentDomain.BaseDirectory
		};
		string globalAssemblyCacheDirectory = NavAutomationHelper.GetGlobalAssemblyCacheDirectory();
		if (Directory.Exists(globalAssemblyCacheDirectory))
		{
			list.Add(globalAssemblyCacheDirectory);
		}
		return list;
	}

	private static Dictionary<string, string> GetLocationOfAssembliesLoadedInServerAppDomain()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Assembly assembly = typeof(object).Assembly;
		dictionary.Add(assembly.GetName().Name, assembly.Location);
		foreach (Assembly item in from p in AppDomain.CurrentDomain.GetAssemblies()
			where !p.IsDynamic
			select p)
		{
			string name = item.GetName().Name;
			if (!dictionary.ContainsKey(name))
			{
				dictionary.Add(name, item.Location);
			}
		}
		return dictionary;
	}
}
