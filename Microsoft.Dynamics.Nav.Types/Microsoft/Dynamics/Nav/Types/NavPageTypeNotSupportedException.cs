using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavPageTypeNotSupportedException : NavMetadataException
{
	public NavPageTypeNotSupportedException()
	{
	}

	public NavPageTypeNotSupportedException(PageType pageType)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.PageTypeNotSupportedException, pageType))
	{
	}

	private NavPageTypeNotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavPageTypeNotSupportedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public NavPageTypeNotSupportedException(string message)
		: base(message)
	{
	}
}
