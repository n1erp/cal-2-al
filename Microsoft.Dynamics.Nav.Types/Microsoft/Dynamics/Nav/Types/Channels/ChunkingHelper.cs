using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal static class ChunkingHelper
{
	private static readonly object syncRoot = new object();

	private static BufferManager instance;

	public static BufferManager BufferManager
	{
		get
		{
			if (instance == null)
			{
				lock (syncRoot)
				{
					if (instance == null)
					{
						int num = OptimalChunkSize((ConfigSettings.Instance == null) ? 28672 : ConfigSettings.Instance.ClientServicesChunkSize);
						instance = BufferManager.CreateBufferManager(num * 100, num);
					}
				}
			}
			return instance;
		}
	}

	public static int OptimalChunkSize(int desiredSize)
	{
		return desiredSize - desiredSize % 3;
	}

	public static void EnsureInfinite(this TimeSpan timeout)
	{
		if (timeout != TimeSpan.MaxValue)
		{
			throw new InvalidOperationException("Asynchronous receiving only supports inifinite timeouts.");
		}
	}

	internal static void MapException(CommunicationException ex)
	{
		if (ex == null)
		{
			throw new ArgumentNullException("ex");
		}
		if (ex.InnerException is IOException && ex.InnerException.InnerException is CommunicationException && ex.InnerException.InnerException.InnerException is SocketException ex2)
		{
			if (ex2.SocketErrorCode == SocketError.ConnectionReset || ex2.SocketErrorCode == SocketError.ConnectionAborted)
			{
				throw new CommunicationObjectAbortedException(ex.Message, ex.InnerException);
			}
			if (ex2.SocketErrorCode == SocketError.TimedOut)
			{
				throw new TimeoutException(ex.Message, ex.InnerException);
			}
			throw new CommunicationException(ex.Message + Environment.NewLine + ex2.SocketErrorCode, ex.InnerException);
		}
	}

	[Conditional("CHUNKING_CHANNEL_LOG")]
	internal static void ChunkingLogWriteLine(string format, params object[] args)
	{
	}
}
