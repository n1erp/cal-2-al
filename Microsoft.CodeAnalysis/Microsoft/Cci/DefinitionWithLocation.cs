using System.Diagnostics;

namespace Microsoft.Cci;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal struct DefinitionWithLocation(IDefinition definition, int startLine, int startColumn, int endLine, int endColumn)
{
	public readonly IDefinition Definition = definition;

	public readonly int StartLine = startLine;

	public readonly int StartColumn = startColumn;

	public readonly int EndLine = endLine;

	public readonly int EndColumn = endColumn;

	private string GetDebuggerDisplay()
	{
		return $"{Definition} => ({StartLine},{StartColumn}) - ({EndLine}, {EndColumn})";
	}
}
