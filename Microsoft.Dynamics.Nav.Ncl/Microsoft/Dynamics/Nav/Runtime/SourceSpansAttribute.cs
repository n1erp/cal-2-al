using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SourceSpansAttribute : Attribute
{
	private readonly long[] encodedSpans;

	public long[] EncodedSpans
	{
		[DebuggerStepThrough]
		get
		{
			return encodedSpans;
		}
	}

	public SourceSpan[] Spans => Array.ConvertAll(encodedSpans, (long lRange) => (SourceSpan)lRange);

	public SourceSpansAttribute(params long[] encodedSpans)
	{
		this.encodedSpans = encodedSpans;
	}
}
