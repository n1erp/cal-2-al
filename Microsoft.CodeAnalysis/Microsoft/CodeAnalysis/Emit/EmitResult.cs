using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Emit;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public class EmitResult
{
	public bool Success { get; }

	public ImmutableArray<Diagnostic> Diagnostics { get; }

	internal EmitResult(bool success, ImmutableArray<Diagnostic> diagnostics)
	{
		Success = success;
		Diagnostics = diagnostics;
	}

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
