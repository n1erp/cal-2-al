using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavXmlNodeGeneric<T> : NavXmlNode, IXmlValueContainer<T> where T : class
{
	private readonly T value;

	bool IXmlValueContainer<T>.HasValue => value != null;

	T IXmlValueContainer<T>.Value => Value;

	public override string DebuggerStringValue => value?.ToString() ?? string.Empty;

	internal T Value
	{
		get
		{
			if (value == null)
			{
				throw new NavNCLNotInitializedException(GetType());
			}
			return value;
		}
	}

	internal NavXmlNodeGeneric(T value)
		: base(value)
	{
		this.value = value;
	}

	public override bool Equals(NavValue other)
	{
		if (!(other is IXmlValueContainer<T> xmlValueContainer))
		{
			return false;
		}
		return Value.Equals(xmlValueContainer.Value);
	}
}
