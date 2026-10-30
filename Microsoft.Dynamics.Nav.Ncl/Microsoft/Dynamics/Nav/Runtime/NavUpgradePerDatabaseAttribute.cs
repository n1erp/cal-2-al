using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class NavUpgradePerDatabaseAttribute : Attribute
{
}
