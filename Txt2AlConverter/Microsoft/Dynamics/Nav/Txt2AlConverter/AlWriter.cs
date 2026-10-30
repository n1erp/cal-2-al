using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Model.TypeSystem;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class AlWriter : IAlWriter
{
	private class AlWriterState : IAlWriterState
	{
		private readonly Stack<string> indentStack = new Stack<string>();

		public bool WriteIndent { get; set; }

		public string Indent { get; set; } = string.Empty;

		public int LinePosition { get; set; }

		public bool PendingEmptyLine { get; set; }

		public bool DataClassificationDefaulting { get; set; }

		public IDotNetDeclarationExtractor DotNetExtractor { get; }

		public IEnumDeclarationExtractor EnumExtractor { get; }

		public ITranslationExtractor TranslationExtractor { get; }

		public ITypeResolver TypeResolver { get; }

		public IFileNameExporter FileNameExporter { get; }

		public Version TargetRuntimeVersion { get; }

		public AlWriterState(IDotNetDeclarationExtractor dotNetExtractor, ITranslationExtractor translationExtractor, IEnumDeclarationExtractor enumExtractor, ITypeResolver typeResolver, Version targetRuntimeVersion, IFileNameExporter fileNameExporter, bool dataClassificationDefaulting)
		{
			TranslationExtractor = translationExtractor;
			DotNetExtractor = dotNetExtractor ?? new NullDotNetDeclarationExtractor(quoteName: true);
			EnumExtractor = enumExtractor ?? NullEnumDeclarationExtractor.Instance;
			TypeResolver = typeResolver ?? new BaseTypeResolver(DotNetExtractor, TranslationExtractor);
			TargetRuntimeVersion = targetRuntimeVersion ?? RuntimeVersion.Latest;
			FileNameExporter = fileNameExporter;
			DataClassificationDefaulting = dataClassificationDefaulting;
			ResetFilePosition();
		}

		public void PushIndent(int increment)
		{
			indentStack.Push(Indent);
			Indent = new string(' ', Indent.Length + increment);
		}

		public void PushIndent()
		{
			indentStack.Push(Indent);
		}

		public void SetIndent(int value)
		{
			Indent = new string(' ', value);
		}

		public void PopIndent()
		{
			Indent = indentStack.Pop();
		}

		public void ResetFilePosition()
		{
			indentStack.Clear();
			Indent = string.Empty;
			indentStack.Push(Indent);
			WriteIndent = false;
			LinePosition = 0;
		}
	}

	private readonly TextWriter destinationStreamWriter;

	private readonly AlWriterState state;

	public bool PendingEmptyLine
	{
		get
		{
			return state.PendingEmptyLine;
		}
		set
		{
			state.PendingEmptyLine = value;
		}
	}

	public string Indent => state.Indent;

	public int LinePosition => state.LinePosition;

	public IAlWriterState State => state;

	public int Indentation { get; }

	public AlWriter(TextWriter writer, AlExportOptions options, IFileNameExporter fileNameExporter, IDotNetDeclarationExtractor dotNetExtractor = null, ITranslationExtractor translationExtractor = null, ITypeResolver typeResolver = null, IEnumDeclarationExtractor enumExtractor = null)
	{
		destinationStreamWriter = writer;
		Indentation = options?.Indentation ?? 2;
		state = new AlWriterState(dotNetExtractor, translationExtractor, enumExtractor, typeResolver, options.TargetRuntimeVersion, fileNameExporter, options.DataClassificationDefaulting);
	}

	public void ResetFilePosition()
	{
		state.ResetFilePosition();
	}

	public void WriteMultilineText(string value)
	{
		PushIndent();
		int linePosition = state.LinePosition;
		SetIndent(linePosition);
		value = value.UnquoteIdentifier();
		bool flag = true;
		foreach (string item in ModelTextHelper.MLTextFormatter(ModelTextHelper.MLTextSplitter(value), 0, forceUseSingleQuotes: true))
		{
			if (!flag)
			{
				WriteLine(',');
			}
			Write(AlUtilities.ConvertMlCommentFormat(item));
			flag = false;
		}
		PopIndent();
	}

	public void WriteLinePossiblyCommentedOut(string value, bool commentedOut)
	{
		if (commentedOut)
		{
			Write("//");
		}
		WriteLine(value);
	}

	public void WriteSection(string sectionName, Action writeSectionContent)
	{
		WriteLine(sectionName);
		WriteLine('{');
		PushIndent(Indentation);
		writeSectionContent();
		PopIndent();
		WriteLine('}');
	}

	public void LowWrite(string value)
	{
		destinationStreamWriter.Write(value);
		state.LinePosition += value.Length;
	}

	public void WriteIndent()
	{
		if (state.WriteIndent)
		{
			LowWrite(state.Indent);
			state.WriteIndent = false;
		}
	}

	public void Write(string value, bool singleQuoteValue = false, bool ignoreIndent = false)
	{
		string value2 = (singleQuoteValue ? AlUtilities.SingleQuote(value) : value);
		if (!ignoreIndent && !string.IsNullOrEmpty(value2))
		{
			WriteIndent();
		}
		LowWrite(value2);
	}

	public void Write(char value)
	{
		WriteIndent();
		LowWrite(value.ToString());
	}

	public void WriteLine(string value, bool singleQuoteVale = false)
	{
		string value2 = (singleQuoteVale ? AlUtilities.SingleQuote(value) : value);
		Write(value2);
		LowWriteLine();
		state.WriteIndent = true;
	}

	public void WriteLine(char value)
	{
		Write(value);
		LowWriteLine();
		state.WriteIndent = true;
	}

	public void WriteLine()
	{
		LowWriteLine();
		state.WriteIndent = true;
	}

	public void WriteProperty(string propName, string value)
	{
		WriteLine(AlUtilities.QuoteStringLiteralIfNeeded(propName, needsSingleQuoting: false) + " = " + AlUtilities.QuoteStringLiteralIfNeeded(value, needsSingleQuoting: false) + ";");
	}

	public void WritePropertyWithValueAsAction(string propName, Action value)
	{
		Write(propName);
		Write(" = ");
		value();
		WriteLine(";");
	}

	public void WritePropertyPossiblyCommentedOut(string propName, string value, bool commentedOut)
	{
		if (commentedOut)
		{
			Write("//");
		}
		WriteProperty(propName, value);
	}

	public void WriteSection(Action writeSectionContent)
	{
		WriteLine('{');
		PushIndent(Indentation);
		writeSectionContent();
		PopIndent();
		WriteLine('}');
	}

	public void PopIndent()
	{
		state.PopIndent();
	}

	public void PushIndent()
	{
		state.PushIndent();
	}

	public void PushIndent(int increment)
	{
		state.PushIndent(increment);
	}

	public void SetIndent(int value)
	{
		state.SetIndent(value);
	}

	private void WriteMultilineEmbracedProperty(int indexOfNewLine, string value, bool needsSingleQuoting)
	{
		PushIndent();
		int linePosition = state.LinePosition;
		SetIndent(linePosition);
		int num = 0;
		while (indexOfNewLine >= 0)
		{
			string text = value.Substring(num, indexOfNewLine - num - ((num + indexOfNewLine > 0 && value[indexOfNewLine - 1] == '\r') ? 1 : 0));
			if (needsSingleQuoting)
			{
				text += "\\";
			}
			WriteLine(text, needsSingleQuoting);
			num = indexOfNewLine + 1;
			indexOfNewLine = value.IndexOf('\n', num);
		}
		Write(value.Substring(num), needsSingleQuoting);
		PopIndent();
	}

	private void LowWriteLine()
	{
		destinationStreamWriter.WriteLine();
		state.LinePosition = 0;
	}

	private void WriteBooleanValue(string value)
	{
		WriteIndent();
		value = value.ToLowerInvariant();
		switch (value)
		{
		case "yes":
		case "true":
			LowWrite("true");
			break;
		case "no":
		case "false":
			LowWrite("false");
			break;
		default:
			LowWrite(value);
			break;
		}
	}
}
