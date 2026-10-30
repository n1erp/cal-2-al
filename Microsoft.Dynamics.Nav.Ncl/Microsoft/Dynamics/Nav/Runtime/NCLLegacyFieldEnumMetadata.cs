namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLLegacyFieldEnumMetadata : NCLFieldEnumBaseMetadata
{
	private readonly int enumId;

	private NCLLegacyFieldEnumMetadata(int enumId, string enumName, string optionString, string optionCaptions)
		: base(optionString)
	{
		this.enumId = enumId;
		if (NavCurrentThread.ResolveAppGroup().GroupId == 0)
		{
			NavGlobal.MetadataProvider.AddLegacyEnumMetadata(enumId, enumName, optionString, optionCaptions);
		}
	}

	public static NCLOptionMetadata Create(int enumId, string enumName, string optionString, string optionCaptions)
	{
		return new NCLLegacyFieldEnumMetadata(enumId, enumName, optionString, optionCaptions);
	}

	protected internal override NCLOptionMetadata GetEnumMetadataFromMetadataProvider()
	{
		return NavGlobal.MetadataProvider.GetEnumMetadata(enumId) ?? NCLOptionMetadata.Default;
	}
}
