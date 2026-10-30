using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableAppIdRequiresLicensedAsExternalException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableAppIdRequiresLicensedAsExternalException()
	{
	}

	public NavNCLUserTableAppIdRequiresLicensedAsExternalException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableAppIdRequiresLicensedAsExternalException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableAppIdRequiresLicensedAsExternalException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
