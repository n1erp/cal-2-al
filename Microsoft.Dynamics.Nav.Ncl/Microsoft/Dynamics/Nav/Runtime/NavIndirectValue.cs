using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavIndirectValue : NavComplexValue
{
	public abstract override bool IsInitialized { get; }

	public abstract override NavValue InnerValue { get; }

	public override bool IsZeroOrEmpty
	{
		get
		{
			if (!IsInitialized)
			{
				return true;
			}
			return InnerValue.IsZeroOrEmpty;
		}
	}

	public override bool IsWrapper => true;

	public override object ValueAsObject
	{
		get
		{
			if (!IsInitialized)
			{
				return null;
			}
			return InnerValue.ValueAsObject;
		}
	}

	public override object ClientObject
	{
		get
		{
			if (!IsInitialized)
			{
				return null;
			}
			return InnerValue.ClientObject;
		}
	}

	protected NavIndirectValue(ITreeObject parent)
		: base(parent)
	{
	}

	public virtual void CheckInitialized()
	{
		if (!IsInitialized)
		{
			throw new NavNCLNotInitializedException(GetType());
		}
	}

	internal bool IsOfNavType(NavType navType)
	{
		if (IsInitialized)
		{
			return navType == InnerValue.NavType;
		}
		return false;
	}

	public override byte[] GetBytes()
	{
		CheckInitialized();
		return InnerValue.GetBytes();
	}

	public override int GetHashCode()
	{
		if (IsInitialized)
		{
			return InnerValue.GetHashCode();
		}
		return -1;
	}

	public override byte ToByte()
	{
		CheckInitialized();
		return InnerValue.ToByte();
	}

	public override int ToInt32()
	{
		CheckInitialized();
		return InnerValue.ToInt32();
	}

	public override long ToInt64()
	{
		CheckInitialized();
		return InnerValue.ToInt64();
	}

	public override DateTime ToDateTime()
	{
		CheckInitialized();
		return InnerValue.ToDateTime();
	}

	public override Decimal18 ToDecimal()
	{
		CheckInitialized();
		return InnerValue.ToDecimal();
	}

	public override string ToString()
	{
		CheckInitialized();
		return InnerValue.ToString();
	}
}
