using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Microsoft.Dynamics.Nav.Types;

public class NavClientServicesAuthenticationBehavior
{
	private readonly NavClientCredentialType credentialType;

	private MessageCredentialType messageCredentialType;

	private TcpClientCredentialType tcpClientCredentialType;

	private SecurityMode tcpSecurityMode;

	private bool needsCredentialCertificate;

	private bool hasCustomValidator;

	private UserNamePasswordValidationMode userNamePasswordValidationMode;

	public MessageCredentialType MessageCredentialType => messageCredentialType;

	public TcpClientCredentialType TcpClientCredentialType => tcpClientCredentialType;

	public SecurityMode TcpSecurityMode => tcpSecurityMode;

	public bool HasCustomValidator => hasCustomValidator;

	public bool NeedsCredentialCertificate => needsCredentialCertificate;

	public UserNamePasswordValidationMode UserNamePasswordValidationMode => userNamePasswordValidationMode;

	public NavClientCredentialType CredentialType => credentialType;

	public NavClientServicesAuthenticationBehavior(NavClientCredentialType clientCredentialType)
	{
		credentialType = clientCredentialType;
		Initialize();
	}

	private void Initialize()
	{
		switch (credentialType)
		{
		case NavClientCredentialType.Windows:
			messageCredentialType = MessageCredentialType.Windows;
			tcpClientCredentialType = TcpClientCredentialType.Windows;
			tcpSecurityMode = SecurityMode.Transport;
			needsCredentialCertificate = false;
			hasCustomValidator = false;
			userNamePasswordValidationMode = UserNamePasswordValidationMode.Windows;
			break;
		case NavClientCredentialType.UserName:
			messageCredentialType = MessageCredentialType.UserName;
			tcpClientCredentialType = TcpClientCredentialType.Windows;
			tcpSecurityMode = SecurityMode.TransportWithMessageCredential;
			needsCredentialCertificate = true;
			hasCustomValidator = false;
			userNamePasswordValidationMode = UserNamePasswordValidationMode.Windows;
			break;
		case NavClientCredentialType.NavUserPassword:
		case NavClientCredentialType.AccessControlService:
			messageCredentialType = MessageCredentialType.UserName;
			tcpClientCredentialType = TcpClientCredentialType.Windows;
			tcpSecurityMode = SecurityMode.TransportWithMessageCredential;
			needsCredentialCertificate = true;
			hasCustomValidator = true;
			userNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
			break;
		case NavClientCredentialType.None:
			messageCredentialType = MessageCredentialType.None;
			tcpClientCredentialType = TcpClientCredentialType.None;
			tcpSecurityMode = SecurityMode.None;
			needsCredentialCertificate = false;
			hasCustomValidator = false;
			userNamePasswordValidationMode = UserNamePasswordValidationMode.Windows;
			break;
		default:
			throw new InvalidOperationException();
		}
	}
}
