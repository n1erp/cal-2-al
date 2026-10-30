using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
public class UserNamePasswordCredentials : ICredentials
{
	[DataMember]
	public string UserName { get; set; }

	[DataMember]
	public string Password
	{
		get
		{
			return SecurePassword.ConvertToUnsecureString();
		}
		set
		{
			SecurePassword = SecureStringExtension.CreateFromUnsecureString(value);
		}
	}

	public SecureString SecurePassword { get; private set; }

	public UserNamePasswordCredentials()
	{
		SecurePassword = new SecureString();
	}

	public NetworkCredential GetCredential(Uri uri, string authType)
	{
		return GetCredentialBase(uri, authType);
	}

	protected virtual NetworkCredential GetCredentialBase(Uri uri, string authenticationType)
	{
		return null;
	}
}
