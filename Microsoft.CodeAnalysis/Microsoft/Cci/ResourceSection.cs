namespace Microsoft.Cci;

internal class ResourceSection
{
	internal readonly byte[] SectionBytes;

	internal readonly uint[] Relocations;

	internal ResourceSection(byte[] sectionBytes, uint[] relocations)
	{
		SectionBytes = sectionBytes;
		Relocations = relocations;
	}
}
