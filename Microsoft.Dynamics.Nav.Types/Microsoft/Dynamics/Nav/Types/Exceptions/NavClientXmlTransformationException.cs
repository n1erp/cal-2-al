using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientXmlTransformationException : NavClientBaseException
{
	[Obsolete("Use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientXmlTransformationException()
	{
	}

	[Obsolete("Use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientXmlTransformationException(string message)
		: base(message)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavClientXmlTransformationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavClientXmlTransformationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
