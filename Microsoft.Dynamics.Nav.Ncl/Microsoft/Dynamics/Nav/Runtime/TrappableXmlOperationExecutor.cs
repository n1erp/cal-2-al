using System;
using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class TrappableXmlOperationExecutor
{
	public static bool ExecuteXmlOperation(DataError errorLevel, Action operation)
	{
		return TrappableOperationExecutor.Execute(errorLevel, () =>
		{
			operation();
			return true;
		}, (Exception exception, DataError error) =>
		{
			if (error == DataError.ThrowError)
			{
				LogWriterHelper.LogExceptionEvent(exception);
			}
			throw MapNativeException(exception);
		});
	}

	public static T ExecuteXmlOperation<T>(Func<T> factory)
	{
		try
		{
			return factory();
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static bool RaiseXmlNodeNotFound(DataError errorLevel)
	{
		if (errorLevel == DataError.ThrowError)
		{
			throw new NavNCLXmlNodeNotFoundException();
		}
		return false;
	}

	public static bool RaiseMemberNotInitialized(DataError errorLevel)
	{
		if (errorLevel == DataError.ThrowError)
		{
			throw new NavNCLMemberNotInitializedException();
		}
		return false;
	}

	public static bool RaiseInvalidOperation(DataError errorLevel)
	{
		if (errorLevel == DataError.ThrowError)
		{
			throw new NavNCLInvalidOperationException();
		}
		return false;
	}

	public static bool RaiseNotSupportedOperation(DataError errorLevel)
	{
		if (errorLevel == DataError.ThrowError)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		return false;
	}

	private static Exception MapNativeException(Exception exception)
	{
		if (exception is ArgumentException || exception is ArgumentNullException || exception is InvalidCastException)
		{
			return new NavNCLArgumentException();
		}
		if (exception is InvalidOperationException)
		{
			return new NavNCLInvalidOperationException();
		}
		if (exception is XmlException ex)
		{
			return new NavNCLXmlException(ex.Message, ex.LineNumber, ex.LinePosition);
		}
		return exception;
	}
}
