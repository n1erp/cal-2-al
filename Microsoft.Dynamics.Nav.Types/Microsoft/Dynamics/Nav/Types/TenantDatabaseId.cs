using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.Types;

public class TenantDatabaseId : RestrictedStringConfigSetting<TenantDatabaseId>
{
	public const int MaxLength = 127;

	public const string ValidateRegularExpression = "^[A-Za-z0-9\\{]([A-Za-z0-9\\-@.{}_]{0,125}[A-Za-z0-9\\.}])?$";

	private const string InvalidStringRegEx = "[^A-Za-z0-9-@.{}]";

	private static readonly Regex validateTenantDatabaseId = new Regex("^[A-Za-z0-9\\{]([A-Za-z0-9\\-@.{}_]{0,125}[A-Za-z0-9\\.}])?$", RegexOptions.Compiled);

	public TenantDatabaseId(string value)
		: base(value, string.Empty, true, 127, "[^A-Za-z0-9-@.{}]")
	{
	}

	public static bool IsValid(string tenantDatabaseId)
	{
		if (string.IsNullOrEmpty(tenantDatabaseId))
		{
			return true;
		}
		return validateTenantDatabaseId.IsMatch(tenantDatabaseId);
	}
}
