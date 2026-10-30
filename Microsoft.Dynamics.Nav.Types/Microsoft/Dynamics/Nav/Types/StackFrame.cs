using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[JsonObject]
public class StackFrame
{
	[DataMember]
	private readonly string methodName;

	[DataMember]
	private readonly ApplicationObjectId objectId;

	[DataMember]
	private readonly string objectName;

	[DataMember]
	private readonly SourceSpan statementSpan;

	[JsonProperty]
	public string MethodName
	{
		[DebuggerStepThrough]
		get
		{
			return methodName;
		}
	}

	[JsonProperty]
	public ApplicationObjectId ObjectId
	{
		[DebuggerStepThrough]
		get
		{
			return objectId;
		}
	}

	[JsonProperty]
	public string ObjectName
	{
		[DebuggerStepThrough]
		get
		{
			return objectName;
		}
	}

	[JsonProperty]
	public SourceSpan StatementSpan
	{
		[DebuggerStepThrough]
		get
		{
			return statementSpan;
		}
	}

	[JsonConstructor]
	public StackFrame(ApplicationObjectId objectId, string objectName, string methodName, SourceSpan statementSpan)
	{
		this.objectId = objectId;
		this.objectName = objectName;
		this.methodName = methodName;
		this.statementSpan = statementSpan;
	}
}
