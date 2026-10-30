using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging.Nea;

internal abstract class NeaStream : Stream
{
	protected static readonly byte[] Header = new byte[8] { 46, 78, 69, 65, 0, 0, 0, 1 };

	protected static readonly byte[] Key = new byte[6] { 15, 11, 81, 137, 184, 120 };
}
