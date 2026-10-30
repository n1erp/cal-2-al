using System;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class DeleteDeltaValidator : IDeltaValidator
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
		if (delta is NavXmlDelete navXmlDelete && !string.IsNullOrEmpty(navXmlDelete.Id) && converterBundle.SourceDefinitionNodes.ContainsKey(navXmlDelete.Id))
		{
			return string.IsNullOrEmpty(navXmlDelete.Destination);
		}
		return false;
	}
}
