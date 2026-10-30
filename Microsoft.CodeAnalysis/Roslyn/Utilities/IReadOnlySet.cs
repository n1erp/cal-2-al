namespace Roslyn.Utilities;

internal interface IReadOnlySet<T>
{
	int Count { get; }

	bool Contains(T item);
}
