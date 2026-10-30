using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Media;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDesignerVirtualTableMediaFactory
{
	private readonly Dictionary<string, Guid> knownMedia = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

	public NavMedia GetNamedMedia(NavDesignerImage image, int parentTableId)
	{
		string text = MakeKey(image.MediaName, parentTableId);
		bool flag = false;
		bool commit = false;
		if (!knownMedia.TryGetValue(text, out var value))
		{
			List<Guid> source = NavMediaHelper.FindMediaByDescription(parentTableId, text).ToList();
			if (source.Any())
			{
				value = source.First();
			}
			else
			{
				using Stream stream = new MemoryStream();
				image.SaveToStream(stream);
				if (!NavCurrentThread.Session.IsTransactionActive())
				{
					NavCurrentThread.Session.BeginTransaction();
					flag = true;
				}
				try
				{
					using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[2] { 2000000181, 2000000184 }))
					{
						using NavMediaImport navMediaImport = new NavMediaImport(NavCurrentThread.Session, NavCurrentThread.Session.CompanyName);
						value = navMediaImport.ImportMediaObject(parentTableId, stream, text, "image/png");
					}
					commit = true;
				}
				finally
				{
					if (flag)
					{
						NavCurrentThread.Session.EndTransaction(commit);
					}
				}
			}
			knownMedia.Add(text, value);
		}
		return new NavMedia(value);
	}

	private static string MakeKey(string mediaName, int parentTableId)
	{
		return "DesignerImage_" + parentTableId + "_" + mediaName;
	}
}
