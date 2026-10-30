using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public class EmitResult
{
	public bool Success { get; private set; }

	public ImmutableArray<Diagnostic> Diagnostics { get; private set; }

	internal EmitResult(bool success, ImmutableArray<Diagnostic> diagnostics)
	{
		Success = success;
		Diagnostics = diagnostics;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	protected virtual string GetDebuggerDisplay()
	{
		string text = "Success = " + (Success ? "true" : "false");
		if (Diagnostics != null)
		{
			return text + ", Diagnostics.Count = " + Diagnostics.Length;
		}
		return text + ", Diagnostics = null";
	}
}
