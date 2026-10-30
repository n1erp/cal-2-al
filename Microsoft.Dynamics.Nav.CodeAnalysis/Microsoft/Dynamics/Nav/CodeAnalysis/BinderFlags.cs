using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
internal enum BinderFlags
{
	None = 0,
	SemanticModel = 0x1000,
	IgnoreAccessibility = 0x400000
}
