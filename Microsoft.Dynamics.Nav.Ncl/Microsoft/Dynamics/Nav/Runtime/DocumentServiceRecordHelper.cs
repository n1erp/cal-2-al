using System;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class DocumentServiceRecordHelper
{
	private readonly IDocumentStorageService documentStorageService;

	public DocumentServiceRecordHelper()
	{
		documentStorageService = NavCurrentThread.Session.DocumentStorageService;
	}

	public static bool Enabled()
	{
		return NavCurrentThread.Session.DocumentStorageService?.Enabled() ?? false;
	}

	internal string SaveDocumentStream(Stream dataStream, string filename, Func<string, string> getFilename)
	{
		if (dataStream == null)
		{
			NavFile.ALErase(DataError.TrapError, getFilename(filename));
			return string.Empty;
		}
		if (!documentStorageService.Enabled())
		{
			throw new NavNCLDocumentServiceException(Lang.DocumentServiceNotConfigured);
		}
		return documentStorageService.Save(dataStream, getFilename(filename), overwrite: false);
	}
}
