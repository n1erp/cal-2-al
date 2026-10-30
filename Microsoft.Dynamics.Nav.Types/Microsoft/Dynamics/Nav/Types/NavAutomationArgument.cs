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
public class NavAutomationArgument
{
	[DataMember]
	private object argument;

	public object Argument
	{
		get
		{
			return DateTimeDataContract.UnwrapIfDateTime(argument);
		}
		set
		{
			argument = DateTimeDataContract.WrapIfDateTime(value);
		}
	}

	public NavAutomationArgument(object argument)
	{
		this.argument = DateTimeDataContract.WrapIfDateTime(argument);
	}
}
