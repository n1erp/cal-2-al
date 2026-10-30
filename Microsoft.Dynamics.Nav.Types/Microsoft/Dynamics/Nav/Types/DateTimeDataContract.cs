using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DateTimeDataContract
{
	[DataMember]
	private long ticks;

	[DataMember]
	private DateTimeKind kind;

	public DateTime DateTime => new DateTime(ticks, kind);

	public DateTimeDataContract(DateTime dateTime)
	{
		ticks = dateTime.Ticks;
		kind = dateTime.Kind;
	}

	public static object WrapIfDateTime(object input)
	{
		if (input == null || input.GetType() != typeof(DateTime))
		{
			return input;
		}
		return new DateTimeDataContract((DateTime)input);
	}

	public static object UnwrapIfDateTime(object input)
	{
		if (input is DateTimeDataContract dateTimeDataContract)
		{
			return dateTimeDataContract.DateTime;
		}
		return input;
	}
}
