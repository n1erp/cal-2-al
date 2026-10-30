using System;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class UploadResponse
{
	private FileBufferedStream fileBufferStream;

	private string clientSideFileName;

	public string ClientSideFileName => clientSideFileName;

	public FileBufferedStream FileBufferedStream => fileBufferStream;

	public UploadResponse(string clientSideFileName, FileBufferedStream fileBufferStream)
	{
		if (string.IsNullOrEmpty(clientSideFileName))
		{
			throw new ArgumentNullException("clientSideFileName");
		}
		if (fileBufferStream == null)
		{
			throw new ArgumentNullException("fileBufferStream");
		}
		this.clientSideFileName = clientSideFileName;
		this.fileBufferStream = fileBufferStream;
	}

	public UploadResponse(string clientSideFileName)
	{
		if (string.IsNullOrEmpty(clientSideFileName))
		{
			throw new ArgumentNullException("clientSideFileName");
		}
		this.clientSideFileName = clientSideFileName;
	}
}
