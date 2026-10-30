namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavCodeBase : NavStringValue
{
	internal const byte AlphaIndicator = byte.MaxValue;

	public abstract byte AlphaIndex { get; }
}
