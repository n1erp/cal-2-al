namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IWebServiceRecord : IWebServiceRecordKey
{
	int ObjectId { get; }

	bool Published { get; }
}
