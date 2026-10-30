using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class LanguageFormatConverter
{
	public const char PagePrefix = 'N';

	public const char TablePrefix = 'T';

	public const char XMLPortPrefix = 'X';

	public const char ReportPrefix = 'R';

	public const char MenuSuitePrefix = 'M';

	public const char QueryPrefix = 'Q';

	public const char CodeunitPrefix = 'C';

	private const char ActionItem = 'G';

	private const char MenuSuiteItem = 'I';

	private const char Column = 'C';

	private const char MaxLen = 'L';

	private const char Key = 'K';

	private const char Field = 'F';

	private const char Property = 'P';

	private const char Language = 'A';

	private const char Variable = 'V';

	private const char DataItem = 'D';

	private const char Code = 'U';

	private const char Event = 'E';

	private const char Delimiter = '-';

	private const char StringValue = ':';

	private const string OutDelimiter = ";";

	private const string OutKey = "Key";

	private const string OutCodeunitRun = "Code26369";

	private const string OutDBInsert = "Code4958";

	private const string OutDBModify = "Code4959";

	private const string OutDBDelete = "Code4960";

	private const string OutDBRename = "Code4961";

	private const string OutDBValidate = "Code4962";

	private const string OutDBLookup = "Code4963";

	internal const string OutPage = "Page";

	internal const string OutActionItem = "ActionItem";

	internal const string OutXmlPort = "XmlP";

	internal const string OutTextConst = "Txt";

	internal const string OutReport = "Rep";

	internal const string OutCodeUnit = "CU";

	internal const string OutMenuSuite = "MenuSuite";

	internal const string OutMenuItem = "MenuItem";

	internal const string OutQuery = "Query";

	internal const string OutColumn = "Column";

	internal const string OutTable = "Tab";

	internal const string OutField = "Field";

	internal const string OutCaptionML = "CapML";

	internal const string OutOptionCaptionML = "OptionCapML";

	internal const string OutToolTipML = "ToolTipML";

	internal const string OutLanguage = "Lang";

	internal const string OutLabel = "Label";

	internal const string OutVariable = "Var";

	internal const string OutDataItem = "DataItem";

	internal const string OutCode = "Code";

	internal const string OutEvent = "Event";

	internal const string OutControl = "Control";

	internal const string OutPromotedActionCategoriesML = "PromotedActionCategoriesML";

	internal const string OutInstructionalTextML = "InstructionalTextML";

	internal const string OutReqFilterHeadingML = "ReqFilterHeadingML";

	internal const string OutYes = "yes";

	internal const char OutControlTriggerDelimiter = '!';

	private readonly StringBuilder b = new StringBuilder(1000);

	private int controlPos;

	private bool hasSubComponent;

	private int index;

	private string line;

	internal static bool IsTextFormat(byte[] bytes, int offset = 0)
	{
		if (bytes.Length >= offset + 2 && char.IsLetter((char)bytes[offset]))
		{
			return char.IsDigit((char)bytes[offset + 1]);
		}
		return false;
	}

	internal string ConvertLineToLangFormat(string inputLine)
	{
		return ProcessLine(inputLine);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Determined by legacy file format")]
	private string ProcessLine(string inputLine)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int length = inputLine.Length;
		b.Clear();
		line = inputLine;
		hasSubComponent = false;
		controlPos = 0;
		index = 0;
		while (index < length)
		{
			switch (line[index++])
			{
			case 'N':
				flag2 = true;
				if (!(flag3 | flag4))
				{
					b.Append("Page");
					ReadAndAppendInt();
					b.Append(";");
				}
				else
				{
					ReadInt();
					b.Append("yes");
				}
				break;
			case 'G':
				hasSubComponent = true;
				if (controlPos != 0)
				{
					b.Remove(controlPos, b.Length - controlPos);
				}
				controlPos = b.Length;
				b.Append("ActionItem");
				ReadAndAppendInt();
				break;
			case 'X':
				flag4 = true;
				b.Append("XmlP");
				ReadAndAppendInt();
				if (!IsRequestPage(length))
				{
					b.Append(";");
				}
				break;
			case 'R':
				flag3 = true;
				b.Append("Rep");
				ReadAndAppendInt();
				if (!IsRequestPage(length))
				{
					b.Append(";");
				}
				break;
			case 'M':
				b.Append("MenuSuite");
				ReadAndAppendInt();
				b.Append(";");
				break;
			case 'I':
				hasSubComponent = true;
				b.Append("MenuItem");
				while (index < length && line[index] != '-')
				{
					b.Append(line[index++]);
				}
				break;
			case 'Q':
				if (index == 1)
				{
					b.Append("Query");
					ReadAndAppendInt();
					b.Append(";");
				}
				else
				{
					CheckForSubComponentAndWrite("Txt");
					ReadAndAppendInt();
				}
				break;
			case 'C':
				if (index == 1)
				{
					b.Append("CU");
					ReadAndAppendInt();
					b.Append(";");
				}
				else if (flag2)
				{
					hasSubComponent = true;
					controlPos = b.Length;
					b.Append("Control");
					ReadAndAppendInt();
				}
				else
				{
					hasSubComponent = true;
					b.Append("Column");
					ReadAndAppendInt();
				}
				break;
			case 'L':
			{
				int id = ReadInt();
				if (id != 999)
				{
					b.Append(id.ToString(CultureInfo.InvariantCulture));
				}
				b.Append(";");
				break;
			}
			case 'T':
				if (index == 1)
				{
					b.Append("Tab");
					ReadAndAppendInt();
					b.Append(";");
				}
				else if (flag3)
				{
					hasSubComponent = true;
					b.Append("Label");
					ReadAndAppendInt();
				}
				break;
			case 'K':
				hasSubComponent = true;
				b.Append("Key");
				ReadAndAppendInt();
				break;
			case 'F':
				hasSubComponent = true;
				controlPos = b.Length;
				b.Append("Field");
				ReadAndAppendInt();
				break;
			case 'P':
			{
				int id = ReadInt();
				HandleProperty(id);
				break;
			}
			case 'A':
				b.Append("Lang");
				ReadAndAppendInt();
				flag = true;
				break;
			case 'V':
				hasSubComponent = true;
				controlPos = b.Length;
				b.Append("Var");
				if (line[index] == '-')
				{
					b.Append("-");
					index++;
				}
				ReadAndAppendInt();
				break;
			case 'D':
				hasSubComponent = true;
				controlPos = b.Length;
				b.Append("DataItem");
				ReadAndAppendInt();
				break;
			case '-':
				b.Append(";");
				break;
			case ':':
				b.Append(":\"");
				b.Append(line.Substring(index));
				b.Append("\"");
				index = length;
				break;
			case 'U':
				hasSubComponent = true;
				b.Append("Code");
				ReadAndAppendInt();
				break;
			case 'E':
				hasSubComponent = true;
				CheckForParentControlAndWrite("Event");
				if (line[index] == '-')
				{
					b.Append("-");
					index++;
				}
				ReadAndAppendInt();
				break;
			default:
				throw new NotSupportedException(line);
			}
		}
		if (!flag)
		{
			return null;
		}
		return b.ToString();
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Determined by legacy property logic")]
	private void HandleProperty(int id)
	{
		switch (id)
		{
		case 8629:
			CheckForSubComponentAndWrite("CapML");
			break;
		case 8632:
			CheckForSubComponentAndWrite("OptionCapML");
			break;
		case 8631:
			CheckForSubComponentAndWrite("ToolTipML");
			break;
		case 18490:
			CheckForSubComponentAndWrite("ReqFilterHeadingML");
			break;
		case 26369:
			hasSubComponent = true;
			b.Append("Code26369");
			break;
		case 4958:
			CheckForParentControlAndWrite("Code4958");
			break;
		case 4959:
			CheckForParentControlAndWrite("Code4959");
			break;
		case 4960:
			CheckForParentControlAndWrite("Code4960");
			break;
		case 4961:
			CheckForParentControlAndWrite("Code4961");
			break;
		case 4962:
			CheckForParentControlAndWrite("Code4962");
			break;
		case 4963:
			CheckForParentControlAndWrite("Code4963");
			break;
		case 18512:
		case 18513:
		case 18514:
		case 18515:
		case 18516:
		case 18517:
		case 18518:
		case 18519:
		case 18520:
		case 18521:
		case 18522:
		case 18523:
		case 18529:
		case 18530:
		case 18531:
			CheckForParentControlAndWrite("Code");
			b.Append(id.ToString(CultureInfo.InvariantCulture));
			break;
		case 55242:
			b.Remove(b.Length - 1, 1);
			break;
		case 26171:
			b.Remove(b.Length - 1, 1);
			break;
		case 55132:
			CheckForSubComponentAndWrite("PromotedActionCategoriesML");
			break;
		case 55119:
			CheckForSubComponentAndWrite("InstructionalTextML");
			break;
		case 54116:
		case 54117:
		case 54118:
		case 54126:
		case 54127:
		case 54136:
		case 54137:
		case 54138:
		case 54139:
		case 54140:
		case 54141:
		case 54142:
		case 54146:
		case 54147:
		case 55140:
		case 55141:
		case 55142:
		case 55143:
		case 55144:
		case 55145:
		case 55146:
		case 55147:
		case 55148:
		case 55149:
		case 55150:
		case 55151:
		case 55160:
		case 55170:
		case 55171:
		case 55172:
		case 55173:
		case 55174:
		case 55175:
		case 57700:
			CheckForParentControlAndWrite("Code");
			b.Append(id.ToString(CultureInfo.InvariantCulture));
			break;
		default:
			CheckForSubComponentAndWrite("P?");
			b.Append(id.ToString(CultureInfo.InvariantCulture));
			break;
		}
	}

	private void CheckForSubComponentAndWrite(string value)
	{
		if (!hasSubComponent)
		{
			b.Append(";");
		}
		b.Append(value);
	}

	private void CheckForParentControlAndWrite(string value)
	{
		hasSubComponent = true;
		if (controlPos != 0)
		{
			b[b.Length - 1] = '!';
		}
		b.Append(value);
	}

	private void ReadAndAppendInt()
	{
		int num = ReadInt();
		b.Append(num.ToString(CultureInfo.InvariantCulture));
	}

	private int ReadInt()
	{
		int num = 0;
		while (char.IsDigit(line[index]))
		{
			num = num * 10 + (line[index++] - 48);
		}
		return num;
	}

	private bool IsRequestPage(int len)
	{
		int num = index;
		if (num + 4 < len && line[num] == '-' && line[num + 1] == 'N' && (line[num + 2] == '2' || line[num + 2] == '1'))
		{
			return line[num + 3] == '-';
		}
		return false;
	}
}
