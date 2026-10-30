using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IWebServiceRecordKey
{
	NavObjectType ObjectType { get; }

	string ServiceName { get; }
}
