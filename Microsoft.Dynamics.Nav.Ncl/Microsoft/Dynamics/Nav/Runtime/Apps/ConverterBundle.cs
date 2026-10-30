using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class ConverterBundle
{
	public Dictionary<string, XmlElement> SourceNodes { get; }

	public Dictionary<string, XmlElement> SourceTempNodes { get; }

	public Dictionary<string, XmlElement> SourceDefinitionNodes { get; }

	public Dictionary<string, int> DistinctALIdentifier { get; }

	public Dictionary<string, string> ALIdentifierMapper { get; }

	public HashSet<int> SourceTempIdSet { get; }

	public XmlDocument MasterPageDocument { get; }

	public ConverterBundle(Dictionary<string, XmlElement> sourceNodes, Dictionary<string, XmlElement> sourceTempNodes, Dictionary<string, XmlElement> sourceDefinitionNodes, Dictionary<string, int> distinctALIdentifier, Dictionary<string, string> alIdentifierMapper, HashSet<int> sourceTempIdSet, XmlDocument masterPageDocument)
	{
		SourceNodes = sourceNodes ?? throw new ArgumentNullException("sourceNodes");
		SourceTempNodes = sourceTempNodes ?? throw new ArgumentNullException("sourceTempNodes");
		SourceDefinitionNodes = sourceDefinitionNodes ?? throw new ArgumentNullException("sourceDefinitionNodes");
		DistinctALIdentifier = distinctALIdentifier ?? throw new ArgumentNullException("distinctALIdentifier");
		ALIdentifierMapper = alIdentifierMapper ?? throw new ArgumentNullException("alIdentifierMapper");
		SourceTempIdSet = sourceTempIdSet ?? throw new ArgumentNullException("sourceTempIdSet");
		MasterPageDocument = masterPageDocument ?? throw new ArgumentNullException("masterPageDocument");
	}
}
