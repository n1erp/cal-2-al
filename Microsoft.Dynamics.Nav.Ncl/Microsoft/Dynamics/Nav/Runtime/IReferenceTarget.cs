namespace Microsoft.Dynamics.Nav.Runtime;

public interface IReferenceTarget<out T> where T : class
{
	IReference<T> GetReference();
}
