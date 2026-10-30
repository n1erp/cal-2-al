using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal interface IOfficeDownloadHandler
{
	string Download(Stream documentStream, string filename);
}
