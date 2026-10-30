namespace Microsoft.Dynamics.Nav.Types.Channels;

internal static class ChunkConstants
{
	public const string Namespace = "http://www.microsoft.com/Dynamics/Channel";

	public const string RealAction = "RealAction";

	public const string RealActionNamespace = "http://www.microsoft.com/Dynamics/Channel/RealAction";

	public const string ChunkId = "ChunkId";

	public const string ChunkNumberHeader = "ChunkNumber";

	public const string ChunkElement = "Chunk";

	public const string ChunkEnd = "ChunkEnd";

	public const string CancelChunkSequence = "http://www.microsoft.com/Dynamics/Channel/CancelChunkSequence";

	public const string Action = "Action";

	internal const int ChunkStartNumber = 1;

	internal const int NotChunkNumber = 0;

	public const string NamespacePrefix = "C";

	public const int DefaultChunkSize = 28672;
}
