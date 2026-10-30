using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public static class CmdletConfigurationEntries
{
	internal const string HelpFileName = "Microsoft.Dynamics.Nav.Model.Tools.dll-Help.xml";

	private static readonly CmdletConfigurationEntry[] cmdletConfigurationEntries = GetCmdletConfigurationEntries(Assembly.GetExecutingAssembly());

	public static CmdletConfigurationEntry[] NavCmdletConfigurationEntries => cmdletConfigurationEntries;

	internal static CmdletConfigurationEntry[] GetCmdletConfigurationEntries(Assembly executingAssembly)
	{
		return (from type in executingAssembly.GetTypes()
			let attribute = type.GetCustomAttribute<CmdletAttribute>()
			where attribute != null
			let name = attribute.VerbName + "-" + attribute.NounName
			select new CmdletConfigurationEntry(name, type, "Microsoft.Dynamics.Nav.Model.Tools.dll-Help.xml")).ToArray();
	}
}
