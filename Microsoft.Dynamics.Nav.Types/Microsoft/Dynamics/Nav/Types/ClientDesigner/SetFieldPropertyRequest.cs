using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[KnownType(typeof(Importance))]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetFieldPropertyRequest : SetControlPropertyRequestBase
{
	public SetFieldPropertyRequest(int pageId, string controlName, string name, object value)
		: base(pageId, controlName, name, value)
	{
	}
}
