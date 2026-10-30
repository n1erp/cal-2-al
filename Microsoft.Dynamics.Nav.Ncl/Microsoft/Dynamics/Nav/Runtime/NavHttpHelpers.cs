using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavHttpHelpers
{
	internal static HttpContent Clone(this HttpContent content)
	{
		return NavThread.RunExternalAction(() => CloneContent(content).WaitThrowingOriginalException());
	}

	private static async Task<HttpContent> CloneContent(HttpContent content)
	{
		if (content == null)
		{
			return null;
		}
		HttpContent httpContent = await CloneContentCore(content);
		foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
		{
			httpContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
		}
		return httpContent;
	}

	private static async Task<HttpContent> CloneContentCore(HttpContent content)
	{
		MemoryStream ms = new MemoryStream();
		await content.CopyToAsync(ms).ConfigureAwait(continueOnCapturedContext: false);
		ms.Position = 0L;
		return new StreamContent(ms);
	}
}
