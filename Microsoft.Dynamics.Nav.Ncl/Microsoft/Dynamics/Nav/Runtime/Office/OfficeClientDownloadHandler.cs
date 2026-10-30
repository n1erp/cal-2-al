using System;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class OfficeClientDownloadHandler : IOfficeDownloadHandler
{
	private readonly ITreeObject parent;

	internal OfficeClientDownloadHandler(ITreeObject parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		this.parent = parent;
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
		using NavInStream source = new NavInStream(parent, new NavStreamProvider(documentStream));
		string fileName = Path.GetFileName(filename);
		NavText toFileNameText = new NavText(fileName);
		ByRef<NavText> toFileName = new ByRef<NavText>(() => toFileNameText, (NavText value) =>
		{
			toFileNameText = value;
		});
		NavFile.ALDownloadFromStream(source, "No dialog.", string.Empty, string.Empty, toFileName, Guid.NewGuid());
		return filename;
	}
}
