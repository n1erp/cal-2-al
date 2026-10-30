namespace Microsoft.Dynamics.Nav.Types;

public interface ITestAction
{
	bool Visible { get; }

	bool Enabled { get; }

	void Invoke();
}
