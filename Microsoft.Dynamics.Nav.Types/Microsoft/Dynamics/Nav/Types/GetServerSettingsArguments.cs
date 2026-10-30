using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class GetServerSettingsArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }
}
