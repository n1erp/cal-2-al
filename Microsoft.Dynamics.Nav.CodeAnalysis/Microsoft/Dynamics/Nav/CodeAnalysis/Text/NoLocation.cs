namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class NoLocation : Location
{
	public static readonly Location Singleton = new NoLocation();

	public override LocationKind Kind => LocationKind.None;

	private NoLocation()
	{
	}

	public override bool Equals(object obj)
	{
		return this == obj;
	}

	public override int GetHashCode()
	{
		return 373847894;
	}
}
