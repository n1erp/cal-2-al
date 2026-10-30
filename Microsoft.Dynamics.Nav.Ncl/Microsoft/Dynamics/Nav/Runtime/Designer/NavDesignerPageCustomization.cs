using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageCustomization : NavDesignerObject
{
	public NavDesignerPageModifications Modifications;

	internal string PageCustomizationMetadata = string.Empty;

	public PageCustomizationSyntax PageCustomizationSyntax { get; }

	public string BasePageName { get; set; }

	internal override ObjectSyntax Syntax => GetObjectSyntax();

	public NavAppObjectMetadataRuntimeDeltas RuntimeDeltas
	{
		get
		{
			if (!string.IsNullOrEmpty(PageCustomizationMetadata))
			{
				XDocument xDocument = XDocument.Parse(PageCustomizationMetadata);
				return NavAppObjectMetadataRuntimeDeltas.FromXml(xDocument);
			}
			return null;
		}
	}

	protected NavDesignerPageCustomization(PageCustomizationSyntax pageCustomization, string name)
		: base(name, pageCustomization)
	{
		PageCustomizationSyntax = pageCustomization;
		Modifications = new NavDesignerPageModifications(pageCustomization);
		BasePageName = pageCustomization.BaseObject.Identifier.ToString().UnquoteIdentifier();
	}

	public static NavDesignerPageCustomization Create(string objectName, string extendsObjectName)
	{
		PageCustomizationSyntax pageCustomization = SyntaxFactory.PageCustomization(objectName, SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(extendsObjectName)));
		return new NavDesignerPageCustomization(pageCustomization, objectName);
	}

	internal static IEnumerable<NavDesignerPageCustomization> CreateWithUniqueId(string source, NavDesignerExtension designerExtension)
	{
		IEnumerable<NavDesignerPageCustomization> enumerable = NavDesignerObjectFactory.CreateFromSource(source).OfType<NavDesignerPageCustomization>();
		foreach (NavDesignerPageCustomization item in enumerable)
		{
			NavDesignerPageCustomization navDesignerPageCustomization;
			if (!designerExtension.IsPageCustomizationIdInUse(item.ObjectName))
			{
				navDesignerPageCustomization = item;
			}
			else
			{
				string unusedPageCustomizationId = designerExtension.GetUnusedPageCustomizationId();
				PageCustomizationSyntax pageCustomization = item.PageCustomizationSyntax?.WithName(SyntaxFactory.IdentifierName(unusedPageCustomizationId));
				navDesignerPageCustomization = new NavDesignerPageCustomization(pageCustomization, unusedPageCustomizationId);
			}
			yield return navDesignerPageCustomization;
		}
	}

	public static NavDesignerPageCustomization Create(PageCustomizationSyntax pageCustomization)
	{
		if (pageCustomization != null)
		{
			string objectName = pageCustomization.GetObjectName();
			return new NavDesignerPageCustomization(pageCustomization, objectName);
		}
		throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The method parameter '{0}' is not valid. It must be of type '{1}'", "pageCustomization", typeof(PageCustomizationSyntax)));
	}

	private ObjectSyntax GetObjectSyntax()
	{
		PageCustomizationSyntax pageCustomizationSyntax = PageCustomizationSyntax;
		if (pageCustomizationSyntax == null)
		{
			return null;
		}
		pageCustomizationSyntax = pageCustomizationSyntax.WithPropertyList(Modifications.PropertyListSyntax).WithLayout(Modifications.LayoutSyntax).WithActions(Modifications.ActionsSyntax);
		PageExtensionViewListSyntax viewsSyntax = Modifications.ViewsSyntax;
		if (pageCustomizationSyntax.Views != null || viewsSyntax.Changes.Count > 0)
		{
			pageCustomizationSyntax = pageCustomizationSyntax.WithViews(viewsSyntax);
		}
		return pageCustomizationSyntax;
	}
}
