using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Common;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public static class MetadataFromResourceLoader
{
	public static T Load<T>(string resourceName)
	{
		byte[] masterPageXml = NavResourceManager.GetMasterPageXml(resourceName);
		if (masterPageXml == null || masterPageXml.Length == 0)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to load master page {0} from resources.", resourceName));
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		using MemoryStream documentStream = new MemoryStream(masterPageXml);
		using XmlReader xmlReader = XmlHelper.SafeXmlReader(documentStream);
		try
		{
			return (T)xmlSerializer.Deserialize(xmlReader);
		}
		catch (InvalidOperationException innerException)
		{
			throw new InvalidOperationException("Not Possible to Deserialize xml", innerException);
		}
	}
}
