namespace Microsoft.Dynamics.Nav.Runtime;

internal class TimestampedObject<T>
{
	public long Timestamp { get; set; }

	public T Value { get; set; }
}
