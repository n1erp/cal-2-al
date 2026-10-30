using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetControlPropertyRequestBase : NavPageDesignerRequest
{
	[DataMember]
	public string ControlName { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public object Value { get; set; }

	public SetControlPropertyRequestBase(int pageId, string controlName, string name, object value)
		: base(pageId)
	{
		ControlName = controlName;
		Name = name;
		Value = value;
	}
}
