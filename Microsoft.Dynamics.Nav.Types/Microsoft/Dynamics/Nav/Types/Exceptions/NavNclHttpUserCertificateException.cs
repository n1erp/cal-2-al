using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclHttpUserCertificateException : NavNCLException
{
	public NavNclHttpUserCertificateException()
	{
	}

	public NavNclHttpUserCertificateException(string message)
		: base(message)
	{
	}

	public NavNclHttpUserCertificateException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected NavNclHttpUserCertificateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
