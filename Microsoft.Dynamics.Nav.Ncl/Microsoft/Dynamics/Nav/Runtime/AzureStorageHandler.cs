using System;
using Microsoft.Dynamics.Nav.Runtime.Media;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AzureStorageHandler : INavMediaStorageHandler
{
	private readonly Lazy<AzureBlobStorage> AzureBlobStorage;

	public AzureStorageHandler(string storageAccount, string storageKey, string containerName)
	{
		if (string.IsNullOrWhiteSpace(storageAccount))
		{
			throw new ArgumentNullException("storageAccount");
		}
		if (string.IsNullOrWhiteSpace(storageKey))
		{
			throw new ArgumentNullException("storageKey");
		}
		if (string.IsNullOrWhiteSpace(containerName))
		{
			throw new ArgumentNullException("containerName");
		}
		AzureBlobStorage = new Lazy<AzureBlobStorage>(() => new AzureBlobStorage(storageAccount, storageKey, containerName));
	}

	public void CleanupExpiredFiles(NavTenant tenant)
	{
		AzureBlobStorage.Value.CleanupExpiredFiles();
	}

	public Uri GetDocumentUrl(Guid documentId)
	{
		return AzureBlobStorage.Value.GetSharedAccessUrlForFile(documentId.ToString());
	}

	public Guid ImportWithUrlAccess(NavStream stream, string fileName, int minutesToExpire)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (minutesToExpire < 1 || minutesToExpire > 43200)
		{
			throw new NavNCLArgumentOutOfRangeException("minutesToExpire");
		}
		NavSession session = NavCurrentThread.Session;
		if (session != null)
		{
			NavTaskSchedulerUtility.ScheduleSystemTask(SystemTaskActionId.DeleteExpiredFiles, new DeleteExpiredFilesSystemTask.DeleteExpiredFilesSystemTaskParameters(), DateTime.UtcNow.AddMinutes(minutesToExpire));
		}
		return AzureBlobStorage.Value.StoreFile(stream.GetBytes(), minutesToExpire);
	}
}
