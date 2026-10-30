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

public class MoveDeltaSyntaxGen : DeltaSyntaxGen
{
	public MoveDeltaSyntaxGen(IDeltaValidator validator)
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
		foreach (NavXmlMove item in deltas.OfType<NavXmlMove>())
		{
			bool flag = false;
			bool flag2 = false;
			if (Validator.Validate(converterBundle, item) && converterBundle.SourceNodes.TryGetValue(item.Destination, out var value) && converterBundle.SourceNodes.TryGetValue(item.Id, out var value2))
			{
				string areaKind = ConverterHelper.GetAreaKind(value, item.Destination, converterBundle.SourceTempNodes);
				if (converterBundle.SourceDefinitionNodes.ContainsKey(item.Destination) || areaKind != null)
				{
					try
					{
						SyntaxNode syntaxNodeFromXml = GetSyntaxNodeFromXml(converterBundle.ALIdentifierMapper, value, value2, areaKind);
						if (syntaxNodeFromXml != null)
						{
							value2.ParentNode.RemoveChild(value2);
							value.AppendChild(value2);
							dictionary.Add(new SyntaxNodeIdentifier(item.Id, PersonalizationV1DeltaType.Move), syntaxNodeFromXml);
							flag = true;
						}
					}
					catch (Exception ex)
					{
						flag2 = true;
						list.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Error(s) occurred when converting element: {0} of type of {1} with error message {2}:", item.Id, PersonalizationV1DeltaType.Move.ToString(), ex.Message), Verbosity.Error));
					}
				}
			}
			if (!flag && !flag2 && !string.IsNullOrEmpty(item.Id))
			{
				list2.Add(new NavXmlDeltaConversionDiagnostics(string.Format(CultureInfo.InvariantCulture, "Cannot convert NavXmlDelta element: {0} of type of {1}", item.Id, PersonalizationV1DeltaType.Move.ToString()), Verbosity.Warning));
			}
		}
		return new DeltaSyntaxGenResult(dictionary, list2, list);
	}

	private static SyntaxNode GetSyntaxNodeFromXml(Dictionary<string, string> alIdentifierMapper, XmlElement parentElement, XmlElement childElement, string areaKind)
	{
		SyntaxNode result = null;
		if (parentElement != null && childElement != null && ConverterElementChecker.VerifyElementType(parentElement, PersonalizationV1DeltaType.Move) && ConverterElementChecker.VerifyElementType(childElement, PersonalizationV1DeltaType.Update))
		{
			switch (parentElement.Attributes["xsi:type"].Value)
			{
			case "ControlContainerPlaceHolder":
			case "ControlContainerDefinition":
			case "ControlGroupDefinition":
				result = SyntaxFactory.ControlMoveChange(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), SyntaxFactory.IdentifierName(areaKind ?? alIdentifierMapper[parentElement.Attributes["ControlGUID"].Value]), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(alIdentifierMapper[childElement.Attributes["ControlGUID"].Value])));
				break;
			case "ActionContainerPlaceHolder":
			case "NavigationGroupDefinition":
			case "ActionContainerDefinition":
			case "ActionGroupDefinition":
				result = SyntaxFactory.ActionMoveChange(SyntaxFactory.Token(SyntaxKind.MoveFirstKeyword), SyntaxFactory.IdentifierName(areaKind ?? alIdentifierMapper[parentElement.Attributes["ControlGUID"].Value]), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(SyntaxFactory.IdentifierName(alIdentifierMapper[childElement.Attributes["ControlGUID"].Value])));
				break;
			default:
				throw new InvalidOperationException("Unreachable!");
			}
		}
		return result;
	}
}
