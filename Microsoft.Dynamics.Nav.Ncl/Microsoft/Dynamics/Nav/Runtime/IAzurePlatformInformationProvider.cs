namespace Microsoft.Dynamics.Nav.Runtime;

public interface IAzurePlatformInformationProvider
{
	bool TryReadAzurePlatformInfo(out AzurePlatformInformation azurePlatformInformation);
}
