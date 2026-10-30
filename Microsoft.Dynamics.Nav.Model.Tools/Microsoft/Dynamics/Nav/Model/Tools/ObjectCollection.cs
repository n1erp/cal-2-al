using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public sealed class ObjectCollection : IEnumerable<ApplicationObject>, IEnumerable
{
	private readonly ConcurrentBag<ApplicationObject>[] caches;

	public ObjectCollection()
	{
		int num = (from e in ElementTypeInfo.Lookup.Values
			where e.Category == CategoryType.Object
			select e.CategoryIndex).Max();
		caches = new ConcurrentBag<ApplicationObject>[num + 1];
		for (int num2 = 0; num2 < caches.Length; num2++)
		{
			caches[num2] = new ConcurrentBag<ApplicationObject>();
		}
	}

	public ObjectCollection(IEnumerable<ApplicationObject> applicationObjects)
		: this()
	{
		applicationObjects.ForEach((ApplicationObject ao) =>
		{
			caches[ao.ElementTypeInfo.CategoryIndex].Add(ao);
		});
	}

	public void Add(ApplicationObject applicationObject)
	{
		caches[applicationObject.ElementTypeInfo.CategoryIndex].Add(applicationObject);
	}

	public IEnumerable<ApplicationObject> GetApplicationObjects()
	{
		return caches.SelectMany((ConcurrentBag<ApplicationObject> cd) => cd);
	}

	public IEnumerable<ApplicationObject> GetApplicationObjects(ElementType elementType)
	{
		return GetApplicationObjects(ElementTypeInfo.Lookup.GetValue(elementType));
	}

	public IEnumerable<ApplicationObject> GetApplicationObjects(ElementTypeInfo elementType)
	{
		return caches[CacheIndex(elementType)];
	}

	public IEnumerable<InvalidApplicationObject> GetInvalidApplicationObjects()
	{
		return from ao in GetApplicationObjects()
			where !ao.IsValid
			select (InvalidApplicationObject)ao;
	}

	public bool Modifies(ApplicationObject target)
	{
		return GetApplicationObjects(ElementType.Modification).Any((ApplicationObject m) => m.Id == target.Id);
	}

	public IEnumerable<IElement> GetElements()
	{
		return GetApplicationObjects().SelectMany((ApplicationObject ao) => ao.GetElements());
	}

	private static int CacheIndex(ElementTypeInfo elementType)
	{
		if (elementType.Category != CategoryType.Object)
		{
			throw new InvalidOperationException();
		}
		return elementType.CategoryIndex;
	}

	IEnumerator<ApplicationObject> IEnumerable<ApplicationObject>.GetEnumerator()
	{
		return GetApplicationObjects().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetApplicationObjects().GetEnumerator();
	}
}
