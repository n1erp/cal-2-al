using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal static class NavMediaLinkHelper
{
	public const int SecurityTokenStringLengthV1 = 43;

	public static Guid ImportMediaObjectForUseInPublicLink(ITreeObject parent, Stream mediaStream, string fileName, DateTime expiration, bool triggerCleanup)
	{
		if (!(parent is NavSession navSession))
		{
			throw new NavNCLInvalidTypeException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidTypeConversion, parent.GetType(), typeof(NavSession)));
		}
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000184 }))
		{
			using NavRecordHandle navRecordHandle = new NavRecordHandle(parent, 2000000184, temporary: false, SecurityFiltering.Validated);
			using MemoryStream memoryStream = new MemoryStream();
			int numBytes = 32;
			Guid guid = Guid.NewGuid();
			string value = CreateSecurityToken(numBytes);
			string mimeTypeForUrlAccessibleMedia = GetMimeTypeForUrlAccessibleMedia(fileName);
			mediaStream.CopyTo(memoryStream);
			memoryStream.Position = 0L;
			string value2 = navSession.User?.Name ?? string.Empty;
			navRecordHandle.Target.ALInit();
			navRecordHandle.Target.SetFieldValue(1, new NavGuid(guid));
			navRecordHandle.Target.SetFieldValue(3, new NavBLOB(memoryStream.ToArray()));
			navRecordHandle.Target.SetFieldValue(7, new NavText(navRecordHandle.Target.GetFieldValue(7).NavDefinedLengthMetadata, navSession.CompanyName));
			navRecordHandle.Target.SetFieldValue(4, new NavText(navRecordHandle.Target.GetFieldValue(4).NavDefinedLengthMetadata, mimeTypeForUrlAccessibleMedia));
			navRecordHandle.Target.SetFieldValue(8, NavDateTime.Create(expiration));
			navRecordHandle.Target.SetFieldValue(11, new NavText(navRecordHandle.Target.GetFieldValue(11).NavDefinedLengthMetadata, fileName));
			navRecordHandle.Target.SetFieldValue(12, new NavText(navRecordHandle.Target.GetFieldValue(12).NavDefinedLengthMetadata, value));
			navRecordHandle.Target.SetFieldValue(13, new NavText(navRecordHandle.Target.GetFieldValue(13).NavDefinedLengthMetadata, value2));
			navRecordHandle.Target.ALInsert(DataError.ThrowError);
			if (triggerCleanup)
			{
				NavTaskSchedulerUtility.ScheduleSystemTask(SystemTaskActionId.DeleteExpiredFiles, new DeleteExpiredFilesSystemTask.DeleteExpiredFilesSystemTaskParameters(), expiration);
			}
			return guid;
		}
	}

	private static string CreateSecurityToken(int numBytes)
	{
		RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
		byte[] array = new byte[numBytes];
		randomNumberGenerator.GetBytes(array);
		string text = Convert.ToBase64String(array);
		return text.Replace('+', '-').Replace('/', '_').TrimEnd('=');
	}

	public static NavUrlAccessibleMedia GetUrlAccessibleMediaById(Guid mediaId, bool retrieveContent)
	{
		if (mediaId == Guid.Empty)
		{
			throw new NavMediaNotFoundException();
		}
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[2] { 2000000181, 2000000184 }))
		{
			using NavRecord navRecord = NavMediaHelper.GetMediaContentRecord(mediaId);
			if (navRecord == null)
			{
				throw new NavMediaNotFoundException();
			}
			string value = (NavText)navRecord.GetFieldValue(12);
			NavDateTime navDateTime = (NavDateTime)navRecord.GetFieldValue(8);
			if (string.IsNullOrWhiteSpace(value) || navDateTime.ToDateTime() < DateTime.UtcNow)
			{
				throw new NavMediaNotFoundException();
			}
			byte[] content = null;
			if (retrieveContent)
			{
				navRecord.ALCalcFields(DataError.ThrowError, 3);
				NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(3);
				content = navBLOB.GetBytes();
			}
			string fileName = (NavText)navRecord.GetFieldValue(11);
			return new NavUrlAccessibleMedia
			{
				MediaId = navRecord.GetFieldValue(1).ToGuid(),
				Content = content,
				MimeType = (NavText)navRecord.GetFieldValue(4),
				Expiration = (NavDateTime)navRecord.GetFieldValue(8),
				FileName = fileName,
				SecurityToken = (NavText)navRecord.GetFieldValue(12)
			};
		}
	}

	public static NavUrlAccessibleMedia GetUrlAccessibleMediaByUri(Uri uri)
	{
		string text = HttpUtility.ParseQueryString(uri.Query).Get("token");
		if (!IsSecurityTokenInExpectedFormat(text))
		{
			throw new NavMediaNotFoundException();
		}
		NavTenant tenantForRequest = GetTenantForRequest(uri);
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000184);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(1);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(4);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(8);
		NCLMetaField fieldByNo5 = metaTableById.GetFieldByNo(11);
		NCLMetaField fieldByNo6 = metaTableById.GetFieldByNo(12);
		FilterExpression filter = new UnaryFilterExpression(FilterExpressionType.Equal, NavText.CreateFromObject(text), fieldByNo6.ExpressionContext);
		NCLMetaTable table = metaTableById;
		ReadOnlyRecordBuffer readOnlyRecordBuffer = NavSqlIsamQueryCommand.FindTop1(tenantForRequest.Database, table, 0, 91, locking: false, onlyCurrentKeyFieldsNeeded: false, metaTableById.PrimaryKey.DefinedSortingFields, filter);
		if (readOnlyRecordBuffer == null || readOnlyRecordBuffer[fieldByNo4.FieldIndex].ToDateTime() < DateTime.UtcNow)
		{
			throw new NavMediaNotFoundException();
		}
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(readOnlyRecordBuffer);
		if (NavSqlBlobCommand.GetBlob(metaTableById, tenantForRequest.Database, 0, new FieldList(fieldByNo2), mutableRecordBuffer, locking: false) == 85132272)
		{
			throw new NavMediaNotFoundException();
		}
		NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer[fieldByNo2.FieldIndex];
		byte[] bytes = navBLOB.GetBytes();
		string fileName = (NavText)readOnlyRecordBuffer[fieldByNo5.FieldIndex];
		return new NavUrlAccessibleMedia
		{
			MediaId = readOnlyRecordBuffer[fieldByNo.FieldIndex].ToGuid(),
			Content = bytes,
			MimeType = (NavText)readOnlyRecordBuffer[fieldByNo3.FieldIndex],
			Expiration = (NavDateTime)readOnlyRecordBuffer[fieldByNo4.FieldIndex],
			FileName = fileName,
			SecurityToken = text
		};
	}

	private static NavTenant GetTenantForRequest(Uri uri)
	{
		NavTenant navTenant = NavEnvironment.Instance.Tenants.GetTenantByUri(uri);
		if (navTenant == null)
		{
			if (!NavEnvironment.Instance.Tenants.IsSingleTenancyMode)
			{
				throw new NavTenantNotFoundException();
			}
			navTenant = NavEnvironment.Instance.Tenants.DefaultTenant;
		}
		return navTenant;
	}

	private static string GetMimeTypeForUrlAccessibleMedia(string fileName)
	{
		return NavMimeMapping.GetMimeMapping(fileName);
	}

	public static bool IsSecurityTokenInExpectedFormat(string securityToken)
	{
		return !string.IsNullOrWhiteSpace(securityToken) && securityToken.Length == 43;
	}
}
