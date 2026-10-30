using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct LocalizableErrorArgument : IFormattable
{
	private readonly MessageID _id;

	internal LocalizableErrorArgument(MessageID id)
	{
		_id = id;
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return ErrorFacts.GetMessage(_id, formatProvider as CultureInfo);
	}
}
