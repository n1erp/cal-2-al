using System;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class OfficeDocumentServiceDownloadHandler : IOfficeDownloadHandler
{
	private readonly DocumentServiceRecordHelper documentStorageService;

	public OfficeDocumentServiceDownloadHandler(DocumentServiceRecordHelper service)
	{
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		documentStorageService = service;
	}

	public string Download(Stream documentStream, string filename)
	{
		if (documentStream == null)
		{
			throw new ArgumentNullException("documentStream");
		}
		if (string.IsNullOrEmpty(filename))
		{
			throw new ArgumentException(Lang._00095_50043);
		}
		if (documentStream.CanSeek)
		{
			documentStream.Seek(0L, SeekOrigin.Begin);
		}
		string text = documentStorageService.SaveDocumentStream(documentStream, filename, GetFileName);
		if (!string.IsNullOrEmpty(text))
		{
			ALSystemOperatingSystem.ALHyperlink(text, Guid.NewGuid());
		}
		return text;
	}

	private string GetFileName(string fileName)
	{
		return fileName;
	}
}
