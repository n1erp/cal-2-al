using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMetadataCSharpErrorException : NavNCLMetadataException
{
	private const string ErrorsName = "Errors";

	public string Errors { get; private set; }

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataCSharpErrorException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataCSharpErrorException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataCSharpErrorException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLMetadataCSharpErrorException(CultureInfo culture, ObjectType objectType, int objectId, string errors, Exception innerException)
		: base(CreateMessage(culture, objectType, objectId, errors), innerException)
	{
		Errors = errors;
	}

	protected NavNCLMetadataCSharpErrorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		Errors = info.GetString("Errors");
	}

	private static string CreateMessage(CultureInfo culture, ObjectType objectType, int objectId, string errors)
	{
		return string.Format(culture, Lang._00095_50173, objectType, objectId, errors);
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("Errors", Errors);
		base.GetObjectData(info, context);
	}
}
