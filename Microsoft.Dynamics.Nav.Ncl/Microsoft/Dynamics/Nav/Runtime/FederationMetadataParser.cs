using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

public class FederationMetadataParser
{
	private MetadataBase metadataBase;

	public FederationMetadataParser(string federationMetadata, bool requireSignature = true)
	{
		metadataBase = ParseFederationMetadata(federationMetadata);
		if (requireSignature)
		{
			ThrowIfNotSigned();
		}
	}

	public string GetIssuerName()
	{
		GetSecurityTokenServiceDescriptor(metadataBase, out var entityDescriptor);
		return entityDescriptor?.EntityId.Id;
	}

	public IList<X509Certificate2> GetIssuerSigningCertificates()
	{
		SecurityTokenServiceDescriptor securityTokenServiceDescriptor = GetSecurityTokenServiceDescriptor(metadataBase, out var _);
		if (securityTokenServiceDescriptor == null || securityTokenServiceDescriptor.Keys == null)
		{
			return new Collection<X509Certificate2>();
		}
		Collection<X509Certificate2> collection = new Collection<X509Certificate2>();
		foreach (KeyDescriptor key in securityTokenServiceDescriptor.Keys)
		{
			if (key.KeyInfo != null && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified) && key.KeyInfo.TryFind<X509RawDataKeyIdentifierClause>(out var clause))
			{
				collection.Add(new X509Certificate2(clause.GetX509RawData()));
			}
		}
		return collection.ToList();
	}

	private static MetadataBase ParseFederationMetadata(string federationMetadata)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(federationMetadata);
		using MemoryStream input = new MemoryStream(bytes);
		using XmlTextReader xmlTextReader = new XmlTextReader(input);
		xmlTextReader.DtdProcessing = DtdProcessing.Prohibit;
		XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlTextReader);
		MetadataSerializer metadataSerializer = new MetadataSerializer
		{
			CertificateValidationMode = X509CertificateValidationMode.None
		};
		return metadataSerializer.ReadMetadata(reader);
	}

	private void ThrowIfNotSigned()
	{
		if (metadataBase.SigningCredentials == null)
		{
			throw new FormatException("Federation metadata must be signed.");
		}
	}

	private static SecurityTokenServiceDescriptor GetSecurityTokenServiceDescriptor(MetadataBase metadataBase, out EntityDescriptor entityDescriptor)
	{
		entityDescriptor = null;
		if (metadataBase is EntitiesDescriptor entitiesDescriptor)
		{
			foreach (EntityDescriptor childEntity in entitiesDescriptor.ChildEntities)
			{
				SecurityTokenServiceDescriptor securityTokenServiceDescriptor = GetSecurityTokenServiceDescriptor(childEntity);
				if (securityTokenServiceDescriptor != null)
				{
					entityDescriptor = childEntity;
					return securityTokenServiceDescriptor;
				}
			}
		}
		if (metadataBase is EntityDescriptor entityDescriptor2)
		{
			SecurityTokenServiceDescriptor securityTokenServiceDescriptor2 = GetSecurityTokenServiceDescriptor(entityDescriptor2);
			if (securityTokenServiceDescriptor2 != null)
			{
				entityDescriptor = entityDescriptor2;
				return securityTokenServiceDescriptor2;
			}
		}
		return null;
	}

	private static SecurityTokenServiceDescriptor GetSecurityTokenServiceDescriptor(EntityDescriptor entityDescriptor)
	{
		foreach (RoleDescriptor roleDescriptor in entityDescriptor.RoleDescriptors)
		{
			if (roleDescriptor is SecurityTokenServiceDescriptor result)
			{
				return result;
			}
		}
		return null;
	}
}
