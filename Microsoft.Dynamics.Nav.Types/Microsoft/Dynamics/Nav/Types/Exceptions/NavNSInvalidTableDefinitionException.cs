using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSInvalidTableDefinitionException : NavMetadataException
{
	public NavNSInvalidTableDefinitionException()
	{
	}

	public NavNSInvalidTableDefinitionException(string message)
		: base(message)
	{
	}

	public NavNSInvalidTableDefinitionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNSInvalidTableDefinitionException(int tableId, Exception innerException)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.InvalidTableDefinition, tableId, innerException.Message), innerException)
	{
		DiagnosticsMessage = string.Format(CultureInfo.CurrentCulture, Lang.InvalidTableDefinition, tableId, (innerException is NavBaseException ex) ? ex.MessageWithoutPrivateInformation : "<redacted>");
	}

	public NavNSInvalidTableDefinitionException(int tableId, int fieldId, Exception innerException)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.InvalidTableDefinitionIncludingField, tableId, fieldId, innerException.Message), innerException)
	{
		DiagnosticsMessage = string.Format(CultureInfo.CurrentCulture, Lang.InvalidTableDefinitionIncludingField, tableId, fieldId, (innerException is NavBaseException ex) ? ex.MessageWithoutPrivateInformation : "<redacted>");
	}

	protected NavNSInvalidTableDefinitionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
