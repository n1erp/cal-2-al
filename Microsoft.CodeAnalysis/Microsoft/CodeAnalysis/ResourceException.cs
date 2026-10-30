using System;

namespace Microsoft.CodeAnalysis;

internal sealed class ResourceException : Exception
{
	internal ResourceException(string name, Exception inner = null)
		: base(name, inner)
	{
	}
}
