using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public class NavParallelExecutionActivity : NavSynchronousBaseActivity
{
	private readonly Parallel parallelExecution = new Parallel();

	public NavParallelExecutionActivity(IEnumerable<Activity> activitiesToExecute)
	{
		if (activitiesToExecute == null)
		{
			throw new ArgumentNullException("activitiesToExecute");
		}
		foreach (Activity item in activitiesToExecute)
		{
			parallelExecution.Branches.Add(item);
		}
	}

	protected override void CacheMetadata(NativeActivityMetadata metadata)
	{
		metadata.AddImplementationChild(parallelExecution);
		base.CacheMetadata(metadata);
	}

	protected override void Execute(NativeActivityContext context)
	{
		context.ScheduleActivity(parallelExecution);
	}
}
