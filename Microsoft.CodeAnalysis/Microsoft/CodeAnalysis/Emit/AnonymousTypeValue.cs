using System.Diagnostics;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit;

[DebuggerDisplay("{Name, nq}")]
internal struct AnonymousTypeValue(string name, int uniqueIndex, ITypeDefinition type)
{
	public readonly string Name = name;

	public readonly int UniqueIndex = uniqueIndex;

	public readonly ITypeDefinition Type = type;
}
