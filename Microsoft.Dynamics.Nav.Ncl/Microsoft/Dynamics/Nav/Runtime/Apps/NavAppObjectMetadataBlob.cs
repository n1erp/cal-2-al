namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppObjectMetadataBlob
{
	public string Content { get; }

	public string FilePath { get; }

	public long SqlTimestamp { get; }

	public NavAppObjectMetadataBlob(string content, string filePath, long sqlTimestamp)
	{
		Content = content;
		FilePath = filePath;
		SqlTimestamp = sqlTimestamp;
	}
}
