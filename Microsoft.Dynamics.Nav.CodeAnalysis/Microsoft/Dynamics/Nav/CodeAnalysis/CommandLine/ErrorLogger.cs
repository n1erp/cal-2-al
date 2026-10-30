using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal class ErrorLogger : IDisposable
{
	private abstract class Value
	{
		private class StringValue : Value
		{
			private readonly string value;

			public StringValue(string valueParameter, ErrorLogger owner)
				: base(owner)
			{
				value = valueParameter;
			}

			public override void Write()
			{
				owner.WriteValue(value);
			}
		}

		private class IntegerValue : Value
		{
			private readonly int value;

			public IntegerValue(int value, ErrorLogger owner)
				: base(owner)
			{
				this.value = value;
			}

			public override void Write()
			{
				owner.WriteValue(value);
			}
		}

		private class GroupValue : Value
		{
			private readonly ImmutableArray<KeyValuePair<string, Value>> keyValuePairs;

			public GroupValue(ImmutableArray<KeyValuePair<string, Value>> keyValuePairs, ErrorLogger owner)
				: base(owner)
			{
				this.keyValuePairs = keyValuePairs;
			}

			public override void Write()
			{
				owner.StartGroup();
				bool isFirst = true;
				foreach (KeyValuePair<string, Value> keyValuePair in keyValuePairs)
				{
					owner.WriteKeyValuePair(keyValuePair, isFirst);
					isFirst = false;
				}
				owner.EndGroup();
			}
		}

		private class ListValue : Value
		{
			private readonly ImmutableArray<Value> values;

			public ListValue(ImmutableArray<Value> values, ErrorLogger owner)
				: base(owner)
			{
				this.values = values;
			}

			public override void Write()
			{
				owner.StartList();
				bool isFirst = true;
				foreach (Value value in values)
				{
					owner.WriteValue(value, isFirst, valueInList: true);
					isFirst = false;
				}
				owner.EndList();
			}
		}

		private readonly ErrorLogger owner;

		private Value(ErrorLogger owner)
		{
			this.owner = owner;
		}

		public static Value Create(string value, ErrorLogger owner)
		{
			return new StringValue(value, owner);
		}

		public static Value Create(int value, ErrorLogger owner)
		{
			return new IntegerValue(value, owner);
		}

		public static Value Create(ImmutableArray<KeyValuePair<string, Value>> values, ErrorLogger owner)
		{
			return new GroupValue(values, owner);
		}

		public static Value Create(ImmutableArray<Value> values, ErrorLogger owner)
		{
			return new ListValue(values, owner);
		}

		public abstract void Write();
	}

	private struct Issue(string id, string message, string description, string title, string category, string helpLink, bool isEnabledByDefault, bool isSuppressedInSource, DiagnosticSeverity defaultSeverity, DiagnosticSeverity severity, int warningLevel, Location location, IReadOnlyList<Location> additionalLocations, IReadOnlyList<string> customTags, ImmutableDictionary<string, string> customProperties)
	{
		public readonly string Id = id;

		public readonly string Message = message;

		public readonly string Description = description;

		public readonly string Title = title;

		public readonly string Category = category;

		public readonly string HelpLink = helpLink;

		public readonly bool IsEnabledByDefault = isEnabledByDefault;

		public readonly bool IsSuppressedInSource = isSuppressedInSource;

		public readonly DiagnosticSeverity DefaultSeverity = defaultSeverity;

		public readonly DiagnosticSeverity Severity = severity;

		public readonly int WarningLevel = warningLevel;

		public readonly Location Location = location;

		public readonly IReadOnlyList<Location> AdditionalLocations = additionalLocations;

		public readonly IReadOnlyList<string> CustomTags = customTags;

		public readonly ImmutableArray<KeyValuePair<string, string>> CustomProperties = ImmutableArray<KeyValuePair<string, string>>.Empty;
	}

	private static class WellKnownStrings
	{
		public const string OutputFormatVersion = "version";

		public const string ToolFileVersion = "fileVersion";

		public const string ToolAssemblyVersion = "productVersion";

		public const string ToolInfo = "toolInfo";

		public const string ToolName = "toolName";

		public const string Issues = "issues";

		public const string DiagnosticId = "ruleId";

		public const string Locations = "locations";

		public const string ShortMessage = "shortMessage";

		public const string FullMessage = "fullMessage";

		public const string IsSuppressedInSource = "isSuppressedInSource";

		public const string Properties = "properties";

		public const string Location = "analysisTarget";

		public const string LocationSyntaxTreePath = "uri";

		public const string LocationSpanInfo = "region";

		public const string LocationSpanStartLine = "startLine";

		public const string LocationSpanStartColumn = "startColumn";

		public const string LocationSpanEndLine = "endLine";

		public const string LocationSpanEndColumn = "endColumn";

		public const string Category = "category";

		public const string Title = "title";

		public const string HelpLink = "helpLink";

		public const string CustomTags = "customTags";

		public const string IsEnabledByDefault = "isEnabledByDefault";

		public const string DefaultSeverity = "defaultSeverity";

		public const string Severity = "severity";

		public const string WarningLevel = "warningLevel";

		public const string CustomProperties = "customProperties";

		public const string None = "<None>";
	}

	internal const string OutputFormatVersion = "0.1";

	private const string IndentDelta = "  ";

	private const char GroupStartChar = '{';

	private const char GroupEndChar = '}';

	private const char ListStartChar = '[';

	private const char ListEndChar = ']';

	private readonly StreamWriter writer;

	private string currentIndent;

	private bool reportedAnyIssues;

	public ErrorLogger(Stream stream, string toolName, string toolFileVersion, Version toolAssemblyVersion)
	{
		DebugAssertHelper.Assert(stream != null);
		DebugAssertHelper.Assert(stream.Position == 0);
		writer = new StreamWriter(stream);
		currentIndent = string.Empty;
		reportedAnyIssues = false;
		WriteHeader(toolName, toolFileVersion, toolAssemblyVersion);
	}

	private void WriteHeader(string toolName, string toolFileVersion, Version toolAssemblyVersion)
	{
		StartGroup();
		WriteSimpleKeyValuePair("version", "0.1", isFirst: true);
		Value toolInfo = GetToolInfo(toolName, toolFileVersion, toolAssemblyVersion);
		WriteKeyValuePair("toolInfo", toolInfo, isFirst: false);
		WriteKey("issues", isFirst: false);
		StartList();
	}

	private Value GetToolInfo(string toolName, string toolFileVersion, Version toolAssemblyVersion)
	{
		ArrayBuilder<KeyValuePair<string, Value>> instance = ArrayBuilder<KeyValuePair<string, Value>>.GetInstance();
		instance.Add(CreateSimpleKeyValuePair("toolName", toolName));
		instance.Add(CreateSimpleKeyValuePair("productVersion", toolAssemblyVersion.ToString(3)));
		instance.Add(CreateSimpleKeyValuePair("fileVersion", GetToolFileVersionSubStr(toolFileVersion)));
		return Value.Create(instance.ToImmutableAndFree(), this);
	}

	private static string GetToolFileVersionSubStr(string toolFileVersion)
	{
		int num = 0;
		for (int i = 0; i < toolFileVersion.Length; i++)
		{
			if (toolFileVersion[i] == '.')
			{
				if (num == 2)
				{
					return toolFileVersion.Substring(0, i);
				}
				num++;
			}
		}
		return toolFileVersion;
	}

	internal static void LogDiagnostic(Diagnostic diagnostic, CultureInfo culture, ErrorLogger errorLogger)
	{
		if (errorLogger != null)
		{
			Issue issue = new Issue(diagnostic.Id, diagnostic.GetMessage(culture), diagnostic.Descriptor.Description.ToString(culture), diagnostic.Descriptor.Title.ToString(culture), diagnostic.Category, diagnostic.Descriptor.HelpLinkUri, diagnostic.IsEnabledByDefault, diagnostic.IsSuppressed, diagnostic.DefaultSeverity, diagnostic.Severity, diagnostic.WarningLevel, diagnostic.Location, diagnostic.AdditionalLocations, diagnostic.CustomTags, diagnostic.Properties);
			errorLogger.LogIssue(issue);
		}
	}

	private void LogIssue(Issue issue)
	{
		Value issueValue = GetIssueValue(issue);
		WriteValue(issueValue, !reportedAnyIssues, valueInList: true);
		reportedAnyIssues = true;
	}

	private Value GetIssueValue(Issue issue)
	{
		ArrayBuilder<KeyValuePair<string, Value>> instance = ArrayBuilder<KeyValuePair<string, Value>>.GetInstance();
		instance.Add(CreateSimpleKeyValuePair("ruleId", issue.Id));
		Value locationsValue = GetLocationsValue(issue.Location, issue.AdditionalLocations);
		instance.Add(KeyValuePair.Create("locations", locationsValue));
		string value = (string.IsNullOrEmpty(issue.Message) ? "<None>" : issue.Message);
		string description = issue.Description;
		if (string.IsNullOrEmpty(description))
		{
			instance.Add(CreateSimpleKeyValuePair("fullMessage", value));
		}
		else
		{
			instance.Add(CreateSimpleKeyValuePair("shortMessage", value));
			instance.Add(CreateSimpleKeyValuePair("fullMessage", description));
		}
		Value propertiesValue = GetPropertiesValue(issue);
		instance.Add(KeyValuePair.Create("properties", propertiesValue));
		return Value.Create(instance.ToImmutableAndFree(), this);
	}

	private Value GetLocationsValue(Location location, IReadOnlyList<Location> additionalLocations)
	{
		ArrayBuilder<Value> instance = ArrayBuilder<Value>.GetInstance();
		Value locationValue = GetLocationValue(location);
		if (locationValue != null)
		{
			instance.Add(locationValue);
		}
		if (additionalLocations != null && additionalLocations.Count > 0)
		{
			foreach (Location additionalLocation in additionalLocations)
			{
				locationValue = GetLocationValue(additionalLocation);
				if (locationValue != null)
				{
					instance.Add(locationValue);
				}
			}
		}
		return Value.Create(instance.ToImmutableAndFree(), this);
	}

	private Value GetLocationValue(Location location)
	{
		if (location.SourceTree == null)
		{
			return null;
		}
		ArrayBuilder<KeyValuePair<string, Value>> instance = ArrayBuilder<KeyValuePair<string, Value>>.GetInstance();
		instance.Add(CreateSimpleKeyValuePair("uri", location.SourceTree.FilePath));
		Value spanInfoValue = GetSpanInfoValue(location.GetLineSpan());
		instance.Add(KeyValuePair.Create("region", spanInfoValue));
		Value value = Value.Create(ImmutableArray.Create(Value.Create(instance.ToImmutableAndFree(), this)), this);
		return Value.Create(ImmutableArray.Create(KeyValuePair.Create("analysisTarget", value)), this);
	}

	private Value GetSpanInfoValue(FileLinePositionSpan lineSpan)
	{
		ArrayBuilder<KeyValuePair<string, Value>> instance = ArrayBuilder<KeyValuePair<string, Value>>.GetInstance();
		instance.Add(CreateSimpleKeyValuePair("startLine", lineSpan.StartLinePosition.Line));
		instance.Add(CreateSimpleKeyValuePair("startColumn", lineSpan.StartLinePosition.Character));
		instance.Add(CreateSimpleKeyValuePair("endLine", lineSpan.EndLinePosition.Line));
		instance.Add(CreateSimpleKeyValuePair("endColumn", lineSpan.EndLinePosition.Character));
		return Value.Create(instance.ToImmutableAndFree(), this);
	}

	private Value GetPropertiesValue(Issue issue)
	{
		ArrayBuilder<KeyValuePair<string, Value>> instance = ArrayBuilder<KeyValuePair<string, Value>>.GetInstance();
		instance.Add(CreateSimpleKeyValuePair("severity", issue.Severity.ToString()));
		if (issue.Severity == DiagnosticSeverity.Warning)
		{
			instance.Add(CreateSimpleKeyValuePair("warningLevel", issue.WarningLevel.ToString(CultureInfo.InvariantCulture)));
		}
		instance.Add(CreateSimpleKeyValuePair("defaultSeverity", issue.DefaultSeverity.ToString()));
		if (!string.IsNullOrEmpty(issue.Title))
		{
			instance.Add(CreateSimpleKeyValuePair("title", issue.Title));
		}
		instance.Add(CreateSimpleKeyValuePair("category", issue.Category));
		if (!string.IsNullOrEmpty(issue.HelpLink))
		{
			instance.Add(CreateSimpleKeyValuePair("helpLink", issue.HelpLink));
		}
		instance.Add(CreateSimpleKeyValuePair("isEnabledByDefault", issue.IsEnabledByDefault.ToString()));
		instance.Add(CreateSimpleKeyValuePair("isSuppressedInSource", issue.IsSuppressedInSource.ToString()));
		if (issue.CustomTags.Count > 0)
		{
			instance.Add(CreateSimpleKeyValuePair("customTags", issue.CustomTags.WhereNotNull().Join(";")));
		}
		if (issue.CustomProperties.Length > 0)
		{
			ArrayBuilder<KeyValuePair<string, Value>> instance2 = ArrayBuilder<KeyValuePair<string, Value>>.GetInstance();
			foreach (KeyValuePair<string, string> customProperty in issue.CustomProperties)
			{
				instance2.Add(CreateSimpleKeyValuePair(customProperty.Key, customProperty.Value));
			}
			Value value = Value.Create(instance2.ToImmutableAndFree(), this);
			instance.Add(KeyValuePair.Create("customProperties", value));
		}
		return Value.Create(instance.ToImmutableAndFree(), this);
	}

	private void WriteKeyValuePair(KeyValuePair<string, Value> kvp, bool isFirst)
	{
		WriteKeyValuePair(kvp.Key, kvp.Value, isFirst);
	}

	private void WriteKeyValuePair(string key, Value value, bool isFirst)
	{
		WriteKey(key, isFirst);
		WriteValue(value);
	}

	private void WriteSimpleKeyValuePair(string key, string value, bool isFirst)
	{
		WriteKey(key, isFirst);
		WriteValue(value);
	}

	private void WriteKey(string key, bool isFirst)
	{
		StartNewEntry(isFirst);
		writer.Write("\"{0}\": ", key);
	}

	private void WriteValue(Value value, bool isFirst = true, bool valueInList = false)
	{
		if (!isFirst | valueInList)
		{
			StartNewEntry(isFirst);
		}
		value.Write();
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
	private void WriteValue(string value)
	{
		writer.Flush();
		throw ExceptionUtilities.SkippedForNow;
	}

	private void WriteValue(int value)
	{
		writer.Write(value);
	}

	private void StartNewEntry(bool isFirst)
	{
		if (!isFirst)
		{
			writer.WriteLine(',');
		}
		else
		{
			writer.WriteLine();
		}
		writer.Write(currentIndent);
	}

	private void StartGroup()
	{
		StartGroupOrListCommon('{');
	}

	private void EndGroup()
	{
		EndGroupOrListCommon('}');
	}

	private void StartList()
	{
		StartGroupOrListCommon('[');
	}

	private void EndList()
	{
		EndGroupOrListCommon(']');
	}

	private void StartGroupOrListCommon(char startChar)
	{
		writer.Write(startChar);
		IncreaseIndentation();
	}

	private void EndGroupOrListCommon(char endChar)
	{
		writer.WriteLine();
		DecreaseIndentation();
		writer.Write(currentIndent + endChar);
	}

	private void IncreaseIndentation()
	{
		currentIndent += "  ";
	}

	private void DecreaseIndentation()
	{
		currentIndent = currentIndent.Substring("  ".Length);
	}

	private KeyValuePair<string, Value> CreateSimpleKeyValuePair(string key, string value)
	{
		Value value2 = Value.Create(value, this);
		return KeyValuePair.Create(key, value2);
	}

	private KeyValuePair<string, Value> CreateSimpleKeyValuePair(string key, int value)
	{
		Value value2 = Value.Create(value, this);
		return KeyValuePair.Create(key, value2);
	}

	public void Dispose()
	{
		EndList();
		EndGroup();
		writer.Dispose();
	}
}
