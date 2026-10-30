using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IFileSystem
{
	byte[] ReadBytes(string path);

	byte[] ReadBytes(string path, int count);

	void WriteBytes(string path, byte[] content);

	bool Exists(string path);

	bool DirectoryExistsForFile(string path);

	IEnumerable<string> GetFiles(string searchPattern);

	string GetDirectoryPath();

	void CreateDirectoryForFile(string path);

	Stream CreateFile(string path);

	Stream OpenRead(string path);

	Stream OpenWrite(string path);

	Stream OpenFile(string filePath, FileMode mode, FileAccess access, FileShare share = FileShare.None, int bufferSize = 4096, FileOptions options = FileOptions.None);

	void DeleteFile(string path);
}
