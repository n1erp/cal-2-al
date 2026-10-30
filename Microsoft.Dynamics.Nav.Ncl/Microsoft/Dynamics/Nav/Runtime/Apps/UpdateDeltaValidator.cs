using System;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class UpdateDeltaValidator : IDeltaValidator
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
		if (delta is NavXmlUpdate navXmlUpdate && !string.IsNullOrEmpty(navXmlUpdate.Id) && navXmlUpdate.Uri != null && navXmlUpdate.Name != null && navXmlUpdate.Value != null && navXmlUpdate.ElementPath == null)
		{
			return converterBundle.SourceDefinitionNodes.ContainsKey(navXmlUpdate.Id);
		}
		return false;
	}
}
