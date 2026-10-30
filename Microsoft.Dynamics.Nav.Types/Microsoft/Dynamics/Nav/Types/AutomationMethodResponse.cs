using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[KnownType(typeof(NavAutomationHandle))]
[KnownType(typeof(CurrencyWrapper))]
[KnownType(typeof(VariantWrapper))]
[KnownType(typeof(ErrorWrapper))]
[KnownType(typeof(DBNull))]
[KnownType(typeof(DateTimeDataContract))]
[KnownType(typeof(SerializableObjectContract))]
[KnownType(typeof(JsonSerializableObjectContract))]
public class AutomationMethodResponse
{
	[DataMember]
	private object returnValue;

	[DataMember]
	private NavAutomationArgument[] arguments;

	public object ReturnValue
	{
		get
		{
			return DateTimeDataContract.UnwrapIfDateTime(returnValue);
		}
		set
		{
			returnValue = DateTimeDataContract.WrapIfDateTime(value);
		}
	}

	public NavAutomationArgument[] Arguments
	{
		get
		{
			return arguments;
		}
		set
		{
			arguments = value;
		}
	}
}
