using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

public class ProjectManifest
{
	private delegate ParsingResult<T> TryGetFromJsonMethod<T>(JsonParser parser, JObject projectManifestJson, string propertyName, Version runtime, IList<Diagnostic> diagnostics);

	private struct ParsingResult<T>
	{
		public bool Success { get; }

		public T Value { get; }

		public string FailureMessage { get; }

		internal ParsingResult(bool success, T value, string message)
		{
			Success = success;
			Value = value;
			FailureMessage = message;
		}

		internal static ParsingResult<T> CreateSuccessResult(T value)
		{
			return new ParsingResult<T>(success: true, value, null);
		}

		internal static ParsingResult<T> CreateFailResult(string message = null)
		{
			return new ParsingResult<T>(success: false, default, message);
		}
	}

	private class JsonParser
	{
		private readonly string filePath;

		internal Dictionary<string, Location> PropertyNamesLocations { get; }

		public JsonParser(string path)
		{
			filePath = path;
			PropertyNamesLocations = new Dictionary<string, Location>();
		}

		internal JObject Parse(string json, IList<Diagnostic> diagnostics)
		{
			using StringReader reader = new StringReader(json);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			return (JObject)DeserializeAndReportErrorOnDuplicates(reader2, string.Empty, diagnostics);
		}

		private JToken DeserializeAndReportErrorOnDuplicates(JsonTextReader reader, string parentName, IList<Diagnostic> diagnostics)
		{
			if (reader.TokenType == JsonToken.None)
			{
				ReadNextToken(reader, diagnostics);
			}
			if (reader.TokenType == JsonToken.StartObject)
			{
				ReadNextToken(reader, diagnostics);
				JObject jObject = new JObject();
				while (reader.TokenType != JsonToken.EndObject)
				{
					string text = reader.Value as string;
					Location location = GetLocation(reader);
					ReadNextToken(reader, diagnostics);
					JToken content = DeserializeAndReportErrorOnDuplicates(reader, text, diagnostics);
					if (!string.IsNullOrEmpty(text))
					{
						if (IsManifestPropertyAllowed(text, parentName))
						{
							if (jObject[text] == null)
							{
								jObject.Add(new JProperty(text, content));
								UpdatePropertyNamesLocation(text, parentName, location);
							}
							else
							{
								DiagnosticInfo info = new DiagnosticInfo(MessageProvider.Instance, 317, text);
								diagnostics.Add(new DiagnosticWithInfo(info, location));
							}
						}
						else
						{
							DiagnosticInfo info2 = new DiagnosticInfo(MessageProvider.Instance, 124, text);
							diagnostics.Add(new DiagnosticWithInfo(info2, location));
						}
					}
					ReadNextToken(reader, diagnostics);
				}
				if (string.IsNullOrEmpty(parentName))
				{
					reader.Read();
				}
				return jObject;
			}
			if (reader.TokenType == JsonToken.StartArray)
			{
				ReadNextToken(reader, diagnostics);
				JArray jArray = new JArray();
				while (reader.TokenType != JsonToken.EndArray)
				{
					jArray.Add(DeserializeAndReportErrorOnDuplicates(reader, parentName, diagnostics));
					ReadNextToken(reader, diagnostics);
				}
				return jArray;
			}
			return new JValue(reader.Value);
		}

		private void ReadNextToken(JsonTextReader reader, IList<Diagnostic> diagnostics)
		{
			reader.Read();
			while (reader.TokenType == JsonToken.Comment)
			{
				reader.Read();
			}
		}

		private static bool IsManifestPropertyAllowed(string propName, string parentName)
		{
			string item = ((!string.IsNullOrEmpty(parentName)) ? (parentName + "_" + propName) : propName);
			return NavAppManifestCompatibility.ManifestPropertiesName.Contains(item);
		}

