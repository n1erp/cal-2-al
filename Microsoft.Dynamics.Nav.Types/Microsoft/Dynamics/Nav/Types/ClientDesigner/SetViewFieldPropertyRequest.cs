using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetViewFieldPropertyRequest : SetFieldPropertyRequest
{
	[DataMember]
	public string ViewName { get; set; }

	public SetViewFieldPropertyRequest(int pageId, string controlName, string name, object value, string viewName)
		: base(pageId, controlName, name, value)
	{
		ViewName = viewName;
	}
}
