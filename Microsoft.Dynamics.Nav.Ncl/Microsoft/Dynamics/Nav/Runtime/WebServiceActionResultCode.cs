namespace Microsoft.Dynamics.Nav.Runtime;

public enum WebServiceActionResultCode
{
	None = 0,
	Get = 200,
	Created = 201,
	Updated = Get,
	Deleted = 204
}
