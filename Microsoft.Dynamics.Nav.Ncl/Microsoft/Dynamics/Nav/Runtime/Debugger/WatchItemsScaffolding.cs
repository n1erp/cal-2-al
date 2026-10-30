using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class WatchItemsScaffolding
{
	public const string OutOfScopeValueString = "<Out Of Scope>";

	private readonly MethodScopeScaffolding methodScopeScaffolding = new MethodScopeScaffolding();

	private readonly List<string> paths = new List<string>();

	private readonly Regex unnecessaryPathPrefixRegex = new Regex(string.Format(CultureInfo.InvariantCulture, "^({2}\\.)|(({0}\\.)?({1}\\.)?)", "<Globals>", "<Global Text Constants>", "<Local Text Constants>"), RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	public string AddPath(string path, NavMethodScope methodScope)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentNullException("path");
		}
		Match match = unnecessaryPathPrefixRegex.Match(path);
		if (match.Success)
		{
			path = path.Substring(match.Length);
		}
		if (methodScope != null)
		{
			try
			{
				methodScopeScaffolding.Update(methodScope, getNodes: true);
				methodScopeScaffolding?.GetDescendantScaffolding(ref path, methodScope, out var _, out var _);
			}
			catch (ScaffoldingPathNotFoundException)
			{
			}
		}
		paths.Add(path);
		return path;
	}

	public void DeletePath(string path)
	{
		paths.Remove(path);
	}

	public void RemoveAllPaths()
	{
		paths.Clear();
	}

	public IEnumerable<LocalNode> Update(NavMethodScope methodScope)
	{
		methodScopeScaffolding.Update(methodScope, getNodes: true);
		int i = 0;
		for (int n = paths.Count; i < n; i++)
		{
			string path = paths[i];
			yield return Update(path, methodScope);
		}
	}

	public LocalNode GetLocalNode(string path, NavMethodScope methodScope, bool showTextConstants = false, bool forceExpandAll = false)
	{
		methodScopeScaffolding.Update(methodScope, getNodes: true, showTextConstants);
		return Update(path, methodScope, forceExpandAll);
	}

	private LocalNode Update(string path, NavMethodScope methodScope, bool forceExpandAll = false)
	{
		object currentInstance = null;
		try
		{
			string pathExpression = path;
			Scaffolding descendantScaffolding = methodScopeScaffolding.GetDescendantScaffolding(ref pathExpression, methodScope, out currentInstance, out var _);
			if (descendantScaffolding == null)
			{
				return new LocalNode(path, string.Empty, "<Out Of Scope>", hasChildren: false, LocalNodeChangeState.New, null);
			}
			LocalNode localNode = descendantScaffolding.GetLocalNode(currentInstance, forceExpandAll);
			return new LocalNode(path, localNode.TypeName, localNode.Summary, localNode.HasChildren, localNode.ChangeState, localNode.Children);
		}
		catch (ScaffoldingPathNotFoundException)
		{
			return new LocalNode(path, string.Empty, "<Out Of Scope>", hasChildren: false, LocalNodeChangeState.New, null);
		}
	}

	public IEnumerable<LocalNode> UpdateNoMethodScope()
	{
		return paths.Select((string p) => new LocalNode(p, string.Empty, string.Empty, hasChildren: false, LocalNodeChangeState.New, null));
	}
}
