using System;

namespace Mono;

internal static class Disposable
{
	public static Disposable<T> Owned<T>(T value) where T : class, IDisposable
	{
		return new Disposable<T>(value, owned: true);
	}

	public static Disposable<T> NotOwned<T>(T value) where T : class, IDisposable
	{
		return new Disposable<T>(value, owned: false);
	}
}
internal struct Disposable<T>(T value, bool owned) : IDisposable where T : class, IDisposable
{
	internal readonly T value = value;

	private readonly bool owned = owned;

	public void Dispose()
	{
		if (value != null && owned)
		{
			value.Dispose();
		}
	}
}
