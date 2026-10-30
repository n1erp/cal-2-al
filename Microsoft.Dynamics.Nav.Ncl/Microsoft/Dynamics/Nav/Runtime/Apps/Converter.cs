using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class Converter
{
	public static ConverterResult ProcessDelta(XmlDocument masterPageDocument, XmlDocument masterPageTemplate, XmlDocument pageDefinition, NavXmlDelta delta, string pageName, bool skipUsingALCompiler = false)
	{
		if (masterPageDocument == null)
		{
			throw new ArgumentNullException("masterPageDocument");
		}
		if (masterPageTemplate == null)
		{
			throw new ArgumentNullException("masterPageTemplate");
		}
		if (delta == null)
		{
			throw new ArgumentNullException("delta");
		}
		ConverterBundle converterBundle = CreateForConverter(masterPageDocument, masterPageTemplate, pageDefinition, pageName, skipUsingALCompiler);
		return new ConverterSyntaxGenManager().GetSyntaxNodes(converterBundle, delta.Items);
	}

	private static ConverterBundle CreateForConverter(XmlDocument masterPageDocument, XmlDocument masterPageTemplate, XmlDocument pageDefinition, string pageName, bool skipUsingALCompiler = false)
	{
		Dictionary<string, XmlElement> controlElements = ConverterHelper.GetControlElements(masterPageDocument);
		Dictionary<string, XmlElement> controlElements2 = ConverterHelper.GetControlElements(masterPageTemplate);
		Dictionary<string, XmlElement> controlElements3 = ConverterHelper.GetControlElements(pageDefinition);
		Dictionary<string, string> alIdentifierMapper = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, int> distinctALIdentifier = ConverterHelper.GetDistinctALIdentifier(controlElements, controlElements3, alIdentifierMapper, pageName, skipUsingALCompiler);
		HashSet<int> idSet = ConverterHelper.GetIdSet(masterPageTemplate);
		return new ConverterBundle(controlElements, controlElements2, controlElements3, distinctALIdentifier, alIdentifierMapper, idSet, masterPageDocument);
	}
}
