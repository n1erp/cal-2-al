namespace Microsoft.CodeAnalysis;

internal struct ReferenceDirective(string file, Location location)
{
	public readonly string File = file;

	public readonly Location Location = location;
}
