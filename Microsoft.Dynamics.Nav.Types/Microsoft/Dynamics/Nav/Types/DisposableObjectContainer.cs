using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class DisposableObjectContainer<TDisposable> : IDisposable where TDisposable : IDisposable
{
	private readonly Dictionary<Type, TDisposable> managers;

	[DebuggerStepThrough]
	public DisposableObjectContainer()
	{
		managers = new Dictionary<Type, TDisposable>(1);
	}

	public bool Add(Type type, TDisposable instanceToAdd)
	{
		bool result = false;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (object.Equals(instanceToAdd, default(TDisposable)))
		{
			throw new ArgumentNullException("instanceToAdd");
		}
		if (!managers.ContainsKey(type))
		{
			managers.Add(type, instanceToAdd);
			result = true;
		}
		return result;
	}

	public TDisposable Get(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		TDisposable result = default;
		if (managers.ContainsKey(type))
		{
			return managers[type];
		}
		return result;
	}

	public void Dispose()
	{
		foreach (Type key in managers.Keys)
		{
			managers[key].Dispose();
		}
		GC.SuppressFinalize(this);
	}
}
