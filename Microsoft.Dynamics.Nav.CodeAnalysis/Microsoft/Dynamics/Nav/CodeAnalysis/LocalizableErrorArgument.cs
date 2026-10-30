using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct LocalizableErrorArgument : IFormattable
{
	private readonly MessageID id;

	internal LocalizableErrorArgument(MessageID id)
	{
		this.id = id;
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return ErrorFacts.GetMessage(id, formatProvider as CultureInfo);
	}
}
