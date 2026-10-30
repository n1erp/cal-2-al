using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required)]
public interface INavDebuggerCallback
{
	[OperationContract]
	BreakpointResponse Break(string debuggingContext, ApplicationObjectId objectId, StackFrame[] stack, LocalNode[] localNodes, string errorMessage);

	[OperationContract]
	void InvalidateSource(string debuggingContext, ApplicationObjectId id);

	[OperationContract]
	void OnFatalDebuggerException(string debuggingContext, ServerFault fault);

	[OperationContract]
	void OnAttachedToConnection(string debuggingContext);

	[OperationContract]
	void OnDetachedFromConnection(string debuggingContext);
}
