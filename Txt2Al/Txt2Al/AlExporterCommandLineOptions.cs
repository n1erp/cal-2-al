using System;
using CommandLine;
using CommandLine.Text;
using Microsoft.Dynamics.Nav.Txt2AlConverter;

namespace Txt2Al;

internal class AlExporterCommandLineOptions
{
	[Option("source", HelpText = "The path of the directory containing the TXT files.", Required = true, MetaValue = "Path")]
	public string SourceDirectory { get; set; }

	[Option("target", HelpText = "The path of the directory into which the converted AL files will be placed.", Required = true, MetaValue = "Path")]
	public string TargetDirectory { get; set; }

	[Option("rename", Required = false, HelpText = "Rename the output files to prevent clashes with the source .txt files.", DefaultValue = true)]
	public bool Rename { get; set; }

	[Option("type", Required = false, HelpText = "The type of object to convert. Allowed values: Codeunit, Table, Page, Report, Query, XmlPort", MetaValue = "ObjectType")]
	public ApplicationObjectTypeForExport Type { get; set; }

	[Option("extensionStartId", Required = false, HelpText = "The starting numeric ID of the extension objects (Default: 70000000). It will be incremented by 1 for each extension object.")]
	public int ExtensionStartId { get; set; } = 70000000;

	[Option("stacktrace", Required = false, HelpText = "Display the stack trace of exceptions raised during the conversion.")]
	public bool Stacktrace { get; set; }

	[Option("fallbackTypeNameResolution", Required = false, HelpText = "Performs a best effort conversion of code that was exported to the new syntax but that might have unresolved references.")]
	public bool FallbackTypeNameResolution { get; set; }

	[Option("injectDotNetAddIns", Required = false, HelpText = "Inject the definition of standard .NET Add-Ins in the resulting .NET package.")]
	public bool InjectDotNetAddIns { get; set; }

	[Option("dotNetAddInsPackage", Required = false, HelpText = "Inject the definition of .NET Add-Ins from the given .NET package in the resulting .NET package.")]
	public string DotNetAddInsPackage { get; set; }

	[Option("multithreaded", Required = false, HelpText = "Run using multiple threads. This improves performance but causes non-repeatable extension numbers and .xlf content.")]
	public bool MultiThreaded { get; set; }

	[Option("dotNetTypePrefix", Required = false, HelpText = "Specify a prefix to be used for all .NET type aliases.")]
	public string DotNetTypePrefix { get; set; } = string.Empty;

	[Option("translationFormat", Required = false, HelpText = "Specify the format to use when generating translation files. The allowed values are: Xliff, Lcg", MetaValue = "ObjectType")]
	public TranslationFileFormat TranslationFileFormat { get; set; }

	[Option("addLegacyTranslationInfo", Required = false, HelpText = "Add information to the translation file that can be used to migrate existing translations.")]
	public bool AddLegacyTranslationInformation { get; set; }

	[Option("runtime", Required = false, HelpText = "Specify the target runtime for the converted AL. The default is the latest supported runtime. The string should be in a format similar to 'major.minor' .")]
	public string TargetRuntimeVersion { get; set; }

	[Option("objectFileNamePattern", Required = false, HelpText = "Specify the pattern to use for naming AL files containing AL objects. {name} - object name, {type} - object type, {id} - object ID. Only takes effect if '--rename' is specified.", DefaultValue = "{name}.{type}")]
	public string ObjectFileNamePattern { get; set; }

	[Option("extensionObjectFileNamePattern", Required = false, HelpText = "Specify the pattern to use for naming AL files containing AL extension objects. {name} - extension object name, {type} - extension object type, {id} - object ID, {targetName} - the name of the object being extended, {targetId} - the id of the object being extended. Only takes effect if '--rename' is specified.", DefaultValue = "{name}.{type}-{targetName}")]
	public string ExtensionObjectFileNamePattern { get; set; }

	[Option("format", Required = false, HelpText = "Format the converted AL code using the standard formatter.", DefaultValue = true)]
	public bool Format { get; set; }

	[Option("dataClassificationDefaulting", Required = false, HelpText = "Specify the DataClassification property for all table fields which don't have it specified.", DefaultValue = false)]
	public bool DataClassificationDefaulting { get; set; }

	[Option("tableDataOnly", Required = false, HelpText = "Export only fields and keys on tables.", DefaultValue = false)]
	public bool TableDataOnly { get; set; }

	[HelpOption]
	public string GetUsage()
	{
		HelpText helpText = new HelpText();
		helpText.Copyright = new CopyrightInfo("Copyright (c) Microsoft Corporation. All rights reserved.", 2017);
		helpText.AdditionalNewLineAfterOption = true;
		helpText.AddDashesToOption = true;
		helpText.AddOptions(this);
		return helpText;
	}

	public Version GetParsedRuntimeVersion()
	{
		if (string.IsNullOrEmpty(TargetRuntimeVersion))
		{
			return null;
		}
		return Version.Parse(TargetRuntimeVersion);
	}
}
