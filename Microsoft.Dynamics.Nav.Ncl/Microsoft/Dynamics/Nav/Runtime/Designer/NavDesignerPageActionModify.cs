using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageActionModify : INavDesignerPageActionChange
{
	private ActionModifyChangeSyntax changeSyntax;

	internal bool HasChanges => changeSyntax.PropertyList.Properties.Count > 0;

	public string ActionName { get; }

	public SyntaxKind SyntaxKind => changeSyntax.Kind;

	public ActionChangeBaseSyntax Syntax => changeSyntax;

	internal NavDesignerPageActionModify(ActionModifyChangeSyntax syntax)
	{
		ActionName = syntax.Name.Identifier.ValueText.UnquoteIdentifier();
		changeSyntax = syntax;
	}

	internal NavDesignerPageActionModify(string actionName, PropertySyntax property)
	{
		ActionName = actionName;
		changeSyntax = SyntaxFactory.ActionModifyChange(actionName).WithPropertyList(SyntaxFactory.PropertyList().AddProperties(property));
	}

	internal bool SetProperty(PropertyKind propertyKind, PropertySyntax property)
	{
		PropertySyntax property2 = GetProperty(propertyKind);
		PropertyListSyntax propertyList;
		if (property2 != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = changeSyntax.PropertyList.Properties.Replace(property2, property);
			propertyList = changeSyntax.PropertyList.Update(properties);
		}
		else
		{
			propertyList = changeSyntax.PropertyList.AddProperties(property);
		}
		changeSyntax = changeSyntax.WithPropertyList(propertyList);
		return true;
	}

	internal bool RemoveProperty(PropertyKind propertyKind)
	{
		PropertySyntax property = GetProperty(propertyKind);
		if (property != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = changeSyntax.PropertyList.Properties.Remove(property);
			PropertyListSyntax propertyList = changeSyntax.PropertyList.Update(properties);
			changeSyntax = changeSyntax.WithPropertyList(propertyList);
			return true;
		}
		return false;
	}

	public PropertySyntax GetProperty(PropertyKind propertyKind)
	{
		return changeSyntax.GetProperty(propertyKind.ToString());
	}
}
