using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[Flags]
internal enum QuickAttributes : byte
{
	None = 0,
	TypeIdentifier = 1,
	TypeForwardedTo = 2
}
