using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public class MethodScopeScaffolding
{
	public const string GlobalNodeName = "<Globals>";

	private static readonly Regex childNameDoesNotNeedQuotesRegex = new Regex("^[_\\w][_\\w\\d]*$", RegexOptions.Compiled);

	private static readonly Regex pathTokenRegex = new Regex("(?<Content>[^.\\s\\\"]+)|(\\\"(?<Content>[^\\\"]+)\\\")", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	private static readonly Regex pathValidationRegex = new Regex("^((([^.\\\"]+)|(\\\"[^\\\"]+\\\"))(\\.(?!$)|(?=$)))+$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	private Scaffolding globals;

	private List<Scaffolding> members = new List<Scaffolding>();

	private TextConstantsScaffolding textConstants;

	private SqlDatabaseScaffolding sqlDatabase;

	private IEnumerable<Scaffolding> Children
	{
		get
		{
			yield return globals;
			if (textConstants != null)
			{
				yield return textConstants;
			}
			if (sqlDatabase != null)
			{
				yield return sqlDatabase;
			}
			foreach (Scaffolding member in members)
			{
				yield return member;
			}
		}
	}

	public void CollapsePath(string path, NavMethodScope methodScope)
	{
		GetDescendantScaffolding(ref path, methodScope, out var _, out var _)?.Collapse();
	}

	public LocalNode[] Update(NavMethodScope instance, bool getNodes, bool showTextConstants = true, ExpandLevels expandLevel = ExpandLevels.None)
	{
		UpdateGlobalsAndTextConstants(instance, showTextConstants, showSqlStatistics: true);
		List<Scaffolding> list = new List<Scaffolding>();
		IList<LocalNode> list2 = ((!getNodes) ? null : GetGlobalAndTextConstantNodes(instance, expandLevel));
		var enumerable = from fieldInfo in instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
			where fieldInfo.FieldType != typeof(EventArgs) && fieldInfo.FieldType != typeof(object)
			where fieldInfo.FieldType != typeof(NavTextConstant)
			where instance.IsAtExitStatement || !IsDefaultReturnValueField(fieldInfo)
			where fieldInfo.IsDebuggerAvailable()
			let name = fieldInfo.AlName()
			let valueBinder = new FieldInfoValueBinder(fieldInfo)
			let value = valueBinder.GetValue(instance)
			where !IsEmptyRecParameter(instance, fieldInfo, value)
			let scaffolding = FindOrCreateScaffolding(name, fieldInfo.FieldType, value, valueBinder, instance)
			select new { scaffolding, value };
		bool forceExpand = (expandLevel & ExpandLevels.Locals) != 0;
		foreach (var item in enumerable)
		{
			item.scaffolding.ShowTextConstants = showTextConstants;
			list.Add(item.scaffolding);
			if (getNodes)
			{
				list2.Add(item.scaffolding.GetLocalNode(item.scaffolding.CorceValue(item.value), forceExpand));
			}
		}
		members = list;
		return list2?.ToArray();
	}

	public LocalNode[] ForceUpdateGlobals(NavMethodScope instance, bool showTextConstants)
	{
		UpdateGlobalsAndTextConstants(instance, showTextConstants, showSqlStatistics: false);
		return GetGlobalAndTextConstantNodes(instance, ExpandLevels.Globals).ToArray();
	}

	private IList<LocalNode> GetGlobalAndTextConstantNodes(NavMethodScope instance, ExpandLevels expandLevel = ExpandLevels.None)
	{
		bool flag = (expandLevel & ExpandLevels.Globals) != 0;
		IList<LocalNode> list = new List<LocalNode> { globals.GetLocalNode(instance.ApplicationObject, flag) };
		if (textConstants != null)
		{
			bool forceExpand = (expandLevel & ExpandLevels.TextConstants) != 0;
			list.Add(textConstants.GetLocalNode(instance, forceExpand));
		}
		if (sqlDatabase != null && !flag && instance.Session.SqlDebuggingStatistics.UseDebuggingBehavior)
		{
			bool forceExpand2 = (expandLevel & ExpandLevels.SqlStatements) != 0;
			list.Add(sqlDatabase.GetLocalNode(instance, forceExpand2));
		}
		return list;
	}

	private void UpdateGlobalsAndTextConstants(NavMethodScope instance, bool showTextConstants, bool showSqlStatistics)
	{
		if (globals == null || !globals.IsCompatible(instance.ApplicationObject))
		{
			globals = ScaffoldingFactory.GetApplicationObjectScaffolding("<Globals>", instance.ApplicationObject.GetType(), new ApplicationObjectFromMethodScopeValueBinder(), instance.ApplicationObject.ObjectId.ObjectType, promotedChildren: true, hideGlobalsNode: true);
			globals.ShowTextConstants = showTextConstants;
		}
		if (showTextConstants && (textConstants == null || !textConstants.IsCompatible(instance)))
		{
			TextConstantsScaffolding textConstantsScaffolding = new TextConstantsScaffolding("<Local Text Constants>", instance.GetType(), new IdentityValueBinder());
			textConstantsScaffolding.Expand(instance);
			if (textConstantsScaffolding.Children.Count > 0)
			{
				textConstantsScaffolding.Collapse();
				textConstants = textConstantsScaffolding;
			}
			else
			{
				textConstants = null;
			}
		}
		NavSession session = instance.Session;
		if (showSqlStatistics && session != null && session.SqlDebuggingStatistics.UseDebuggingBehavior)
		{
			SqlDatabaseScaffolding sqlDatabaseScaffolding = new SqlDatabaseScaffolding();
			sqlDatabaseScaffolding.Expand(session);
			sqlDatabase = ((sqlDatabaseScaffolding.Children.Count > 0) ? sqlDatabaseScaffolding : null);
		}
	}

	internal IEnumerable<LocalNode> ExpandPath(string path, NavMethodScope methodScope, bool showTextConstants)
	{
		Scaffolding descendantScaffolding = GetDescendantScaffolding(ref path, methodScope, out var currentInstance, out var _);
		if (descendantScaffolding == null)
		{
			return new LocalNode[0];
		}
		descendantScaffolding.ShowTextConstants = showTextConstants;
		return descendantScaffolding.GetChildrenLocalNodes(currentInstance);
	}

	internal Scaffolding GetDescendantScaffolding(ref string path, NavMethodScope methodScope, out object currentInstance, out List<Scaffolding> childSequence, out object parentInstance)
	{
		if (!pathValidationRegex.IsMatch(path))
		{
			throw new InvalidOperationException("Path is not valid");
		}
		string[] array = (from Match m in pathTokenRegex.Matches(path)
			select m.Groups["Content"].Value).ToArray();
		childSequence = new List<Scaffolding>(array.Length);
		currentInstance = methodScope;
		Scaffolding scaffolding = FindChild(array, 0, Children, childSequence, ref currentInstance);
		parentInstance = currentInstance;
		if (scaffolding == null)
		{
			return null;
		}
		currentInstance = scaffolding.GetValue(parentInstance);
		for (int num = 1; num < array.Length; num++)
		{
			scaffolding.Expand(currentInstance);
			scaffolding = FindChild(array, num, scaffolding.Children, childSequence, ref currentInstance);
			parentInstance = currentInstance;
			if (scaffolding == null)
			{
				return null;
			}
			currentInstance = scaffolding.GetValue(parentInstance);
		}
		if (array.Length == childSequence.Count)
		{
			path = FormatPath(from c in childSequence
				where !c.PromotedChildren
				select c.Name);
		}
		return scaffolding;
	}

	internal Scaffolding GetDescendantScaffolding(ref string pathExpression, NavMethodScope methodScope, out object currentInstance, out List<Scaffolding> childSequence)
	{
		object parentInstance;
		return GetDescendantScaffolding(ref pathExpression, methodScope, out currentInstance, out childSequence, out parentInstance);
	}

	internal bool TrySetDescendantValue(string pathExpression, NavMethodScope methodScope, string valueAsString)
	{
		object currentInstance;
		List<Scaffolding> childSequence;
		object parentInstance;
		return GetDescendantScaffolding(ref pathExpression, methodScope, out currentInstance, out childSequence, out parentInstance)?.TryParseAndSetValue(parentInstance, valueAsString) ?? false;
	}

	private static string FormatPath(IEnumerable<string> pathElements)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (string pathElement in pathElements)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append('.');
			}
			if (childNameDoesNotNeedQuotesRegex.IsMatch(pathElement))
			{
				stringBuilder.Append(pathElement);
			}
			else
			{
				stringBuilder.Append('"').Append(pathElement).Append('"');
			}
		}
		return stringBuilder.ToString();
	}

	private static bool IsDefaultReturnValueField(FieldInfo fieldInfo)
	{
		object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(ReturnValueAttribute), inherit: false);
		int num = 0;
		if (num < customAttributes.Length)
		{
			ReturnValueAttribute returnValueAttribute = (ReturnValueAttribute)customAttributes[num];
			return returnValueAttribute.VariableName == null;
		}
		return false;
	}

	private static bool IsEmptyRecParameter(NavMethodScope instance, FieldInfo info, object value)
	{
		if (value == null && instance.ApplicationObject.NclType == NavNclType.NavCodeunit)
		{
			return info.FieldType == typeof(INavRecordHandle);
		}
		return false;
	}

	private Scaffolding FindChild(string[] pathTokens, int index, IEnumerable<Scaffolding> children, List<Scaffolding> childSequence, ref object currentInstance)
	{
		string childName = pathTokens[index];
		Scaffolding scaffolding = FindChildImpl(childName, children, childSequence, ref currentInstance);
		if (scaffolding != null)
		{
			return scaffolding;
		}
		if (index == 0)
		{
			return null;
		}
		throw new ScaffoldingPathNotFoundException(string.Format(CultureInfo.CurrentCulture, "Element '{0}' is not a member of '{1}'.", pathTokens[index], FormatPath(pathTokens.Take(index))));
	}

	private Scaffolding FindChildImpl(string childName, IEnumerable<Scaffolding> children, List<Scaffolding> childSequence, ref object currentInstance)
	{
		Scaffolding scaffolding = children.FirstOrDefault((Scaffolding c) => c.Name.Equals(childName, StringComparison.OrdinalIgnoreCase));
		if (scaffolding != null)
		{
			childSequence.Add(scaffolding);
			return scaffolding;
		}
		foreach (Scaffolding item in children.Where((Scaffolding c) => c.PromotedChildren))
		{
			object currentInstance2 = item.GetValue(currentInstance);
			item.Expand(currentInstance2);
			List<Scaffolding> list = new List<Scaffolding>();
			scaffolding = FindChildImpl(childName, item.Children, list, ref currentInstance2);
			if (scaffolding != null)
			{
				childSequence.Add(item);
				childSequence.AddRange(list);
				currentInstance = currentInstance2;
				return scaffolding;
			}
		}
		return null;
	}

	private Scaffolding FindOrCreateScaffolding(string name, Type targetType, object value, ValueBinder valueBinder, object parentInstance)
	{
		Scaffolding scaffolding = members.FirstOrDefault((Scaffolding c) => c.Name == name && c.IsCompatible(value));
		if (scaffolding != null)
		{
			scaffolding.ReplaceValueBinder(valueBinder);
			return scaffolding;
		}
		return ScaffoldingFactory.GetScaffolding(name, targetType, valueBinder, parentInstance, false);
	}
}
