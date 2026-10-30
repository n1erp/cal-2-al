using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class AnalyzerLoadFailureEventArgs : EventArgs
{
	public enum FailureErrorCode
	{
		None,
		UnableToLoadAnalyzer,
		UnableToCreateAnalyzer,
		NoAnalyzers
	}

	public string TypeName { get; }

	public string Message { get; }

	public FailureErrorCode ErrorCode { get; }

	public Exception Exception { get; }

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
