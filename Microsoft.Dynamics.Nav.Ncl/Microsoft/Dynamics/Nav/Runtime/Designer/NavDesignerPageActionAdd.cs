using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageActionAdd : INavDesignerPageActionPositionalChange, INavDesignerPageActionChange
{
	private ActionAddChangeSyntax actionAddChangeSyntax;

	public string ParentName
	{
		get
		{
			if (actionAddChangeSyntax.ChangeKeyword.Kind == SyntaxKind.AddFirstKeyword || actionAddChangeSyntax.ChangeKeyword.Kind == SyntaxKind.AddLastKeyword)
			{
				return actionAddChangeSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				switch (actionAddChangeSyntax.ChangeKeyword.Kind)
				{
				case SyntaxKind.AddFirstKeyword:
				case SyntaxKind.AddAfterKeyword:
					actionAddChangeSyntax = actionAddChangeSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				case SyntaxKind.AddLastKeyword:
				case SyntaxKind.AddBeforeKeyword:
					actionAddChangeSyntax = actionAddChangeSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddLastKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				}
			}
		}
	}

	public string AnchorActionName
	{
		get
		{
			if (actionAddChangeSyntax.ChangeKeyword.Kind == SyntaxKind.AddAfterKeyword || actionAddChangeSyntax.ChangeKeyword.Kind == SyntaxKind.AddBeforeKeyword)
			{
				return actionAddChangeSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				switch (actionAddChangeSyntax.ChangeKeyword.Kind)
				{
				case SyntaxKind.AddFirstKeyword:
				case SyntaxKind.AddAfterKeyword:
					actionAddChangeSyntax = actionAddChangeSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddAfterKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				case SyntaxKind.AddLastKeyword:
				case SyntaxKind.AddBeforeKeyword:
					actionAddChangeSyntax = actionAddChangeSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddBeforeKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				}
			}
		}
	}

	internal ImmutableList<NavDesignerPageAction> Actions
	{
		get
		{
			return actionAddChangeSyntax.Actions.Select((ActionBaseSyntax c) => NavDesignerPageFactory.CreatePageAction(c)).ToImmutableList();
		}
		set
		{
			actionAddChangeSyntax = actionAddChangeSyntax.WithActions(default(SyntaxList<ActionBaseSyntax>).AddRange(value.Select((NavDesignerPageAction c) => c.Syntax)));
		}
	}

	public SyntaxKind SyntaxKind => actionAddChangeSyntax.ChangeKeyword.Kind;

	public ActionChangeBaseSyntax Syntax => actionAddChangeSyntax;

	internal NavDesignerPageActionAdd(NavDesignerPageAction newAction, ActionAreaKind actionAreaKind, string parentName = null, string previousActionName = null)
		: this(new NavDesignerPageAction[1] { newAction }, actionAreaKind, parentName, previousActionName)
	{
	}

	internal NavDesignerPageActionAdd(IEnumerable<NavDesignerPageAction> actionList, ActionAreaKind actionAreaKind, string parentName = null, string previousActionName = null)
	{
		SyntaxList<ActionBaseSyntax> actions = default(SyntaxList<ActionBaseSyntax>).AddRange(actionList.Select((NavDesignerPageAction c) => c.Syntax));
		if (!string.IsNullOrEmpty(previousActionName))
		{
			actionAddChangeSyntax = SyntaxFactory.ActionAddChange(SyntaxFactory.Token(SyntaxKind.AddAfterKeyword), SyntaxFactory.IdentifierName(previousActionName), actions);
		}
		else if (!string.IsNullOrEmpty(parentName))
		{
			actionAddChangeSyntax = SyntaxFactory.ActionAddChange(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword), SyntaxFactory.IdentifierName(parentName), actions);
		}
		else
		{
			actionAddChangeSyntax = SyntaxFactory.ActionAddChange(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword), SyntaxFactory.IdentifierName(actionAreaKind.ToString()), actions);
		}
	}

	internal NavDesignerPageActionAdd(ActionAddChangeSyntax addSyntax)
	{
		actionAddChangeSyntax = addSyntax;
	}

	internal bool SetProperty(string actionName, PropertyKind propertyKind, PropertySyntax property)
	{
		NavDesignerPageAction navDesignerPageAction = Actions.Single((NavDesignerPageAction c) => c.ActionName.Equals(actionName));
		PropertySyntax property2 = GetProperty(navDesignerPageAction, propertyKind);
		PropertyListSyntax propertyListSyntax;
		if (property2 != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = navDesignerPageAction.Syntax.PropertyList.Properties.Replace(property2, property);
			propertyListSyntax = navDesignerPageAction.Syntax.PropertyList.Update(properties);
		}
		else
		{
			propertyListSyntax = navDesignerPageAction.Syntax.PropertyList.AddProperties(property);
		}
		NavDesignerPageAction newValue = NavDesignerPageFactory.CreatePageAction(navDesignerPageAction.Syntax, propertyListSyntax);
		Actions = Actions.Replace(navDesignerPageAction, newValue);
		return true;
	}

	internal bool RemoveProperty(string actionName, PropertyKind propertyKind)
	{
		NavDesignerPageAction navDesignerPageAction = Actions.Single((NavDesignerPageAction c) => c.ActionName.Equals(actionName));
		PropertySyntax property = GetProperty(navDesignerPageAction, propertyKind);
		if (property != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = navDesignerPageAction.Syntax.PropertyList.Properties.Remove(property);
			PropertyListSyntax propertyListSyntax = navDesignerPageAction.Syntax.PropertyList.Update(properties);
			NavDesignerPageAction newValue = NavDesignerPageFactory.CreatePageAction(navDesignerPageAction.Syntax, propertyListSyntax);
			Actions = Actions.Replace(navDesignerPageAction, newValue);
			return true;
		}
		return false;
	}

	private static PropertySyntax GetProperty(NavDesignerPageAction action, PropertyKind propertyKind)
	{
		return action.Syntax.GetProperty(propertyKind.ToString());
	}
}
