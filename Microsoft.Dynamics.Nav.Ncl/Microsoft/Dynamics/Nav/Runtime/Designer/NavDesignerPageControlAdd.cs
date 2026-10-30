using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageControlAdd : INavDesignerPageControlPositionalChange, INavDesignerPageControlChange
{
	private ControlAddChangeSyntax controlAddSyntax;

	public string ParentName
	{
		get
		{
			if (controlAddSyntax.ChangeKeyword.Kind == SyntaxKind.AddFirstKeyword || controlAddSyntax.ChangeKeyword.Kind == SyntaxKind.AddLastKeyword)
			{
				return controlAddSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				switch (controlAddSyntax.ChangeKeyword.Kind)
				{
				case SyntaxKind.AddFirstKeyword:
				case SyntaxKind.AddAfterKeyword:
					controlAddSyntax = controlAddSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				case SyntaxKind.AddLastKeyword:
				case SyntaxKind.AddBeforeKeyword:
					controlAddSyntax = controlAddSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddLastKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				}
			}
		}
	}

	public string AnchorControlName
	{
		get
		{
			if (controlAddSyntax.ChangeKeyword.Kind == SyntaxKind.AddAfterKeyword || controlAddSyntax.ChangeKeyword.Kind == SyntaxKind.AddBeforeKeyword)
			{
				return controlAddSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				switch (controlAddSyntax.ChangeKeyword.Kind)
				{
				case SyntaxKind.AddFirstKeyword:
				case SyntaxKind.AddAfterKeyword:
					controlAddSyntax = controlAddSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddAfterKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				case SyntaxKind.AddLastKeyword:
				case SyntaxKind.AddBeforeKeyword:
					controlAddSyntax = controlAddSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddBeforeKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				}
			}
		}
	}

	internal ImmutableList<NavDesignerPageControl> Controls
	{
		get
		{
			return controlAddSyntax.Controls.Select((ControlBaseSyntax c) => NavDesignerPageFactory.CreatePageControl(c)).ToImmutableList();
		}
		set
		{
			controlAddSyntax = controlAddSyntax.WithControls(default(SyntaxList<ControlBaseSyntax>).AddRange(value.Select((NavDesignerPageControl c) => c.Syntax)));
		}
	}

	public SyntaxKind SyntaxKind => controlAddSyntax.ChangeKeyword.Kind;

	public ControlChangeBaseSyntax Syntax => controlAddSyntax;

	internal NavDesignerPageControlAdd(NavDesignerPageControl newControl, string parentName = null, string previousControlName = null)
		: this(new NavDesignerPageControl[1] { newControl }, parentName, previousControlName)
	{
	}

	internal NavDesignerPageControlAdd(IEnumerable<NavDesignerPageControl> controlList, string parentName = null, string previousControlName = null)
	{
		SyntaxList<ControlBaseSyntax> controls = default(SyntaxList<ControlBaseSyntax>).AddRange(controlList.Select((NavDesignerPageControl c) => c.Syntax));
		if (!string.IsNullOrEmpty(previousControlName))
		{
			controlAddSyntax = SyntaxFactory.ControlAddChange(SyntaxFactory.Token(SyntaxKind.AddAfterKeyword), SyntaxFactory.IdentifierName(previousControlName), controls);
		}
		else if (!string.IsNullOrEmpty(parentName))
		{
			controlAddSyntax = SyntaxFactory.ControlAddChange(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword), SyntaxFactory.IdentifierName(parentName), controls);
		}
		else
		{
			controlAddSyntax = SyntaxFactory.ControlAddChange(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword), SyntaxFactory.IdentifierName(AreaKind.Content.ToString()), controls);
		}
	}

	internal NavDesignerPageControlAdd(ControlAddChangeSyntax addSyntax)
	{
		controlAddSyntax = addSyntax;
	}

	internal bool SetProperty(string controlName, PropertyKind propertyKind, PropertySyntax property)
	{
		NavDesignerPageControl navDesignerPageControl = Controls.Single((NavDesignerPageControl c) => c.ControlName.Equals(controlName));
		PropertySyntax property2 = GetProperty(navDesignerPageControl, propertyKind);
		PropertyListSyntax propertyListSyntax;
		if (property2 != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = navDesignerPageControl.Syntax.PropertyList.Properties.Replace(property2, property);
			propertyListSyntax = navDesignerPageControl.Syntax.PropertyList.Update(properties);
		}
		else
		{
			propertyListSyntax = navDesignerPageControl.Syntax.PropertyList.AddProperties(property);
		}
		NavDesignerPageControl newValue = NavDesignerPageFactory.CreatePageControl(navDesignerPageControl.Syntax, propertyListSyntax);
		Controls = Controls.Replace(navDesignerPageControl, newValue);
		return true;
	}

	internal bool RemoveProperty(string controlName, PropertyKind propertyKind)
	{
		NavDesignerPageControl navDesignerPageControl = Controls.Single((NavDesignerPageControl c) => c.ControlName.Equals(controlName));
		PropertySyntax property = GetProperty(navDesignerPageControl, propertyKind);
		if (property != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = navDesignerPageControl.Syntax.PropertyList.Properties.Remove(property);
			PropertyListSyntax propertyListSyntax = navDesignerPageControl.Syntax.PropertyList.Update(properties);
			NavDesignerPageControl newValue = NavDesignerPageFactory.CreatePageControl(navDesignerPageControl.Syntax, propertyListSyntax);
			Controls = Controls.Replace(navDesignerPageControl, newValue);
			return true;
		}
		return false;
	}

	private static PropertySyntax GetProperty(NavDesignerPageControl control, PropertyKind propertyKind)
	{
		return control.Syntax.GetProperty(propertyKind.ToString());
	}
}
