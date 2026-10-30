using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal class CodeFixCollection
{
	public object Provider { get; }

	public TextSpan TextSpan { get; }

	public ImmutableArray<CodeFix> Fixes { get; }

	public CodeFixCollection(object provider, TextSpan span, ImmutableArray<CodeFix> fixes)
	{
		Provider = provider;
		TextSpan = span;
		Fixes = fixes.NullToEmpty();
	}
}
