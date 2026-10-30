using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Compiler;

public class ALParseException : Exception
{
	public int LineNo { get; private set; }

	public int LinePos { get; private set; }

	public ALParseException()
	{
	}

	public ALParseException(string message)
		: base(message)
	{
	}

	public ALParseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected ALParseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public ALParseException(int lineNo, int linePos, string message)
		: base(message)
	{
		LineNo = lineNo;
		LinePos = linePos;
	}
}
