using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public struct CompanyExternalIdentifier
{
	public string Name { get; set; }

	public Guid ODataId { get; set; }

	public override bool Equals(object obj)
	{
		if (!(obj is CompanyExternalIdentifier companyExternalIdentifier))
		{
			return false;
		}
		if (Name.Equals(companyExternalIdentifier.Name, StringComparison.Ordinal))
		{
			return ODataId == companyExternalIdentifier.ODataId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode() ^ ODataId.GetHashCode();
	}

	public static bool operator ==(CompanyExternalIdentifier id1, CompanyExternalIdentifier id2)
	{
		return id1.Equals(id2);
	}

	public static bool operator !=(CompanyExternalIdentifier id1, CompanyExternalIdentifier id2)
	{
		return !id1.Equals(id2);
	}
}
