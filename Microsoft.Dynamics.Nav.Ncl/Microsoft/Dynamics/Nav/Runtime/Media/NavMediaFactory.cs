using System;
using System.Drawing;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaFactory
{
	public static NavMediaBase ProcessMediaObject(string mediaFilename)
	{
		if (mediaFilename == null)
		{
			throw new ArgumentNullException("mediaFilename");
		}
		FileStream fileStream = null;
		try
		{
			try
			{
				fileStream = File.Open(mediaFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			catch (Exception ex)
			{
				NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: false);
				throw;
			}
			return ProcessMediaObject(fileStream, saveStream: true, NavMimeMapping.GetMimeMapping(mediaFilename));
		}
		finally
		{
			fileStream?.Dispose();
		}
	}

	public static NavMediaBase ProcessMediaObject(byte[] blob, string mimeType = "")
	{
		if (blob == null)
		{
			throw new ArgumentNullException("blob");
		}
		MemoryStream mediaStream = new MemoryStream(blob);
		return ProcessMediaObject(mediaStream, saveStream: false, mimeType);
	}

	public static NavMediaBase ProcessMediaObject(Stream mediaStream, bool saveStream, string mimeType = "")
	{
		if (string.IsNullOrEmpty(mimeType))
		{
			try
			{
				Image imageObject = Image.FromStream(mediaStream);
				return saveStream ? new NavMediaImage(imageObject, mediaStream) : new NavMediaImage(imageObject);
			}
			catch (ArgumentException)
			{
				mimeType = "application/octet-stream";
			}
		}
		if (NavMediaImageTypes.IsSupportedMimeType(mimeType))
		{
			return new NavMediaImage(mediaStream, saveStream: false);
		}
		if (NavMediaPdfDocument.IsSupportedMimeType(mimeType))
		{
			return new NavMediaPdfDocument(mediaStream);
		}
		if (NavMediaWordDocument.IsSupportedMimeType(mimeType))
		{
			return new NavMediaWordDocument(mediaStream, mimeType);
		}
		if (NavMediaExcelDocument.IsSupportedMimeType(mimeType))
		{
			return new NavMediaExcelDocument(mediaStream, mimeType);
		}
		if (NavMediaPowerPointDocument.IsSupportedMimeType(mimeType))
		{
			return new NavMediaPowerPointDocument(mediaStream, mimeType);
		}
		if (NavMediaOneNoteDocument.IsSupportedMimeType(mimeType))
		{
			return new NavMediaOneNoteDocument(mediaStream, mimeType);
		}
		if (NavMediaTextDocument.IsSupportedMimeType(mimeType))
		{
			return new NavMediaTextDocument(mediaStream, mimeType);
		}
		return new NavMediaBinaryFile(mediaStream, mimeType);
	}
}
