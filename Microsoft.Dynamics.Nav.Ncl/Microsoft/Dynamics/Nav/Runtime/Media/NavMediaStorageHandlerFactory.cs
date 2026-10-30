using System;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Encryption;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

public sealed class NavMediaStorageHandlerFactory
{
	internal const string StorageAccountNameSecret = "TEMPORARYDOCUMENTSTORAGEACCOUNT";

	internal const string StorageAccountKeySecret = "TEMPORARYDOCUMENTSTORAGEKEY";

	private const string DefaultContainerName = "financialsdocuments";

	private NavMediaStorageHandlerFactory()
	{
	}

	public static INavMediaStorageHandler CreateStorageHandler()
	{
		Func<ISystemEncryptionProvider> getEncryptionProvider = () => TenantEncryptionProviderFactory.GetTenantEncryptionProvider(() => NavCurrentThread.Session);
		return CreateStorageHandler(getEncryptionProvider);
	}

	internal static INavMediaStorageHandler CreateStorageHandler(Func<ISystemEncryptionProvider> getEncryptionProvider)
	{
		if (NavEnvironment.Topology.IsMediaStoredInAzure)
		{
			return CreateAzureStorageHandler(getEncryptionProvider());
		}
		return new NavMediaStorageHandler();
	}

	private static INavMediaStorageHandler CreateAzureStorageHandler(ISystemEncryptionProvider encryptionProvider)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		if (!(encryptionProvider is IAzureKeyVaultSecretProvider azureKeyVaultSecretProvider))
		{
			mostSpecificInstance.SendTraceTag("00001JG", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, "Could not access the Azure Key Vault encryption provider.");
			throw new NavNCLDocumentServiceException(Lang.AzureBlobStorageNotConfigured);
		}
		string secret;
		string secret2;
		try
		{
			secret = azureKeyVaultSecretProvider.GetSecret("TEMPORARYDOCUMENTSTORAGEACCOUNT");
			secret2 = azureKeyVaultSecretProvider.GetSecret("TEMPORARYDOCUMENTSTORAGEKEY");
		}
		catch (Exception innerException)
		{
			mostSpecificInstance.SendTraceTag("00001JH", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, "Failed to find storage account details in Azure Key Vault.");
			throw new NavNCLDocumentServiceException(Lang.AzureBlobStorageNotConfigured, innerException);
		}
		if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(secret2))
		{
			mostSpecificInstance.SendTraceTag("00001JI", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, "Azure blob storage is not configured in Azure Key Vault.");
			throw new NavNCLDocumentServiceException(Lang.AzureBlobStorageNotConfigured);
		}
		return new AzureStorageHandler(secret, secret2, "financialsdocuments");
	}
}
