using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class WebServiceSystemTenantView : IEnumerable<IWebServiceRecord>, IEnumerable
{
	private class WebServiceRecordCollection : KeyedCollection<IWebServiceRecordKey, IWebServiceRecord>
	{
		protected override IWebServiceRecordKey GetKeyForItem(IWebServiceRecord item)
		{
			return item;
		}
	}

	private WebServiceRecordCollection records = new WebServiceRecordCollection();

	public int Count => records.Count;

	private WebServiceSystemTenantView()
	{
	}

	public static WebServiceSystemTenantView Create()
	{
		WebServiceSystemTenantView webServiceSystemTenantView = new WebServiceSystemTenantView();
		NavSession session = NavCurrentThread.Session;
		using WebServiceSystemTable webServiceSystemTable = WebServiceSystemTable.Create(session);
		using WebServiceTenantTable webServiceTenantTable = WebServiceTenantTable.Create(session);
		WebServiceSystemTable.WebServiceIsDisallowed isServiceDisallowed = webServiceSystemTable.GetCachedIsDisallowed();
		webServiceSystemTenantView.records = ProcessRecords(from item in webServiceSystemTable.Get()
			where !isServiceDisallowed(item.ObjectType, item.ObjectId)
			select item, from item in webServiceTenantTable.Get()
			where !isServiceDisallowed(item.ObjectType, item.ObjectId)
			select item);
		return webServiceSystemTenantView;
	}

	public static WebServiceSystemTenantView Create(NavObjectType objectType, int objectId)
	{
		WebServiceSystemTenantView webServiceSystemTenantView = new WebServiceSystemTenantView();
		NavSession session = NavCurrentThread.Session;
		using (WebServiceSystemTable webServiceSystemTable = WebServiceSystemTable.Create(session))
		{
			if (!webServiceSystemTable.IsDisallowed(objectType, objectId))
			{
				using WebServiceTenantTable webServiceTenantTable = WebServiceTenantTable.Create(session);
				webServiceSystemTenantView.records = ProcessRecords(webServiceSystemTable.Get(objectType, objectId), webServiceTenantTable.Get(objectType, objectId));
			}
		}
		return webServiceSystemTenantView;
	}

	public static WebServiceSystemTenantView Create(NavObjectType objectType, string serviceName)
	{
		return Create(objectType, serviceName, includeDisallowedObjects: false);
	}

	public static WebServiceSystemTenantView Create(NavObjectType objectType, string serviceName, bool includeDisallowedObjects)
	{
		WebServiceSystemTenantView webServiceSystemTenantView = new WebServiceSystemTenantView();
		NavSession session = NavCurrentThread.Session;
		using WebServiceSystemTable webServiceSystemTable = WebServiceSystemTable.Create(session);
		using WebServiceTenantTable webServiceTenantTable = WebServiceTenantTable.Create(session);
		Collection<IWebServiceRecord> collection = new Collection<IWebServiceRecord>();
		Collection<IWebServiceRecord> collection2 = new Collection<IWebServiceRecord>();
		IWebServiceRecord webServiceRecord = webServiceSystemTable.Get(objectType, serviceName);
		if (webServiceRecord != null && (includeDisallowedObjects || (!includeDisallowedObjects && !webServiceSystemTable.IsDisallowed(webServiceRecord.ObjectType, webServiceRecord.ObjectId))))
		{
			collection.Add(webServiceRecord);
		}
		IWebServiceRecord webServiceRecord2 = webServiceTenantTable.Get(objectType, serviceName);
		if (webServiceRecord2 != null && (includeDisallowedObjects || (!includeDisallowedObjects && !webServiceSystemTable.IsDisallowed(webServiceRecord2.ObjectType, webServiceRecord2.ObjectId))))
		{
			collection2.Add(webServiceRecord2);
		}
		webServiceSystemTenantView.records = ProcessRecords(collection, collection2);
		return webServiceSystemTenantView;
	}

	public IEnumerator<IWebServiceRecord> GetEnumerator()
	{
		return records.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return records.GetEnumerator();
	}

	private static WebServiceRecordCollection ProcessRecords(IEnumerable<IWebServiceRecord> systemRecords, IEnumerable<IWebServiceRecord> tenantRecords)
	{
		WebServiceRecordCollection webServiceRecordCollection = new WebServiceRecordCollection();
		foreach (IWebServiceRecord systemRecord in systemRecords)
		{
			webServiceRecordCollection.Add(systemRecord);
		}
		foreach (IWebServiceRecord tenantRecord in tenantRecords)
		{
			if (!webServiceRecordCollection.Contains(tenantRecord))
			{
				webServiceRecordCollection.Add(tenantRecord);
			}
		}
		return webServiceRecordCollection;
	}
}
