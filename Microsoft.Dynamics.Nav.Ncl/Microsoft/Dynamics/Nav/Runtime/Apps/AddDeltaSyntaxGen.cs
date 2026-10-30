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

public class AddDeltaSyntaxGen : DeltaSyntaxGen
{
	public AddDeltaSyntaxGen(IDeltaValidator validator)
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
		foreach (NavXmlAdd item in deltas.OfType<NavXmlAdd>())
		{
			bool flag = false;
			bool flag2 = false;
			if (Validator.Validate(converterBundle, item) && converterBundle.SourceNodes.TryGetValue(item.Destination, out var value))
			{
				if (!string.IsNullOrEmpty(item.ParentPath))
				{
					value = value.SelectSingleNode(item.ParentPath) as XmlElement;
				}
				string areaKind = ConverterHelper.GetAreaKind(value, item.Destination, converterBundle.SourceTempNodes);
				if (converterBundle.SourceDefinitionNodes.ContainsKey(item.Destination) || areaKind != null)
				{
					XmlElement xmlElement = ConverterHelper.ConvertAddNode(item.Node, converterBundle.MasterPageDocument, converterBundle.SourceNodes, converterBundle.SourceDefinitionNodes, converterBundle.DistinctALIdentifier, converterBundle.ALIdentifierMapper);
					try
					{
						SyntaxNode syntaxNodeFromXml = GetSyntaxNodeFromXml(converterBundle.SourceTempIdSet, value, xmlElement);
						if (syntaxNodeFromXml != null)
						{
							value.AppendChild(xmlElement);
							dictionary.Add(new SyntaxNodeIdentifier(item.Id, PersonalizationV1DeltaType.Add), SyntaxFactory.ActionAddChange(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword), areaKind ?? converterBundle.ALIdentifierMapper[value.Attributes["ControlGUID"].Value]).AddActions(syntaxNodeFromXml as ActionBaseSyntax));
							flag = true;
						}
					}
					catch (Exception ex)
					{
						flag2 = true;
						list.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Error(s) occurred when converting element: {0} of type of {1} with error message {2}:", item.Id, PersonalizationV1DeltaType.Add.ToString(), ex.Message), Verbosity.Error));
					}
				}
			}
			if (!flag && !flag2 && !string.IsNullOrEmpty(item.Id))
			{
				list2.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Cannot convert NavXmlDelta element: {0} of type of {1}", item.Id, PersonalizationV1DeltaType.Add.ToString()), Verbosity.Warning));
			}
		}
		return new DeltaSyntaxGenResult(dictionary, list2, list);
	}

	private static SyntaxNode GetSyntaxNodeFromXml(HashSet<int> sourceTempIdSet, XmlElement parentElement, XmlElement childElement)
	{
		SyntaxNode result = null;
		if (parentElement != null && childElement != null && (!int.TryParse(childElement.Attributes["ClonedFromID"]?.Value, out var result2) || !sourceTempIdSet.Contains(result2)) && ConverterElementChecker.VerifyElementType(parentElement, PersonalizationV1DeltaType.Add) && ConverterElementChecker.VerifyElementType(childElement, PersonalizationV1DeltaType.Update))
		{
			PropertySyntaxAbstractFactory factory = PropertySyntaxFactoryProducer.GetFactory(childElement);
			List<PropertySyntax> list = new List<PropertySyntax>();
			bool flag = false;
			bool flag2 = false;
			List<SyntaxNode> list2 = new List<SyntaxNode>();
			if (string.Equals(childElement.Attributes["xsi:type"]?.Value, "ActionGroupDefinition") && childElement.ChildNodes.Count > 0)
			{
				foreach (object childNode in childElement.ChildNodes)
				{
					SyntaxNode syntaxNodeFromXml = GetSyntaxNodeFromXml(sourceTempIdSet, childElement, childNode as XmlElement);
					if (syntaxNodeFromXml != null)
					{
						list2.Add(syntaxNodeFromXml);
					}
				}
			}
			foreach (XmlAttribute attribute in childElement.Attributes)
			{
				if (!Enum.TryParse<PropertyKind>(attribute.Name, ignoreCase: true, out var result3) || string.IsNullOrEmpty(attribute.Value))
				{
					continue;
				}
				PropertySyntax propertySyntax = factory.GetPropertySyntax(result3, attribute.Value);
				if (propertySyntax != null)
				{
					if (result3 == PropertyKind.Promoted)
					{
						flag = true;
					}
					if (result3 == PropertyKind.PromotedIsBig)
					{
						flag2 = true;
					}
					list.Add(propertySyntax);
				}
			}
			list.AddRange(SpecialPropertySyntaxHandler.GetSpecialPropertySyntax(childElement));
			if (!flag & flag2)
			{
				list.Add(SyntaxFactory.Property(PropertyKind.Promoted, SyntaxFactory.BooleanPropertyValue(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.TrueKeyword)))));
			}
			if (list.Count > 0)
			{
				result = GetSyntaxNodeFromXml(childElement, list.ToArray(), list2.Select((SyntaxNode node) => node as ActionBaseSyntax).ToArray());
			}
		}
		return result;
	}

	private static SyntaxNode GetSyntaxNodeFromXml(XmlElement element, PropertySyntax[] propertySyntaxArr, ActionBaseSyntax[] childSyntaxNode)
	{
		switch (element.Attributes["xsi:type"]?.Value)
		{
		case "ActionDefinition":
		{
			PageActionSyntax pageActionSyntax = SyntaxFactory.PageAction(SyntaxFactory.IdentifierName(element.Attributes["Name"].Value));
			PropertyListSyntax propertyListSyntax2 = SyntaxFactory.PropertyList();
			PropertySyntaxOrEmpty[] items = propertySyntaxArr;
			return pageActionSyntax.WithPropertyList(propertyListSyntax2.AddProperties(items));
		}
		case "ActionGroupDefinition":
		{
			PageActionGroupSyntax pageActionGroupSyntax = SyntaxFactory.PageActionGroup(SyntaxFactory.IdentifierName(element.Attributes["Name"].Value)).AddActions(childSyntaxNode);
			PropertyListSyntax propertyListSyntax = SyntaxFactory.PropertyList();
			PropertySyntaxOrEmpty[] items = propertySyntaxArr;
			return pageActionGroupSyntax.WithPropertyList(propertyListSyntax.AddProperties(items));
		}
		default:
			throw new InvalidOperationException("Unsupported");
		}
	}
}
