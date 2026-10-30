using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Optional")]
public struct Optional<T>(T value)
{
	public bool HasValue { get; } = true;

	public T Value { get; } = value;

	public static implicit operator Optional<T>(T value)
	{
		return new Optional<T>(value);
	}
}
