using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

public static class NavMediaExport
{
	internal static int ExportMediaSetObjects(DataError errorLevel, int parentId, int parentCompanyToken, Guid mediaSet, string filePrefix)
	{
		if (mediaSet.Equals(Guid.Empty))
		{
			return 0;
		}
		int mediaSetTableId = NavMediaHelper.GetMediaSetTableId(parentId);
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaSetTableId);
		int num = 0;
		if (parentCompanyToken != -1)
		{
			string companyNameFromToken = NavCurrentThread.Session.Database.GetCompanyNameFromToken(parentCompanyToken);
			NavMediaHelper.SetCompanyfilter(navRecord, 3, companyNameFromToken);
		}
		NavGuid navGuid = new NavGuid(mediaSet);
		navRecord.ALSetFilter(1, navGuid.ToString());
		if (!navRecord.ALFind(DataError.TrapError, "-"))
		{
			return 0;
		}
		do
		{
			Guid mediaId = navRecord.GetFieldValue(2).ToGuid();
			if (ExportMediaObjectToFile(errorLevel, mediaId, filePrefix, num + 1))
			{
				num++;
			}
		}
		while (navRecord.ALNext() != 0);
		return num;
	}

	internal static bool ExportMediaObjectToFile(DataError errorLevel, Guid mediaId, string fileBaseName, int index)
	{
		if (string.IsNullOrEmpty(fileBaseName))
		{
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw new NavNCLMissingFileNameException();
		}
		string text;
		bool flag;
		using (NavFile navFile = new NavFile(NavCurrentThread.Session))
		{
			navFile.ALTextMode = false;
			navFile.ALWriteMode = true;
			try
			{
				if (index >= 0)
				{
					try
					{
						string path = Path.GetDirectoryName(fileBaseName) ?? string.Empty;
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileBaseName);
						string extension = Path.GetExtension(fileBaseName);
						text = Path.Combine(path, string.Format(CultureInfo.CurrentCulture, "{0}-{1}{2}", fileNameWithoutExtension, index, extension));
					}
					catch (PathTooLongException ex)
					{
						throw new NavNCLInvalidPathException(ex.Message);
					}
					catch (ArgumentException ex2)
					{
						throw new NavNCLInvalidPathException(ex2.Message);
					}
				}
				else
				{
					text = fileBaseName;
				}
				navFile.ALCreate(text);
			}
			catch (NavBaseException) when (errorLevel == DataError.TrapError)
			{
				return false;
			}
			using NavOutStream navOutStream = navFile.ALCreateOutStream(NavCurrentThread.Session);
			try
			{
				flag = ExportMediaObject(navOutStream.InternalStream, mediaId);
			}
			catch (NavBaseException)
			{
				if (errorLevel == DataError.ThrowError)
				{
					navFile.ALClose();
					NavFile.ALErase(errorLevel, text);
					throw;
				}
				flag = false;
			}
		}
		if (!flag)
		{
			NavFile.ALErase(DataError.TrapError, text);
		}
		return flag;
	}

	public static bool ExportMediaObject(Stream mediaStream, Guid alMediaId)
	{
		if (mediaStream == null)
		{
			return false;
		}
		bool allowCache = false;
		string mimeType = string.Empty;
		byte[] mediaContent = NavMediaHelper.GetMediaContent(alMediaId, ref allowCache, ref mimeType);
		if (mediaContent == null || mediaContent.Length == 0)
		{
			return false;
		}
		mediaStream.Write(mediaContent, 0, mediaContent.Length);
		return true;
	}
}
