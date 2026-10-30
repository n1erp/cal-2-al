using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class NavObjectIdAttribute : Attribute
{
	private int objectId;

	public int ObjectId
	{
		get
		{
			return objectId;
		}
		set
		{
			objectId = value;
		}
	}
}
