using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal sealed class NavAppTenantWebServiceChecker(IFileSystem fileSystem) : NavAppXmlChecker(NavAppXmlSchemas.SupportedTenantWebServices, "web service", fileSystem)
{
	private const string StartCharacterExpTxt = "[\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}]";

	private const string OtherCharacterExpTxt = "[\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]";

	private readonly Regex nameCheckRegex = new Regex("^[\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}][\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]{0,}$");

	protected override NavAppFileTypes FileTypes => NavAppFileTypes.TenantWebService;

	protected override NavAppCheckerResult ValidateDocuments(NavAppCheckerResult result, IEnumerable<(NavAppSourceFile sourceFile, XDocument xmlDocument)> documentMap)
	{
		foreach (var item in documentMap)
		{
			foreach (XElement item2 in item.xmlDocument.Descendants("TenantWebService"))
			{
				string text = item2.Element("ServiceName")?.Value;
				if (string.IsNullOrWhiteSpace(text) || !nameCheckRegex.IsMatch(text))
				{
					NavAppError navAppError = NavAppError.FromMessage(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.InvalidTenantWebServiceName, text, item.sourceFile.FileLocation), NavAppErrorType.InvalidOperation);
					result.AddError(navAppError);
				}
			}
		}
		return result;
	}
}
