using System;
using System.Xml;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ContentAddMetadata : ContentDestinationMetadata
{
	public Action<XmlWriter> ContentWriteAction { get; set; }
}
