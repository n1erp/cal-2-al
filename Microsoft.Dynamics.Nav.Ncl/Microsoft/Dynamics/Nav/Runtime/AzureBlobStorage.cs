using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzureBlobStorage
{
	private CloudBlobContainer blobContainer;

	private const int DefaultSharedAccessLinkExpiry = 30;

	private Func<string, CloudBlockBlob> GetBlockBlobReference;

	private Func<CloudBlockBlob, DateTimeOffset, string> GetSharedAccessSignature = (CloudBlockBlob blockBlob, DateTimeOffset expiryTime) =>
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		return ((CloudBlob)blockBlob).GetSharedAccessSignature(new SharedAccessBlobPolicy
		{
			Permissions = (SharedAccessBlobPermissions)1,
			SharedAccessExpiryTime = expiryTime
		});
	};

	internal const string ExpiresMetadataKey = "Expires";

	private string ContainerName
	{
		get
		{
			CloudBlobContainer val = blobContainer;
			return ((val != null) ? val.Name : null) ?? string.Empty;
		}
	}

	public AzureBlobStorage(CloudBlobContainer blobContainer)
	{
		this.blobContainer = blobContainer;
		GetBlockBlobReference = this.blobContainer.GetBlockBlobReference;
	}

	public AzureBlobStorage(string accountName, string accountKey, string containerName)
	{
		if (string.IsNullOrWhiteSpace(accountName))
		{
			throw new ArgumentNullException("accountName");
		}
		if (string.IsNullOrWhiteSpace(accountKey))
		{
			throw new ArgumentNullException("accountKey");
		}
		if (string.IsNullOrWhiteSpace(containerName))
		{
			throw new ArgumentNullException("containerName");
		}
		blobContainer = InitializeBlobContainer(accountName, accountKey, containerName);
		GetBlockBlobReference = blobContainer.GetBlockBlobReference;
	}

	public void CleanupExpiredFiles()
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using ITraceScope traceScope = mostSpecificInstance.CreateTraceScope("00001JA", DiagnosticParameter.SystemMetadata("CleanupExpiredFiles"), MonitorType.None, Category.Runtime);
		try
		{
			mostSpecificInstance.SendTraceTag("00001IW", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Finding files in the {0} storage container that have expired.", ContainerName);
			IEnumerable<CloudBlockBlob> enumerable = from blob in blobContainer.ListBlobs((string)null, false, (BlobListingDetails)2, (BlobRequestOptions)null, (OperationContext)null)
				select (CloudBlockBlob)(object)((blob is CloudBlockBlob) ? blob : null) into blob
				where blob != null && ((CloudBlob)blob).Metadata.ContainsKey("Expires")
				select blob;
			mostSpecificInstance.SendTraceTag("00001IX", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Found {0} files in the blob storage container.", enumerable.Count());
			int num = 0;
			foreach (CloudBlockBlob item in enumerable)
			{
				string text = ((CloudBlob)item).Metadata["Expires"];
				if (!string.IsNullOrWhiteSpace(text) && DateTime.TryParse(text, out var result))
				{
					result = result.ToUniversalTime();
					if (result < DateTime.UtcNow)
					{
						((CloudBlob)item).DeleteIfExists((DeleteSnapshotsOption)0, (AccessCondition)null, (BlobRequestOptions)null, (OperationContext)null);
						num++;
					}
				}
			}
			mostSpecificInstance.SendTraceTag("00001IY", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "{0} file(s) cleaned from blob storage container.", num);
		}
		catch (Exception ex)
		{
			traceScope.SetResult(ex);
			throw new NavNCLDocumentServiceException(string.Format(CultureInfo.CurrentCulture, Lang.AzureBlobStorageCleanFailure, ContainerName), ex);
		}
	}

	public bool DeleteFile(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name");
		}
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using ITraceScope traceScope = mostSpecificInstance.CreateTraceScope("00001JB", DiagnosticParameter.SystemMetadata("DeleteFile"), MonitorType.None, Category.Runtime);
		try
		{
			mostSpecificInstance.SendTraceTag("00001IZ", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Deleting file from the {0} storage container.", ContainerName);
			CloudBlockBlob val = GetBlockBlobReference(name);
			return ((CloudBlob)val).DeleteIfExists((DeleteSnapshotsOption)0, (AccessCondition)null, (BlobRequestOptions)null, (OperationContext)null);
		}
		catch (Exception ex)
		{
			traceScope.SetResult(ex);
			throw new NavNCLDocumentServiceException(Lang.AzureBlobStorageDeletionFailure, ex);
		}
	}

	public void GetFile(string fileName, Stream stream)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using ITraceScope traceScope = mostSpecificInstance.CreateTraceScope("00001JC", DiagnosticParameter.SystemMetadata("GetFile"), MonitorType.None, Category.Runtime);
		try
		{
			mostSpecificInstance.SendTraceTag("00001J0", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Fetching file from the {0} storage container.", ContainerName);
			CloudBlockBlob val = GetBlockBlobReference(fileName);
			((CloudBlob)val).DownloadToStream(stream, (AccessCondition)null, (BlobRequestOptions)null, (OperationContext)null);
			stream.Seek(0L, SeekOrigin.Begin);
			mostSpecificInstance.SendTraceTag("00001J1", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Received file from the container. Size: {0} bytes.", stream.Length);
		}
		catch (Exception ex)
		{
			traceScope.SetResult(ex);
			throw new NavNCLDocumentServiceException(Lang.AzureBlobStorageGetFileFailure, ex);
		}
	}

	public Uri GetSharedAccessUrlForFile(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using ITraceScope traceScope = mostSpecificInstance.CreateTraceScope("00001JD", DiagnosticParameter.SystemMetadata("GetSharedAccessUrlForFile"), MonitorType.None, Category.Runtime);
		try
		{
			mostSpecificInstance.SendTraceTag("00001J2", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Getting shared access signature for temporary file.");
			CloudBlockBlob val = GetBlockBlobReference(fileName);
			if (!((CloudBlob)val).Exists((BlobRequestOptions)null, (OperationContext)null))
			{
				throw new InvalidOperationException(Lang.AzureBlobStorageFileNotFound);
			}
			if (!((CloudBlob)val).Metadata.TryGetValue("Expires", out var value) || !DateTimeOffset.TryParse(value, out var result))
			{
				result = DateTime.UtcNow.AddMinutes(30.0);
			}
			else if (result < DateTime.UtcNow)
			{
				throw new InvalidOperationException(Lang.AzureBlobStorageFileExpired);
			}
			string text = GetSharedAccessSignature(val, result);
			mostSpecificInstance.SendTraceTag("00001J3", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Shared access signature created.");
			return new Uri(((CloudBlob)val).StorageUri.PrimaryUri.ToString() + text.ToString());
		}
		catch (Exception ex)
		{
			traceScope.SetResult(ex);
			throw new NavNCLDocumentServiceException(Lang.AzureBlobStorageCreateSASFailure, ex);
		}
	}

	public Guid StoreFile(byte[] content, int minutesToExpire = 30)
	{
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		if (minutesToExpire < 1)
		{
			throw new NavNCLArgumentOutOfRangeException("minutesToExpire");
		}
		using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(content, 0, content.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return StoreFile(memoryStream, minutesToExpire);
	}

	public Guid StoreFile(Stream stream, int minutesToExpire = 30)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (minutesToExpire < 1)
		{
			throw new NavNCLArgumentOutOfRangeException("minutesToExpire");
		}
		TimeSpan duration = TimeSpan.FromMinutes(minutesToExpire);
		return StoreFile(stream, duration);
	}

	internal Guid StoreFile(Stream stream, TimeSpan duration)
	{
		Guid result = Guid.NewGuid();
		DateTime dateTime = DateTime.UtcNow.Add(duration);
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using ITraceScope traceScope = mostSpecificInstance.CreateTraceScope("00001JE", DiagnosticParameter.SystemMetadata("StoreFile"), MonitorType.None, Category.Runtime);
		try
		{
			mostSpecificInstance.SendTraceTag("00001J4", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Storing temporary file in Azure Blob Storage. Size: {0} bytes.", stream.Length);
			CloudBlockBlob val = GetBlockBlobReference(result.ToString());
			string text = dateTime.ToString("o", CultureInfo.InvariantCulture);
			mostSpecificInstance.SendTraceTag("00001J5", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Setting expiry metadata on the file for {0}.", text);
			((CloudBlob)val).Metadata.Add("Expires", text);
			val.UploadFromStream(stream, (AccessCondition)null, (BlobRequestOptions)null, (OperationContext)null);
			return result;
		}
		catch (Exception ex)
		{
			traceScope.SetResult(ex);
			throw new NavNCLDocumentServiceException(Lang.AzureBlobStorageStoreFileFailure, ex);
		}
	}

	private static CloudBlobContainer InitializeBlobContainer(string accountName, string accountKey, string containerName)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using ITraceScope traceScope = mostSpecificInstance.CreateTraceScope("00001JF", DiagnosticParameter.SystemMetadata("InitializeBlobContainer"), MonitorType.None, Category.Runtime);
		try
		{
			mostSpecificInstance.SendTraceTag("00001J6", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Initializing Azure Blob Storage container.");
			CloudStorageAccount val = CloudStorageAccount.Parse(string.Format(CultureInfo.InvariantCulture, "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountName, accountKey));
			mostSpecificInstance.SendTraceTag("00001J7", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Azure cloud storage account found.");
			CloudBlobClient val2 = val.CreateCloudBlobClient();
			CloudBlobContainer containerReference = val2.GetContainerReference(containerName);
			if (!containerReference.Exists((BlobRequestOptions)null, (OperationContext)null))
			{
				mostSpecificInstance.SendTraceTag("00001J8", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Creating blob container named {0}.", containerName);
				containerReference.CreateIfNotExists((BlobRequestOptions)null, (OperationContext)null);
			}
			mostSpecificInstance.SendTraceTag("00001J9", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Blob container reference created.");
			return containerReference;
		}
		catch (Exception ex)
		{
			traceScope.SetResult(ex);
			throw new NavNCLDocumentServiceException(string.Format(CultureInfo.CurrentCulture, Lang.AzureBlobStorageInitializationFailure, containerName), ex);
		}
	}
}
