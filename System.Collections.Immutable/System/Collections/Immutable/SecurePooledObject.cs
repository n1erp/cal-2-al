using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

internal class SecurePooledObject<T>
{
	private readonly T _value;

	private int _owner;

	internal int Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

	internal SecurePooledObject(T newValue)
	{
		Requires.NotNullAllowStructs(newValue, "newValue");
		_value = newValue;
	}

	internal T Use<TCaller>(ref TCaller caller) where TCaller : struct, ISecurePooledObjectUser
	{
		if (!IsOwned(ref caller))
		{
			Requires.FailObjectDisposed(caller);
		}
		return _value;
	}

	internal bool TryUse<TCaller>(ref TCaller caller, out T value) where TCaller : struct, ISecurePooledObjectUser
	{
		if (IsOwned(ref caller))
		{
			value = _value;
			return true;
		}
		value = default;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsOwned<TCaller>(ref TCaller caller) where TCaller : struct, ISecurePooledObjectUser
	{
		return caller.PoolUserId == _owner;
	}
}
