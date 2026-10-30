using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public sealed class AnalyzerLoadFailureEventArgs : EventArgs
{
	public enum FailureErrorCode
	{
		None,
		UnableToLoadAnalyzer,
		UnableToCreateAnalyzer,
		NoAnalyzers
	}

	public string TypeName { get; private set; }

	public string Message { get; private set; }

	public FailureErrorCode ErrorCode { get; private set; }

	public Exception Exception { get; private set; }

	public AnalyzerLoadFailureEventArgs(FailureErrorCode errorCode, string message, Exception exceptionOpt = null, string typeNameOpt = null)
	{
		if (errorCode <= FailureErrorCode.None || errorCode > FailureErrorCode.NoAnalyzers)
		{
			throw new ArgumentOutOfRangeException("errorCode");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		ErrorCode = errorCode;
		Message = message;
		TypeName = typeNameOpt;
		Exception = exceptionOpt;
	}
}
