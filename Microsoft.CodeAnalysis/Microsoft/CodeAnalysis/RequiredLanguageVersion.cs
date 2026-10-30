using System;

namespace Microsoft.CodeAnalysis;

internal abstract class RequiredLanguageVersion : IFormattable
{
	public abstract override string ToString();

	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return ToString();
	}
}
