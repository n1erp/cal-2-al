using System;
using System.ComponentModel;

namespace Microsoft.CodeAnalysis;

public enum SourceCodeKind
{
	Regular,
	Script,
	[Obsolete("Use Script instead", false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	Interactive
}
