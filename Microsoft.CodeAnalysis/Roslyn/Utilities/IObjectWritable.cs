namespace Roslyn.Utilities;

internal interface IObjectWritable
{
	bool ShouldReuseInSerialization { get; }

	void WriteTo(ObjectWriter writer);
}
