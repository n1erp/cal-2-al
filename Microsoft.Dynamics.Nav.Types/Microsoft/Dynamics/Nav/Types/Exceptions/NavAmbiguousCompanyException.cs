using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAmbiguousCompanyException : NavBaseException
{
	public NavAmbiguousCompanyException()
	{
	}

	public NavAmbiguousCompanyException(string servicePath)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.ODataAmbiguousCompany, servicePath))
	{
	}

	public NavAmbiguousCompanyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAmbiguousCompanyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
