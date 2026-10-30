using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class NavAppSourceFileCollection : KeyedCollection<string, NavAppSourceFile>
{
	protected override string GetKeyForItem(NavAppSourceFile item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		return item.FileLocation;
	}
}
