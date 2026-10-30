using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[JsonObject]
public class BreakpointDefinition
{
	[DataMember]
	private readonly long breakpointId;

	[DataMember]
	private readonly string condition;

	[DataMember]
	private readonly string internalMethodName;

	[DataMember]
	private readonly string methodName;

	[DataMember]
	private readonly ApplicationObjectId objectId;

	[DataMember]
	private readonly SourceSpan sourceSpan;

	[DataMember]
	private readonly SourceSpan relativeSourceSpan;

	[JsonProperty]
	public long BreakpointId
	{
		[DebuggerStepThrough]
		get
		{
			return breakpointId;
		}
	}

	[JsonProperty]
	public string Condition
	{
		[DebuggerStepThrough]
		get
		{
			return condition;
		}
	}

	[JsonProperty]
	public string InternalMethodName
	{
		[DebuggerStepThrough]
		get
		{
			return internalMethodName;
		}
	}

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
	public SourceSpan SourceSpan
	{
		[DebuggerStepThrough]
		get
		{
			return sourceSpan;
		}
	}

	[JsonProperty]
	public SourceSpan RelativeSourceSpan
	{
		[DebuggerStepThrough]
		get
		{
			return relativeSourceSpan;
		}
	}

	[JsonConstructor]
	public BreakpointDefinition(ApplicationObjectId objectId, SourceSpan sourceSpan, SourceSpan relativeSourceSpan, long breakpointId, string methodName, string internalMethodName, string condition)
	{
		this.objectId = objectId;
		this.condition = condition;
		this.sourceSpan = sourceSpan;
		this.relativeSourceSpan = relativeSourceSpan;
		this.breakpointId = breakpointId;
		this.methodName = methodName;
		this.internalMethodName = internalMethodName;
	}
}
