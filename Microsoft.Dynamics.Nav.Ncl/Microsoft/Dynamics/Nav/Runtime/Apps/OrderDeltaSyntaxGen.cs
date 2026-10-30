using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class OrderDeltaSyntaxGen : DeltaSyntaxGen
{
	private const string Separator = "; ";

	private static readonly string EmptyGuid = Guid.Empty.ToString();

	public OrderDeltaSyntaxGen(IDeltaValidator validator)
		: base(validator)
	{
	}

	public override DeltaSyntaxGenResult GenerateSyntaxNodes(ConverterBundle converterBundle, IList<NavXmlBase> deltas)
	{
		if (converterBundle == null)
		{
			throw new ArgumentNullException("converterBundle");
		}
		if (deltas == null)
		{
			throw new ArgumentNullException("deltas");
		}
		Dictionary<SyntaxNodeIdentifier, SyntaxNode> dictionary = new Dictionary<SyntaxNodeIdentifier, SyntaxNode>();
		List<NavXmlDeltaConversionDiagnostics> list = new List<NavXmlDeltaConversionDiagnostics>();
		List<NavXmlDeltaConversionDiagnostics> list2 = new List<NavXmlDeltaConversionDiagnostics>();
		foreach (NavXmlOrder item in deltas.OfType<NavXmlOrder>())
		{
			bool flag = false;
			bool flag2 = false;
			if (Validator.Validate(converterBundle, item) && converterBundle.SourceNodes.TryGetValue(item.Id, out var value))
			{
				string areaKind = ConverterHelper.GetAreaKind(value, item.Id, converterBundle.SourceTempNodes);
				if (areaKind != null || converterBundle.SourceDefinitionNodes.ContainsKey(item.Id))
				{
					try
					{
						Dictionary<string, SyntaxNode> syntaxNodesFromXml = GetSyntaxNodesFromXml(converterBundle.SourceDefinitionNodes, converterBundle.ALIdentifierMapper, value, NavXmlDeltaUtilities.StringToArray(item.Sequence, "; "), areaKind);
						foreach (KeyValuePair<string, SyntaxNode> item2 in syntaxNodesFromXml)
						{
							SyntaxNodeIdentifier key = new SyntaxNodeIdentifier(item2.Key, PersonalizationV1DeltaType.Order);
							if (!dictionary.ContainsKey(key))
							{
								dictionary.Add(key, item2.Value);
							}
							flag = true;
						}
					}
					catch (Exception ex)
					{
						flag2 = true;
						list.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Error(s) occurred when converting element: {0} of type of {1} with error message {2}:", item.Id, PersonalizationV1DeltaType.Order.ToString(), ex.Message), Verbosity.Error));
					}
				}
			}
			if (!flag && !flag2 && !string.IsNullOrEmpty(item.Id))
			{
				list2.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Cannot convert NavXmlDelta element: {0} of type of {1}", item.Id, PersonalizationV1DeltaType.Order.ToString()), Verbosity.Warning));
			}
		}
		return new DeltaSyntaxGenResult(dictionary, list2, list);
	}

	private static Dictionary<string, SyntaxNode> GetSyntaxNodesFromXml(Dictionary<string, XmlElement> sourceDefNodes, Dictionary<string, string> alIdentifierMapper, XmlElement parentElement, string[] sequence, string areaKind)
	{
		Dictionary<string, SyntaxNode> dictionary = new Dictionary<string, SyntaxNode>(StringComparer.OrdinalIgnoreCase);
		if (sequence.Length != 0 && parentElement != null && parentElement.ChildNodes.Count != 0 && ConverterElementChecker.VerifyElementType(parentElement, PersonalizationV1DeltaType.Order))
		{
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			List<string> sourceList = new List<string>();
			List<string> list = new List<string>();
			for (int i = 0; i < parentElement.ChildNodes.Count; i++)
			{
				XmlElement xmlElement = parentElement.ChildNodes[i] as XmlElement;
				string text = xmlElement?.Attributes?["ControlGUID"]?.Value;
				if (text != null && ConverterElementChecker.VerifyElementType(xmlElement, PersonalizationV1DeltaType.Update) && sourceDefNodes.ContainsKey(text) && alIdentifierMapper.ContainsKey(text))
				{
					dictionary2.Add(text, alIdentifierMapper[text]);
					sourceList.Add(text);
				}
			}
			int j;
			for (j = 0; j < sequence.Length; j++)
			{
				int num = sourceList.FindIndex((string f) => f.Equals(sequence[j], StringComparison.OrdinalIgnoreCase));
				if (num > -1)
				{
					list.Add(sourceList[num]);
				}
			}
			if (sourceList.Count > 0 && !list.Contains(sourceList[0]))
			{
				list.Insert(0, sourceList[0]);
			}
			for (int num2 = 0; num2 < sourceList.Count; num2++)
			{
				int index = sourceList.Count - 1 - num2;
				if (list.Contains(sourceList[index]))
				{
					break;
				}
				int index2 = list.Count - num2;
				list.Insert(index2, sourceList[index]);
			}
			int i2;
			for (i2 = 1; i2 < sourceList.Count - 1; i2++)
			{
				if (!list.Contains(sourceList[i2]))
				{
					int num3 = list.FindIndex((string f) => f.Equals(sourceList[i2 - 1], StringComparison.OrdinalIgnoreCase));
					list.Insert(num3 + 1, sourceList[i2]);
				}
			}
			Dictionary<string, Tuple<HashSet<string>, HashSet<string>>> dictionary3 = new Dictionary<string, Tuple<HashSet<string>, HashSet<string>>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, Tuple<HashSet<string>, HashSet<string>>> dictionary4 = new Dictionary<string, Tuple<HashSet<string>, HashSet<string>>>(StringComparer.OrdinalIgnoreCase);
			GetAnchorsList(sourceList, dictionary3);
			GetAnchorsList(list, dictionary4);
			string text2 = EmptyGuid;
			alIdentifierMapper.TryGetValue(parentElement.Attributes["ControlGUID"].Value, out var value);
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				string text3 = list[num4];
				if (HasDifference(text3, dictionary3, dictionary4))
				{
					if (text2 == EmptyGuid)
					{
						dictionary.Add(text3, GetSyntaxNodeFromXml(parentElement, SyntaxKind.MoveFirstKeyword, areaKind ?? value, dictionary2[text3]));
					}
					else
					{
						dictionary.Add(text3, GetSyntaxNodeFromXml(parentElement, SyntaxKind.MoveAfterKeyword, dictionary2[text2], dictionary2[text3]));
					}
				}
				text2 = text3;
			}
		}
		return dictionary;
	}

	private static bool HasDifference(string guid, Dictionary<string, Tuple<HashSet<string>, HashSet<string>>> sourceAnchorsList, Dictionary<string, Tuple<HashSet<string>, HashSet<string>>> targetAnchorsList)
	{
		if (targetAnchorsList.TryGetValue(guid, out var value) && sourceAnchorsList.TryGetValue(guid, out var value2))
		{
			if (!value.Item1.SetEquals(value2.Item1))
			{
				return !value.Item2.SetEquals(value2.Item2);
			}
			return false;
		}
		return true;
	}

	private static void GetAnchorsList(List<string> oriList, Dictionary<string, Tuple<HashSet<string>, HashSet<string>>> results)
	{
		HashSet<string> hashSet = new HashSet<string>(oriList);
		HashSet<string> hashSet2 = new HashSet<string>();
		for (int i = 0; i < oriList.Count; i++)
		{
			hashSet.Remove(oriList[i]);
			if (i > 0)
			{
				hashSet2.Add(oriList[i - 1]);
			}
			results.Add(oriList[i], Tuple.Create(new HashSet<string>(hashSet2), new HashSet<string>(hashSet)));
		}
	}

	private static SyntaxNode GetSyntaxNodeFromXml(XmlElement element, SyntaxKind syntaxKind, string anchor, string name)
	{
		switch (element.Attributes["xsi:type"].Value)
		{
		case "ControlContainerPlaceHolder":
		case "ControlContainerDefinition":
		case "ControlGroupDefinition":
			return SyntaxFactory.ControlMoveChange(SyntaxFactory.Token(syntaxKind), SyntaxFactory.IdentifierName(anchor), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(name)));
		case "ActionContainerPlaceHolder":
		case "NavigationGroupDefinition":
		case "ActionContainerDefinition":
		case "ActionGroupDefinition":
			return SyntaxFactory.ActionMoveChange(SyntaxFactory.Token(syntaxKind), SyntaxFactory.IdentifierName(anchor), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(name)));
		default:
			throw new InvalidOperationException("Unreachable!");
		}
	}
}
