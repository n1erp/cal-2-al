using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal struct EmbeddedResource
{
	public readonly uint Offset;

	public readonly ManifestResourceAttributes Attributes;

	public readonly string Name;

	internal EmbeddedResource(uint offset, ManifestResourceAttributes attributes, string name)
	{
		Offset = offset;
		Attributes = attributes;
		Name = name;
	}
}
