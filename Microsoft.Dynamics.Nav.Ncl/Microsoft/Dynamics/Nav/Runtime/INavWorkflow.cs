using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavWorkflow<in TStartRequest, in TResumeRequest, out TStatus> where TStartRequest : class where TResumeRequest : class where TStatus : class
{
	NavWorkflowState State { get; }

	void StartWorkflow(TStartRequest startRequest);

	void StopWorkflow();

	void ResumeWorkflow(TResumeRequest resumeRequest);

	TStatus GetWorkflowStatus();
}
