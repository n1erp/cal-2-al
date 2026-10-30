using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public class NavRequestSuspendActivity : NavSynchronousBaseActivity
{
	[RequiredArgument]
	public InArgument<string> BookmarkName { get; set; }

	protected sealed override bool CanInduceIdle => true;

	protected override void Execute(NativeActivityContext context)
	{
		string name = ArgumentsHandler.LoadInArgument(context, BookmarkName);
		context.CreateBookmark(name, Callback);
	}

	protected virtual void Callback(NativeActivityContext context, Bookmark bookmark, object value)
	{
	}
}
