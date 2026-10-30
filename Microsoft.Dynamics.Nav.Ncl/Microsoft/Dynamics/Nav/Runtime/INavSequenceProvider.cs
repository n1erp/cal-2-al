namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavSequenceProvider
{
	long GetNextValue(string sequenceName);

	void UpdateSequenceCurrentValue(string sequenceName, long startValue);
}
