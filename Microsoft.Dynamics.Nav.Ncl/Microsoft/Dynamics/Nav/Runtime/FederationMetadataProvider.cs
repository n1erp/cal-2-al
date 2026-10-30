using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class FederationMetadataProvider
{
	private Uri federationMetadataUri;

	private string cachedIssuerName;

	private IList<X509Certificate2> cachedSigningCertificates;

	private DateTime lastRetrievalTime = DateTime.MinValue;

	private TimeSpan retrievalInterval;

	private static readonly TimeSpan DefaultRetrievalInterval = TimeSpan.FromHours(2.0);

	private NavDiagnostics diagnostic;

	private object metadataFetchLock = new object();

	private static NavMultidimensionMetric getValidSigningCertificateDurationMetric = new NavMultidimensionMetric("GetValidSigningCertificateWithThumbprintDurationMetric", (string[])null);

	private bool HasRefreshIntervalPassed => DateTime.UtcNow - lastRetrievalTime >= retrievalInterval;

	public virtual string IssuerName
	{
		get
		{
			if (HasRefreshIntervalPassed || cachedIssuerName == null)
			{
				LoadFederationMetadata();
			}
			return cachedIssuerName;
		}
		private set
		{
			cachedIssuerName = value;
		}
	}

	public virtual IList<X509Certificate2> SigningCertificates
	{
		get
		{
			if (HasRefreshIntervalPassed || cachedSigningCertificates == null)
			{
				LoadFederationMetadata();
			}
			return cachedSigningCertificates;
		}
		private set
		{
			cachedSigningCertificates = value;
		}
	}

	public FederationMetadataProvider(string federationMetadataLocation, NavDiagnostics diagnostic)
		: this(new Uri(federationMetadataLocation), diagnostic)
	{
	}

	public FederationMetadataProvider(Uri federationMetadataLocation, NavDiagnostics diagnostic)
		: this(federationMetadataLocation, DefaultRetrievalInterval, diagnostic)
	{
	}

	internal FederationMetadataProvider(Uri federationMetadataLocation, TimeSpan federationMetadataRetrievalInterval, NavDiagnostics diagnostic)
	{
		if (federationMetadataLocation == null)
		{
			throw new ArgumentNullException("federationMetadataLocation");
		}
		federationMetadataUri = federationMetadataLocation;
		retrievalInterval = federationMetadataRetrievalInterval;
		this.diagnostic = diagnostic;
	}

	private void ForceRefreshCachedSigningCertificates()
	{
		LoadFederationMetadata(forceRefresh: true);
	}

	public X509Certificate2 GetValidSigningCertificateWithThumbprint(string tokenCertificateThumbprint, bool retry = true)
	{
		X509Certificate2 x509Certificate = null;
		using (new IfxTimingScope((NavMetric)getValidSigningCertificateDurationMetric, (string[])null))
		{
			x509Certificate = SigningCertificates.FirstOrDefault((X509Certificate2 signingCertificate) => signingCertificate.Thumbprint == tokenCertificateThumbprint);
			if ((x509Certificate == null) & retry)
			{
				ForceRefreshCachedSigningCertificates();
				x509Certificate = SigningCertificates.FirstOrDefault((X509Certificate2 signingCertificate) => signingCertificate.Thumbprint == tokenCertificateThumbprint);
			}
		}
		diagnostic?.SendTraceTag("00006G5", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Validating Signing Certificate Thumbprint {0} using Federation Metadata location {1}, Success = {2}, Retry = {3}", tokenCertificateThumbprint, federationMetadataUri, x509Certificate != null, retry));
		return x509Certificate;
	}

	private void LoadFederationMetadata(bool forceRefresh = false)
	{
		if (string.IsNullOrWhiteSpace(federationMetadataUri.ToString()))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationInvalidValue, "ClientServicesFederationMetadataLocation"));
		}
		lock (metadataFetchLock)
		{
			if (forceRefresh || HasRefreshIntervalPassed)
			{
				try
				{
					string federationMetadata = new WebClient().DownloadString(federationMetadataUri);
					FederationMetadataParser federationMetadataParser = new FederationMetadataParser(federationMetadata);
					IssuerName = federationMetadataParser.GetIssuerName();
					SigningCertificates = federationMetadataParser.GetIssuerSigningCertificates();
					lastRetrievalTime = DateTime.UtcNow;
					diagnostic?.SendTraceTag("00006G6", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "The Federation Metadata document is successfully retrieved from the specified location: {0}", federationMetadataUri));
					return;
				}
				catch (Exception ex)
				{
					string text = string.Format(Thread.CurrentThread.CurrentCulture, Lang.FederationMetadataRetrievalFailure, federationMetadataUri, ex.Message);
					InvalidOperationException ex2 = new InvalidOperationException(text, ex);
					diagnostic?.SendExceptionTag("00006G7", Category.Security, ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata(text), null, forceLog: true);
					throw ex2;
				}
			}
		}
	}
}
