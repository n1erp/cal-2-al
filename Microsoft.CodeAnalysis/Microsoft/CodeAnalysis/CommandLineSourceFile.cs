using System.Diagnostics;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{Path,nq}")]
public struct CommandLineSourceFile(string path, bool isScript)
{
	private readonly string _path = path;

	private readonly bool _isScript = isScript;

	public string Path => _path;

	public bool IsScript => _isScript;
}
