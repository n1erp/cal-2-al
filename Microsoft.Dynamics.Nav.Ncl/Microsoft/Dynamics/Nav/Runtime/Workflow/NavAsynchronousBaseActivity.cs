using System;
using System.Activities;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public abstract class NavAsynchronousBaseActivity : AsyncCodeActivity
{
	protected INavActivityArgumentHandler ArgumentsHandler { get; private set; }

	protected INavWorkflowTenantSessionManager SessionManager { get; private set; }

	protected NavAsynchronousBaseActivity()
		: this(new NavActivityArgumentHandler())
	{
	}

	protected internal NavAsynchronousBaseActivity(INavActivityArgumentHandler argumentsHandler)
	{
		if (argumentsHandler == null)
		{
			throw new ArgumentNullException("argumentsHandler");
		}
		ArgumentsHandler = argumentsHandler;
	}

	protected virtual void ExecuteBeforeAsync(AsyncCodeActivityContext context)
	{
		SessionManager = context.GetExtension<INavWorkflowTenantSessionManager>();
		ArgumentsHandler.CheckForNull(SessionManager, "SessionManager");
	}

	protected virtual void ExecuteAsync()
	{
	}

	protected virtual void ExecuteAfterAsync(AsyncCodeActivityContext context)
	{
	}

	protected sealed override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
	{
		ExecuteBeforeAsync(context);
		Task task = NavTaskFactory.StartNewTask((object _) =>
		{
			ExecuteAsync();
		}, state);
		if (callback != null)
		{
			task.ContinueWith((Task res) =>
			{
				callback(res);
			});
		}
		return task;
	}

	protected sealed override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
	{
		try
		{
			Task task = result as Task;
			task.LogOrFailOnExceptionReturnOriginalTask(failFast: false).Wait();
			ExecuteAfterAsync(context);
		}
		catch (AggregateException ex)
		{
			throw ex.InnerException;
		}
	}
}
