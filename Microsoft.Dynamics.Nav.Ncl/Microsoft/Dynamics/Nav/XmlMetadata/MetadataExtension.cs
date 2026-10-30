namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class MetadataExtension<TItem>
{
	private readonly TItem item;

	private object webExtensions;

	public object WebExtensions
	{
		get
		{
			return webExtensions;
		}
		set
		{
			webExtensions = value;
		}
	}

	public TItem Item => item;

	public MetadataExtension(TItem itemToExtend)
	{
		item = itemToExtend;
	}
}
