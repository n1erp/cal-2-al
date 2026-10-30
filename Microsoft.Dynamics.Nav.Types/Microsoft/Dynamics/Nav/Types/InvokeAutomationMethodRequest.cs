using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class InvokeAutomationMethodRequest<TArgumentsType>
{
	[DataMember]
	public int Handle { get; set; }

	[DataMember]
	public int TypeHandle { get; set; }

	[DataMember]
	public Guid FormHandle { get; set; }

	[DataMember]
	public string VariableName { get; set; }

	[DataMember]
	public string MethodName { get; set; }

	[DataMember]
	public BindingFlags Binding { get; set; }

	[DataMember]
	public ParameterModifier[] Modifiers { get; set; }

	[DataMember]
	public TArgumentsType[] Arguments { get; set; }
}
