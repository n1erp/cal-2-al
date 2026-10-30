using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class StringBuilderExtensions
{
	public static void JoinAndAppend(this StringBuilder stringBuilder, string separator, IEnumerable<string> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (separator == null)
		{
			separator = string.Empty;
		}
		using IEnumerator<string> enumerator = values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		string current = enumerator.Current;
		if (current != null)
		{
			stringBuilder.Append(current);
		}
		while (enumerator.MoveNext())
		{
			stringBuilder.Append(separator);
			string current2 = enumerator.Current;
			if (current2 != null)
			{
				stringBuilder.Append(current2);
			}
		}
	}
}
