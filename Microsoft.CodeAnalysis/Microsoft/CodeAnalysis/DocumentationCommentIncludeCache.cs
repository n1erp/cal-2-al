using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class DocumentationCommentIncludeCache : CachingFactory<string, KeyValuePair<string, XDocument>>
{
	private const int Size = 5;

	private static readonly XmlReaderSettings s_xmlSettings = new XmlReaderSettings
	{
		DtdProcessing = DtdProcessing.Prohibit
	};

	internal static int CacheMissCount { get; private set; }

	public DocumentationCommentIncludeCache(XmlReferenceResolver resolver)
		: base(5, (Func<string, KeyValuePair<string, XDocument>>)((string key) => MakeValue(resolver, key)), (Func<string, int>)KeyHashCode, (Func<string, KeyValuePair<string, XDocument>, bool>)KeyValueEquality)
	{
		CacheMissCount = 0;
	}

	public XDocument GetOrMakeDocument(string resolvedPath)
	{
		return GetOrMakeValue(resolvedPath).Value;
	}

	private static KeyValuePair<string, XDocument> MakeValue(XmlReferenceResolver resolver, string resolvedPath)
	{
		CacheMissCount++;
		using Stream input = resolver.OpenReadChecked(resolvedPath);
		using XmlReader reader = XmlReader.Create(input, s_xmlSettings);
		XDocument value = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
		return KeyValuePairUtil.Create(resolvedPath, value);
	}

	private static int KeyHashCode(string resolvedPath)
	{
		return resolvedPath.GetHashCode();
	}

	private static bool KeyValueEquality(string resolvedPath, KeyValuePair<string, XDocument> pathAndDocument)
	{
		return resolvedPath == pathAndDocument.Key;
	}
}