		private void UpdatePropertyNamesLocation(string propName, string parentName, Location propLocation, int index = 0)
		{
			string propertyKeyValue = GetPropertyKeyValue(propName, parentName, index);
			if (!PropertyNamesLocations.ContainsKey(propertyKeyValue))
			{
				PropertyNamesLocations.Add(propertyKeyValue, propLocation);
			}
			else
			{
				UpdatePropertyNamesLocation(propName, parentName, propLocation, index + 1);
			}
		}

		private Location GetLocation(JsonTextReader reader)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return Location.None;
			}
			int line = reader.LineNumber - 1;
			int num = ((string)reader.Value).Length + 2;
			int start = reader.LinePosition - num;
			TextSpan textSpan = new TextSpan(start, num);
			LinePosition start2 = new LinePosition(line, textSpan.Start - 1);
			LinePosition end = new LinePosition(line, textSpan.End - 1);
			return Location.Create(lineSpan: new LinePositionSpan(start2, end), filePath: filePath, textSpan: textSpan);
		}
	}

	private static readonly Version DefaultCompatibilityId = new Version("0.0.0.0");

	private const int UndefinedVersionNumber = -1;

	public const string ProjectManifestFileName = "app.json";

	public const string ProjectManifestExtension = ".json";

	public NavAppManifest AppManifest { get; internal set; }

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public IEnumerable<SymbolReferenceSpecification> GetAllReferences()
	{
		return AppManifest.GetAllReferences();
	}

	internal static ProjectManifest ReadFromFile(string path, IList<Diagnostic> diagnostics)
	{
		try
		{
			return ReadFromString(path, File.ReadAllText(path), diagnostics);
		}
		catch (IOException ex)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_InvalidManifest, Location.None, ex.Message);
			return null;
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public static ProjectManifest ReadFromString(string manifestPath, string manifestString, IList<Diagnostic> diagnostics)
	{
		try
		{
			JsonParser jsonParser = new JsonParser(manifestPath);
			JObject jObject = jsonParser.Parse(manifestString, diagnostics);
			ProjectManifest projectManifest = new ProjectManifest
			{
				AppManifest = new NavAppManifest
				{
					AppCompatibilityId = DefaultCompatibilityId
				}
			};
			NavAppManifest appManifest = projectManifest.AppManifest;
			Version runtime = (appManifest.Runtime = GetRuntimeVersion(jsonParser, jObject, diagnostics));
			appManifest.AppName = GetStringValueFromJsonObject(jsonParser, jObject, "name", runtime, diagnostics);
			appManifest.AppPublisher = GetStringValueFromJsonObject(jsonParser, jObject, "publisher", runtime, diagnostics);
			appManifest.AppBrief = GetStringValueFromJsonObject(jsonParser, jObject, "brief", runtime, diagnostics);
			appManifest.AppDescription = GetStringValueFromJsonObject(jsonParser, jObject, "description", runtime, diagnostics);
			appManifest.AppId = GetGuidValueFromJsonObject(jsonParser, jObject, "id", runtime, diagnostics);
			appManifest.AppVersion = GetVersionValueFromJsonObject(jsonParser, jObject, "version", runtime, diagnostics);
			appManifest.AppPrivacyStatement = GetStringValueFromJsonObject(jsonParser, jObject, "privacyStatement", runtime, diagnostics);
			appManifest.AppEula = GetStringValueFromJsonObject(jsonParser, jObject, "EULA", runtime, diagnostics);
			appManifest.AppHelp = GetStringValueFromJsonObject(jsonParser, jObject, "help", runtime, diagnostics);
			appManifest.AppUrl = GetStringValueFromJsonObject(jsonParser, jObject, "url", runtime, diagnostics);
			appManifest.AppLogo = GetStringValueFromJsonObject(jsonParser, jObject, "logo", runtime, diagnostics);
			appManifest.AppHelpBaseUrl = GetStringValueFromJsonObject(jsonParser, jObject, "helpBaseUrl", runtime, diagnostics);
			appManifest.ContextSensitiveHelpUrl = GetStringValueFromJsonObject(jsonParser, jObject, "contextSensitiveHelpUrl", runtime, diagnostics);
			appManifest.Target = CompilationTargetHelpers.ParseSafe(GetStringValueFromJsonObject(jsonParser, jObject, "target", runtime, diagnostics));
			appManifest.ShowMyCode = GetBooleanValueFromJsonObject(jsonParser, jObject, "showMyCode", runtime, diagnostics) ?? false;
			appManifest.ReadonlyErrorsAsWarnings = GetBooleanValueFromJsonObject(jsonParser, jObject, "readonlyErrorsAsWarnings", runtime, diagnostics) ?? false;
			appManifest.Application = GetVersionValueFromJsonObject(jsonParser, jObject, "application", runtime, diagnostics);
			appManifest.Platform = GetVersionValueFromJsonObject(jsonParser, jObject, "platform", runtime, diagnostics);
			appManifest.Test = GetVersionValueFromJsonObject(jsonParser, jObject, "test", runtime, diagnostics);
			appManifest.CompilerFeatures = GetCompilerFeatures(jsonParser, jObject, runtime, diagnostics);
			appManifest.AppScreenshots = TryGetFromJsonObject(TryGetScreenshots, jsonParser, jObject, "screenshots", runtime, diagnostics);
			appManifest.Dependencies = TryGetFromJsonObject(TryGetExtensionDependencies, jsonParser, jObject, "dependencies", runtime, diagnostics);
			appManifest.IdSpaces = TryGetFromJsonObject(TryGetIdRanges, jsonParser, jObject, "idRanges", runtime, diagnostics) ?? IdSpaces.BaseAppDevelopmentIdSpaces;
			appManifest.AppSupportedLocales = TryGetFromJsonObject(TryGetSupportedLocales, jsonParser, jObject, "supportedLocales", runtime, diagnostics);
			appManifest.SetPropertyLocations(jsonParser.PropertyNamesLocations);
			appManifest.ManifestLocation = manifestPath;
			return projectManifest;
		}
		catch (Exception ex)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_InvalidManifest, Location.None, ex.Message);
			return null;
		}
	}

	private static CompilerFeatures GetCompilerFeatures(JsonParser parser, JObject projectManifestJson, Version runtime, IList<Diagnostic> diagnostics)
	{
		CompilerFeatures compilerFeatures = CompilerFeatures.None;
		if (TryGetArray(parser, projectManifestJson, "features", returnNull: false, diagnostics, out var array))
		{
			foreach (JToken item in array)
			{
				string text = (string)item;
				ValidatePropertyValueCompatibility(parser, text, runtime, diagnostics, "features");
				CompilerFeaturesExtensions.TryGetFromRawString(text, out var compilerFeatures2);
				compilerFeatures |= compilerFeatures2;
			}
		}
		return compilerFeatures;
	}

	private static T TryGetFromJsonObject<T>(TryGetFromJsonMethod<T> func, JsonParser parser, JObject projectManifestJson, string propertyName, Version runtime, IList<Diagnostic> diagnostics)
	{
		ParsingResult<T> parsingResult = func(parser, projectManifestJson, propertyName, runtime, diagnostics);
		if (!parsingResult.Success)
		{
			string propertyKeyValue = GetPropertyKeyValue(propertyName);
			Location propertyLocation = GetPropertyLocation(parser, propertyKeyValue);
			AddDiagnostic(diagnostics, ErrorCode.ERR_InvalidManifest, propertyLocation, parsingResult.FailureMessage ?? string.Empty);
		}
		return parsingResult.Value;
	}

	private static ParsingResult<NavAppSupportedLocales> TryGetSupportedLocales(JsonParser parser, JObject projectManifestJson, string propertyName, Version runtime, IList<Diagnostic> diagnostics)
	{
		if (!TryGetArray(parser, projectManifestJson, propertyName, returnNull: true, diagnostics, out var array))
		{
			return ParsingResult<NavAppSupportedLocales>.CreateFailResult();
		}
		if (array == null)
		{
			return ParsingResult<NavAppSupportedLocales>.CreateSuccessResult(null);
		}
		ValidatePropertyCompatibility(parser, propertyName, runtime, diagnostics);
		NavAppSupportedLocales value = null;
		ArrayBuilder<string> arrayBuilder = new ArrayBuilder<string>();
		try
		{
			foreach (JToken item in array)
			{
				arrayBuilder.Add(item.Value<string>());
			}
			value = new NavAppSupportedLocales(arrayBuilder.ToArray());
		}
		finally
		{
			arrayBuilder.Free();
		}
		return ParsingResult<NavAppSupportedLocales>.CreateSuccessResult(value);
	}

	private static ParsingResult<IEnumerable<string>> TryGetScreenshots(JsonParser parser, JObject projectManifestJson, string propertyName, Version runtime, IList<Diagnostic> diagnostics)
	{
		if (!TryGetArray(parser, projectManifestJson, propertyName, returnNull: false, diagnostics, out var array))
		{
			return ParsingResult<IEnumerable<string>>.CreateFailResult();
		}
		ValidatePropertyCompatibility(parser, propertyName, runtime, diagnostics);
		List<string> list = new List<string>();
		foreach (JToken item in array)
		{
			list.Add(item.Value<string>());
		}
		return ParsingResult<IEnumerable<string>>.CreateSuccessResult(list);
	}

	private static ParsingResult<IdSpaces> TryGetIdRanges(JsonParser parser, JObject projectManifestJson, string propertyName, Version runtime, IList<Diagnostic> diagnostics)
	{
		if (!TryGetArray(parser, projectManifestJson, propertyName, returnNull: true, diagnostics, out var array))
		{
			return ParsingResult<IdSpaces>.CreateFailResult();
		}
		projectManifestJson.TryGetValue("idRange", out var value);
		if (array == null && value == null)
		{
			return ParsingResult<IdSpaces>.CreateSuccessResult(null);
		}
		if (array != null && value != null)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_DuplicateIdRange, GetPropertyLocation(parser, "idRange"));
			return ParsingResult<IdSpaces>.CreateFailResult();
		}
		if (array != null)
		{
			return GetIdRanges(parser, propertyName, runtime, diagnostics, array);
		}
		return GetIdRange(parser, runtime, diagnostics, value);
	}

	private static ParsingResult<IdSpaces> GetIdRange(JsonParser parser, Version runtime, IList<Diagnostic> diagnostics, JToken idRangeToken)
	{
		if (idRangeToken != null)
		{
			ValidatePropertyCompatibility(parser, "idRange", runtime, diagnostics);
			IdSpace idSpace = GetIdSpace(parser, runtime, diagnostics, (JObject)idRangeToken);
			if (idSpace != null)
			{
				idSpace.ValidateRange(diagnostics, GetPropertyLocation(parser, GetPropertyKeyValue("idRange")));
				return ParsingResult<IdSpaces>.CreateSuccessResult(new IdSpaces(new IdSpace[1] { idSpace }));
			}
		}
		return ParsingResult<IdSpaces>.CreateSuccessResult(null);
	}

	private static ParsingResult<IdSpaces> GetIdRanges(JsonParser parser, string propertyName, Version runtime, IList<Diagnostic> diagnostics, JArray rangesArray)
	{
		int count = rangesArray.Count;
		ArrayBuilder<IdSpace> instance = ArrayBuilder<IdSpace>.GetInstance(count);
		try
		{
			if (rangesArray != null)
			{
				ValidatePropertyCompatibility(parser, propertyName, runtime, diagnostics);
				for (int i = 0; i < count; i++)
				{
					JObject rangeJson = rangesArray[i] as JObject;
					IdSpace idSpace = GetIdSpace(parser, runtime, diagnostics, rangeJson);
					if (idSpace == null)
					{
						return ParsingResult<IdSpaces>.CreateFailResult();
					}
					instance.Add(idSpace);
				}
			}
			if (instance.Any())
			{
				IdSpaces idSpaces = new IdSpaces(instance.ToArray());
				idSpaces.ValidateRanges(diagnostics, GetPropertyLocation(parser, GetPropertyKeyValue(propertyName)));
				return ParsingResult<IdSpaces>.CreateSuccessResult(idSpaces);
			}
			return ParsingResult<IdSpaces>.CreateSuccessResult(null);
		}
		finally
		{
			instance.Free();
		}
	}

	private static IdSpace GetIdSpace(JsonParser parser, Version runtime, IList<Diagnostic> diagnostics, JObject rangeJson)
	{
		if (rangeJson == null)
		{
			return null;
		}
		if (!int.TryParse(GetStringValueFromJsonObject(parser, rangeJson, "from", runtime, diagnostics), out var result) || !int.TryParse(GetStringValueFromJsonObject(parser, rangeJson, "to", runtime, diagnostics), out var result2))
		{
			return null;
		}
		return new IdSpace(result, result2);
	}

	private static ParsingResult<List<NavAppDependency>> TryGetExtensionDependencies(JsonParser parser, JObject projectManifestJson, string propertyName, Version runtime, IList<Diagnostic> diagnostics)
	{
		if (!TryGetArray(parser, projectManifestJson, propertyName, returnNull: false, diagnostics, out var array))
		{
			return ParsingResult<List<NavAppDependency>>.CreateFailResult();
		}
		ValidatePropertyCompatibility(parser, propertyName, runtime, diagnostics);
		List<NavAppDependency> list = new List<NavAppDependency>();
		for (int i = 0; i < array.Count; i++)
		{
			if (!(array[i] is JObject json))
			{
				return ParsingResult<List<NavAppDependency>>.CreateFailResult();
			}
			NavAppDependency navAppDependency = new NavAppDependency();
			navAppDependency.Name = GetStringValueFromJsonObject(parser, json, "name", runtime, diagnostics, propertyName, i);
			navAppDependency.Publisher = GetStringValueFromJsonObject(parser, json, "publisher", runtime, diagnostics, propertyName, i);
			navAppDependency.MinVersion = GetVersionValueFromJsonObject(parser, json, "version", runtime, diagnostics, propertyName, i);
			navAppDependency.AppId = GetGuidValueFromJsonObject(parser, json, "appId", runtime, diagnostics, propertyName, i);
			navAppDependency.CompatibilityId = DefaultCompatibilityId;
			list.Add(navAppDependency);
		}
		return ParsingResult<List<NavAppDependency>>.CreateSuccessResult(list);
	}

	private static string GetStringValueFromJsonObject(JsonParser parser, JObject json, string name, Version runtime, IList<Diagnostic> diagnostics, string parentProperty = null, int index = 0)
	{
		if (json.TryGetValue(name, out var value))
		{
			ValidatePropertyCompatibility(parser, name, runtime, diagnostics, parentProperty, index);
			return value.Value<string>();
		}
		ValidatePropertyRequired(parser, name, diagnostics, parentProperty);
		return null;
	}

	private static bool? GetBooleanValueFromJsonObject(JsonParser parser, JObject json, string name, Version runtime, IList<Diagnostic> diagnostics)
	{
		if (json.TryGetValue(name, out var value))
		{
			ValidatePropertyCompatibility(parser, name, runtime, diagnostics);
			return value.Value<bool>();
		}
		ValidatePropertyRequired(parser, name, diagnostics);
		return null;
	}

	private static bool TryGetArray(JsonParser parser, JObject obj, string propertyName, bool returnNull, IList<Diagnostic> diagnostics, out JArray array)
	{
		JToken value = obj.GetValue(propertyName, StringComparison.OrdinalIgnoreCase);
		if (value != null)
		{
			array = value as JArray;
			if (array == null)
			{
				string propertyKeyValue = GetPropertyKeyValue(propertyName);
				if (NavAppManifestCompatibility.ManifestPropertiesInfos.TryGetValue(propertyKeyValue.ToUpperInvariant(), out var value2) && value2.IsArray)
				{
					Location propertyLocation = GetPropertyLocation(parser, propertyKeyValue);
					AddDiagnostic(diagnostics, ErrorCode.ERR_WrongManifestPropertyType, propertyLocation, propertyName, "array");
				}
				return false;
			}
			return true;
		}
		array = (returnNull ? null : new JArray());
		return true;
	}

	private static Guid GetGuidValueFromJsonObject(JsonParser parser, JObject json, string propertyName, Version runtime, IList<Diagnostic> diagnostics, string parentProperty = null, int index = 0)
	{
		string stringValueFromJsonObject = GetStringValueFromJsonObject(parser, json, propertyName, runtime, diagnostics, parentProperty, index);
		return ParseGuid(parser, stringValueFromJsonObject, diagnostics, propertyName, parentProperty, index);
	}

	private static Guid ParseGuid(JsonParser parser, string guid, IList<Diagnostic> diagnostics, string propertyName, string parentProperty, int index)
	{
		if (guid == null)
		{
			return default;
		}
		if (string.IsNullOrEmpty(guid) || !Guid.TryParse(guid, out var result))
		{
			string propertyKeyValue = GetPropertyKeyValue(propertyName, parentProperty, index);
			Location propertyLocation = GetPropertyLocation(parser, propertyKeyValue);
			AddDiagnostic(diagnostics, ErrorCode.ERR_InvalidManifestGuid, propertyLocation, guid ?? string.Empty);
			return default;
		}
		return result;
	}

	private static Version GetVersionValueFromJsonObject(JsonParser parser, JObject json, string propertyName, Version runtime, IList<Diagnostic> diagnostics, string parentProperty = null, int index = 0)
	{
		string stringValueFromJsonObject = GetStringValueFromJsonObject(parser, json, propertyName, runtime, diagnostics, parentProperty, index);
		return ParseVersion(parser, stringValueFromJsonObject, diagnostics, propertyName, parentProperty, index);
	}

	private static Version ParseVersion(JsonParser parser, string version, IList<Diagnostic> diagnostics, string propertyName, string parentProperty, int index)
	{
		if (version == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(version) || !Version.TryParse(version, out var result) || result.Major == -1 || result.Minor == -1 || result.Build == -1 || result.Revision == -1)
		{
			string propertyKeyValue = GetPropertyKeyValue(propertyName, parentProperty, index);
			Location propertyLocation = GetPropertyLocation(parser, propertyKeyValue);
			AddDiagnostic(diagnostics, ErrorCode.ERR_InvalidManifestVersion, propertyLocation, version ?? string.Empty, propertyName);
			return null;
		}
		return result;
	}

	private static Version GetRuntimeVersion(JsonParser parser, JObject projectManifestJson, IList<Diagnostic> diagnostics)
	{
		if (projectManifestJson.TryGetValue("runtime", out var value))
		{
			Version version = ParseRuntimeVersion(parser, value.Value<string>(), diagnostics);
			if (!RuntimeVersion.SupportedVersions.Contains(version))
			{
				Location propertyLocation = GetPropertyLocation(parser, "runtime");
				AddDiagnostic(diagnostics, ErrorCode.ERR_UnsupportedManifestRuntimeVersion, propertyLocation, version, "runtime");
				return RuntimeVersion.CurrentRelease;
			}
			return version;
		}
		return RuntimeVersion.CurrentRelease;
	}

	private static Version ParseRuntimeVersion(JsonParser parser, string version, IList<Diagnostic> diagnostics)
	{
		if (string.IsNullOrEmpty(version))
		{
			return null;
		}
		if (!RuntimeVersion.TryParse(version, out var result))
		{
			Location propertyLocation = GetPropertyLocation(parser, "runtime");
			AddDiagnostic(diagnostics, ErrorCode.ERR_InvalidManifestVersion_Short, propertyLocation, version, "runtime");
			return RuntimeVersion.CurrentRelease;
		}
		return result;
	}

	private static Location GetPropertyLocation(JsonParser parser, string property)
	{
		if (parser.PropertyNamesLocations.TryGetValue(property, out var value))
		{
			return value;
		}
		return Location.None;
	}

	private static string GetPropertyKeyValue(string propertyName, string parentProperty = null, int index = 0)
	{
		string text = ((!string.IsNullOrEmpty(parentProperty)) ? (parentProperty + "_" + propertyName) : propertyName);
		if (index != 0)
		{
			return text + "_" + index.ToString(CultureInfo.InvariantCulture);
		}
		return text;
	}

	private static void ValidatePropertyValueCompatibility(JsonParser parser, string propertyName, Version runtime, IList<Diagnostic> diagnostics, string parentProperty)
	{
		string propertyKeyValue = GetPropertyKeyValue(propertyName, parentProperty);
		if (NavAppManifestCompatibility.ManifestPropertyValuesInfos.TryGetValue(propertyKeyValue.ToUpperInvariant(), out var value))
		{
			Location propertyLocation = GetPropertyLocation(parser, propertyKeyValue);
			ValidateCompatibility(runtime, value.Compatibility, propertyKeyValue, propertyLocation, diagnostics);
		}
	}

	private static void ValidatePropertyCompatibility(JsonParser parser, string propertyName, Version runtime, IList<Diagnostic> diagnostics, string parentProperty = null, int index = 0)
	{
		string propertyKeyValue = GetPropertyKeyValue(propertyName, parentProperty, index);
		if (NavAppManifestCompatibility.ManifestPropertiesInfos.TryGetValue(propertyKeyValue.ToUpperInvariant(), out var value))
		{
			Location propertyLocation = GetPropertyLocation(parser, propertyKeyValue);
			ValidateCompatibility(runtime, value.Compatibility, propertyKeyValue, propertyLocation, diagnostics);
		}
	}

	private static void ValidatePropertyRequired(JsonParser parser, string propertyName, IList<Diagnostic> diagnostics, string parentProperty = null, int index = 0)
	{
		string propertyKeyValue = GetPropertyKeyValue(propertyName, parentProperty, index);
		if (NavAppManifestCompatibility.ManifestPropertiesInfos.TryGetValue(propertyKeyValue.ToUpperInvariant(), out var value) && value.IsRequired)
		{
			Location propertyLocation = GetPropertyLocation(parser, propertyKeyValue);
			propertyName = propertyName.Replace('_', '.');
			AddDiagnostic(diagnostics, ErrorCode.ERR_MissingRequiredManifestProperty, propertyLocation, propertyName);
		}
	}

	private static void ValidateCompatibility(Version runtime, VersionCompatibility version, string errorDisplayName, Location location, IList<Diagnostic> diagnostics)
	{
		if (version != null && !version.IsSupported(runtime))
		{
			errorDisplayName = errorDisplayName.Replace('_', '.');
			AddDiagnostic(diagnostics, ErrorCode.ERR_FeatureNotAvailable, location, errorDisplayName, runtime.ToMajorMinorString(), version.ToString());
		}
	}

	private static void AddDiagnostic(ICollection<Diagnostic> diagnostics, ErrorCode errorCode, Location location, params object[] messageArgs)
	{
		DiagnosticInfo info = new DiagnosticInfo(MessageProvider.Instance, (int)errorCode, messageArgs);
		diagnostics.Add(new DiagnosticWithInfo(info, location));
	}
}
