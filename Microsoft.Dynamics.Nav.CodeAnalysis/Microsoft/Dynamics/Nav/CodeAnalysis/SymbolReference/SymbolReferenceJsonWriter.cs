using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public static class SymbolReferenceJsonWriter
{
	public static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
	{
		NullValueHandling = NullValueHandling.Ignore
	};

	public static void WriteModule(Stream stream, ModuleDefinition module)
	{
		Write(stream, module);
	}

	public static void WriteLanguageElement(Stream stream, LanguageElement element)
	{
		Write(stream, element);
	}

	private static void Write(Stream stream, object value)
	{
		using StreamWriter textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
		JsonSerializer.Create(SerializerSettings).Serialize(textWriter, value);
	}
}
