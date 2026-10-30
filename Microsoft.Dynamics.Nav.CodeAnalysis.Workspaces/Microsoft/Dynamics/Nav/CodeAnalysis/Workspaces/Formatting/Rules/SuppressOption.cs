using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

[Flags]
internal enum SuppressOption
{
	None = 0,
	NoWrappingIfOnSingleLine = 1,
	NoWrappingIfOnMultipleLine = 2,
	NoWrapping = NoWrappingIfOnSingleLine | NoWrappingIfOnMultipleLine,
	NoSpacingIfOnSingleLine = 4,
	NoSpacingIfOnMultipleLine = 8,
	NoSpacing = NoSpacingIfOnSingleLine | NoSpacingIfOnMultipleLine,
	IgnoreElastic = 0x10
}
