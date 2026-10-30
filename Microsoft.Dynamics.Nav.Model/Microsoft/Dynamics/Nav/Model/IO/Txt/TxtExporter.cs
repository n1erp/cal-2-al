using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Compiler;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public sealed class TxtExporter : IApplicationObjectExporter
{
	private enum LastExportedType
	{
		None,
		Normal,
		Container,
		Trigger
	}

	private readonly TxtFileModelInfo modelInfo;

	private readonly TxtExportOptions options;

	private static readonly TxtExportOptions DefaultOptions = new TxtExportOptions();

	private StreamWriter destinationStreamWriter;

	private string indent = string.Empty;

	private readonly Stack<string> indentStack = new Stack<string>();

	private bool writeIndent;

	private int linePos;

	private static readonly char[] NeedsBracketsChars = new char[2] { ';', '}' };

	private int NextLinePos
	{
		get
		{
			if (!writeIndent)
			{
				return linePos;
			}
			return indent.Length;
		}
	}

	public TxtExporter(TxtFileModelInfo modelInfo)
		: this(modelInfo, DefaultOptions)
	{
	}

	public TxtExporter(TxtFileModelInfo modelInfo, TxtExportOptions options)
	{
		this.modelInfo = modelInfo;
		this.options = options ?? DefaultOptions;
	}

	public void ExportObject(ApplicationObject applicationObject, Stream stream)
	{
		ExportObjects(new ApplicationObject[1] { applicationObject }, stream);
	}

	public void ExportObjects(IEnumerable<ApplicationObject> applicationObjects, Stream stream, Action progressAction = null)
	{
		using (destinationStreamWriter = new StreamWriter(stream, options.Encoding, 32768, leaveOpen: true))
		{
			foreach (ApplicationObject applicationObject in applicationObjects)
			{
				ExportObject(applicationObject);
				progressAction?.Invoke();
			}
			destinationStreamWriter.Flush();
		}
	}

	private void ExportObject(ApplicationObject ao)
	{
		indentStack.Clear();
		indent = string.Empty;
		indentStack.Push(indent);
		writeIndent = false;
		linePos = 0;
		if (options.DevBaseCompatibility)
		{
			TranformToDevBaseFormat(ao);
		}
		string text = ao.RootElement.GetStringProperty(PropertyType.Name) ?? string.Empty;
		string text2 = ((ao.ElementTypeInfo.ElementType == ElementType.Modification) ? string.Format(CultureInfo.InvariantCulture, "{0} {1}", ao.ElementTypeInfo.Name, text) : string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", ao.ElementTypeInfo.Name, ao.Id.ObjectNumber, text));
		if (text != text.Trim())
		{
			text2 = "[" + text2 + "]";
		}
		text2 = "OBJECT " + text2;
		WriteSection(text2, () =>
		{
			ExportObjectProperties(ao);
			ExportParentElement(ao.RootElement);
			if (ao.ElementTypeInfo.ElementType != ElementType.MenuSuite)
			{
				ExportCodeSection(ao.RootElement);
			}
			if (ao.ElementTypeInfo.ElementType == ElementType.Report)
			{
				ExportRdlSection(ao.RootElement);
				ExportWordLayoutSection(ao.RootElement);
			}
		});
		WriteLine();
	}

	private static void TranformToDevBaseFormat(ApplicationObject ao)
	{
		if (ao.ElementTypeInfo.ElementType == ElementType.Modification)
		{
			new DevBaseExportCompatibilityVisitor(ao.RootElement).Visit();
		}
	}

	private void ExportObjectProperties(ApplicationObject ao)
	{
		WriteSection("OBJECT-PROPERTIES", () =>
		{
			WriteLine("Date=" + (options.ClearObjectProperties ? string.Empty : FormatWithBrackets(ao.ObjectProperties.Date)) + ";");
			WriteLine("Time=" + (options.ClearObjectProperties ? string.Empty : FormatWithBrackets(ao.ObjectProperties.Time)) + ";");
			if (!options.ClearObjectProperties && !string.IsNullOrEmpty(ao.ObjectProperties.Modified))
			{
				WriteLine("Modified=" + ao.ObjectProperties.Modified + ";");
			}
			WriteLine("Version List=" + (options.ClearObjectProperties ? string.Empty : FormatWithBrackets(ao.ObjectProperties.VersionList)) + ";");
		});
	}

	private void ExportCodeSection(IElement element)
	{
		WriteSection("CODE", () =>
		{
			ExportVariables(element);
			WriteLine();
			foreach (IElement item in from m in element.GetSubElements()
				where m.ElementTypeInfo.ElementType == ElementType.Procedure || m.ElementTypeInfo.ElementType == ElementType.Event
				select m)
			{
				ExportFunctionHeader(item);
				ExportFunctionBody(item);
				WriteLine();
			}
			WriteLine("BEGIN");
			IElement element2 = element.GetSubElements(ElementType.Documentation).FirstOrDefault();
			if (element2 != null)
			{
				WriteLine('{');
				PushIndent(2);
				foreach (string codeLine in element2.GetCodeLines())
				{
					WriteLine(codeLine);
				}
				PopIndent();
				WriteLine('}');
			}
			WriteLine("END.");
		});
	}

	private void ExportRdlSection(IElement element)
	{
		WriteSection("RDLDATA", () =>
		{
			bool flag = false;
			PushIndent();
			SetIndent(0);
			foreach (string rdlDataLine in element.GetRdlDataLines())
			{
				WriteLine(rdlDataLine);
				flag = true;
			}
			PopIndent();
			if (flag)
			{
				WriteLine("END_OF_RDLDATA");
			}
		});
	}

	private void ExportWordLayoutSection(IElement element)
	{
		if (!element.GetWordLayoutLines().Any())
		{
			return;
		}
		WriteSection("WORDLAYOUT", () =>
		{
			bool flag = false;
			PushIndent();
			SetIndent(0);
			foreach (string wordLayoutLine in element.GetWordLayoutLines())
			{
				WriteLine(wordLayoutLine);
				flag = true;
			}
			PopIndent();
			if (flag)
			{
				WriteLine("END_OF_WORDLAYOUT");
			}
		});
	}

	private void ExportFunctionHeader(IElement function)
	{
		if (function.GetPropertySetState(PropertyType.Attributes))
		{
			string stringProperty = function.GetStringProperty(PropertyType.Attributes);
			if (!string.IsNullOrEmpty(stringProperty))
			{
				string[] array = stringProperty.Replace("][", "]|[").Split('|');
				foreach (string value in array)
				{
					WriteLine(value);
				}
			}
		}
		if (function.IsTrue(PropertyType.Local))
		{
			Write("LOCAL ");
		}
		if (function.ElementTypeInfo.ElementType == ElementType.Event)
		{
			Write("EVENT ");
			string stringProperty2 = function.GetStringProperty(PropertyType.EventSource);
			Write(TextUtilities.QuoteIdentifierIfNeeded(stringProperty2));
			Write("@" + function.GetStringProperty(PropertyType.EventSourceId) + "::");
		}
		else
		{
			Write("PROCEDURE ");
		}
		WriteIdentifierNameAndId(function);
		Write('(');
		bool flag = true;
		foreach (IElement subElement in function.GetSubElements(ElementType.Parameter))
		{
			if (!flag)
			{
				Write(';');
			}
			if (subElement.IsTrue(PropertyType.AsVar))
			{
				Write("VAR ");
			}
			WriteIdentifier(subElement);
			flag = false;
		}
		Write(')');
		IElement element = function.GetSubElements(ElementType.ReturnValue).FirstOrDefault();
		if (element != null)
		{
			WriteIdentifier(element, prependNameWithSpace: true);
		}
		WriteLine(';');
	}

	private void ExportParentElement(IElement element)
	{
		WriteSection("PROPERTIES", () =>
		{
			switch (ExportPropertiesAndTriggers(element, InlineItem.IdAndName, '\0', NextLinePos))
			{
			case LastExportedType.Trigger:
				WriteLine();
				break;
			default:
				WriteLine(';');
				break;
			case LastExportedType.None:
			case LastExportedType.Container:
				break;
			}
		});
		foreach (ElementRelationExport item in from s in ElementRelationExport.Definitions
			where s.ParentType == element.ElementTypeInfo.ElementType && s.ContainerProperty == PropertyType.Unknown && !string.IsNullOrEmpty(s.ContainerName)
			orderby s.ExportOrder
			select s)
		{
			ExportContainment(element, item);
		}
	}

	private LastExportedType ExportPropertiesAndTriggers(IElement element, InlineItem[] inlineProperties, char startWith, int propIndent)
	{
		LastExportedType result = LastExportedType.None;
		bool notFirstProperty = false;
		bool lastPropertyWasTrigger = false;
		bool lastPropertyTerminated = false;
		List<PropertyUsage> list = (from propertyUsage in PropertyUsage.Definitions
			where propertyUsage.ElementType == element.ElementTypeInfo.ElementType
			orderby propertyUsage.AttributeId
			select propertyUsage).ToList();
		List<ElementRelationExport> list2 = (from t in ElementRelationExport.Definitions
			where t.ParentType == element.ElementTypeInfo.ElementType && t.ChildCategory == CategoryType.Trigger && (t.ChildType != ElementType.Documentation || t.ParentType == ElementType.Change)
			orderby t.AttributeId
			select t).ToList();
		int num = 0;
		int num2 = 0;
		while (num < list.Count || num2 < list2.Count)
		{
			if (num < list.Count && (num2 >= list2.Count || list[num].AttributeId < list2[num2].AttributeId))
			{
				PropertyUsage p = list[num];
				if (!inlineProperties.HasProperty(p.PropertyType) && p.PropertyType != PropertyType.RdlData)
				{
					if (p.DataType == DataTypes.Container)
					{
						ElementRelationExport elementRelationExport = ElementRelationExport.Definitions.FirstOrDefault((ElementRelationExport r) => r.ContainerProperty == p.PropertyType && element.GetSubElementsCount(r.ChildType) > 0);
						if (elementRelationExport != null)
						{
							List<IElement> list3 = element.GetSubElements(elementRelationExport.ChildType).ToList();
							if (list3.Count > 0)
							{
								WritePropertySeparator(notFirstProperty, startWith, lastPropertyWasTrigger, lastPropertyTerminated, ref propIndent);
								Write((p.PropertyType == PropertyType.AddedChildren && !options.DevBaseCompatibility) ? "ChangedElements" : EnumNameLookup<PropertyType>.EnumToString(p.PropertyType));
								Write('=');
								switch (elementRelationExport.ChildType)
								{
								case ElementType.Variable:
									ExportVariables(element, elementRelationExport.ContainerName);
									break;
								case ElementType.ReturnValue:
									WriteLine(elementRelationExport.ContainerName);
									WriteLine('{');
									PushIndent(2);
									WriteIdentifier(list3.First(), prependNameWithSpace: true);
									WriteLine(';');
									PopIndent();
									WriteLine('}');
									break;
								case ElementType.Procedure:
								case ElementType.Event:
									WriteLine(elementRelationExport.ContainerName);
									WriteLine('{');
									PushIndent(2);
									foreach (IElement item in list3)
									{
										ExportFunctionHeader(item);
										ExportFunctionBody(item);
										WriteLine();
									}
									PopIndent();
									WriteLine('}');
									break;
								default:
									ExportContainment(element, elementRelationExport);
									break;
								}
								result = LastExportedType.Container;
								lastPropertyTerminated = true;
								notFirstProperty = true;
								lastPropertyWasTrigger = false;
							}
						}
					}
					else if (element.GetPropertySetState(p.PropertyType))
					{
						WritePropertySeparator(notFirstProperty, startWith, lastPropertyWasTrigger, lastPropertyTerminated, ref propIndent);
						Write(EnumNameLookup<PropertyType>.EnumToString(p.PropertyType));
						Write('=');
						WriteEmbracedProperty(element, p.PropertyType);
						result = LastExportedType.Normal;
						lastPropertyTerminated = false;
						notFirstProperty = true;
						lastPropertyWasTrigger = false;
					}
				}
				num++;
				continue;
			}
			ElementRelationExport elementRelationExport2 = list2[num2];
			foreach (IElement subElement in element.GetSubElements(elementRelationExport2.ChildType))
			{
				WritePropertySeparator(notFirstProperty, startWith, lastPropertyWasTrigger, lastPropertyTerminated, ref propIndent);
				string text = EnumNameLookup<ElementType>.EnumToString(elementRelationExport2.ChildType);
				Write(text);
				Write("=");
				SetIndent(propIndent + text.Length + 1);
				ExportFunctionBody(subElement);
				notFirstProperty = true;
				lastPropertyWasTrigger = true;
				lastPropertyTerminated = true;
				result = LastExportedType.Trigger;
			}
			num2++;
		}
		return result;
	}

	private void ExportFunctionBody(IElement function)
	{
		ExportVariables(function);
		WriteLine("BEGIN");
		PushIndent(2);
		foreach (string codeLine in function.GetCodeLines())
		{
			WriteLine(codeLine);
		}
		PopIndent();
		WriteLine("END;");
	}

	private void ExportVariables(IElement function, string containerName = null)
	{
		IElement[] array = function.GetSubElements(ElementType.Variable).ToArray();
		if (array.Any())
		{
			if (!string.IsNullOrEmpty(containerName))
			{
				WriteLine(containerName);
				WriteLine('{');
			}
			else
			{
				WriteLine("VAR");
			}
			PushIndent(2);
			IElement[] array2 = array;
			foreach (IElement ident in array2)
			{
				WriteIdentifier(ident);
				WriteLine(';');
			}
			PopIndent();
			if (!string.IsNullOrEmpty(containerName))
			{
				WriteLine('}');
			}
		}
	}

	private void ExportContainment(IElement element, ElementRelationExport section)
	{
		bool hasChildren = element.GetSubElements(section.ChildType).Any();
		if (section.SkipIfEmpty && !hasChildren)
		{
			return;
		}
		WriteSection(section.ContainerName, () =>
		{
			int nextLinePos = NextLinePos;
			if (hasChildren)
			{
				if (section.Multiplicity != MultiplicityType.ZeroToOne && section.Multiplicity != MultiplicityType.One)
				{
					InlineTemplate inlineTemplate = modelInfo.GetInlineTemplate(section.ChildType);
					{
						foreach (IElement subElement in element.GetSubElements(section.ChildType))
						{
							Write("{ ");
							bool flag = false;
							if (inlineTemplate == null)
							{
								ExportPropertiesAndTriggers(subElement, null, '\0', linePos);
							}
							else if (inlineTemplate.Items == null || inlineTemplate.Items.Length == 0)
							{
								ExportParentElement(subElement);
							}
							else
							{
								for (int i = 0; i < inlineTemplate.Items.Length - 1; i++)
								{
									if (i > 0)
									{
										Write(';');
									}
									WriteEmbracedProperty(subElement, inlineTemplate.Items[i].Property);
									WritePaddingTo(nextLinePos + inlineTemplate.Items[i].EndPos);
								}
								flag = inlineTemplate.Items[inlineTemplate.Items.Length - 1].EndPos < 0;
								int num = Math.Abs(inlineTemplate.Items[inlineTemplate.Items.Length - 1].EndPos) + nextLinePos;
								ExportPropertiesAndTriggers(subElement, inlineTemplate.Items, (linePos > num) ? '\\' : ';', num + 1);
							}
							WriteLine(" }");
							if (flag)
							{
								WriteLine();
							}
							SetIndent(nextLinePos);
						}
						return;
					}
				}
				ExportParentElement(element.GetFirstSubElement(section.ChildType));
			}
		});
	}

	private static bool NeedsBrackets(string value)
	{
		if (value[0] != ' ' && value[0] != '[' && value.IndexOfAny(NeedsBracketsChars) < 0)
		{
			if (value.Length > 1)
			{
				return value[value.Length - 1] == ' ';
			}
			return false;
		}
		return true;
	}

	private static string FormatWithBrackets(string value)
	{
		if (string.IsNullOrEmpty(value) || !NeedsBrackets(value))
		{
			return value;
		}
		return "[" + value.Replace("]", "]]") + "]";
	}

	private static void FormatIdentifierNameAndId(StringBuilder sb, IElement ident, bool prependNameWithSpace)
	{
		bool flag = prependNameWithSpace;
		if (ident.GetPropertySetState(PropertyType.Name))
		{
			if (prependNameWithSpace)
			{
				sb.Append(' ');
				prependNameWithSpace = false;
			}
			string stringProperty = ident.GetStringProperty(PropertyType.Name);
			if (TextUtilities.NeedsIdentifierQuotes(stringProperty))
			{
				sb.Append('"');
				sb.Append(stringProperty.Replace("\"", "\"\""));
				sb.Append('"');
			}
			else
			{
				sb.Append(stringProperty);
			}
		}
		if (!flag || ident.Id != 0)
		{
			if (prependNameWithSpace)
			{
				sb.Append(' ');
			}
			sb.Append('@');
			sb.Append(ident.Id);
		}
	}

	private void WriteIdentifierNameAndId(IElement ident, bool prependNameWithSpace = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FormatIdentifierNameAndId(stringBuilder, ident, prependNameWithSpace);
		Write(stringBuilder.ToString());
	}

	private void WriteIdentifier(IElement ident, bool prependNameWithSpace = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FormatIdentifierNameAndId(stringBuilder, ident, prependNameWithSpace);
		Write(stringBuilder.ToString());
		Write(" : ");
		if (ident.GetPropertySetState(PropertyType.Dimensions))
		{
			Write(string.Format(CultureInfo.InvariantCulture, "ARRAY [{0}] OF ", ident.GetStringProperty(PropertyType.Dimensions)));
		}
		if (ident.IsTrue(PropertyType.Temporary))
		{
			Write("TEMPORARY ");
		}
		string value = ident.GetStringProperty(PropertyType.DataType);
		switch (value.ToUpperInvariant())
		{
		case "TEXTCONST":
		{
			Write(value);
			if (!ident.GetPropertySetState(PropertyType.TextConstString))
			{
				break;
			}
			string text = ident.GetStringProperty(PropertyType.TextConstString).Replace("'", "''");
			if (options.Encoding.GetByteCount(text) < 1024)
			{
				Write(" '");
				Write(text);
				Write('\'');
				break;
			}
			PushIndent(2);
			WriteLine();
			bool flag = true;
			foreach (string item in ModelTextHelper.MLTextFormatter(ModelTextHelper.MLTextSplitter(text), 0))
			{
				if (!flag)
				{
					WriteLine(',');
				}
				Write('\'');
				Write(item);
				Write('\'');
				flag = false;
			}
			PopIndent();
			break;
		}
		case "OPTION":
			if (ident.GetPropertySetState(PropertyType.OptionString) && !string.IsNullOrEmpty(value = ident.GetStringProperty(PropertyType.OptionString)))
			{
				Write('\'');
				Write(value.Replace("'", "''"));
				Write('\'');
			}
			else
			{
				Write("Option");
			}
			break;
		case "AUTOMATION":
		case "OCX":
		case "DOTNET":
		{
			Write(value);
			string stringProperty = ident.GetStringProperty(PropertyType.ExternalDataType);
			if (stringProperty != null)
			{
				Write(" \"");
				Write(stringProperty);
				Write('"');
				if (ident.IsTrue(PropertyType.WithEvents))
				{
					Write(" WITHEVENTS");
				}
				if (ident.IsTrue(PropertyType.RunOnClient))
				{
					Write(" RUNONCLIENT");
				}
				if (ident.IsTrue(PropertyType.SuppressDispose))
				{
					Write(" SUPPRESSDISPOSE");
				}
			}
			break;
		}
		case "XMLPORT":
			Write("XMLport");
			break;
		default:
			Write(value);
			break;
		}
		if (ident.GetPropertySetState(PropertyType.Length))
		{
			Write(string.Format(CultureInfo.InvariantCulture, "[{0}]", ident.GetInt32Property(PropertyType.Length)));
		}
		if (ident.GetPropertySetState(PropertyType.Subtype))
		{
			Write(' ');
			Write(ident.GetStringProperty(PropertyType.Subtype));
		}
		if (ident.IsTrue(PropertyType.InDataSet))
		{
			Write(" INDATASET");
		}
		if (ident.TryGetStringProperty(PropertyType.SecurityFiltering, out value) && !string.IsNullOrEmpty(value))
		{
			Write(string.Format(CultureInfo.InvariantCulture, " SECURITYFILTERING({0})", value));
		}
		if (ident.IsTrue(PropertyType.NonDebuggable))
		{
			Write(" NonDebuggable");
		}
	}

	private void WriteEmbracedProperty(IElement element, PropertyType propertyType)
	{
		string text = element.GetStringProperty(propertyType);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		bool flag = NeedsBrackets(text);
		if (flag)
		{
			Write('[');
			text = text.Replace("]", "]]");
		}
		if (PropertyTypeInfo.Lookup.GetValue(propertyType).DataType == DataTypes.MLText)
		{
			PushIndent();
			int num = linePos;
			SetIndent(num);
			bool flag2 = true;
			foreach (string item in ModelTextHelper.MLTextFormatter(ModelTextHelper.MLTextSplitter(text), 0))
			{
				if (!flag2)
				{
					WriteLine(';');
				}
				Write(item);
				flag2 = false;
			}
			PopIndent();
		}
		else
		{
			int num2 = text.IndexOf('\n');
			if (num2 >= 0)
			{
				PushIndent();
				int num3 = linePos;
				SetIndent(num3);
				int num4 = 0;
				while (num2 >= 0)
				{
					string value = text.Substring(num4, num2 - num4 - ((num4 + num2 > 0 && text[num2 - 1] == '\r') ? 1 : 0));
					WriteLine(value);
					num4 = num2 + 1;
					num2 = text.IndexOf('\n', num4);
				}
				Write(text.Substring(num4));
				PopIndent();
			}
			else
			{
				Write(text);
			}
		}
		if (flag)
		{
			Write(']');
		}
	}

	private void WritePaddingTo(int pos)
	{
		while (linePos < pos)
		{
			LowWrite(" ");
		}
	}

	private void WritePropertySeparator(bool notFirstProperty, char startWith, bool lastPropertyWasTrigger, bool lastPropertyTerminated, ref int propIndent)
	{
		if (notFirstProperty)
		{
			if (lastPropertyWasTrigger)
			{
				WriteLine();
			}
			if (!lastPropertyTerminated)
			{
				WriteLine(';');
			}
			SetIndent(propIndent);
			return;
		}
		switch (startWith)
		{
		case ';':
			Write(';');
			break;
		case '\\':
			WriteLine(';');
			SetIndent(propIndent);
			break;
		default:
			propIndent = NextLinePos;
			break;
		}
	}

	private void WriteSection(string name, Action action)
	{
		WriteLine(name);
		WriteLine('{');
		PushIndent(2);
		action();
		PopIndent();
		WriteLine('}');
	}

	private void LowWrite(string value)
	{
		destinationStreamWriter.Write(value);
		linePos += value.Length;
	}

	private void LowWriteLine()
	{
		destinationStreamWriter.WriteLine();
		linePos = 0;
	}

	private void WriteIndent()
	{
		if (writeIndent)
		{
			LowWrite(indent);
			writeIndent = false;
		}
	}

	private void Write(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			WriteIndent();
		}
		LowWrite(value);
	}

	private void Write(char value)
	{
		WriteIndent();
		LowWrite(value.ToString());
	}

	private void WriteLine(string value)
	{
		Write(value);
		LowWriteLine();
		writeIndent = true;
	}

	private void WriteLine(char value)
	{
		Write(value);
		LowWriteLine();
		writeIndent = true;
	}

	private void WriteLine()
	{
		LowWriteLine();
		writeIndent = true;
	}

	private void PushIndent(int increment)
	{
		indentStack.Push(indent);
		indent = new string(' ', indent.Length + increment);
	}

	private void PushIndent()
	{
		indentStack.Push(indent);
	}

	private void SetIndent(int value)
	{
		indent = new string(' ', value);
	}

	private void PopIndent()
	{
		indent = indentStack.Pop();
	}
}
