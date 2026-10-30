using System.Net;

namespace Microsoft.Dynamics.Nav.Runtime.Data.Graph;

internal class WebExceptionHandler
{
	public static bool IsTransientException(WebException webException)
	{
		if (webException.Response is HttpWebResponse { StatusCode: var statusCode } && (statusCode == HttpStatusCode.InternalServerError || (uint)(statusCode - 502) <= 2u))
		{
			return true;
		}
		WebExceptionStatus status = webException.Status;
		if (status == WebExceptionStatus.ConnectionClosed || status == WebExceptionStatus.KeepAliveFailure || status == WebExceptionStatus.Timeout)
		{
			return true;
		}
		return false;
	}
}
