using System;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class TrappableHttpOperationExecutor
{
	internal enum ExecutionOrigin
	{
		HttpHeaders,
		HttpUri,
		HttpContent,
		HttpClient,
		HttpCertificate
	}

	internal static bool Execute(DataError errorLevel, ExecutionOrigin origin, Action operation)
	{
		return TrappableOperationExecutor.Execute(errorLevel, () =>
		{
			operation();
			return true;
		}, (Exception exception, DataError error) =>
		{
			HandleExceptions(error, origin, exception);
		});
	}

	internal static bool Execute(DataError errorLevel, ExecutionOrigin origin, Func<bool> operation)
	{
		return TrappableOperationExecutor.Execute(errorLevel, operation, (Exception exception, DataError error) =>
		{
			HandleExceptions(error, origin, exception);
		});
	}

	private static void HandleExceptions(DataError errorLevel, ExecutionOrigin origin, Exception exception)
	{
		if (errorLevel == DataError.ThrowError)
		{
			LogWriterHelper.LogExceptionEvent(exception);
		}
		(string, Exception) exceptionInformation = GetExceptionInformation(exception);
		if (!(exceptionInformation.Item2 is NavNCLException))
		{
			switch (origin)
			{
			case ExecutionOrigin.HttpContent:
				throw new NavNclHttpContentInvalidAccessException(exceptionInformation.Item1, exceptionInformation.Item2);
			case ExecutionOrigin.HttpHeaders:
				throw new NavNclInvalidHeaderException(exceptionInformation.Item1, exceptionInformation.Item2);
			case ExecutionOrigin.HttpUri:
				throw new NavNclCreateUrlException(exceptionInformation.Item1, exceptionInformation.Item2);
			case ExecutionOrigin.HttpClient:
				throw new NavNclHttpRequestException(exceptionInformation.Item1, exceptionInformation.Item2);
			case ExecutionOrigin.HttpCertificate:
				throw new NavNclHttpUserCertificateException(Lang.HttpClientInvalidUserCertificate, exceptionInformation.Item2);
			}
		}
		throw exceptionInformation.Item2;
	}

	private static (string Message, Exception InnerException) GetExceptionInformation(Exception exception)
	{
		string item = exception.Message;
		Exception item2 = exception;
		if (exception is AggregateException ex)
		{
			AggregateException ex2 = ex.Flatten();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(ex2.Message);
			foreach (Exception innerException in ex2.InnerExceptions)
			{
				stringBuilder.AppendLine(innerException.Message);
			}
			item2 = ex2;
			item = stringBuilder.ToString();
		}
		return (Message: item, InnerException: item2);
	}
}
