using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFieldEvent
{
	private string name = string.Empty;

	private int id;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public event EventHandler<EventArgs> Validate;

	public event EventHandler<EventArgs> Lookup;

	internal void CallValidateEventHandler()
	{
		if (Validate != null)
		{
			Validate(this, EventArgs.Empty);
		}
	}

	internal void CallLookupEventHandler()
	{
		if (Lookup != null)
		{
			Lookup(this, EventArgs.Empty);
		}
	}
}
