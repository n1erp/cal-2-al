using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface IDeltaValidator
{
	bool Validate(ConverterBundle converterBundle, NavXmlBase delta);
}
