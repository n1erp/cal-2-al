using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class WebServiceRecordKey : IWebServiceRecordKey
{
	public NavObjectType ObjectType { get; private set; }

	public string ServiceName { get; private set; }

	protected WebServiceRecordKey(NavObjectType type, string serviceName)
	{
		if (serviceName == null)
		{
			throw new ArgumentNullException("serviceName");
		}
		ObjectType = type;
		ServiceName = serviceName;
	}

	public static bool operator ==(WebServiceRecordKey left, WebServiceRecordKey right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		if (left.ObjectType == right.ObjectType)
		{
			return left.ServiceName == right.ServiceName;
		}
		return false;
	}

	public static bool operator !=(WebServiceRecordKey left, WebServiceRecordKey right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is WebServiceRecordKey webServiceRecordKey))
		{
			return false;
		}
		return this == webServiceRecordKey;
	}

	public override int GetHashCode()
	{
		if (ServiceName != null)
		{
			return ObjectType.GetHashCode() ^ ServiceName.GetHashCode();
		}
		return ObjectType.GetHashCode();
	}
}
