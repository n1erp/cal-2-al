using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal sealed class NavAppPermissionSetChecker : NavAppXmlChecker
{
	protected override NavAppFileTypes FileTypes => NavAppFileTypes.PermissionSet;

	public NavAppPermissionSetChecker(IFileSystem fileSystem)
		: base(NavAppXmlSchemas.SupportedPermissionSets, "permission set", fileSystem)
	{
	}

	protected override NavAppCheckerResult ValidateDocuments(NavAppCheckerResult result, IEnumerable<(NavAppSourceFile sourceFile, XDocument xmlDocument)> documentMap)
	{
		List<string> list = new List<string>();
		foreach (var item in documentMap)
		{
			NavAppResult navAppResult = ValidatePermissionSet(item.xmlDocument.Root.Elements(NavAppPermissionXmlPart.PermissionSet.ToString()), list);
			if (navAppResult.Failure)
			{
				item.sourceFile.AddError(Category, navAppResult.NavAppError);
			}
		}
		(from perm in list
			group perm by perm into g
			where g.Count() > 1
			select g).Do((IGrouping<string, string> group) =>
		{
			result.PassErrors.Add(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.DuplicatePermissionSetFound, group.Key));
		});
		return result;
	}

	private static NavAppResult ValidatePermissionSet(IEnumerable<XElement> sets, ICollection<string> setIDs)
	{
		List<NavAppError> list = new List<NavAppError>();
		foreach (XElement set in sets)
		{
			string attribute = set.GetAttribute<string>(NavAppPermissionXmlPart.RoleID.ToString(), null);
			setIDs.Add(attribute);
			foreach (XElement item in set.Elements(NavAppPermissionXmlPart.Permission.ToString()))
			{
				NavAppResult navAppResult = ValidateSecurityFilter(item, attribute);
				if (navAppResult.Failure)
				{
					list.Add(navAppResult.NavAppError);
				}
			}
		}
		if (!list.Any())
		{
			return NavAppResult.Ok();
		}
		return NavAppResult.Fail(NavAppError.FromMultipleErrors("Security Filter Errors", list));
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private static NavAppResult ValidateSecurityFilter(XContainer element, string setId)
	{
		string elementValue = element.GetElementValue<string>(NavAppPermissionXmlPart.SecurityFilter);
		if (string.IsNullOrWhiteSpace(elementValue))
		{
			return NavAppResult.Ok();
		}
		try
		{
			int position = 0;
			if (!TryReadTokenUntilDelimiter(elementValue, ref position, ':', delimiterRequired: true, out var result))
			{
				return NavAppResult.Fail(NavAppError.FromMessage(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.SecurityFilterParseHeader, setId, element.GetElementValue<string>(NavAppPermissionXmlPart.ObjectType), element.GetElementValue<int>(NavAppPermissionXmlPart.ObjectID), NavAppCheckerStrings.SecurityFilterObjectNameMissing), NavAppErrorType.InvalidOperation));
			}
			ConsumeWhitespace(elementValue, ref position);
			while (position < elementValue.Length)
			{
				if (!TryReadTokenUntilDelimiter(elementValue, ref position, '=', delimiterRequired: true, out result))
				{
					return NavAppResult.Fail(NavAppError.FromMessage(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.SecurityFilterParseHeader, setId, element.GetElementValue<string>(NavAppPermissionXmlPart.ObjectType), element.GetElementValue<int>(NavAppPermissionXmlPart.ObjectID), NavAppCheckerStrings.SecurityFilterValueMissing), NavAppErrorType.InvalidOperation));
				}
				ConsumeWhitespace(elementValue, ref position);
				if (!TryReadTokenUntilDelimiter(elementValue, ref position, ',', delimiterRequired: false, out result))
				{
					return NavAppResult.Fail(NavAppError.FromMessage(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.SecurityFilterParseHeader, setId, element.GetElementValue<string>(NavAppPermissionXmlPart.ObjectType), element.GetElementValue<int>(NavAppPermissionXmlPart.ObjectID), NavAppCheckerStrings.SecurityFilterMissingExpressionAfterSeparator), NavAppErrorType.InvalidOperation));
				}
			}
		}
		catch (Exception ex)
		{
			return NavAppResult.Fail(NavAppError.FromMessage(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.SecurityFilterParseHeader, setId, element.GetElementValue<string>(NavAppPermissionXmlPart.ObjectType), element.GetElementValue<int>(NavAppPermissionXmlPart.ObjectID), ex.Message), NavAppErrorType.InvalidOperation));
		}
		return NavAppResult.Ok();
	}

	private static bool TryReadTokenUntilDelimiter(string input, ref int position, char delimiter, bool delimiterRequired, out string result)
	{
		ConsumeWhitespace(input, ref position);
		if (position >= input.Length)
		{
			result = string.Empty;
			return !delimiterRequired;
		}
		int num = position++;
		int num2 = -1;
		if (input[num] == delimiter)
		{
			result = string.Empty;
			return true;
		}
		if (input[num] != '"')
		{
			num2 = input.IndexOf(delimiter, position);
			if (num2 < 0)
			{
				if (delimiterRequired)
				{
					result = null;
					return false;
				}
				num2 = input.Length - 1;
				position = input.Length;
			}
			else
			{
				position = num2 + 1;
				num2--;
			}
			while (char.IsWhiteSpace(input, num2))
			{
				num2--;
			}
			result = input.Substring(num, num2 - num + 1);
			return true;
		}
		num++;
		bool flag = false;
		while (position < input.Length)
		{
			if (input[position] == '"')
			{
				if (position + 1 >= input.Length || input[position + 1] != '"')
				{
					num2 = position - 1;
					position++;
					break;
				}
				position++;
				flag = true;
			}
			position++;
		}
		if (num2 < 0)
		{
			result = null;
			return false;
		}
		result = input.Substring(num, num2 - num + 1);
		if (flag)
		{
			result = result.Replace("\"\"", "\"");
		}
		ConsumeWhitespace(input, ref position);
		if (position < input.Length)
		{
			return input[position++] == delimiter;
		}
		return !delimiterRequired;
	}

	private static void ConsumeWhitespace(string input, ref int position)
	{
		while (position < input.Length && char.IsWhiteSpace(input, position))
		{
			position++;
		}
	}
}
