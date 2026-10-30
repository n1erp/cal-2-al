using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public sealed class TxtImporter
{
	private readonly TxtFileModelInfo modelInfo;

	private readonly TxtImportOptions options;

	private static readonly TxtImportOptions DefaultOptions = new TxtImportOptions();

	private readonly StringBuilder stringBuilder = new StringBuilder(1024);

	private TxtFileTokenizer tokenizer;

	private Token token;

	private ElementType currentObjectType;

	private int currentObjectId;

	public TxtImporter(TxtFileModelInfo modelInfo)
		: this(modelInfo, DefaultOptions)
	{
	}

	public TxtImporter(TxtFileModelInfo modelInfo, TxtImportOptions options)
	{
		this.modelInfo = modelInfo;
		this.options = options ?? DefaultOptions;
	}

	public List<ApplicationObject> ImportFromStream(Stream stream)
	{
		tokenizer = new TxtFileTokenizer(stream, options.Encoding);
		List<ApplicationObject> list = new List<ApplicationObject>();
		UpdateProgress(stream.Position, stream.Length);
		while (NextToken() == Token.Object)
		{
			token = NextToken();
			while (true)
			{
				try
				{
					list.Add(ImportObject());
				}
				catch (TxtImportException ex)
				{
					if (options.IncludeInvalidObjects)
					{
						list.Add(new InvalidApplicationObject(ex.ObjectType, ex.ObjectId, ex.Message, ex.LineNo, ex.LinePos));
					}
					if (!Warning(ex.LineNo, ex.LinePos, ex.ObjectType, ex.ObjectId, ex.Message))
					{
						throw;
					}
					if (RecoverFromFailureToNextObject())
					{
						continue;
					}
				}
				break;
			}
			UpdateProgress(stream.Position, stream.Length);
		}
		AssertToken(Token.EndOfStream);
		UpdateProgress(stream.Position, stream.Length);
		return list;
	}

	private bool RecoverFromFailureToNextObject()
	{
		while (!tokenizer.EndOfStream)
		{
			if ((token = tokenizer.NextToken()) == Token.Object && tokenizer.GetTokenLinePos() == 0 && (token = tokenizer.NextToken()) != Token.Minus)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateProgress(long progress, long total)
	{
		if (options.ProgressAction != null)
		{
			options.ProgressAction(progress, total);
		}
	}

	private bool Warning(int lineNo, int linePos, ElementType objectType, int objectId, string message)
	{
		if (options.WarningAction != null)
		{
			options.WarningAction(lineNo, linePos, objectType, objectId, message);
			return true;
		}
		return false;
	}

	private ApplicationObject ImportObject()
	{
		IElement element = null;
		currentObjectType = ElementType.Unknown;
		currentObjectId = 0;
		try
		{
			bool flag = false;
			if (token == Token.OpenSquareBracket)
			{
				flag = true;
				NextToken();
			}
			bool flag2 = false;
			switch (token)
			{
			case Token.Codeunit:
				element = GenericElement.Create(ElementType.Codeunit);
				break;
			case Token.Table:
				element = GenericElement.Create(ElementType.Table);
				break;
			case Token.Page:
				element = GenericElement.Create(ElementType.Page);
				break;
			case Token.Query:
				element = GenericElement.Create(ElementType.Query);
				break;
			case Token.Report:
				element = GenericElement.Create(ElementType.Report);
				break;
			case Token.XmlPort:
				element = GenericElement.Create(ElementType.XmlPort);
				break;
			case Token.MenuSuite:
				element = GenericElement.Create(ElementType.MenuSuite);
				break;
			case Token.Modification:
				element = GenericElement.Create(ElementType.Modification);
				break;
			case Token.Dataport:
				element = GenericElement.Create(ElementType.Dataport);
				flag2 = true;
				break;
			case Token.Form:
				element = GenericElement.Create(ElementType.Form);
				flag2 = true;
				break;
			default:
				ThrowImportException(NonLocalizable.TxtImport_ObjectTypeExpected);
				return null;
			}
			currentObjectType = element.ElementTypeInfo.ElementType;
			if (element.ElementTypeInfo.ElementType != ElementType.Modification)
			{
				AssertNextToken(Token.Number);
				currentObjectId = tokenizer.TokenAsInt32;
				element.SetInt32Property(PropertyType.Id, currentObjectId);
			}
			if (flag2)
			{
				ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_ObjectTypeNotSupported, element.ElementTypeInfo.Name));
			}
			string name = string.Empty;
			if (flag)
			{
				tokenizer.RunWithTokenizerOptions(TokenizerOptions.None, () =>
				{
					token = tokenizer.ReadUntilToken(out name, Token.CloseSquareBracket, Token.EndOfLine);
				});
				AssertToken(Token.CloseSquareBracket);
				name = name.Substring(1);
			}
			else
			{
				name = tokenizer.ReadToEndOfLineAsString().Trim();
			}
			element.SetStringProperty(PropertyType.Name, name);
			ApplicationObject applicationObject = new ApplicationObject(element);
			AssertNextToken(Token.OpenCurlyBracket);
			if (NextToken() == Token.Object)
			{
				ParseObjectProperties(applicationObject);
				NextToken();
			}
			ParseElement(element);
			if (options.DevBaseCompatibility)
			{
				new DevBaseImportCompatibilityVisitor(element).Visit();
			}
			return applicationObject;
		}
		catch (TxtImportException ex)
		{
			if (element != null)
			{
				ex.ObjectType = element.ElementTypeInfo.ElementType;
				ex.ObjectId = element.Id;
			}
			throw;
		}
		catch (Exception ex2)
		{
			throw new TxtImportException(tokenizer.LineNo, tokenizer.LinePos, tokenizer.GetTokenLine(), ex2.Message, ex2, element?.ElementTypeInfo.ElementType ?? ElementType.Unknown, element?.Id ?? 0);
		}
	}

	private void ParseElement(IElement element)
	{
		if (token == Token.Properties)
		{
			AssertNextToken(Token.OpenCurlyBracket);
			ParsePropertiesOrTriggers(element);
			NextToken();
		}
		while (!tokenizer.EndOfStream && token != Token.Code && token != Token.CloseCurlyBracket)
		{
			AssertToken(Token.Identifier);
			string tokenAsString = tokenizer.TokenAsString;
			if (!modelInfo.GetScopedElementRelationLookup(element).TryGetValue(tokenAsString, out var value))
			{
				StringBuilder stringBuilder = new StringBuilder(string.Join(", ", from r in element.ElementTypeInfo.Relations.Values
					where r.ContainerName != null && element.GetSubElementsCount(r.ChildType) == 0
					select r.ContainerName));
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_CodeOrEndCurlyExpected, stringBuilder));
			}
			if (element.GetSubElementsCount(value.ChildType) > 0)
			{
				ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_SectionIsAlreadyImported, value.ContainerName));
			}
			ParseContainment(element, value);
			NextToken();
		}
		if (token == Token.Code)
		{
			ParseCode(element);
			NextToken();
		}
		if (token == Token.RdlData && element.ElementTypeInfo.ElementType == ElementType.Report)
		{
			ParseRdlData(element);
			NextToken();
		}
		if (token == Token.WordLayout && element.ElementTypeInfo.ElementType == ElementType.Report)
		{
			ParseWordLayout(element);
			NextToken();
		}
		AssertToken(Token.CloseCurlyBracket);
	}

	private void ThrowImportException(string message)
	{
		throw new TxtImportException(tokenizer.LineNo, tokenizer.LinePos, tokenizer.GetTokenLine(), message);
	}

	private void ThrowExpectedTokenTextImportException(string expectedTokenText)
	{
		ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_TokenExpected, expectedTokenText));
	}

	private void ParseContainment(IElement parent, ElementRelationExport relation)
	{
		AssertNextToken(Token.OpenCurlyBracket);
		NextToken();
		if (relation.Unsupported)
		{
			ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_ElementContainerNotSupported, relation.ContainerName));
		}
		if (relation.MultiplicityIsSingle)
		{
			IElement element = GenericElement.Create(relation.ChildType);
			parent.AddSubElement(element);
			ParseElement(element);
		}
		else
		{
			InlineTemplate inlineTemplate = modelInfo.GetInlineTemplate(relation.ChildType);
			while (!tokenizer.EndOfStream && token == Token.OpenCurlyBracket)
			{
				IElement element = GenericElement.Create(relation.ChildType);
				parent.AddSubElement(element);
				if (inlineTemplate != null && inlineTemplate.Items.Length == 0)
				{
					NextToken();
					ParseElement(element);
				}
				else
				{
					if (inlineTemplate != null)
					{
						InlineItem[] items = inlineTemplate.Items;
						for (int i = 0; i < items.Length; i++)
						{
							InlineItem inlineItem = items[i];
							if (inlineItem.Property == PropertyType.Unknown)
							{
								break;
							}
							string text = ReadPropertyValue();
							if (text.Length > 0)
							{
								element.SetStringProperty(inlineItem.Property, text);
							}
						}
					}
					if (inlineTemplate == null || token == Token.Semicolon)
					{
						ParsePropertiesOrTriggers(element);
					}
				}
				NextToken();
			}
		}
		AssertToken(Token.CloseCurlyBracket);
	}

	private void ParseObjectProperties(ApplicationObject parent)
	{
		PropertyType[] source = new PropertyType[4]
		{
			PropertyType.Date,
			PropertyType.Time,
			PropertyType.Modified,
			PropertyType.VersionList
		};
		if (tokenizer.ReadToEndOfLineAsString().Trim() != "-PROPERTIES")
		{
			ThrowExpectedTokenTextImportException("OBJECT-PROPERTIES");
		}
		AssertNextToken(Token.OpenCurlyBracket);
		while (true)
		{
			string key = ReadPropertyName();
			if (token == Token.CloseCurlyBracket)
			{
				break;
			}
			if (!EnumNameLookup<PropertyType>.TryStringToEnum(key, out var value) && !source.Contains(value))
			{
				UnexpectedToken();
			}
			AssertToken(Token.Equal);
			string text = ReadPropertyValue();
			switch (value)
			{
			case PropertyType.Date:
				parent.ObjectProperties.Date = text;
				break;
			case PropertyType.Time:
				parent.ObjectProperties.Time = text;
				break;
			case PropertyType.VersionList:
				parent.ObjectProperties.VersionList = text;
				break;
			case PropertyType.Modified:
				parent.ObjectProperties.Modified = text;
				break;
			}
		}
		AssertToken(Token.CloseCurlyBracket);
	}

	private void ParsePropertiesOrTriggers(IElement parent)
	{
		ElementRelationExportLookup elementRelationExportLookup = null;
		PropertyUsageLookup propertyUsageLookup = null;
		string text;
		while (true)
		{
			text = ReadPropertyName();
			if (token != Token.Equal)
			{
				AssertToken(Token.CloseCurlyBracket);
				return;
			}
			if (propertyUsageLookup == null)
			{
				propertyUsageLookup = modelInfo.GetScopedPropertyUsageLookup(parent);
			}
			if (!options.DevBaseCompatibility && text == "ChangedElements")
			{
				text = EnumNameLookup<PropertyType>.EnumToString(PropertyType.AddedChildren);
			}
			ElementRelationExport value2;
			if (propertyUsageLookup.TryGetValue(text, out var value))
			{
				if (value.DataType == DataTypes.Container)
				{
					NextToken();
					string tokenAsString = tokenizer.TokenAsString;
					if (elementRelationExportLookup == null)
					{
						elementRelationExportLookup = modelInfo.GetScopedElementRelationLookup(parent);
					}
					if (!elementRelationExportLookup.TryGetValue(tokenAsString, out value2))
					{
						ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_InvalidPropertyOrTriggerName, tokenAsString, EnumNameLookup<ElementType>.EnumToString(parent.ElementTypeInfo.ElementType)));
						return;
					}
					switch (value2.ChildType)
					{
					case ElementType.ReturnValue:
					{
						AssertNextToken(Token.OpenCurlyBracket);
						NextToken();
						IElement element = GenericElement.Create(ElementType.ReturnValue);
						parent.AddSubElement(element);
						ParseVariableOrParameter(element);
						AssertNextToken(Token.CloseCurlyBracket);
						break;
					}
					case ElementType.Variable:
						AssertNextToken(Token.OpenCurlyBracket);
						ParseVariables(parent);
						parent.SetStringProperty(PropertyType.AddedChildren, value2.ContainerName);
						break;
					case ElementType.Procedure:
					case ElementType.Event:
						AssertNextToken(Token.OpenCurlyBracket);
						NextToken();
						do
						{
							ParseMethod(parent);
						}
						while (token != Token.CloseCurlyBracket);
						break;
					default:
						ParseContainment(parent, value2);
						break;
					}
					if (value2.ParentType == ElementType.Change)
					{
						parent.SetStringProperty(PropertyType.AddedChildren, value2.ContainerName);
					}
				}
				else
				{
					string value3 = ReadPropertyValue(preserveFormatting: true);
					if (!value.Obsolete)
					{
						parent.SetStringProperty(value.PropertyType, value3);
					}
					else
					{
						Warning(tokenizer.LineNo, tokenizer.LinePos, currentObjectType, currentObjectId, string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_ObsoletePropertyName, EnumNameLookup<PropertyType>.EnumToString(value.PropertyType), parent.ElementTypeInfo.Name));
					}
					if (token == Token.CloseCurlyBracket)
					{
						return;
					}
				}
			}
			else
			{
				if (elementRelationExportLookup == null)
				{
					elementRelationExportLookup = modelInfo.GetScopedElementRelationLookup(parent);
				}
				if (!elementRelationExportLookup.TryGetValue(text, out value2))
				{
					break;
				}
				if (value2.Child.Category != CategoryType.Trigger)
				{
					ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_InvalidPropertyOrTriggerName, text, EnumNameLookup<ElementType>.EnumToString(parent.ElementTypeInfo.ElementType)));
					return;
				}
				IElement element2 = GenericElement.Create(value2.Child);
				if (!value2.Obsolete)
				{
					parent.AddSubElement(element2);
				}
				else
				{
					Warning(tokenizer.LineNo, tokenizer.LinePos, currentObjectType, currentObjectId, string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_ObsoleteTriggerName, value2.Child.Name, value2.Parent.Name));
				}
				ParseTriggerCode(element2);
			}
		}
		ThrowImportException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_InvalidPropertyOrTriggerName, text, EnumNameLookup<ElementType>.EnumToString(parent.ElementTypeInfo.ElementType)));
	}

	private void ParseTriggerCode(IElement trigger)
	{
		if (NextToken() == Token.Var)
		{
			ParseVariables(trigger);
		}
		AssertToken(Token.Begin);
		ParseCodelines(trigger);
	}

	private void ParseVariables(IElement parent)
	{
		while (true)
		{
			NextToken();
			if (token != Token.Semicolon)
			{
				if (token < Token.Identifier && token != Token.At && token != Token.Colon)
				{
					break;
				}
				IElement element = GenericElement.Create(ElementType.Variable);
				parent.AddSubElement(element);
				ParseVariableOrParameter(element);
				AssertToken(Token.Semicolon);
			}
		}
	}

	private void ParseVariableOrParameter(IElement variable)
	{
		if (this.token >= Token.Identifier)
		{
			string tokenAsProcessedString = tokenizer.TokenAsProcessedString;
			if (tokenAsProcessedString.Length > 0)
			{
				variable.SetStringProperty(PropertyType.Name, tokenAsProcessedString);
			}
			NextToken();
		}
		if (this.token == Token.At)
		{
			int value = ReadIdentifierId();
			variable.SetInt32Property(PropertyType.Id, value);
			NextToken();
		}
		AssertToken(Token.Colon);
		NextToken();
		if (this.token == Token.Array)
		{
			AssertNextToken(Token.OpenSquareBracket);
			this.token = tokenizer.ReadUntilToken(out var result, Token.CloseSquareBracket);
			AssertToken(Token.CloseSquareBracket);
			variable.SetStringProperty(PropertyType.Dimensions, result);
			AssertNextToken(Token.Of);
			NextToken();
		}
		if (this.token == Token.Temporary)
		{
			variable.SetStringProperty(PropertyType.Temporary, "Yes");
			AssertNextToken(Token.Record);
		}
		Token token = this.token;
		string text = null;
		switch (this.token)
		{
		case Token.String:
			text = "Option";
			variable.SetStringProperty(PropertyType.OptionString, tokenizer.TokenAsProcessedString);
			NextToken();
			break;
		case Token.TextConst:
			if (NextToken() != Token.Semicolon)
			{
				AssertToken(Token.String);
				string text2 = tokenizer.TokenAsProcessedString;
				while (NextToken() == Token.Comma)
				{
					AssertNextToken(Token.String);
					text2 = text2 + ";" + tokenizer.TokenAsProcessedString;
				}
				variable.SetStringProperty(PropertyType.TextConstString, text2);
			}
			break;
		case Token.Codeunit:
		case Token.Record:
		case Token.Form:
		case Token.Report:
		case Token.Dataport:
		case Token.XmlPort:
		case Token.Page:
		case Token.Query:
		case Token.TestPage:
		case Token.TestRequestPage:
			NextToken();
			if (this.token == Token.Number)
			{
				int tokenAsInt = tokenizer.TokenAsInt32;
				variable.SetInt32Property(PropertyType.Subtype, tokenAsInt);
			}
			else
			{
				AssertIdentifier();
				string tokenAsString = tokenizer.TokenAsString;
				variable.SetStringProperty(PropertyType.Subtype, tokenAsString);
			}
			NextToken();
			break;
		case Token.DotNet:
		case Token.OCX:
		case Token.Automation:
			NextToken();
			if (this.token == Token.Semicolon)
			{
				break;
			}
			AssertIdentifier();
			variable.SetStringProperty(PropertyType.ExternalDataType, tokenizer.TokenAsProcessedString);
			while (NextToken() >= Token.Identifier)
			{
				switch (this.token)
				{
				case Token.RunOnClient:
					variable.SetStringProperty(PropertyType.RunOnClient, "Yes");
					break;
				case Token.WithEvents:
					variable.SetStringProperty(PropertyType.WithEvents, "Yes");
					break;
				case Token.SuppressDispose:
					variable.SetStringProperty(PropertyType.SuppressDispose, "Yes");
					break;
				case Token.NonDebuggable:
					variable.SetStringProperty(PropertyType.NonDebuggable, "Yes");
					break;
				default:
					AssertToken(Token.Semicolon);
					return;
				}
			}
			break;
		default:
			text = tokenizer.TokenAsString;
			if (NextToken() == Token.OpenSquareBracket)
			{
				AssertNextToken(Token.Number);
				variable.SetInt32Property(PropertyType.Length, tokenizer.TokenAsInt32);
				AssertNextToken(Token.CloseSquareBracket);
				NextToken();
			}
			break;
		}
		if (text == null)
		{
			text = token.ToString();
		}
		variable.SetStringProperty(PropertyType.DataType, text);
		if (this.token == Token.InDataSet)
		{
			if (variable.SupportsPropertyType(PropertyType.InDataSet))
			{
				variable.SetStringProperty(PropertyType.InDataSet, "Yes");
			}
			NextToken();
		}
		if (this.token == Token.SecurityFiltering)
		{
			AssertNextToken(Token.OpenParenthesis);
			AssertNextToken(Token.Identifier);
			if (variable.SupportsPropertyType(PropertyType.SecurityFiltering))
			{
				variable.SetStringProperty(PropertyType.SecurityFiltering, tokenizer.TokenAsString);
			}
			AssertNextToken(Token.CloseParenthesis);
			NextToken();
		}
		if (this.token == Token.NonDebuggable)
		{
			if (variable.SupportsPropertyType(PropertyType.NonDebuggable))
			{
				variable.SetStringProperty(PropertyType.NonDebuggable, "Yes");
			}
			NextToken();
		}
	}

	private void ParseCodelines(IElement method)
	{
		TokenizerOptions tokenizerOptions = TokenizerOptions.SkipWhiteSpace | TokenizerOptions.NoPingProcessing;
		int lineIndentation = tokenizer.LineIndentation;
		token = tokenizer.NextToken(tokenizerOptions);
		AssertToken(Token.EndOfLine);
		List<string> list = new List<string>(50);
		while (!tokenizer.EndOfStream)
		{
			token = tokenizer.NextToken(tokenizerOptions);
			if (token == Token.End && tokenizer.LineIndentation == lineIndentation)
			{
				AssertNextToken(Token.Semicolon);
				break;
			}
			if (token == Token.EndOfLine)
			{
				list.Add(TrimMax(tokenizer.GetTokenLine().TrimEnd('\r', '\n'), lineIndentation + 2));
			}
		}
		if (list.Count > 0)
		{
			method.SetCodeLines(list);
		}
	}

	private void ParseCode(IElement result)
	{
		AssertNextToken(Token.OpenCurlyBracket);
		if (NextToken() == Token.CloseCurlyBracket)
		{
			return;
		}
		if (token == Token.Var)
		{
			ParseVariables(result);
		}
		while (token != Token.Begin && token != Token.CloseCurlyBracket && !tokenizer.EndOfStream)
		{
			ParseMethod(result);
		}
		if (NextToken() == Token.OpenCurlyBracket)
		{
			token = tokenizer.NextToken(TokenizerOptions.SkipWhiteSpace);
			AssertToken(Token.EndOfLine);
			List<string> list = null;
			while (true)
			{
				string text = tokenizer.ReadToEndOfLineAsString();
				if (string.Equals(text.TrimEnd(), "    }", StringComparison.Ordinal))
				{
					break;
				}
				if (list == null)
				{
					list = new List<string>(50);
				}
				list.Add(TrimMax(text, 6));
			}
			if (list != null)
			{
				IElement element = GenericElement.Create(ElementType.Documentation);
				result.AddSubElement(element);
				element.SetCodeLines(list);
			}
			NextToken();
		}
		AssertToken(Token.End);
		AssertNextToken(Token.Point);
		AssertNextToken(Token.CloseCurlyBracket);
	}

	private void ParseRdlData(IElement report)
	{
		List<string> list = new List<string>(1024);
		AssertNextToken(Token.OpenCurlyBracket);
		NextToken(TokenizerOptions.SkipWhiteSpace);
		AssertToken(Token.EndOfLine);
		string text = tokenizer.ReadToEndOfLineAsString();
		if (text.IndexOf("<?xml", StringComparison.OrdinalIgnoreCase) < 0)
		{
			if (text.Trim() != "}")
			{
				ThrowImportException(NonLocalizable.TxtImport_ReportLayoutExpected);
			}
			return;
		}
		list.Add(text);
		while (!tokenizer.EndOfStream)
		{
			text = tokenizer.ReadToEndOfLineAsString();
			list.Add(text);
			if (text.Length >= 9 && text[text.Length - 1] == '>' && text[text.Length - 9] == '<' && (ushort)(text[text.Length - 7] | 0x20) == 114 && text.Trim().ToUpperInvariant() == "</REPORT>")
			{
				break;
			}
		}
		report.SetRdlDataLines(list);
		AssertNextToken(Token.Identifier);
		if (tokenizer.TokenAsString.ToUpperInvariant() != "END_OF_RDLDATA")
		{
			ThrowExpectedTokenTextImportException("END_OF_RDLDATA");
		}
		else
		{
			AssertNextToken(Token.CloseCurlyBracket);
		}
	}

	private void ParseWordLayout(IElement report)
	{
		List<string> list = new List<string>(1024);
		AssertNextToken(Token.OpenCurlyBracket);
		NextToken(TokenizerOptions.SkipWhiteSpace);
		AssertToken(Token.EndOfLine);
		string text = tokenizer.ReadToEndOfLineAsString();
		if (!(text.Trim() == "}"))
		{
			while (text.Trim().ToUpperInvariant() != "END_OF_WORDLAYOUT" && !tokenizer.EndOfStream)
			{
				list.Add(text);
				text = tokenizer.ReadToEndOfLineAsString();
			}
			report.SetWordLayoutLines(list);
			if (tokenizer.EndOfStream)
			{
				ThrowExpectedTokenTextImportException("END_OF_WORDLAYOUT");
			}
			AssertNextToken(Token.CloseCurlyBracket);
		}
	}

	private void ParseMethod(IElement parent)
	{
		string text = null;
		bool flag = false;
		if (token == Token.OpenSquareBracket)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			while (token == Token.OpenSquareBracket)
			{
				stringBuilder.Append('[');
				stringBuilder.Append(ReadAttribute());
				stringBuilder.Append(']');
				NextToken();
			}
			text = stringBuilder.ToString();
		}
		if (token == Token.Local)
		{
			flag = true;
			NextToken();
		}
		ElementType elementType;
		switch (token)
		{
		case Token.Procedure:
			elementType = ElementType.Procedure;
			break;
		case Token.Event:
			elementType = ElementType.Event;
			break;
		default:
			AssertTokens(Token.Procedure, Token.Event);
			return;
		}
		IElement element = GenericElement.Create(elementType);
		parent.AddSubElement(element);
		if (text != null)
		{
			element.SetStringProperty(PropertyType.Attributes, text);
		}
		if (flag)
		{
			element.SetStringProperty(PropertyType.Local, "Yes");
		}
		if (elementType == ElementType.Event)
		{
			NextToken();
			AssertIdentifier();
			element.SetStringProperty(PropertyType.EventSource, tokenizer.TokenAsProcessedString);
			AssertNextToken(Token.At);
			int value = ReadIdentifierId();
			element.SetInt32Property(PropertyType.EventSourceId, value);
			AssertNextToken(Token.Colon);
			AssertNextToken(Token.Colon);
		}
		NextToken();
		AssertIdentifier();
		string tokenAsProcessedString = tokenizer.TokenAsProcessedString;
		element.SetStringProperty(PropertyType.Name, tokenAsProcessedString);
		NextToken();
		if (token == Token.At)
		{
			int value2 = ReadIdentifierId();
			element.SetInt32Property(PropertyType.Id, value2);
			NextToken();
		}
		if (token == Token.OpenParenthesis)
		{
			if (NextToken() != Token.CloseParenthesis)
			{
				while (true)
				{
					IElement element2 = GenericElement.Create(ElementType.Parameter);
					element.AddSubElement(element2);
					if (token == Token.Var)
					{
						element2.SetStringProperty(PropertyType.AsVar, "Yes");
						NextToken();
					}
					ParseVariableOrParameter(element2);
					if (token == Token.CloseParenthesis)
					{
						break;
					}
					AssertToken(Token.Semicolon);
					NextToken();
				}
			}
			NextToken();
		}
		if (token != Token.Semicolon)
		{
			IElement element3 = GenericElement.Create(ElementType.ReturnValue);
			element.AddSubElement(element3);
			ParseVariableOrParameter(element3);
		}
		AssertToken(Token.Semicolon);
		ParseMethodBody(element);
		NextToken();
	}

	private void ParseMethodBody(IElement method)
	{
		if (NextToken() == Token.Var)
		{
			ParseVariables(method);
		}
		AssertToken(Token.Begin);
		ParseCodelines(method);
	}

	private string ReadPropertyName()
	{
		token = tokenizer.ReadUntilToken(out var result, Token.Equal, Token.CloseCurlyBracket, Token.EndOfLine);
		if (result.Length > 0)
		{
			AssertToken(Token.Equal);
		}
		return result;
	}

	private int ReadIdentifierId()
	{
		NextToken();
		if (token == Token.Minus)
		{
			AssertNextToken(Token.Number);
			return -tokenizer.TokenAsInt32;
		}
		AssertToken(Token.Number);
		return tokenizer.TokenAsInt32;
	}

	private string ReadAttribute()
	{
		token = tokenizer.ReadUntilToken(out var result, Token.CloseSquareBracket, Token.EndOfLine);
		AssertToken(Token.CloseSquareBracket);
		return result;
	}

	private string ReadPropertyValue(bool preserveFormatting = false)
	{
		stringBuilder.Clear();
		TokenizerOptions tokenOptions = tokenizer.TokenOptions;
		try
		{
			bool flag = true;
			tokenizer.TokenOptions = TokenizerOptions.SkipWhiteSpace | TokenizerOptions.SkipLineBreak | TokenizerOptions.NoPingProcessing | TokenizerOptions.NoKeywordLookup;
			NextToken();
			string result;
			int num;
			if (this.token == Token.OpenSquareBracket)
			{
				num = tokenizer.LineIndentation + 1;
				tokenizer.TokenOptions = TokenizerOptions.NoPingProcessing | TokenizerOptions.NoKeywordLookup;
				Token token = Token.None;
				while (!tokenizer.EndOfStream)
				{
					this.token = tokenizer.ReadUntilToken(out result, Token.EndOfLine, Token.CloseSquareBracket);
					if (preserveFormatting && !flag && token == Token.EndOfLine)
					{
						stringBuilder.AppendLine();
						result = ((result.Length > num) ? result.Substring(num) : string.Empty);
					}
					token = this.token;
					stringBuilder.Append(result);
					tokenizer.TokenOptions = TokenizerOptions.NoPingProcessing | TokenizerOptions.NoKeywordLookup;
					if (this.token == Token.CloseSquareBracket)
					{
						if (NextToken() != Token.CloseSquareBracket)
						{
							if (this.token == Token.WhiteSpace || this.token == Token.EndOfLine)
							{
								NextToken(tokenOptions);
							}
							if (this.token != Token.Semicolon && this.token != Token.CloseCurlyBracket)
							{
								AssertTokens(Token.Semicolon, Token.CloseCurlyBracket);
							}
							return stringBuilder.ToString();
						}
						stringBuilder.Append(']');
					}
					if (!preserveFormatting)
					{
						tokenizer.TokenOptions |= TokenizerOptions.SkipWhiteSpace | TokenizerOptions.SkipLineBreak;
					}
					flag = false;
				}
				UnexpectedToken();
				return null;
			}
			if (this.token == Token.Semicolon || this.token == Token.CloseCurlyBracket)
			{
				return string.Empty;
			}
			stringBuilder.Append(tokenizer.TokenAsString);
			tokenizer.TokenOptions = TokenizerOptions.TrimTrailingWhiteSpace | TokenizerOptions.NoPingProcessing | TokenizerOptions.NoKeywordLookup;
			num = tokenizer.LineIndentation;
			while (!tokenizer.EndOfStream)
			{
				this.token = tokenizer.ReadUntilToken(out result, Token.EndOfLine, Token.Semicolon, Token.CloseCurlyBracket);
				if (preserveFormatting && !flag)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append((result.Length > num) ? result.Substring(num) : string.Empty);
				}
				else
				{
					stringBuilder.Append(result);
				}
				flag = false;
				if (this.token != Token.EndOfLine)
				{
					return stringBuilder.ToString();
				}
				if (!preserveFormatting)
				{
					tokenizer.TokenOptions |= TokenizerOptions.SkipLeadingWhiteSpace;
				}
			}
			UnexpectedToken();
			return null;
		}
		finally
		{
			tokenizer.TokenOptions = tokenOptions;
		}
	}

	private static string TrimMax(string s, int trimMax)
	{
		int i = 0;
		for (int num = ((s.Length < trimMax) ? s.Length : trimMax); i < num && s[i] == ' '; i++)
		{
		}
		if (i == 0)
		{
			return s;
		}
		return s.Substring(i);
	}

	private void NextToken(TokenizerOptions nextTokenOptions)
	{
		token = tokenizer.NextToken(nextTokenOptions);
	}

	private Token NextToken()
	{
		token = tokenizer.NextToken();
		return token;
	}

	private void AssertNextToken(Token expectedToken)
	{
		NextToken();
		AssertToken(expectedToken);
	}

	private void AssertToken(Token expectedToken)
	{
		if (token != expectedToken)
		{
			ThrowExpectedTokenTextImportException(TxtFileTokenizer.FormatToken(expectedToken));
		}
	}

	private void AssertTokens(params Token[] expectedTokens)
	{
		if (!expectedTokens.Contains(token))
		{
			ThrowImportException(string.Format(CultureInfo.InvariantCulture, "{0} or {1} expected.", string.Join(", ", expectedTokens.Select(TxtFileTokenizer.FormatToken).Take(expectedTokens.Length - 1)), TxtFileTokenizer.FormatToken(expectedTokens.Last())));
		}
	}

	private void AssertIdentifier()
	{
		if (token < Token.Identifier)
		{
			AssertToken(Token.Identifier);
		}
	}

	private void UnexpectedToken()
	{
		throw new TxtImportException(tokenizer.LineNo, tokenizer.LinePos, tokenizer.GetTokenLine(), string.Format(CultureInfo.InvariantCulture, NonLocalizable.TxtImport_TokenUnexpected, TxtFileTokenizer.FormatToken(token)));
	}
}
