using System;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class OrderDeltaValidator : IDeltaValidator
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
		if (delta is NavXmlOrder navXmlOrder && !string.IsNullOrEmpty(navXmlOrder.Id))
		{
			return navXmlOrder.Sequence != null;
		}
		return false;
	}
}
