using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CreatePageControlRequestBase : AddPageControlRequestBase
{
	[DataMember]
	public string Name { get; set; }

	public CreatePageControlRequestBase(int pageId, string parentControlName, string relativeControlName, string name)
		: base(pageId, parentControlName, relativeControlName)
	{
		Name = name;
	}
}
