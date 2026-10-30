using System;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class MoveDeltaValidator : IDeltaValidator
{
	public bool Validate(ConverterBundle converterBundle, NavXmlBase delta)
	{
		if (converterBundle == null)
		{
			throw new ArgumentNullException("converterBundle");
		}
		if (delta == null)
		{
			throw new ArgumentNullException("delta");
		}
		if (delta is NavXmlMove navXmlMove && !string.IsNullOrEmpty(navXmlMove.Id) && !string.IsNullOrEmpty(navXmlMove.Destination))
		{
			return converterBundle.SourceDefinitionNodes.ContainsKey(navXmlMove.Id);
		}
		return false;
	}
}
