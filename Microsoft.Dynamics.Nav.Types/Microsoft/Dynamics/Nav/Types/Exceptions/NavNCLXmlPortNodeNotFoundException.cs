using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortNodeNotFoundException : NavNCLException
{
	public NavNCLXmlPortNodeNotFoundException()
	{
	}

	public NavNCLXmlPortNodeNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortNodeNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortNodeNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortNodeNotFoundException Create(CultureInfo culture, string nodeName, Guid nodeId)
	{
		return new NavNCLXmlPortNodeNotFoundException(string.Format(culture, Lang._00095_50103, nodeName, nodeId));
	}

	public static NavNCLXmlPortNodeNotFoundException CreateSetTableView(CultureInfo culture, string portName, int tableId, string tableName)
	{
		return new NavNCLXmlPortNodeNotFoundException(string.Format(culture, Lang._00095_50100, portName, tableId, tableName));
	}
}
