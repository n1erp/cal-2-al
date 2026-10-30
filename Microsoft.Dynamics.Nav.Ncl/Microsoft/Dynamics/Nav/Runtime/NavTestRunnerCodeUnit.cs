using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavTestRunnerCodeUnit : NavCodeunit
{
	private NavTestRunnerCodeUnit previouslyExecutingRunner;

	public override CodeunitSubType SubType => CodeunitSubType.TestRunner;

	internal NavTestRunnerCodeUnit PreviouslyExecutingRunner
	{
		get
		{
			return previouslyExecutingRunner;
		}
		set
		{
			previouslyExecutingRunner = value;
		}
	}

	public virtual bool CommitTestFunctions => true;

	public virtual bool CommitTestCodeunits => true;

	public virtual bool OnTestRunMethodsHaveTestPermissionsParameter => false;

	protected NavTestRunnerCodeUnit(ITreeObject parent, int objectId)
		: this(parent, objectId, 0)
	{
	}

	protected NavTestRunnerCodeUnit(ITreeObject parent, int objectId, int tableId)
		: base(parent, objectId, tableId)
	{
	}

	protected internal override bool DoRun(DataError errorLevel, NavRecord record)
	{
		NavTestExecution testExecution = NavCurrentThread.Session.TestExecution;
		testExecution.EnterTestRunner(this);
		try
		{
			return base.DoRun(errorLevel, record);
		}
		finally
		{
			testExecution.LeaveTestRunner();
		}
	}

	public virtual bool OnBeforeTestRun(int codeUnitId, NavText codeUnitName, NavText functionName)
	{
		return true;
	}

	public virtual bool OnBeforeTestRun(int codeUnitId, NavText codeUnitName, NavText functionName, NavTestPermissions testPermissions)
	{
		return true;
	}

	public virtual void OnAfterTestRun(int codeUnitId, NavText codeUnitName, NavText functionName, bool success)
	{
	}

	public virtual void OnAfterTestRun(int codeUnitId, NavText codeUnitName, NavText functionName, NavTestPermissions testPermissions, bool success)
	{
	}
}
