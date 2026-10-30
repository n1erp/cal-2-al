using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal interface IPackageWriter
{
	void WriteBytes(byte[] content, string path);

	void WriteStream(Stream stream, string path);

	void WithStream(string path, Action<Stream> action);

	void WriteString(string contents, string path);
}
