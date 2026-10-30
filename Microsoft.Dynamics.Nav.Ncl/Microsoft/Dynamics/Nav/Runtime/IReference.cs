namespace Microsoft.Dynamics.Nav.Runtime;

public interface IReference<out T> where T : class
{
	T Target { get; }

	T TargetSafe { get; }

	void ClearTarget();
}
