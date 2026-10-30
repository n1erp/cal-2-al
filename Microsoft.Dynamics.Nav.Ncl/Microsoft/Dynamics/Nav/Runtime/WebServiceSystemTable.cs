using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class WebServiceSystemTable : WebServiceTable
{
	public delegate bool WebServiceIsDisallowed(NavObjectType objectType, int objectId);

	private sealed class DisallowedObjects
	{
		private Dictionary<int, IList<NavObjectType>> disallowedObjects = new Dictionary<int, IList<NavObjectType>>();

		public void Add(NavObjectType objectType, int objectId)
		{
			IList<NavObjectType> value = null;
			if (disallowedObjects.TryGetValue(objectId, out value))
			{
				if (!value.Contains(objectType))
				{
					value.Add(objectType);
				}
			}
			else
			{
				disallowedObjects.Add(objectId, new List<NavObjectType> { objectType });
			}
		}

		public bool Contains(NavObjectType objectType, int objectId)
		{
			bool result = false;
			IList<NavObjectType> value = null;
			if (disallowedObjects.TryGetValue(objectId, out value) && value != null)
			{
				result = value.Contains(objectType);
			}
			return result;
		}
	}

	private WebServiceSystemTable(NavSession session)
		: base(session, 2000000076, 3, 6, 9, 12)
	{
	}

	public static WebServiceSystemTable Create(NavSession session)
	{
		return new WebServiceSystemTable(session);
	}

	public WebServiceIsDisallowed GetCachedIsDisallowed()
	{
		DisallowedObjects disallowedObjects = new DisallowedObjects();
		using (new MaximizedPermissionScope(Session, new int[1] { 2000000076 }))
		{
			Subject.Clear();
			Subject.ALSetFilter(12, "%1", NavBoolean.Create(value: false));
			if (Subject.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					IWebServiceRecord webServiceRecord = CreateRecord();
					disallowedObjects.Add(webServiceRecord.ObjectType, webServiceRecord.ObjectId);
				}
				while (Subject.NextRecord());
			}
		}
		return disallowedObjects.Contains;
	}

	public bool IsDisallowed(NavObjectType objectType, int objectId)
	{
		using (new MaximizedPermissionScope(Session, new int[1] { 2000000076 }))
		{
			Subject.Clear();
			Subject.ALSetFilter(3, "%1", NavInteger.Create((int)objectType));
			Subject.ALSetFilter(6, "%1", NavInteger.Create(objectId));
			Subject.ALSetFilter(12, "%1", NavBoolean.Create(value: false));
			return Subject.ALFind(DataError.TrapError, "-");
		}
	}
}
