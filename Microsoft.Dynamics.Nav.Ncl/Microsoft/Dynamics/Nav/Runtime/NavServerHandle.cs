using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavServerHandle : NavInteropHandleReferenceCount
{
	private Type objectType;

	private object instance;

	internal Type ObjectType => objectType;

	internal object Instance
	{
		get
		{
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	internal NavServerHandle(Type objectType, object instance, bool suppressDispose)
		: base(suppressDispose)
	{
		this.objectType = objectType;
		this.instance = instance;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (objectType != null && instance != null && !SuppressDispose)
			{
				NavAutomationHelper.InvokeDispose(instance);
			}
			instance = null;
			objectType = null;
		}
	}
}
