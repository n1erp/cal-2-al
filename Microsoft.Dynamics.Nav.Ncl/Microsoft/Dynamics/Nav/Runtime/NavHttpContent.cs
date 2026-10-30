using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavHttpContent : NavComplexValue
{
	private HttpContent content;

	private NavHttpHeaders headers;

	internal HttpContent Content
	{
		get
		{
			return content;
		}
		set
		{
			if (content != value)
			{
				content?.Dispose();
				content = value;
				UpdateHeaders(value);
			}
		}
	}

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public override NavNclType NclType => NavNclType.NavHttpContent;

	public void ALClear()
	{
		Clear();
	}

	public bool ALGetHeaders(DataError errorLevel, ByRef<NavHttpHeaders> headersReference)
	{
		return GetHeaders(errorLevel, headersReference);
	}

	public bool ALReadAs(DataError errorLevel, ByRef<NavText> text)
	{
		return ReadAs(errorLevel, text);
	}

	public bool ALReadAs(DataError errorLevel, ByRef<NavInStream> inStream)
	{
		return ReadAs(errorLevel, inStream);
	}

	public bool ALReadAs(ITreeObject parent, DataError errorLevel, ByRef<NavInStream> inStream)
	{
		return ReadAs(errorLevel, inStream);
	}

	public void ALLoadFrom(NavText text)
	{
		LoadFrom(text);
	}

	public void ALLoadFrom(NavInStream stream)
	{
		LoadFrom(stream);
	}

	public void ALAssign(NavHttpContent other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (this != other)
		{
			Dispose();
			Content = other.Content.Clone();
		}
	}

	public NavHttpContent(ITreeObject parent)
		: this(parent, string.Empty)
	{
	}

	internal NavHttpContent(ITreeObject parent, string content)
		: this(parent, new StringContent(content))
	{
	}

	internal NavHttpContent(ITreeObject parent, HttpContent content)
		: base(parent)
	{
		Content = content;
	}

	public override int GetHashCode()
	{
		return Content?.GetHashCode() ?? 0;
	}

	public override void Clear()
	{
		Dispose();
		Content = new StringContent(string.Empty);
	}

	private bool GetHeaders(DataError errorLevel, ByRef<NavHttpHeaders> headersReference)
	{
		if (headers == null)
		{
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw new NavNCLNotInitializedException(GetType());
		}
		headersReference.Value = headers;
		return true;
	}

	private bool ReadAs(DataError errorLevel, ByRef<NavText> text)
	{
		if (Content == null)
		{
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw new NavNCLNotInitializedException(GetType());
		}
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpContent, () =>
		{
			NavThread.RunExternalAction(() =>
			{
				string value = Content.ReadAsStringAsync().WaitThrowingOriginalException();
				text.Value = new NavText(text.Value.MaxLength, value);
			});
		});
	}

	private bool ReadAs(DataError errorLevel, ByRef<NavInStream> stream)
	{
		if (Content == null)
		{
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw new NavNCLNotInitializedException(GetType());
		}
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpContent, () =>
		{
			NavThread.RunExternalAction(() =>
			{
				Stream stream2 = MemoryStreamManager.GetStream();
				Content.ReadAsStreamAsync().WaitThrowingOriginalException().CopyTo(stream2);
				stream2.Position = 0L;
				stream.Value = new NavInStream(stream.Value.Tree.Parent, new NavStreamProvider(stream2, stream.Value.Tree.Parent, disposeStream: true));
			});
		});
	}

	private void LoadFrom(NavText text)
	{
		Content = new StringContent(text);
	}

	private void LoadFrom(NavInStream stream)
	{
		Content = new StreamContent(stream.InternalStream);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			content?.Dispose();
			content = null;
			headers = null;
		}
		base.Dispose(disposing);
	}

	private void UpdateHeaders(HttpContent newContent)
	{
		if (newContent == null)
		{
			headers = null;
			return;
		}
		if (headers == null)
		{
			headers = new NavHttpHeaders(newContent.Headers);
			return;
		}
		HttpContentHeaders httpContentHeaders = newContent.Headers;
		foreach (KeyValuePair<string, IEnumerable<string>> header in headers.Headers)
		{
			if (!httpContentHeaders.Contains(header.Key))
			{
				if (header.Value.Count() == 1)
				{
					httpContentHeaders.TryAddWithoutValidation(header.Key, header.Value.First());
				}
				else
				{
					httpContentHeaders.TryAddWithoutValidation(header.Key, header.Value);
				}
			}
		}
		headers.Headers = httpContentHeaders;
	}
}
