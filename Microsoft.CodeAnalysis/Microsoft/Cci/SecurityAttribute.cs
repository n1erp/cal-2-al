using System.Reflection;

namespace Microsoft.Cci;

internal struct SecurityAttribute
{
	public DeclarativeSecurityAction Action { get; }

	public ICustomAttribute Attribute { get; }

	public SecurityAttribute(DeclarativeSecurityAction action, ICustomAttribute attribute)
	{
		Action = action;
		Attribute = attribute;
	}
}
