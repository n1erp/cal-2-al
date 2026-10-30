namespace Mono.Cecil;

internal struct Range(uint index, uint length)
{
	public uint Start = index;

	public uint Length = length;
}
