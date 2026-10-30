using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public static class PackageEmittedContentWriter
{
	private static JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
	{
		NullValueHandling = NullValueHandling.Ignore
	};

	public static void WriteContent(Stream stream, ICollection<PackageEmittedContent> content)
	{
		using StreamWriter textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
		JsonSerializer.Create(SerializerSettings).Serialize(textWriter, content);
	}

	public static ICollection<PackageEmittedContent> ReadContent(Stream stream)
	{
		using StreamReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return JsonSerializer.Create(SerializerSettings).Deserialize<List<PackageEmittedContent>>(reader2);
	}
}
