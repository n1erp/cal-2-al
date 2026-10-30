using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public abstract class NavSynchronousBaseActivity : NativeActivity
{
	protected INavActivityArgumentHandler ArgumentsHandler { get; private set; }

	protected override bool CanInduceIdle => true;

	protected NavSynchronousBaseActivity()
		: this(new NavActivityArgumentHandler())
	{
	}

	internal NavSynchronousBaseActivity(INavActivityArgumentHandler argumentsHandler)
	{
		ArgumentsHandler = argumentsHandler;
	}
}
