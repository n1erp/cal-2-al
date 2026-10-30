using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(LogType = EventLogEntryType.Error)]
public class NavAppObjectMetadataException : NavNCLMetadataException
{
	public NavAppObjectMetadataException()
	{
	}

	public NavAppObjectMetadataException(string message)
		: base(message)
	{
	}

	public NavAppObjectMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
		DiagnosticsMessage = message;
	}

	private NavAppObjectMetadataException(string message, string diagnosticMessage, Exception innerException)
		: base(message, innerException)
	{
		DiagnosticsMessage = diagnosticMessage;
	}

	protected NavAppObjectMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavAppObjectMetadataException Create(string message)
	{
		return new NavAppObjectMetadataException(message);
	}

	public static NavAppObjectMetadataException Create(string message, Exception innerException)
	{
		return new NavAppObjectMetadataException(message, innerException);
	}

	public static NavAppObjectMetadataException Create(string appTechnicalName, ApplicationObjectId objectId, string errorDetails)
	{
		return new NavAppObjectMetadataException(AddErrorMessage(SelectRootMessage(null, appTechnicalName, objectId), errorDetails));
	}

	public static NavAppObjectMetadataException Create(string appTechnicalName, ApplicationObjectId objectId, Exception innerException)
	{
		return CreateWithDiagnosticMessage(SelectRootMessage(null, appTechnicalName, objectId), innerException);
	}

	public static NavAppObjectMetadataException Create(ApplicationObjectId? sourceObjectId, string appTechnicalName, ApplicationObjectId targetObjectId, string errorDetails)
	{
		return new NavAppObjectMetadataException(AddErrorMessage(SelectRootMessage(sourceObjectId, appTechnicalName, targetObjectId), errorDetails));
	}

	public static NavAppObjectMetadataException Create(ApplicationObjectId? sourceObjectId, string appTechnicalName, ApplicationObjectId targetObjectId, Exception innerException)
	{
		return CreateWithDiagnosticMessage(SelectRootMessage(sourceObjectId, appTechnicalName, targetObjectId), innerException);
	}

	private static NavAppObjectMetadataException CreateWithDiagnosticMessage(string rootMessage, Exception innerException)
	{
		string text = innerException.GetType().Name + " - ";
		string message = AddErrorMessage(rootMessage, text + innerException.Message);
		string text2 = ((innerException is NavBaseException ex) ? ex.MessageWithoutPrivateInformation : "<redacted>");
		string diagnosticMessage = AddErrorMessage(rootMessage, text + text2);
		return new NavAppObjectMetadataException(message, diagnosticMessage, innerException);
	}

	private static string SelectRootMessage(ApplicationObjectId? sourceObjectId, string appTechnicalName, ApplicationObjectId targetObjectId)
	{
		if (!sourceObjectId.HasValue)
		{
			return string.Format(CultureInfo.InvariantCulture, "An error occurred while applying changes from the '{0}' app to the application object of type '{1}' with the ID '{2}'.", appTechnicalName, targetObjectId.ObjectType, targetObjectId.ObjectNumber);
		}
		return string.Format(CultureInfo.InvariantCulture, "An error occurred while applying changes from the object of type '{0}' with ID '{1}' in the '{2}' app to the application object of type '{3}' with the ID '{4}'", sourceObjectId.Value.ObjectType, sourceObjectId.Value.ObjectNumber, appTechnicalName, targetObjectId.ObjectType, targetObjectId.ObjectNumber);
	}

	private static string AddErrorMessage(string rootMessage, string errorMessage)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} The error was: {1}", rootMessage, errorMessage);
	}
}
