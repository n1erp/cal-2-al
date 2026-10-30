using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public class NavSerialExecutionActivity : NavSynchronousBaseActivity
{
	internal readonly Sequence SequenceExecution = new Sequence();

	public NavSerialExecutionActivity(IEnumerable<Activity> activitiesToExecute, string displayName = null)
	{
		if (activitiesToExecute == null)
		{
			throw new ArgumentNullException("activitiesToExecute");
		}
		foreach (Activity item in activitiesToExecute)
		{
			SequenceExecution.Activities.Add(item);
		}
		if (displayName != null)
		{
			DisplayName = displayName;
		}
	}

	protected override void CacheMetadata(NativeActivityMetadata metadata)
	{
		metadata.AddImplementationChild(SequenceExecution);
		base.CacheMetadata(metadata);
	}

	protected override void Execute(NativeActivityContext context)
	{
		context.ScheduleActivity(SequenceExecution);
	}
}
