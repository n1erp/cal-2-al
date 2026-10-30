namespace Microsoft.Dynamics.Nav.Runtime;

public class NavAppObjectExtensionEntry
{
	public int RuntimeAppId { get; private set; }

	public NCLApplicationObjectExtension ExtensionObject { get; private set; }

	public NavAppObjectExtensionEntry(int runtimeAppId, NCLApplicationObjectExtension groupObject)
	{
		RuntimeAppId = runtimeAppId;
		ExtensionObject = groupObject;
	}
}
