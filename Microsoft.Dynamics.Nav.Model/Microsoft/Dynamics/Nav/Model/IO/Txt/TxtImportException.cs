using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public class TxtImportException : TypeSystemException
{
	public int LineNo { get; private set; }

	public int LinePos { get; private set; }

	public string Line { get; private set; }

	public ElementType ObjectType { get; set; }

	public int ObjectId { get; set; }

	public TxtImportException(int lineNo, int linePos, string line, string message)
		: base(message)
	{
		LineNo = lineNo;
		LinePos = linePos;
		Line = line;
	}

	public TxtImportException(int lineNo, int linePos, string line, string message, Exception innerException, ElementType objectType, int objectId)
		: base(innerException, message)
	{
		LineNo = lineNo;
		LinePos = linePos;
		Line = line;
		ObjectType = objectType;
		ObjectId = objectId;
	}

	public TxtImportException()
	{
	}

	public TxtImportException(string message)
		: base(message)
	{
	}

	public TxtImportException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected TxtImportException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
