using System.Globalization;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class ErrorInfo
{
	public string File { get; private set; }

	public string Message { get; private set; }

	public int LineNo { get; private set; }

	public int LinePos { get; private set; }

	public ErrorInfo(string file, string message, int lineNo, int linePos)
	{
		File = file;
		Message = message;
		LineNo = lineNo;
		LinePos = linePos;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, NonLocalizable.ReadError, File, LineNo, LinePos, Message);
	}
}
