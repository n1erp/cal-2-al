using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavConfigurationPackageFileAlreadyExistsException : NavException
{
	public static NavConfigurationPackageFileAlreadyExistsException Create(string code)
	{
		return new NavConfigurationPackageFileAlreadyExistsException(Lang.ConfigurationPackageFileAlreadyExistsError, code);
	}

	public NavConfigurationPackageFileAlreadyExistsException(string message, string code)
		: base(string.Format(CultureInfo.CurrentCulture, message, code))
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavConfigurationPackageFileAlreadyExistsException()
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavConfigurationPackageFileAlreadyExistsException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavConfigurationPackageFileAlreadyExistsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	protected NavConfigurationPackageFileAlreadyExistsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
