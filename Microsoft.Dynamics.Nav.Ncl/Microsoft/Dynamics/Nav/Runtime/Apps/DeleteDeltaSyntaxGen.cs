using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class DeleteDeltaSyntaxGen : DeltaSyntaxGen
{
	public DeleteDeltaSyntaxGen(IDeltaValidator validator)
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
		foreach (NavXmlDelete item in deltas.OfType<NavXmlDelete>())
		{
			bool flag = false;
			bool flag2 = false;
			if (Validator.Validate(converterBundle, item) && converterBundle.SourceNodes.TryGetValue(item.Id, out var value))
			{
				try
				{
					SyntaxNode syntaxNodeFromXml = GetSyntaxNodeFromXml(converterBundle.ALIdentifierMapper, value);
					if (syntaxNodeFromXml != null)
					{
						value.ParentNode.RemoveChild(value);
						dictionary.Add(new SyntaxNodeIdentifier(item.Id, PersonalizationV1DeltaType.Delete), syntaxNodeFromXml);
						flag = true;
					}
				}
				catch (Exception ex)
				{
					flag2 = true;
					list.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Error(s) occurred when converting element: {0} of type of {1} with error message {2}:", item.Id, PersonalizationV1DeltaType.Delete.ToString(), ex.Message), Verbosity.Error));
				}
			}
			if (!flag && !flag2 && !string.IsNullOrEmpty(item.Id))
			{
				list2.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Cannot convert NavXmlDelta element: {0} of type of {1}", item.Id, PersonalizationV1DeltaType.Delete.ToString()), Verbosity.Warning));
			}
		}
		return new DeltaSyntaxGenResult(dictionary, list2, list);
	}

	private static SyntaxNode GetSyntaxNodeFromXml(Dictionary<string, string> alIdentifierMapper, XmlElement element)
	{
		SyntaxNode result = null;
		if (element != null && ConverterElementChecker.VerifyElementType(element, PersonalizationV1DeltaType.Delete))
		{
			switch (element.Attributes["xsi:type"].Value)
			{
			case "InfopartPageDefinition":
			case "ControlDefinition":
			case "ControlGroupDefinition":
				result = SyntaxFactory.ControlModifyChange(alIdentifierMapper[element.Attributes["ControlGUID"].Value]).WithPropertyList(SyntaxFactory.PropertyList().AddProperties(SyntaxFactory.Property(PropertyKind.Visible, SyntaxFactory.BooleanPropertyValue(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.FalseKeyword))))));
				break;
			case "ActionDefinition":
			case "ActionGroupDefinition":
				result = SyntaxFactory.ActionModifyChange(alIdentifierMapper[element.Attributes["ControlGUID"].Value]).WithPropertyList(SyntaxFactory.PropertyList().AddProperties(SyntaxFactory.Property(PropertyKind.Visible, SyntaxFactory.BooleanPropertyValue(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.FalseKeyword))))));
				break;
			default:
				throw new InvalidOperationException("Unreachable!");
			}
		}
		return result;
	}
}
