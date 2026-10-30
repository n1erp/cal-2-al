using System;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class OfficeDownloadHandlerFactory
{
	internal static IOfficeDownloadHandler Create(ITreeObject parent)
	{
		if (DocumentServiceRecordHelper.Enabled())
		{
			DocumentServiceRecordHelper service = new DocumentServiceRecordHelper();
			return new OfficeDocumentServiceDownloadHandler(service);
		}
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		return new OfficeClientDownloadHandler(parent);
	}
}
