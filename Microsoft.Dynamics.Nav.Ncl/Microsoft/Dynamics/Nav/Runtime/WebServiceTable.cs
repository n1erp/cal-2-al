using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class WebServiceTable : IDisposable
{
	private int tableId;

	private int objectTypeField;

	private int objectIdField;

	private int serviceNameField;

	private int publishedField;

	protected NavSession Session { get; private set; }

	protected NavRecord Subject { get; private set; }

	protected WebServiceTable(NavSession session, int tableId, int objectTypeField, int objectIdField, int serviceNameField, int publishedField)
	{
		Session = session;
		Subject = new NavRecord(session, tableId, SecurityFiltering.Ignored);
		this.tableId = tableId;
		this.objectTypeField = objectTypeField;
		this.objectIdField = objectIdField;
		this.serviceNameField = serviceNameField;
		this.publishedField = publishedField;
	}

	public virtual IEnumerable<IWebServiceRecord> Get()
	{
		using (new MaximizedPermissionScope(Session, new int[1] { tableId }))
		{
			Subject.Clear();
			if (Subject.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					yield return CreateRecord();
				}
				while (Subject.NextRecord());
			}
		}
	}

	public virtual IEnumerable<IWebServiceRecord> Get(NavObjectType objectType, int objectId)
	{
		using (new MaximizedPermissionScope(Session, new int[1] { tableId }))
		{
			Subject.Clear();
			Subject.ALSetFilter(objectTypeField, "%1", NavInteger.Create((int)objectType));
			Subject.ALSetFilter(objectIdField, "%1", NavInteger.Create(objectId));
			if (Subject.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					yield return CreateRecord();
				}
				while (Subject.NextRecord());
			}
		}
	}

	public virtual IWebServiceRecord Get(NavObjectType objectType, string serviceName)
	{
		IWebServiceRecord result = null;
		using (new MaximizedPermissionScope(Session, new int[1] { tableId }))
		{
			Subject.Clear();
			Subject.SetFieldValue(objectTypeField, NavInteger.Create((int)objectType));
			Subject.SetFieldValue(serviceNameField, NavText.Create(serviceName));
			if (Subject.Get(DataError.TrapError, NavRecordId.CreateFromNavRecord(Subject)))
			{
				result = CreateRecord();
			}
		}
		return result;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual IWebServiceRecord CreateRecord()
	{
		return new WebServiceRecord(Subject.GetFieldValue(objectIdField).ToInt32(), (NavObjectType)Subject.GetFieldValue(objectTypeField).ToInt32(), Subject.GetFieldValue(serviceNameField).ToString(), Subject.GetFieldValue(publishedField).ToBoolean());
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && Subject != null)
		{
			Subject.Dispose();
			Subject = null;
		}
	}
}
