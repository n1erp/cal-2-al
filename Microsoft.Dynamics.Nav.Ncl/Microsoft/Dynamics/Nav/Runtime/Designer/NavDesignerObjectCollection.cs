using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerObjectCollection
{
	public Dictionary<int, NavDesignerPage> Pages { get; } = new Dictionary<int, NavDesignerPage>();

	public Dictionary<int, NavDesignerPageExtension> PageExtensions { get; } = new Dictionary<int, NavDesignerPageExtension>();

	public Dictionary<string, NavDesignerPageCustomization> PageCustomizations { get; } = new Dictionary<string, NavDesignerPageCustomization>();

	public Dictionary<int, NavDesignerTable> Tables { get; } = new Dictionary<int, NavDesignerTable>();

	public Dictionary<int, NavDesignerTableExtension> TableExtensions { get; } = new Dictionary<int, NavDesignerTableExtension>();

	public Dictionary<string, NavDesignerProfile> Profiles { get; internal set; } = new Dictionary<string, NavDesignerProfile>(StringComparer.OrdinalIgnoreCase);

	public IList<NavDesignerObject> OtherObjects { get; } = new List<NavDesignerObject>();

	public IEnumerable<NavDesignerObject> AllObjects => ((IEnumerable<NavDesignerObject>)Tables.Values).Concat((IEnumerable<NavDesignerObject>)Pages.Values).Concat(TableExtensions.Values).Concat(PageExtensions.Values)
		.Concat(PageCustomizations.Values)
		.Concat(Profiles.Values)
		.Concat(OtherObjects);

	public void AddObject(NavDesignerObject designerObject)
	{
		if (!(designerObject is NavDesignerPage navDesignerPage))
		{
			if (!(designerObject is NavDesignerPageExtension navDesignerPageExtension))
			{
				if (!(designerObject is NavDesignerPageCustomization navDesignerPageCustomization))
				{
					if (!(designerObject is NavDesignerTable navDesignerTable))
					{
						if (!(designerObject is NavDesignerTableExtension navDesignerTableExtension))
						{
							if (designerObject is NavDesignerProfile navDesignerProfile)
							{
								Profiles.Add(navDesignerProfile.ObjectName, navDesignerProfile);
							}
							else
							{
								OtherObjects.Add(designerObject);
							}
						}
						else
						{
							TableExtensions.Add(navDesignerTableExtension.ObjectId.ObjectNumber, navDesignerTableExtension);
						}
					}
					else
					{
						Tables.Add(navDesignerTable.ObjectId.ObjectNumber, navDesignerTable);
					}
				}
				else
				{
					PageCustomizations.Add(navDesignerPageCustomization.ObjectName, navDesignerPageCustomization);
				}
			}
			else
			{
				PageExtensions.Add(navDesignerPageExtension.ObjectId.ObjectNumber, navDesignerPageExtension);
			}
		}
		else
		{
			Pages.Add(navDesignerPage.ObjectId.ObjectNumber, navDesignerPage);
		}
	}
}
