using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class TrappableOperationExecutor
{
	public static bool Execute(DataError errorLevel, Func<bool> operation, Action<Exception, DataError> processNativeException = null)
	{
		try
		{
			try
			{
				return operation();
			}
			catch (Exception ex)
			{
				if (errorLevel == DataError.ThrowError)
				{
					LogWriterHelper.LogExceptionEvent(ex);
				}
				processNativeException?.Invoke(ex, errorLevel);
				throw;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public static bool Execute(DataError errorLevel, Action operation, Func<Exception, Exception> mapNativeException)
	{
		try
		{
			try
			{
				operation();
				return true;
			}
			catch (Exception ex)
			{
				if (errorLevel == DataError.ThrowError)
				{
					LogWriterHelper.LogExceptionEvent(ex);
				}
				throw mapNativeException(ex);
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	public static T ExecuteFactory<T>(Func<T> factory, Func<Exception, Exception> mapNativeException)
	{
		try
		{
			return factory();
		}
		catch (Exception arg)
		{
			throw mapNativeException(arg);
		}
	}
}
