using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class ExpiringValue<T> : IExpiringValue
{
	public T Value { get; private set; }

	public DateTime ExpirationTime { get; private set; }

	public bool Expired => ExpirationTime < DateTime.UtcNow;

	protected ExpiringValue(T value, DateTime expirationTime)
	{
		Value = value;
		ExpirationTime = expirationTime;
	}

	public void ExpireItem()
	{
		ExpirationTime = DateTime.UtcNow;
	}
}
