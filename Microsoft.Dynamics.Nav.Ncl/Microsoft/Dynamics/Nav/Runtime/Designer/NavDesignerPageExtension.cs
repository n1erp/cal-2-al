using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageExtension : NavDesignerApplicationObject
{
	public PageExtensionSyntax PageExtensionSyntax { get; }

	internal NavDesignerPageModifications Modifications { get; set; }

	public string BasePageName { get; set; }

	internal override ObjectSyntax Syntax => GetObjectSyntax();

	protected NavDesignerPageExtension(PageExtensionSyntax pageExtension, int id, string name)
		: base(new ApplicationObjectId(ObjectType.PageExtension, id), name)
	{
		PageExtensionSyntax = pageExtension;
		Modifications = new NavDesignerPageModifications(pageExtension);
		BasePageName = pageExtension.BaseObject.Identifier.ToString().UnquoteIdentifier();
	}

	public static NavDesignerPageExtension Create(int id, string objectName, string extendsObjectName)
	{
		PageExtensionSyntax pageExtension = SyntaxFactory.PageExtension(id, objectName, extendsObjectName);
		return new NavDesignerPageExtension(pageExtension, id, objectName);
	}

	public static NavDesignerPageExtension Create(PageExtensionSyntax pageExtension)
	{
		if (pageExtension != null)
		{
			int objectId = pageExtension.GetObjectId();
			string objectName = pageExtension.GetObjectName();
			return new NavDesignerPageExtension(pageExtension, objectId, objectName);
		}
		throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The method parameter '{0}' is not valid. It must be of type '{1}'", "pageExtension", typeof(PageExtensionSyntax)));
	}

	private ObjectSyntax GetObjectSyntax()
	{
		PageExtensionSyntax pageExtensionSyntax = PageExtensionSyntax;
		if (pageExtensionSyntax == null)
		{
			return null;
		}
		pageExtensionSyntax = pageExtensionSyntax.WithPropertyList(Modifications.PropertyListSyntax).WithLayout(Modifications.LayoutSyntax).WithActions(Modifications.ActionsSyntax);
		PageExtensionViewListSyntax viewsSyntax = Modifications.ViewsSyntax;
		if (pageExtensionSyntax.Views != null || viewsSyntax.Changes.Count > 0)
		{
			pageExtensionSyntax = pageExtensionSyntax.WithViews(viewsSyntax);
		}
		return pageExtensionSyntax;
	}
}
