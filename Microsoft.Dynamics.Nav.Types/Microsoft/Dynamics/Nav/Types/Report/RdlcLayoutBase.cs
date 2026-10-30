using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal abstract class RdlcLayoutBase
{
	private readonly Dictionary<string, XElement> itemDefinitions = new Dictionary<string, XElement>();

	private readonly Dictionary<string, List<XElement>> itemReferences = new Dictionary<string, List<XElement>>();

	private readonly Dictionary<string, List<XElement>> unresolvedReferences = new Dictionary<string, List<XElement>>();

	private readonly RdlcDocumentContainer rdlcDocument;

	private const string TokenBaseFormat = "{0}!";

	protected string Token { get; private set; }

	protected ElementType ElementType
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	protected bool DocumentIsAnalyzed { get; set; }

	protected Dictionary<string, XElement> ItemDefinitions => itemDefinitions;

	protected Dictionary<string, List<XElement>> ItemReferences => itemReferences;

	protected Dictionary<string, List<XElement>> UnresolvedReferences => unresolvedReferences;

	protected RdlcDocumentContainer Document => rdlcDocument;

	public virtual int DefinitionCount => itemDefinitions.Count;

	public virtual int ReferenceCount => itemReferences.Count;

	public XNamespace Ns => rdlcDocument.Ns;

	protected string TokenFormatString { get; private set; }

	protected string TokenFormatNameString { get; private set; }

	protected RdlcLayoutBase(string token, ElementType type, RdlcDocumentContainer layout)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		ElementType = type;
		Token = token;
		TokenFormatString = string.Format(CultureInfo.CurrentCulture, "{0}!", token);
		TokenFormatNameString = TokenFormatString + "{0}.Value";
		rdlcDocument = layout;
	}

	public bool IsDefined(string name)
	{
		return itemDefinitions.ContainsKey(name);
	}

	public bool IsReferenced(string name)
	{
		return itemReferences.ContainsKey(name);
	}

	public void Clear()
	{
		itemReferences.Clear();
		unresolvedReferences.Clear();
	}

	protected IReportChangeLogCollection Delete(string sourceName, DeleteAction actionOnDelete, string replacementText)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected Obj, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected I4, but got Unknown
		if (!IsDefined(sourceName))
		{
			return MessageItemNotDefined(sourceName);
		}
		XElement xElement = Definition(sourceName);
		IList<XElement> list = References(sourceName);
		xElement.Remove();
		UpdateDefinitionDictionary(sourceName, string.Empty, null);
		if (list == null || !list.Any())
		{
			return MessageItemDeletedIgnore(sourceName);
		}
		ReportChangeLogCollection val = new ReportChangeLogCollection();
		foreach (XElement item in list)
		{
			switch (actionOnDelete - 1)
			{
			case 1:
				ReplaceReferenceWithText(sourceName, item, string.Empty);
				break;
			case 4:
				ReplaceReferenceWithText(sourceName, item, replacementText);
				break;
			case 0:
				val.Add(DeleteReferenceNode(sourceName, item));
				break;
			}
		}
		UpdateReferencesDictionary(sourceName, string.Empty, null);
		bool withReferenceErrors = ((IEnumerable<IReportChangeLog>)val).Any();
		val.AddRange((IEnumerable<IReportChangeLog>)MessageItemDeleted(sourceName, withReferenceErrors));
		return (IReportChangeLogCollection)(object)val;
	}

	protected IReportChangeLogCollection RenameInReferences(string oldName, string newName)
	{
		List<XElement> list = References(oldName) as List<XElement>;
		if (list == null || !list.Any())
		{
			list = FindUnresolvedReferences(oldName) as List<XElement>;
			if (list == null || !list.Any())
			{
				return MessageUpgradeSuccessIgnore(oldName, newName);
			}
		}
		foreach (XElement item in list)
		{
			string text = string.Format(CultureInfo.InvariantCulture, TokenFormatNameString, oldName);
			string newValue = string.Format(CultureInfo.InvariantCulture, TokenFormatNameString, newName);
			if (item.Value.Contains(text))
			{
				item.SetValue(item.Value.Replace(text, newValue));
			}
			foreach (XElement item2 in item.Descendants())
			{
				if (item2.Value.Contains(oldName))
				{
					item2.SetValue(item2.Value.Replace(text, newValue));
				}
			}
		}
		UpdateReferencesDictionary(oldName, newName, list);
		return MessageUpgradeSuccess(oldName, newName);
	}

	protected static IEnumerable<string> ReferenceNameFromExpression(string value, string token)
	{
		List<string> list = new List<string>();
		int startIndex = 0;
		int length = token.Length;
		while ((startIndex = value.IndexOf(token, startIndex, StringComparison.Ordinal)) >= 0)
		{
			startIndex += length;
			int num = value.IndexOf(".", startIndex, StringComparison.Ordinal);
			string item = value.Substring(startIndex, num - startIndex);
			list.Add(item);
			startIndex = num + 1;
		}
		return list;
	}

	protected static int CountReferences(string value, string token)
	{
		int num = 0;
		int startIndex = 0;
		int length = token.Length;
		while ((startIndex = value.IndexOf(token, startIndex, StringComparison.Ordinal)) >= 0)
		{
			startIndex += length;
			num++;
		}
		return num;
	}

	protected void ResolveReference(bool throwOnErrors, XElement value)
	{
		IEnumerable<string> referenceName = ReferenceNameFromExpression(value.Value, TokenFormatString);
		ResolveReferenceElement(throwOnErrors, value, referenceName);
	}

	protected XElement Definition(string name)
	{
		itemDefinitions.TryGetValue(name, out var value);
		return value;
	}

	protected IList<XElement> References(string name)
	{
		if (itemReferences.TryGetValue(name, out var value))
		{
			return value;
		}
		return new List<XElement>();
	}

	protected IList<XElement> FindUnresolvedReferences(string name)
	{
		if (unresolvedReferences.TryGetValue(name, out var value))
		{
			return value;
		}
		return new List<XElement>();
	}

	protected void UpdateReferencesDictionary(string oldName, string newName, List<XElement> element)
	{
		itemReferences.Remove(oldName);
		unresolvedReferences.Remove(oldName);
		if (element != null)
		{
			itemReferences.Add(newName, element);
		}
	}

	protected void UpdateDefinitionDictionary(string oldName, string newName, XElement element)
	{
		itemDefinitions.Remove(oldName);
		if (element != null)
		{
			itemDefinitions.Add(newName, element);
		}
	}

	protected IReportChangeLogCollection MessageUpgradeSuccess(string oldName, string newName)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected Obj, but got Unknown
		ReportChangeLog val = new ReportChangeLog((UpgradeStatus)2, oldName, string.Format(CultureInfo.CurrentCulture, Lang.ReportUpdateChangeSuccess, ElementType, oldName, newName));
		ReportChangeLogCollection val2 = new ReportChangeLogCollection();
		val2.Add((IReportChangeLog)(object)val);
		return (IReportChangeLogCollection)val2;
	}

	protected IReportChangeLogCollection MessageUpgradeSuccessIgnore(string oldName, string newName)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected Obj, but got Unknown
		ReportChangeLog val = new ReportChangeLog((UpgradeStatus)3, oldName, string.Format(CultureInfo.CurrentCulture, Lang.ReportUpdateChangeSuccessIgnore, ElementType, oldName, newName));
		ReportChangeLogCollection val2 = new ReportChangeLogCollection();
		val2.Add((IReportChangeLog)(object)val);
		return (IReportChangeLogCollection)val2;
	}

	protected IReportChangeLogCollection MessageItemNotDefined(string oldName)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		ReportChangeLog val = new ReportChangeLog((UpgradeStatus)1, oldName, string.Format(CultureInfo.CurrentCulture, Lang.ReportItemNotDefinedInDataSet, ElementType, oldName));
		ReportChangeLogCollection val2 = new ReportChangeLogCollection();
		val2.Add((IReportChangeLog)(object)val);
		return (IReportChangeLogCollection)val2;
	}

	protected IReportChangeLogCollection MessageItemAllreadyDefined(string oldName, string newName)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected Obj, but got Unknown
		ReportChangeLog val = new ReportChangeLog((UpgradeStatus)5, oldName, string.Format(CultureInfo.CurrentCulture, Lang.ReportItemAlreadyDefined, ElementType, oldName, newName));
		ReportChangeLogCollection val2 = new ReportChangeLogCollection();
		val2.Add((IReportChangeLog)(object)val);
		return (IReportChangeLogCollection)val2;
	}

	protected IReportChangeLogCollection MessageItemDeleted(string itemName, bool withReferenceErrors)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected Obj, but got Unknown
		string arg = (withReferenceErrors ? Lang.ReportItemReferenceDeleteSuffix : string.Empty);
		ReportChangeLog val = new ReportChangeLog((UpgradeStatus)(withReferenceErrors ? 5 : 2), itemName, string.Format(CultureInfo.CurrentCulture, Lang.ReportItemReferenceDelete, ElementType, itemName, arg));
		ReportChangeLogCollection val2 = new ReportChangeLogCollection();
		val2.Add((IReportChangeLog)(object)val);
		return (IReportChangeLogCollection)val2;
	}

	protected IReportChangeLogCollection MessageItemDeletedIgnore(string itemName)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		ReportChangeLog val = new ReportChangeLog((UpgradeStatus)3, itemName, string.Format(CultureInfo.CurrentCulture, Lang.ReportItemReferenceDeleteIgnore, ElementType, itemName));
		ReportChangeLogCollection val2 = new ReportChangeLogCollection();
		val2.Add((IReportChangeLog)(object)val);
		return (IReportChangeLogCollection)val2;
	}

	private IReportChangeLog DeleteReferenceNode(string sourceName, XElement reference)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected Obj, but got Unknown
		if (reference.Name.LocalName.Equals("Value"))
		{
			XName xName = Ns + "Textbox";
			if (reference.Parent != null && reference.Parent.Name.LocalName.Equals("TextRun"))
			{
				XElement parent = reference.Parent;
				while (parent != null && parent.Name != xName)
				{
					parent = parent.Parent;
				}
				parent?.Remove();
			}
		}
		else
		{
			if (CountReferences(reference.Value, ".Value") > 1)
			{
				return (IReportChangeLog)new ReportChangeLog((UpgradeStatus)5, sourceName, string.Format(CultureInfo.CurrentCulture, Lang.ReportFieldReferenceUpdateError, reference));
			}
			if (reference.Parent != null && reference.Parent.Elements().Count() == 1)
			{
				reference.Parent.Remove();
			}
			else
			{
				reference.Remove();
			}
		}
		return null;
	}

	private void ResolveReferenceElement(bool throwOnErrors, XElement value, IEnumerable<string> referenceName)
	{
		foreach (string item in referenceName)
		{
			if (!itemDefinitions.ContainsKey(item))
			{
				if (throwOnErrors)
				{
					throw new NavReportLayoutValidationException(Token + " reference {0} not defined in design", item);
				}
				if (UnresolvedReferences.ContainsKey(item))
				{
					UnresolvedReferences[item].Add(value);
				}
				else
				{
					UnresolvedReferences.Add(item, new List<XElement> { value });
				}
			}
			if (itemReferences.ContainsKey(item))
			{
				itemReferences[item].Add(value);
				continue;
			}
			itemReferences.Add(item, new List<XElement> { value });
		}
	}

	private void ReplaceReferenceWithText(string sourceName, XElement reference, string newReferenceString)
	{
		string text = string.Format(CultureInfo.InvariantCulture, TokenFormatNameString, sourceName);
		if (reference.Value.Contains(text))
		{
			reference.Value = reference.Value.Replace(text, "\"" + newReferenceString + "\"");
		}
	}
}
