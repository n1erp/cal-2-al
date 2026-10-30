using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ManagedTemporaryNavTableInstanceIdentifier : NavTableInstanceIdentifier
{
	private readonly DataAccess dataAccess;

	internal ManagedTemporaryNavTableInstanceIdentifier(int tableId, DataAccess dataAccess)
		: base(tableId, isTemporary: true)
	{
		this.dataAccess = dataAccess;
	}

	public override bool Equals(NavTableInstanceIdentifier other)
	{
		if (other is ManagedTemporaryNavTableInstanceIdentifier managedTemporaryNavTableInstanceIdentifier && IsTemporary == other.IsTemporary && managedTemporaryNavTableInstanceIdentifier.dataAccess == dataAccess)
		{
			return base.Equals(other);
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (other is ManagedTemporaryNavTableInstanceIdentifier other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.CombineHashCodes(base.GetHashCode(), dataAccess.GetHashCode());
	}
}
