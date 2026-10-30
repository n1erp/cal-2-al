using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IByRef
{
	object ObjectValue { get; set; }

	Type ObjectType { get; }
}
