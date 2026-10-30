using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavStreamProvider : IDisposable
{
	Stream GetStream();
}
