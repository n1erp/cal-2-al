using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public static class CertificateHelper
{
	public static bool TryFindCertificateFromThumbprint(string certificateThumbprint, out X509Certificate2 certificate)
	{
		try
		{
			certificate = FindCertificateFromThumbprint(certificateThumbprint);
			return true;
		}
		catch (Exception)
		{
			certificate = null;
			return false;
		}
	}

	public static X509Certificate2 FindCertificateFromThumbprint(string certificateThumbprint)
	{
		X509Certificate2 certificate = null;
		if (!TryGetCertificateFromThumbprint(certificateThumbprint, out certificate, StoreName.My, StoreLocation.CurrentUser) && !TryGetCertificateFromThumbprint(certificateThumbprint, out certificate))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.SystemEncryptionClientCertificateNotFoundInEeitherOfStoreLocations, certificateThumbprint));
		}
		return certificate;
	}

	public static X509Certificate2 GetCertificateFromBase64StringContent(string certContent, string certPassword, StoreLocation storeLocation)
	{
		return new X509Certificate2(Convert.FromBase64String(certContent), certPassword, (X509KeyStorageFlags)(((storeLocation == StoreLocation.CurrentUser) ? 1 : 2) | 0x10));
	}

	public static void InstallCertificate(X509Certificate2 certificate, StoreLocation storeLocation = StoreLocation.CurrentUser, StoreName storeName = StoreName.My)
	{
		X509Store x509Store = new X509Store(storeName, storeLocation);
		try
		{
			x509Store.Open(OpenFlags.ReadWrite);
			if (!x509Store.Certificates.Contains(certificate))
			{
				x509Store.Add(certificate);
			}
		}
		finally
		{
			x509Store.Close();
		}
	}

	public static void InstallCertificateFromBase64StringContent(string certContent, string certPassword, StoreLocation storeLocation = StoreLocation.CurrentUser, StoreName storeName = StoreName.My)
	{
		InstallCertificate(GetCertificateFromBase64StringContent(certContent, certPassword, storeLocation), storeLocation, storeName);
	}

	public static void AddServiceCredentialCertificate(ServiceHostBase serviceHost, X509Certificate2 certificate, bool certificateValidationEnabled, UserNamePasswordValidationMode mode)
	{
		ServiceCredentials serviceCredentials = serviceHost.Description.Behaviors.Find<ServiceCredentials>();
		if (serviceCredentials == null)
		{
			serviceCredentials = new ServiceCredentials();
			serviceHost.Description.Behaviors.Add(serviceCredentials);
		}
		serviceCredentials.UserNameAuthentication.UserNamePasswordValidationMode = mode;
		serviceCredentials.ServiceCertificate.Certificate = certificate;
		if (!certificateValidationEnabled)
		{
			return;
		}
		if (!ServerUserSettings.Instance.DisableTokenSigningCertificateValidation.Value)
		{
			try
			{
				serviceCredentials.ServiceCertificate.Certificate.GetRSAPrivateKey();
			}
			catch (CryptographicException)
			{
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.CertificateHasNoValidPrivateKey, serviceCredentials.ServiceCertificate.Certificate.Subject));
			}
		}
		if (!serviceCredentials.ServiceCertificate.Certificate.Verify())
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ValidCertificateNotFound, serviceCredentials.ServiceCertificate.Certificate.ToString()));
		}
	}

	private static bool TryGetCertificateFromThumbprint(string certificateThumbprint, out X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
	{
		try
		{
			certificate = GetCertificateFromThumbprint(certificateThumbprint, storeName, storeLocation);
			return true;
		}
		catch (Exception)
		{
			certificate = null;
			return false;
		}
	}

	private static X509Certificate2 GetCertificateFromThumbprint(string certificateThumbprint, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
	{
		CertificateValidator.ValidateCertificateThumbprint(certificateThumbprint);
		X509Store x509Store = new X509Store(storeName, storeLocation);
		try
		{
			x509Store.Open(OpenFlags.ReadOnly);
			X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, validOnly: false);
			if (x509Certificate2Collection.Count == 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.SystemEncryptionClientCertificateNotFound, storeLocation, storeName, certificateThumbprint));
			}
			return x509Certificate2Collection[0];
		}
		catch (Exception exception)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.SystemEncryptionClientCertificateNotFound, storeLocation, storeName, certificateThumbprint), exception);
		}
		finally
		{
			x509Store.Close();
		}
	}
}
