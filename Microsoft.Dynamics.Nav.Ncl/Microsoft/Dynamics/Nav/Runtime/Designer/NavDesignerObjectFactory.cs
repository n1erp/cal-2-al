using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal static class NavDesignerObjectFactory
{
	internal static IEnumerable<NavDesignerObject> CreateFromSource(string objectSource)
	{
		SyntaxTree syntaxTree = SyntaxTree.ParseObjectText(objectSource);
		IEnumerable<ObjectSyntax> enumerable = syntaxTree.GetRoot().ChildNodes().OfType<ObjectSyntax>();
		foreach (ObjectSyntax item in enumerable)
		{
			switch (item.Kind)
			{
			case SyntaxKind.PageObject:
				yield return NavDesignerPage.Create((PageSyntax)item);
				break;
			case SyntaxKind.PageExtensionObject:
				yield return NavDesignerPageExtension.Create((PageExtensionSyntax)item);
				break;
			case SyntaxKind.PageCustomizationObject:
				yield return NavDesignerPageCustomization.Create((PageCustomizationSyntax)item);
				break;
			case SyntaxKind.TableObject:
				yield return NavDesignerTable.Create((TableSyntax)item);
				break;
			case SyntaxKind.TableExtensionObject:
				yield return NavDesignerTableExtension.Create((TableExtensionSyntax)item);
				break;
			case SyntaxKind.ProfileObject:
				yield return NavDesignerProfile.Create((ProfileSyntax)item);
				break;
			case SyntaxKind.EnumType:
			case SyntaxKind.EnumExtensionType:
			case SyntaxKind.CodeunitObject:
			case SyntaxKind.ReportObject:
			case SyntaxKind.XmlPortObject:
			case SyntaxKind.QueryObject:
				yield return NavDesignerApplicationObject.CreateFromSyntax((ApplicationObjectSyntax)item);
				break;
			case SyntaxKind.ControlAddInObject:
			case SyntaxKind.DotNetPackage:
				yield return NavDesignerObject.CreateFromObjectSyntax(item);
				break;
			default:
				ReportWarningForUnhandledObject(item);
				yield return NavDesignerObject.CreateFromObjectSyntax(item);
				break;
			}
		}
	}

	private static void ReportWarningForUnhandledObject(ObjectSyntax obj)
	{
		string message = string.Format(CultureInfo.CurrentCulture, "Unexpected object type '{0}' in NavDesignerObjectFactory.", obj.Kind.ToString());
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00008IN", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, message);
	}
}
