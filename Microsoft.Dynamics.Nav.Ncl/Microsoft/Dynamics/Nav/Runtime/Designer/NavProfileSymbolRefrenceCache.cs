using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavProfileSymbolRefrenceCache
{
	private readonly Dictionary<NavProfileKey, Dictionary<string, SymbolReferenceSpecification>> profileModuleSymbolReferenceMapping = new Dictionary<NavProfileKey, Dictionary<string, SymbolReferenceSpecification>>();

	public SymbolReferenceSpecification GetProfileSymbolRefrence(NavProfileKey profileKey)
	{
		string profileScope = GetProfileScope(profileKey);
		if (!profileModuleSymbolReferenceMapping.ContainsKey(profileKey))
		{
			profileModuleSymbolReferenceMapping[profileKey] = new Dictionary<string, SymbolReferenceSpecification>(StringComparer.OrdinalIgnoreCase);
		}
		if (!profileModuleSymbolReferenceMapping[profileKey].ContainsKey(profileScope))
		{
			profileModuleSymbolReferenceMapping[profileKey][profileScope] = new SymbolReferenceSpecification("Microsoft", Guid.NewGuid().ToString(), new Version());
		}
		return profileModuleSymbolReferenceMapping[profileKey][profileScope];
	}

	public void Remove(NavProfileKey profileKey)
	{
		if (profileModuleSymbolReferenceMapping.ContainsKey(profileKey))
		{
			string profileScope = GetProfileScope(profileKey);
			profileModuleSymbolReferenceMapping[profileKey].Remove(profileScope);
		}
	}

	public IEnumerable<SymbolReferenceSpecification> GetAllValues()
	{
		return profileModuleSymbolReferenceMapping.SelectMany((KeyValuePair<NavProfileKey, Dictionary<string, SymbolReferenceSpecification>> n) => n.Value.Select((KeyValuePair<string, SymbolReferenceSpecification> m) => m.Value)).ToList();
	}

	private static string GetProfileScope(NavProfileKey profileKey)
	{
		if (profileKey.Scope != ProfileScope.Tenant)
		{
			return "SYSTEM";
		}
		if (!(profileKey.AppId.Value == Guid.Empty))
		{
			return NavCurrentThread.ResolveAppGroup().OrderedAppMetadata.FirstOrDefault((NavAppRuntimeMetadata a) => ((WrappedValue<Guid>)(object)a.AppId).Value == profileKey.AppId.Value).Version.ToString();
		}
		return NavCurrentThread.Session.Tenant.Id;
	}
}
