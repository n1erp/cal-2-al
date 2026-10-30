namespace Microsoft.Dynamics.Nav.Runtime.Encryption;

public interface IAzureKeyVaultSecretProvider
{
	string GetSecret(string secretName);
}
