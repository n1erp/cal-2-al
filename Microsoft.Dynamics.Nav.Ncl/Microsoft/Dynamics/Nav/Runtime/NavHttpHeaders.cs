using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavHttpHeaders : NavValue
{
	public static NavHttpHeaders Default => new NavHttpHeaders(new HttpRequestMessage().Headers);

	internal HttpHeaders Headers { get; set; }

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public override NavNclType NclType => NavNclType.NavHttpHeaders;

	public bool ALAdd(DataError errorLevel, string key, string value)
	{
		return Add(errorLevel, key, value);
	}

	public bool ALTryAddWithoutValidation(DataError errorLevel, string key, string value)
	{
		return TryAddWithoutValidation(errorLevel, key, value);
	}

	public bool ALContains(string key)
	{
		return Contains(key);
	}

	public void ALClear()
	{
		Clear();
	}

	public bool ALRemove(DataError errorLevel, string key)
	{
		return Remove(errorLevel, key);
	}

	public bool ALGetValues(DataError errorLevel, string key, NavArray<NavText> resultValues)
	{
		return GetValues(errorLevel, key, resultValues);
	}

	internal NavHttpHeaders(HttpHeaders headers)
	{
		Headers = headers;
	}

	public override int GetHashCode()
	{
		return Headers?.GetHashCode() ?? 0;
	}

	private bool Add(DataError errorLevel, string key, string value)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpHeaders, () =>
		{
			Headers.Add(key, value);
		});
	}

	private bool TryAddWithoutValidation(DataError errorLevel, string key, string value)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpHeaders, () => Headers.TryAddWithoutValidation(key, value));
	}

	private bool Contains(string key)
	{
		try
		{
			return Headers.Contains(key);
		}
		catch (Exception ex)
		{
			throw new NavNclInvalidHeaderException(ex.Message);
		}
	}

	private void Clear()
	{
		Headers.Clear();
	}

	private bool Remove(DataError errorLevel, string key)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpHeaders, () =>
		{
			Headers.Remove(key);
		});
	}

	private bool GetValues(DataError errorLevel, string key, NavArray<NavText> resultValues)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpHeaders, () =>
		{
			if (Headers.TryGetValues(key, out var values))
			{
				ImmutableArray<string> immutableArray = values.ToImmutableArray();
				NavArray<NavText> navArray = new NavArray<NavText>(new NavText(string.Empty), resultValues.Length);
				if (immutableArray.Length > resultValues.Length)
				{
					throw new NavNCLArrayLengthMismatchException(immutableArray.Length, resultValues.Length);
				}
				for (int i = 0; i < immutableArray.Length; i++)
				{
					navArray[new int[1] { i }] = new NavText(immutableArray[i]);
				}
				ALSystemArray.ALCopyArray(resultValues, navArray, 1, navArray.Length);
				return;
			}
			throw new NavNclInvalidHeaderException();
		});
	}
}
