namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IXmlValueContainer<out T>
{
	T Value { get; }

	bool HasValue { get; }
}
