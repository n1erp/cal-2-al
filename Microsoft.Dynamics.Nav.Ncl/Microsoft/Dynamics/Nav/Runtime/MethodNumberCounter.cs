namespace Microsoft.Dynamics.Nav.Runtime;

public class MethodNumberCounter
{
	private int count;

	public int Next()
	{
		return count++;
	}
}
