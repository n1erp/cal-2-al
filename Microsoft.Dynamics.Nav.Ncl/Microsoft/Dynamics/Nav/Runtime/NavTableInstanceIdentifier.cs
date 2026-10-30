using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTableInstanceIdentifier : IEquatable<NavTableInstanceIdentifier>
{
	private readonly int tableId;

	private readonly bool isTemporary;

	internal int TableId => tableId;

	internal bool IsTemporary => isTemporary;

	internal NavTableInstanceIdentifier(int tableId, bool isTemporary)
	{
		this.tableId = tableId;
		this.isTemporary = isTemporary;
	}

	public virtual bool Equals(NavTableInstanceIdentifier other)
	{
		if (other != null && TableId == other.TableId)
		{
			return IsTemporary == other.IsTemporary;
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (other is NavTableInstanceIdentifier other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (IsTemporary ? 1 : 0) | (tableId << 1);
	}
}
