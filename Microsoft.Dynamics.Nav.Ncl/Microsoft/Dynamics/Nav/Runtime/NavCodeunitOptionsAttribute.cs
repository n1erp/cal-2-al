using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Class)]
public sealed class NavCodeunitOptionsAttribute : Attribute
{
	public NavCodeunitOptions Options { get; }

	public int? TableId { get; }

	public CodeunitSubType? Subtype { get; }

	public bool IsSingleInstance => (Options & NavCodeunitOptions.SingleInstance) != 0;

	public bool IsEventManualBinding => (Options & NavCodeunitOptions.EventManualBinding) != 0;

	public NavCodeunitOptionsAttribute(NavCodeunitOptions options = (NavCodeunitOptions)0)
	{
		Options = options;
	}

	public NavCodeunitOptionsAttribute(NavCodeunitOptions options, int tableId, CodeunitSubType subtype)
	{
		Subtype = subtype;
		TableId = tableId;
		Options = options;
	}
}
