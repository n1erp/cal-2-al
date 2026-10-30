namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLFieldEnumMetadata : NCLFieldEnumBaseMetadata
{
	private readonly int enumId;

	public NCLFieldEnumMetadata(int enumId)
		: base(string.Empty)
	{
		this.enumId = enumId;
	}

	public static NCLOptionMetadata Create(int enumId)
	{
		return new NCLFieldEnumMetadata(enumId);
	}

	protected internal override NCLOptionMetadata GetEnumMetadataFromMetadataProvider()
	{
		return NavGlobal.MetadataProvider.GetEnumMetadata(enumId) ?? NCLOptionMetadata.Default;
	}
}
