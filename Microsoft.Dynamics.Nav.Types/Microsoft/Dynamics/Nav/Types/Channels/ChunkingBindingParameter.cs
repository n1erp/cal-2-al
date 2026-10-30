using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types.Channels;

public class ChunkingBindingParameter
{
	private readonly HashSet<string> chunkedActions = new HashSet<string>();

	public HashSet<string> ChunkedActions
	{
		[DebuggerStepThrough]
		get
		{
			return chunkedActions;
		}
	}
}
