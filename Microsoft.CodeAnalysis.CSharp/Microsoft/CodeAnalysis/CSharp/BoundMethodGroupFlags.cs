using System;

namespace Microsoft.CodeAnalysis.CSharp;

[Flags]
internal enum BoundMethodGroupFlags
{
	None = 0,
	SearchExtensionMethods = 1,
	HasImplicitReceiver = 2
}
