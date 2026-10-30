using System.IO;
using System.Xml;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class XmlMetadataCleaner
{
	internal static void CleanForInvalidCharacters(Stream navObject, ref XmlDocument document)
	{
		Microsoft.Dynamics.Nav.Types.Metadata.XmlMetadataCleaner.CleanForInvalidCharacters(navObject, LogWriterHelper.LogEvent, ref document);
	}
}
