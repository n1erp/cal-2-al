namespace Microsoft.Dynamics.Nav.Runtime;

public class ByRefInstance<T> : ByRef<T>
{
	private T value;

	public ByRefInstance()
	{
		SetGetterSetter(() => value, (T v) =>
		{
			value = v;
		});
	}

	public ByRefInstance(T initialValue)
		: this()
	{
		value = initialValue;
	}
}
