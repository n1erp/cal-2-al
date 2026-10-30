using System;

namespace Microsoft.Dynamics.Nav.Types;

public interface ISystemEncryptionProvider : IDisposable
{
	bool IsKeyCreated { get; }

	bool IsKeyPresent { get; }

	string PublicKey { get; }

	void CreateKey();

	string Decrypt(string encryptedText);

	void DeleteKey();

	string Encrypt(string plaintext);

	void ExportKey(string fileName, string keyPassword);

	void ImportKey(string fileName, string keyPassword, bool overwrite = false);
}
