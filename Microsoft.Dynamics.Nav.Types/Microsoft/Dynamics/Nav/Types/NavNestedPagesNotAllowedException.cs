using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavNestedPagesNotAllowedException : NavBuilderException
{
	public NavNestedPagesNotAllowedException()
		: base(Lang.NestedPagesNotAllowed)
	{
	}

	private NavNestedPagesNotAllowedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNestedPagesNotAllowedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public NavNestedPagesNotAllowedException(string message)
		: base(message)
	{
	}
}
