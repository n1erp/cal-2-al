using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
[DebuggerDisplay("{FullName}")]
public class DotNetAssemblyInfo : IEquatable<DotNetAssemblyInfo>
{
	public string FullName { get; }

	public string Name { get; }

	public string Version { get; }

	public string Culture { get; }

	public string PublicKeyToken { get; }

	public string Location { get; }

	public DotNetAssemblyInfo(string name, string version, string culture, string publicKeyToken, string location)
	{
		Name = name;
		Version = version;
		Culture = culture;
		PublicKeyToken = publicKeyToken;
		FullName = DotNetUtilities.GetAssemblyName(name, version, culture, publicKeyToken);
		Location = location;
	}

	public bool Equals(DotNetAssemblyInfo other)
	{
		if (other == null)
		{
			return false;
		}
		return FullName == other.FullName;
	}

	public override bool Equals(object obj)
	{
		DotNetAssemblyInfo other = obj as DotNetAssemblyInfo;
		return Equals(other);
	}

	public override int GetHashCode()
	{
		return FullName.GetHashCode();
	}
}
