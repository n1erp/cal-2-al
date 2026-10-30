using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.Types;

public class TenantId : RestrictedStringConfigSetting<TenantId>
{
	public const int MaxLength = 63;

	public const string ValidateRegularExpression = "^[A-Za-z0-9]([A-Za-z0-9\\-]{0,61}[A-Za-z0-9])?$";

	private const string InvalidStringRegEx = "[^A-Za-z0-9-]";

	private static readonly Regex validateTenantId = new Regex("^[A-Za-z0-9]([A-Za-z0-9\\-]{0,61}[A-Za-z0-9])?$", RegexOptions.Compiled);

	public TenantId(string value)
		: base(value, string.Empty, true, 63, "[^A-Za-z0-9-]")
	{
	}

	public static bool IsValid(string tenantId)
	{
		return validateTenantId.IsMatch(tenantId);
	}
}
