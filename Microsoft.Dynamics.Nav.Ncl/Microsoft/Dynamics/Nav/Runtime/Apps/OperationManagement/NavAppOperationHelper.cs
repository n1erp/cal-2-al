using System;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal static class NavAppOperationHelper
{
	public static string GetOperationKey(this INavAppDeployOperationKey operationKey)
	{
		return GetKey(operationKey.TenantId, operationKey.AppId, null);
	}

	public static string GetOperationKey(this INavAppVersionedDeployOperationKey operationKey)
	{
		return GetKey(operationKey.TenantId, operationKey.AppId, operationKey.Version);
	}

	private static string GetKey(string tenantId, Guid? appId, Version version)
	{
		return tenantId?.ToUpperInvariant() + appId?.ToString("N") + version;
	}

	public static NavAppTenantOperationFields.NavAppOperationStatus ToOperationStatus(this NavAppOperationResultStatus resultStatus)
	{
		NavAppTenantOperationFields.NavAppOperationStatus result = NavAppTenantOperationFields.NavAppOperationStatus.Unknown;
		switch (resultStatus)
		{
		case NavAppOperationResultStatus.Failure:
			result = NavAppTenantOperationFields.NavAppOperationStatus.Failed;
			break;
		case NavAppOperationResultStatus.Success:
			result = NavAppTenantOperationFields.NavAppOperationStatus.Completed;
			break;
		}
		return result;
	}

	public static NavAppOperationRequestReference CreateDeployTargetReference(NavAppTenantOperationFields.NavAppOperationStatus status, string details, string description, NavAppDeployTargetOperationMetadata metadata)
	{
		return CreateReference(status, details, description, metadata, NavAppTenantOperationFields.NavAppOperationType.DeployTarget);
	}

	public static NavAppOperationRequestReference CreateDeployPackageReference(NavAppTenantOperationFields.NavAppOperationStatus status, string details, string description, NavAppDeployPackageOperationMetadata metadata)
	{
		return CreateReference(status, details, description, metadata, NavAppTenantOperationFields.NavAppOperationType.DeployPackage);
	}

	private static NavAppOperationRequestReference CreateReference(NavAppTenantOperationFields.NavAppOperationStatus status, string details, string description, NavAppOperationMetadata metadata, NavAppTenantOperationFields.NavAppOperationType type)
	{
		return new NavAppOperationRequestReference
		{
			Status = status,
			Details = details,
			MetadataVersion = NavAppOperationMetadataVersion.Initial,
			Metadata = metadata,
			OperationType = type,
			Description = description
		};
	}

	public static string DeployTargetDescription(this Guid appId)
	{
		return Lang.NavAppDeployTargetOperationDescription.FormatWithResolvedCulture(appId);
	}

	public static string UploadDescription(this INavAppReference appInfo)
	{
		return Lang.NavAppUploadOperationDescription.FormatWithResolvedCulture(NavAppReferenceExtensions.FriendlyDisplayNameWithQuotes(appInfo));
	}

	public static string DisplayString(this DeploymentSchedule schedule)
	{
		string result = string.Empty;
		switch (schedule)
		{
		case DeploymentSchedule.Current:
			result = Lang.NavAppPublishScheduleImmediate.FormatWithResolvedCulture();
			break;
		case DeploymentSchedule.NextMajorUpdate:
			result = Lang.NavAppPublishScheduleNextMajorUpdate.FormatWithResolvedCulture();
			break;
		case DeploymentSchedule.NextMinorUpdate:
			result = Lang.NavAppPublishScheduleNextMinorUpdate.FormatWithResolvedCulture();
			break;
		}
		return result;
	}

	public static NavAppTenantOperationRecord DeserializeOperationRecord(this NavSqlCommand command)
	{
		NavAppTenantOperationRecord navAppTenantOperationRecord = new NavAppTenantOperationRecord
		{
			OperationId = command.Reader.GetGuid(1),
			StartedOn = command.Reader.GetDateTime(2),
			OperationType = (NavAppTenantOperationFields.NavAppOperationType)command.Reader.GetInt32(3),
			Status = (NavAppTenantOperationFields.NavAppOperationStatus)command.Reader.GetInt32(4),
			Details = command.ReadAndDeserializeBlobString(5),
			MetadataVersion = (NavAppOperationMetadataVersion)command.Reader.GetInt32(6)
		};
		navAppTenantOperationRecord.Metadata = command.ReadAndDeserializeMetadata(7, navAppTenantOperationRecord.OperationType, navAppTenantOperationRecord.MetadataVersion);
		navAppTenantOperationRecord.Description = command.Reader.GetString(9);
		return navAppTenantOperationRecord;
	}

	public static string ReadAndDeserializeBlobString(this NavSqlCommand command, int ordinal)
	{
		using NavBLOB navBLOB = new NavBLOB();
		command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
		using StreamReader streamReader = new StreamReader(navBLOB.GetNewStream(), Encoding.UTF8);
		return streamReader.ReadToEnd();
	}

	public static object SerializeBlobString(this string content)
	{
		object result = DBNull.Value;
		if (content != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			using NavBLOB navBLOB = NavBLOB.CreateFromObject(bytes);
			result = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
		}
		return result;
	}

	public static NavAppOperationMetadata ReadAndDeserializeMetadata(this NavSqlCommand command, int ordinal, NavAppTenantOperationFields.NavAppOperationType type, NavAppOperationMetadataVersion metadataVersion)
	{
		NavAppOperationMetadata result = null;
		using (NavBLOB navBLOB = new NavBLOB())
		{
			command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
			using Stream metadataStream = navBLOB.GetNewStream();
			switch (type)
			{
			case NavAppTenantOperationFields.NavAppOperationType.DeployTarget:
				result = DeserializeDeployTargetMetadata(metadataStream, metadataVersion);
				break;
			case NavAppTenantOperationFields.NavAppOperationType.DeployPackage:
				result = DeserializeDeployPackageMetadata(metadataStream, metadataVersion);
				break;
			}
		}
		return result;
	}

	public static NavAppDeployTargetOperationMetadata DeserializeDeployTargetMetadata(Stream metadataStream, NavAppOperationMetadataVersion metadataVersion)
	{
		return DeserializeDeployMetadata<NavAppDeployTargetOperationMetadata>(metadataStream, metadataVersion);
	}

	public static NavAppDeployPackageOperationMetadata DeserializeDeployPackageMetadata(Stream metadataStream, NavAppOperationMetadataVersion metadataVersion)
	{
		return DeserializeDeployMetadata<NavAppDeployPackageOperationMetadata>(metadataStream, metadataVersion);
	}

	private static T DeserializeDeployMetadata<T>(Stream metadataStream, NavAppOperationMetadataVersion metadataVersion) where T : NavAppDeployOperationMetadata
	{
		if (metadataVersion == NavAppOperationMetadataVersion.Initial)
		{
			using (StreamReader streamReader = new StreamReader(metadataStream))
			{
				string value = streamReader.ReadToEnd();
				return JsonConvert.DeserializeObject<T>(value);
			}
		}
		return null;
	}

	public static object SerializeMetadata(this NavAppOperationRequestReference operation)
	{
		object result = DBNull.Value;
		NavAppTenantOperationFields.NavAppOperationType operationType = operation.OperationType;
		if ((uint)operationType <= 1u)
		{
			result = SerializeDeployMetadata(operation.Metadata as NavAppDeployOperationMetadata);
		}
		return result;
	}

	public static object SerializeDeployMetadata(NavAppDeployOperationMetadata metadata)
	{
		object result = DBNull.Value;
		string text = JsonConvert.SerializeObject(metadata);
		if (text != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			using NavBLOB navBLOB = NavBLOB.CreateFromObject(bytes);
			result = navBLOB.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
		}
		return result;
	}
}
