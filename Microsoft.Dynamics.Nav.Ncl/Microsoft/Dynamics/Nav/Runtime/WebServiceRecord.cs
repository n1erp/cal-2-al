using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class WebServiceRecord : WebServiceRecordKey, IWebServiceRecord, IWebServiceRecordKey
{
	public int ObjectId { get; private set; }

	public bool Published { get; private set; }

	public WebServiceRecord(int id, NavObjectType type, string serviceName, bool published)
		: base(type, serviceName)
	{
		ObjectId = id;
		Published = published;
	}
}
