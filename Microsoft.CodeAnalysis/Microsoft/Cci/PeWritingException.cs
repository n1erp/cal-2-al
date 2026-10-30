using System;

namespace Microsoft.Cci;

internal sealed class PeWritingException : Exception
{
	public PeWritingException(Exception inner)
		: base(inner.Message, inner)
	{
	}
}
