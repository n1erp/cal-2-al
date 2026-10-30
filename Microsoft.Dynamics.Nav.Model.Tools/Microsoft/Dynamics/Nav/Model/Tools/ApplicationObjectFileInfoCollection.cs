using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class ApplicationObjectFileInfoCollection : IEnumerable<ApplicationObjectFileInfo>, IEnumerable
{
	private IDictionary<ApplicationObject, ApplicationObjectFileInfo> info;

	public IEnumerable<FileInfo> Files => info.Values.Select((ApplicationObjectFileInfo i) => new FileInfo(i.FileName));

	public ObjectCollection ObjectCollection => new ObjectCollection(info.Keys);

	public Dictionary<ApplicationObject, FileInfo> ObjectFileMap => info.Values.ToDictionary((ApplicationObjectFileInfo i) => i.ApplicationObject, (ApplicationObjectFileInfo i) => new FileInfo(i.FileName));

	public ApplicationObjectFileInfo this[ApplicationObject index]
	{
		get
		{
			if (index == null)
			{
				return null;
			}
			ApplicationObjectFileInfo value = null;
			info.TryGetValue(index, out value);
			return value;
		}
	}

	private ApplicationObjectFileInfoCollection(IDictionary<ApplicationObject, ApplicationObjectFileInfo> info)
	{
		this.info = info;
	}

	internal static ApplicationObjectFileInfoCollection Create(IEnumerable<ApplicationObjectFileInfo> collection, string context)
	{
		Dictionary<ApplicationObject, ApplicationObjectFileInfo> dictionary = new Dictionary<ApplicationObject, ApplicationObjectFileInfo>(ApplicationObject.EqualityComparer);
		foreach (ApplicationObjectFileInfo item in collection)
		{
			try
			{
				dictionary.Add(item.ApplicationObject, item);
			}
			catch (ArgumentException)
			{
				throw new ModelToolException(NonLocalizable.DuplicateObjects, item.ObjectType, item.Id, context);
			}
		}
		return new ApplicationObjectFileInfoCollection(dictionary);
	}

	public IEnumerator<ApplicationObjectFileInfo> GetEnumerator()
	{
		return info.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return info.Values.GetEnumerator();
	}
}
