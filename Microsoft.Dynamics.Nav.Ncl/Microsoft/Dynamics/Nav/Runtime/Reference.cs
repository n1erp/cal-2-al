using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

public class Reference<T> : IReference<T> where T : class
{
	private T target;

	public T Target => target;

	public T TargetSafe
	{
		get
		{
			T val = target;
			if (val == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Reference to object of type {0} is null", typeof(T).Name));
			}
			return val;
		}
	}

	public Reference()
	{
	}

	public Reference(T target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		this.target = target;
	}

	public void ClearTarget()
	{
		target = null;
	}
}
