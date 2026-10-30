using System;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SessionEventClientTypeAttribute : Attribute
{
	public ClientType ClientType { get; private set; }

	public SessionEventClientTypeAttribute(ClientType clientType)
	{
		ClientType = clientType;
	}
}
