using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Dynamics.Nav.Types.Media;

[Serializable]
public class NavMediaDescriptionCollection : ICollection<NavMediaDescription>, IEnumerable<NavMediaDescription>, IEnumerable
{
	private readonly IList<NavMediaDescription> mediaDescriptions;

	public Collection<NavMediaDescription> MediaDescriptions => new Collection<NavMediaDescription>(mediaDescriptions);

	public int Count => mediaDescriptions.Count;

	public bool IsReadOnly => false;

	public NavMediaDescription this[int index]
	{
		get
		{
			return mediaDescriptions[index];
		}
		set
		{
			mediaDescriptions[index] = value;
		}
	}

	public NavMediaDescriptionCollection()
	{
		mediaDescriptions = new List<NavMediaDescription>();
	}

	public IEnumerator<NavMediaDescription> GetEnumerator()
	{
		return mediaDescriptions.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(NavMediaDescription item)
	{
		if (item != null)
		{
			mediaDescriptions.Add(item);
		}
	}

	public void Clear()
	{
		mediaDescriptions.Clear();
	}

	public bool Contains(NavMediaDescription item)
	{
		return mediaDescriptions.Contains(item);
	}

	public void CopyTo(NavMediaDescription[] array, int arrayIndex)
	{
		mediaDescriptions.CopyTo(array, arrayIndex);
	}

	public bool Remove(NavMediaDescription item)
	{
		return mediaDescriptions.Remove(item);
	}

	public int IndexOf(NavMediaDescription item)
	{
		return mediaDescriptions.IndexOf(item);
	}

	public void Insert(int index, NavMediaDescription item)
	{
		mediaDescriptions.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		mediaDescriptions.RemoveAt(index);
	}
}
