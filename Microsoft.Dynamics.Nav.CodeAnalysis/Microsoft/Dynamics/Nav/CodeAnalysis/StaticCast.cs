using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class StaticCast<T>
{
	internal static ImmutableArray<T> From<TDerived>(ImmutableArray<TDerived> from) where TDerived : class, T
	{
		return ImmutableArray<T>.CastUp(from);
	}
}
