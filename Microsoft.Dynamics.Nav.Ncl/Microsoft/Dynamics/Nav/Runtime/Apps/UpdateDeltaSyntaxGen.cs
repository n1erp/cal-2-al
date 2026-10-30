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

public class UpdateDeltaSyntaxGen : DeltaSyntaxGen
{
	public UpdateDeltaSyntaxGen(IDeltaValidator validator)
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
		foreach (IGrouping<string, NavXmlUpdate> item in from d in deltas.OfType<NavXmlUpdate>()
			group d by d.Id)
		{
			bool flag = false;
			bool flag2 = false;
			List<NavXmlUpdate> list3 = new List<NavXmlUpdate>();
			foreach (NavXmlUpdate item2 in item)
			{
				if (Validator.Validate(converterBundle, item2))
				{
					list3.Add(item2);
				}
			}
			if (converterBundle.SourceNodes.TryGetValue(item.Key, out var value))
			{
				try
				{
					SyntaxNode syntaxNodeFromXml = GetSyntaxNodeFromXml(converterBundle.ALIdentifierMapper, value, list3);
					if (syntaxNodeFromXml != null)
					{
						dictionary.Add(new SyntaxNodeIdentifier(item.Key, PersonalizationV1DeltaType.Update), syntaxNodeFromXml);
						flag = true;
					}
				}
				catch (Exception ex)
				{
					flag2 = true;
					list.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Error(s) occurred when converting element: {0} of type of {1} with error message {2}:", item.Key, PersonalizationV1DeltaType.Update.ToString(), ex.Message), Verbosity.Error));
				}
			}
			if (!flag && !flag2 && !string.IsNullOrEmpty(item.Key))
			{
				list2.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Cannot convert NavXmlDelta element: {0} of type of {1}", item.Key, PersonalizationV1DeltaType.Update.ToString()), Verbosity.Warning));
			}
		}
		return new DeltaSyntaxGenResult(dictionary, list2, list);
	}

	private static SyntaxNode GetSyntaxNodeFromXml(Dictionary<string, string> alIdentifierMapper, XmlElement element, IList<NavXmlUpdate> updates)
	{
		SyntaxNode result = null;
		if (element != null && ConverterElementChecker.VerifyElementType(element, PersonalizationV1DeltaType.Update))
		{
			List<PropertySyntax> list = new List<PropertySyntax>();
			PropertySyntaxAbstractFactory factory = PropertySyntaxFactoryProducer.GetFactory(element);
			bool flag = false;
			bool flag2 = false;
			string text = null;
			foreach (NavXmlUpdate update in updates)
			{
				if (Enum.TryParse<PropertyKind>(update.Name, ignoreCase: true, out var result2) && !string.IsNullOrEmpty(update.Value))
				{
					PropertySyntax propertySyntax = factory.GetPropertySyntax(result2, update.Value);
					if (propertySyntax != null)
					{
						list.Add(propertySyntax);
					}
				}
			}
			if (flag && !flag2)
			{
				list.Add(SyntaxFactory.Property(PropertyKind.Caption, SyntaxFactory.LabelPropertyValue(SyntaxFactory.Label(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(text))))));
			}
			if (list.Count > 0)
			{
				string elementName = alIdentifierMapper[element.Attributes["ControlGUID"].Value];
				result = GetSyntaxNodeFromXml(element, elementName, list.ToArray());
			}
		}
		return result;
	}

	private static SyntaxNode GetSyntaxNodeFromXml(XmlElement element, string elementName, PropertySyntax[] propertySyntaxArr)
	{
		switch (element.Attributes["xsi:type"].Value)
		{
		case "InfopartPageDefinition":
		case "InfopartChartDefinition":
		case "InfopartSystemDefinition":
		case "ControlDefinition":
		case "ControlGroupDefinition":
		case "StackGroupDefinition":
		{
			ControlModifyChangeSyntax controlModifyChangeSyntax = SyntaxFactory.ControlModifyChange(elementName);
			PropertyListSyntax propertyListSyntax2 = SyntaxFactory.PropertyList();
			PropertySyntaxOrEmpty[] items = propertySyntaxArr;
			return controlModifyChangeSyntax.WithPropertyList(propertyListSyntax2.AddProperties(items));
		}
		case "ActionDefinition":
		case "SeparatorDefinition":
		case "ActionGroupDefinition":
		{
			ActionModifyChangeSyntax actionModifyChangeSyntax = SyntaxFactory.ActionModifyChange(elementName);
			PropertyListSyntax propertyListSyntax = SyntaxFactory.PropertyList();
			PropertySyntaxOrEmpty[] items = propertySyntaxArr;
			return actionModifyChangeSyntax.WithPropertyList(propertyListSyntax.AddProperties(items));
		}
		default:
			throw new InvalidOperationException("Unreachable!");
		}
	}
}
