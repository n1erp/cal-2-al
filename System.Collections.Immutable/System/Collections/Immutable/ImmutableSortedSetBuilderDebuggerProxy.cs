using System.Diagnostics;

namespace System.Collections.Immutable;

internal class ImmutableSortedSetBuilderDebuggerProxy<T>
{
	private readonly ImmutableSortedSet<T>.Builder _set;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Contents => _set.ToArray(_set.Count);

	public ImmutableSortedSetBuilderDebuggerProxy(ImmutableSortedSet<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		_set = builder;
	}
}
