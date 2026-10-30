namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppObjectMetadataTimestampRecord<T>
{
	public T MetadataObject { get; set; }

	public long Timestamp { get; set; }

	public NavAppObjectMetadataTimestampRecord(T metadataObject, long timestamp)
	{
		MetadataObject = metadataObject;
		Timestamp = timestamp;
	}
}
