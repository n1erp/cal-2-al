using System;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class AddDeltaValidator : IDeltaValidator
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
		if (delta is NavXmlAdd navXmlAdd && !string.IsNullOrEmpty(navXmlAdd.Id) && navXmlAdd.Node != null)
		{
			return navXmlAdd.Destination != null;
		}
		return false;
	}
}
