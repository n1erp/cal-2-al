using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideValidateTableRelationException : NavCSideDataException
{
	public NavCSideValidateTableRelationException()
	{
	}

	public NavCSideValidateTableRelationException(string message)
		: base(message)
	{
	}

	public NavCSideValidateTableRelationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCSideValidateTableRelationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavCSideValidateTableRelationException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavCSideValidateTableRelationException(int errorCode, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(errorCode, culture, message, args)
	{
	}

	public static NavCSideValidateTableRelationException CreateRelatedRecordNotFound(int errorCode, string fieldNo, string parentTableCaption, object fieldValue, string sourceTableCaption)
	{
		return new NavCSideValidateTableRelationException(errorCode, CultureInfo.CurrentCulture, Lang.RelatedRecordNotFound, DiagnosticParameter.SystemMetadata(fieldNo), DiagnosticParameter.SystemMetadata(parentTableCaption), DiagnosticParameter.CustomerContent(fieldValue), DiagnosticParameter.SystemMetadata(sourceTableCaption));
	}
}
