namespace Microsoft.Dynamics.Nav.Runtime;

public interface IDatabasePasswordPublicEncryptionKeyStorage
{
	string GetPublicKey(NavDatabase database);

	void PutPublicKey(NavDatabase database, string key, bool overwrite);
}
