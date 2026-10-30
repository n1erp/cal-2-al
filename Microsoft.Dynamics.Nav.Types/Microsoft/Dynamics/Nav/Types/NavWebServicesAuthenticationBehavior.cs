using System;
using System.Net;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

public class NavWebServicesAuthenticationBehavior
{
	private readonly NavClientCredentialType credentialType;

	private readonly bool useNtlm;

	private AuthenticationSchemes authenticationScheme;

	private HttpClientCredentialType httpClientCredentialType;

	public AuthenticationSchemes AuthenticationScheme => authenticationScheme;

	public HttpClientCredentialType HttpClientCredentialType => httpClientCredentialType;

	public NavClientCredentialType CredentialType => credentialType;

	public NavWebServicesAuthenticationBehavior(NavClientCredentialType clientCredentialType, bool useNtlm)
	{
		credentialType = clientCredentialType;
		this.useNtlm = useNtlm;
		Initialize();
	}

	public NavWebServicesAuthenticationBehavior(NavClientCredentialType clientCredentialType)
		: this(clientCredentialType, useNtlm: false)
	{
	}

	private void Initialize()
	{
		switch (credentialType)
		{
		case NavClientCredentialType.Windows:
			authenticationScheme = (useNtlm ? AuthenticationSchemes.Ntlm : AuthenticationSchemes.Negotiate);
			httpClientCredentialType = (useNtlm ? HttpClientCredentialType.Ntlm : HttpClientCredentialType.Windows);
			break;
		case NavClientCredentialType.UserName:
			authenticationScheme = AuthenticationSchemes.Digest;
			httpClientCredentialType = HttpClientCredentialType.Digest;
			break;
		case NavClientCredentialType.NavUserPassword:
		case NavClientCredentialType.AccessControlService:
			authenticationScheme = AuthenticationSchemes.Anonymous;
			httpClientCredentialType = HttpClientCredentialType.None;
			break;
		default:
			throw new InvalidOperationException();
		}
	}
}
