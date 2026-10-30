using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[Flags]
internal enum AttributeAllowedOn
{
	Method = 1,
	Variable = 2
}
