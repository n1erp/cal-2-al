using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal class RuleSetProcessor
{
	private const string RuleSetNodeName = "RuleSet";

	private const string RuleSetNameAttributeName = "Name";

	private const string RuleSetDescriptionAttributeName = "Description";

	private const string RuleSetToolsVersionAttributeName = "ToolsVersion";

	private const string RulesNodeName = "Rules";

	private const string RulesAnalyzerIdAttributeName = "AnalyzerId";

	private const string RulesNamespaceAttributeName = "RuleNamespace";

	private const string RuleNodeName = "Rule";

	private const string RuleIdAttributeName = "Id";

	private const string IncludeNodeName = "Include";

	private const string IncludePathAttributeName = "Path";

	private const string IncludeAllNodeName = "IncludeAll";

	private const string RuleActionAttributeName = "Action";

	private const string RuleActionNoneValue = "None";

	private const string RuleActionHiddenValue = "Hidden";

	private const string RuleActionInfoValue = "Info";

	private const string RuleActionWarningValue = "Warning";

	private const string RuleActionErrorValue = "Error";

	private const string RuleActionDefaultValue = "Default";

	public static RuleSet LoadFromFile(string filePath)
	{
		filePath = FileUtilities.NormalizeAbsolutePath(filePath);
		XmlReaderSettings defaultXmlReaderSettings = GetDefaultXmlReaderSettings();
		XElement ruleSetNode = null;
		using (Stream input = FileUtilities.OpenRead(filePath))
		{
			using XmlReader reader = XmlReader.Create(input, defaultXmlReaderSettings);
			XDocument xDocument;
			try
			{
				xDocument = XDocument.Load(reader);
			}
			catch (Exception ex)
			{
				throw new InvalidRuleSetException(ex.Message);
			}
			ruleSetNode = xDocument.Elements("RuleSet").ToList()[0];
		}
		return ReadRuleSet(ruleSetNode, filePath);
	}

	private static RuleSet ReadRuleSet(XElement ruleSetNode, string filePath)
	{
		ImmutableDictionary<string, ReportDiagnostic>.Builder builder = ImmutableDictionary.CreateBuilder<string, ReportDiagnostic>();
		ReportDiagnostic generalOption = ReportDiagnostic.Default;
		ImmutableArray<RuleSetInclude>.Builder builder2 = ImmutableArray.CreateBuilder<RuleSetInclude>();
		ValidateAttribute(ruleSetNode, "ToolsVersion");
		ValidateAttribute(ruleSetNode, "Name");
		foreach (XElement item in ruleSetNode.Elements())
		{
			if (item.Name == "Rules")
			{
				foreach (KeyValuePair<string, ReportDiagnostic> item2 in ReadRules(item))
				{
					string key = item2.Key;
					ReportDiagnostic value = item2.Value;
					if (builder.TryGetValue(key, out var value2))
					{
						if (value2 != value)
						{
							throw new InvalidRuleSetException(string.Format(CodeAnalysisResources.RuleSetHasDuplicateRules, key, value2, value));
						}
					}
					else
					{
						builder.Add(key, value);
					}
				}
			}
			else if (item.Name == "Include")
			{
				builder2.Add(ReadRuleSetInclude(item));
			}
			else if (item.Name == "IncludeAll")
			{
				generalOption = ReadIncludeAll(item);
			}
		}
		return new RuleSet(filePath, generalOption, builder.ToImmutable(), builder2.ToImmutable());
	}

	private static List<KeyValuePair<string, ReportDiagnostic>> ReadRules(XElement rulesNode)
	{
		string analyzer = ReadNonEmptyAttribute(rulesNode, "AnalyzerId");
		string space = ReadNonEmptyAttribute(rulesNode, "RuleNamespace");
		List<KeyValuePair<string, ReportDiagnostic>> list = new List<KeyValuePair<string, ReportDiagnostic>>();
		foreach (XElement item in rulesNode.Elements())
		{
			if (item.Name == "Rule")
			{
				list.Add(ReadRule(item, analyzer, space));
			}
		}
		return list;
	}

	private static KeyValuePair<string, ReportDiagnostic> ReadRule(XElement ruleNode, string analyzer, string space)
	{
		string key = ReadNonEmptyAttribute(ruleNode, "Id");
		ReportDiagnostic value = ReadAction(ruleNode, allowDefault: false);
		return new KeyValuePair<string, ReportDiagnostic>(key, value);
	}

	private static RuleSetInclude ReadRuleSetInclude(XElement includeNode)
	{
		string includePath = ReadNonEmptyAttribute(includeNode, "Path");
		ReportDiagnostic action = ReadAction(includeNode, allowDefault: true);
		return new RuleSetInclude(includePath, action);
	}

	private static ReportDiagnostic ReadAction(XElement node, bool allowDefault)
	{
		string text = ReadNonEmptyAttribute(node, "Action");
		if (string.Equals(text, "Warning"))
		{
			return ReportDiagnostic.Warn;
		}
		if (string.Equals(text, "Error"))
		{
			return ReportDiagnostic.Error;
		}
		if (string.Equals(text, "Info"))
		{
			return ReportDiagnostic.Info;
		}
		if (string.Equals(text, "Hidden"))
		{
			return ReportDiagnostic.Hidden;
		}
		if (string.Equals(text, "None"))
		{
			return ReportDiagnostic.Suppress;
		}
		if (string.Equals(text, "Default") & allowDefault)
		{
			return ReportDiagnostic.Default;
		}
		throw new InvalidRuleSetException(string.Format(CodeAnalysisResources.RuleSetBadAttributeValue, "Action", text));
	}

	private static ReportDiagnostic ReadIncludeAll(XElement includeAllNode)
	{
		return ReadAction(includeAllNode, allowDefault: false);
	}

	private static string ReadNonEmptyAttribute(XElement node, string attributeName)
	{
		XAttribute xAttribute = node.Attribute(attributeName);
		if (xAttribute == null)
		{
			throw new InvalidRuleSetException(string.Format(CodeAnalysisResources.RuleSetMissingAttribute, node.Name, attributeName));
		}
		if (string.IsNullOrEmpty(xAttribute.Value))
		{
			throw new InvalidRuleSetException(string.Format(CodeAnalysisResources.RuleSetBadAttributeValue, attributeName, xAttribute.Value));
		}
		return xAttribute.Value;
	}

	private static XmlReaderSettings GetDefaultXmlReaderSettings()
	{
		return new XmlReaderSettings
		{
			CheckCharacters = true,
			CloseInput = true,
			ConformanceLevel = ConformanceLevel.Document,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			IgnoreWhitespace = true,
			DtdProcessing = DtdProcessing.Prohibit
		};
	}

	private static void ValidateAttribute(XElement node, string attributeName)
	{
		ReadNonEmptyAttribute(node, attributeName);
	}
}
