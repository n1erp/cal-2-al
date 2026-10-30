using System;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract]
public class AzureKeyVaultSettings
{
	[DataMember]
	[TelemetrySystemMetadata]
	public Guid ClientId { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public StoreLocation CertificateStoreLocation { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public StoreName CertificateStoreName { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string CertificateThumbprint { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string KeyIdentifier { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string EncryptionAlgorithm { get; private set; }

	public AzureKeyVaultSettings(Guid clientId, StoreLocation certStoreLocation, StoreName certStoreName, string certThumbprint, Uri keyIdentifierUrl)
	{
		ClientId = clientId;
		CertificateStoreLocation = certStoreLocation;
		CertificateStoreName = certStoreName;
		CertificateThumbprint = certThumbprint;
		KeyIdentifier = keyIdentifierUrl.AbsoluteUri;
		EncryptionAlgorithm = "RSA1_5";
	}
}
