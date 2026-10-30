namespace Microsoft.Dynamics.Nav.Runtime;

public enum UserActionRequired
{
	None,
	UpdateNameIdentifier,
	EnterUserNameAndAuthenticationKey,
	UpdateAuthenticationObjectId,
	UpdateAuthenticationEmail,
	InitializeAsNewUser,
	UpdateDirectoryRoleId
}
