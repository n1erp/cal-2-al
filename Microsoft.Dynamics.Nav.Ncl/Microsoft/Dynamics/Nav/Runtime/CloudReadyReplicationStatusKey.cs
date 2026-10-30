using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

public class CloudReadyReplicationStatusKey
{
	private readonly string jointKey;

	public string TableName { get; }

	public string CompanyName { get; }

	public CloudReadyReplicationStatusKey(string tableName, string companyName)
	{
		TableName = tableName;
		CompanyName = companyName;
		jointKey = GenerateKey();
	}

	private string GenerateKey()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}${1}", CompanyName, TableName);
	}

	public override int GetHashCode()
	{
		return jointKey.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		CloudReadyReplicationStatusKey cloudReadyReplicationStatusKey = (CloudReadyReplicationStatusKey)obj;
		return jointKey == cloudReadyReplicationStatusKey.jointKey;
	}
}
