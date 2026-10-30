using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectMetadataBlobCache : INCLObjectMetadataBlobCache
{
	public const int BufferSize = 81920;

	private readonly ConcurrentDictionary<int, SemaphoreSlim> metadataFileSemaphores = new ConcurrentDictionary<int, SemaphoreSlim>();

	private int metadataFileSemaphoresUsageCount;

	private readonly object MetadataFileSemaphoresLockObject = new object();

	public NavBLOB GetOrAddBlob(NCLObjectMetadataCacheKey key, BlobGenerator blobGenerator)
	{
		NavBLOB blob = null;
		if (!TypeIsSupportedForCaching(key.ObjectType))
		{
			blob = blobGenerator(key);
		}
		else
		{
			string metadataFileName = key.GetMetadataFileName();
			PerformInFileLock(metadataFileName, () =>
			{
				if (!TryGetRecordBlob(out blob, key.GetMetadataFileName(), key.GetOffsetTimestamp()))
				{
					blob = blobGenerator(key);
					CacheRecordBlob(blob, key);
				}
			});
		}
		return blob;
	}

	private static bool TryGetRecordBlob(out NavBLOB blob, string metadataFileName, long objectTimestamp)
	{
		blob = null;
		if (File.GetLastWriteTimeUtc(metadataFileName).Ticks == objectTimestamp)
		{
			try
			{
				blob = new NavBLOB(File.ReadAllBytes(metadataFileName), useContentInstance: true);
				return true;
			}
			catch (IOException)
			{
			}
		}
		return false;
	}

	private static void CacheRecordBlob(NavBLOB blob, NCLObjectMetadataCacheKey key)
	{
		string metadataFileName = key.GetMetadataFileName();
		long offsetTimestamp = key.GetOffsetTimestamp();
		if (metadataFileName == null)
		{
			return;
		}
		try
		{
			using (FileStream destination = new FileStream(metadataFileName, FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.None))
			{
				blob.GetNewStream().CopyTo(destination);
			}
			File.SetLastWriteTimeUtc(metadataFileName, new DateTime(offsetTimestamp));
		}
		catch (IOException)
		{
		}
	}

	private void PerformInFileLock(string metadataFileName, Action lockedAction)
	{
		SemaphoreSlim semaphoreSlim = null;
		try
		{
			lock (MetadataFileSemaphoresLockObject)
			{
				semaphoreSlim = metadataFileSemaphores.GetOrAdd(metadataFileName.GetHashCode(), (int f) => new SemaphoreSlim(1, 1));
				metadataFileSemaphoresUsageCount++;
			}
			semaphoreSlim.Wait();
			lockedAction();
		}
		finally
		{
			if (metadataFileName != null && semaphoreSlim != null)
			{
				semaphoreSlim.Release();
				lock (MetadataFileSemaphoresLockObject)
				{
					metadataFileSemaphoresUsageCount--;
					if (metadataFileSemaphoresUsageCount == 0)
					{
						metadataFileSemaphores.Clear();
					}
				}
			}
		}
	}

	public static bool TypeIsSupportedForCaching(ObjectType objectType)
	{
		switch (objectType)
		{
		case ObjectType.Table:
		case ObjectType.Report:
		case ObjectType.XmlPort:
		case ObjectType.MenuSuite:
		case ObjectType.Page:
		case ObjectType.Query:
			return true;
		default:
			return false;
		}
	}
}
