using System;
using System.Diagnostics;

namespace Roslyn.Utilities;

[Conditional("EMIT_CODE_ANALYSIS_ATTRIBUTES")]
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
internal sealed class PerformanceSensitiveAttribute : Attribute
{
	public string Uri { get; }

	public string Constraint { get; set; }

	public bool AllowCaptures { get; set; }

	public bool AllowGenericEnumeration { get; set; }

	public bool AllowLocks { get; set; }

	public bool OftenCompletesSynchronously { get; set; }

	public bool IsParallelEntry { get; set; }

	public PerformanceSensitiveAttribute(string uri)
	{
		Uri = uri;
	}
}
