namespace Microsoft.Dynamics.Nav.Runtime;

public class NavValueOptionMetadata : NavValueMetadata
{
	private NCLOptionMetadata optionMetadata;

	private static NavValueOptionMetadata defaultValue = new NavValueOptionMetadata(NCLOptionMetadata.Default);

	public static NavValueOptionMetadata Default => defaultValue;

	public override NCLOptionMetadata NavOptionMetadata => optionMetadata;

	public NavValueOptionMetadata(NCLOptionMetadata optionMetadata)
		: base(NavNclType.NavOption)
	{
		this.optionMetadata = optionMetadata;
	}
}
