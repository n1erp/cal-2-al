using System;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public class AlExporterException : Exception
{
	public AlExporterException(string message, IElement targetElement)
		: base(AddTargetInfoToMessage(message, targetElement))
	{
	}

	public AlExporterException(string message)
		: base(message)
	{
	}

	private static string AddTargetInfoToMessage(string message, IElement targetElement)
	{
		targetElement.TryGetStringProperty(PropertyType.Target, out var value);
		if (!string.IsNullOrWhiteSpace(value))
		{
			return message + " (Target: " + value + ")";
		}
		return message;
	}
}
