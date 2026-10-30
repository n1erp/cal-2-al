using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class StreamErrorLogger : ErrorLogger, IDisposable
{
	private sealed class DiagnosticDescriptorSet
	{
		private sealed class Comparer : IEqualityComparer<DiagnosticDescriptor>
		{
			public bool Equals(DiagnosticDescriptor x, DiagnosticDescriptor y)
			{
				if (x == y)
				{
					return true;
				}
				if (x == null || y == null)
				{
					return false;
				}
				if (x.Category == y.Category && x.DefaultSeverity == y.DefaultSeverity && x.Description.Equals(y.Description) && x.HelpLinkUri == y.HelpLinkUri && x.Id == y.Id && x.IsEnabledByDefault == y.IsEnabledByDefault && x.Title.Equals(y.Title))
				{
					return x.CustomTags.SequenceEqual(y.CustomTags);
				}
				return false;
			}

			public int GetHashCode(DiagnosticDescriptor obj)
			{
				if (obj == null)
				{
					return 0;
				}
				return Hash.Combine(obj.Category.GetHashCode(), Hash.Combine(obj.DefaultSeverity.GetHashCode(), Hash.Combine(obj.Description.GetHashCode(), Hash.Combine(obj.HelpLinkUri.GetHashCode(), Hash.Combine(obj.Id.GetHashCode(), Hash.Combine(obj.IsEnabledByDefault.GetHashCode(), Hash.Combine(obj.Title.GetHashCode(), Hash.CombineValues(obj.CustomTags))))))));
			}
		}

		private Dictionary<string, int> _counters = new Dictionary<string, int>();

		private Dictionary<DiagnosticDescriptor, string> _keys = new Dictionary<DiagnosticDescriptor, string>(new Comparer());

		public int Count => _keys.Count;

		public string Add(DiagnosticDescriptor descriptor)
		{
			if (_keys.TryGetValue(descriptor, out var value))
			{
				return value;
			}
			if (!_counters.TryGetValue(descriptor.Id, out var value2))
			{
				_counters.Add(descriptor.Id, 0);
				_keys.Add(descriptor, descriptor.Id);
				return descriptor.Id;
			}
			do
			{
				value2 = (_counters[descriptor.Id] = value2 + 1);
				value = descriptor.Id + "-" + value2.ToString("000", CultureInfo.InvariantCulture);
			}
			while (_counters.ContainsKey(value));
			_keys.Add(descriptor, value);
			return value;
		}

		public List<KeyValuePair<string, DiagnosticDescriptor>> ToSortedList()
		{
			List<KeyValuePair<string, DiagnosticDescriptor>> list = new List<KeyValuePair<string, DiagnosticDescriptor>>(Count);
			foreach (KeyValuePair<DiagnosticDescriptor, string> key in _keys)
			{
				list.Add(new KeyValuePair<string, DiagnosticDescriptor>(key.Value, key.Key));
			}
			list.Sort((KeyValuePair<string, DiagnosticDescriptor> x, KeyValuePair<string, DiagnosticDescriptor> y) => string.CompareOrdinal(x.Key, y.Key));
			return list;
		}
	}

	private readonly JsonWriter _writer;

	private readonly DiagnosticDescriptorSet _descriptors;

	private readonly CultureInfo _culture;

	private static readonly Uri _fileRoot = new Uri("file:///");

	public StreamErrorLogger(Stream stream, string toolName, string toolFileVersion, Version toolAssemblyVersion, CultureInfo culture)
	{
		_writer = new JsonWriter(new StreamWriter(stream));
		_descriptors = new DiagnosticDescriptorSet();
		_culture = culture;
		_writer.WriteObjectStart();
		_writer.Write("$schema", "http://json.schemastore.org/sarif-1.0.0");
		_writer.Write("version", "1.0.0");
		_writer.WriteArrayStart("runs");
		_writer.WriteObjectStart();
		_writer.WriteObjectStart("tool");
		_writer.Write("name", toolName);
		_writer.Write("version", toolAssemblyVersion.ToString());
		_writer.Write("fileVersion", toolFileVersion);
		_writer.Write("semanticVersion", toolAssemblyVersion.ToString(3));
		_writer.Write("language", culture.Name);
		_writer.WriteObjectEnd();
		_writer.WriteArrayStart("results");
	}

	public override void LogDiagnostic(Diagnostic diagnostic)
	{
		_writer.WriteObjectStart();
		_writer.Write("ruleId", diagnostic.Id);
		string text = _descriptors.Add(diagnostic.Descriptor);
		if (text != diagnostic.Id)
		{
			_writer.Write("ruleKey", text);
		}
		_writer.Write("level", GetLevel(diagnostic.Severity));
		string message = diagnostic.GetMessage(_culture);
		if (!string.IsNullOrEmpty(message))
		{
			_writer.Write("message", message);
		}
		if (diagnostic.IsSuppressed)
		{
			_writer.WriteArrayStart("suppressionStates");
			_writer.Write("suppressedInSource");
			_writer.WriteArrayEnd();
		}
		WriteLocations(diagnostic.Location, diagnostic.AdditionalLocations);
		WriteProperties(diagnostic);
		_writer.WriteObjectEnd();
	}

	private void WriteLocations(Location location, IReadOnlyList<Location> additionalLocations)
	{
		if (HasPath(location))
		{
			_writer.WriteArrayStart("locations");
			_writer.WriteObjectStart();
			_writer.WriteKey("resultFile");
			WritePhysicalLocation(location);
			_writer.WriteObjectEnd();
			_writer.WriteArrayEnd();
		}
		if (additionalLocations == null || additionalLocations.Count <= 0 || !additionalLocations.Any((Location l) => HasPath(l)))
		{
			return;
		}
		_writer.WriteArrayStart("relatedLocations");
		foreach (Location additionalLocation in additionalLocations)
		{
			if (HasPath(additionalLocation))
			{
				_writer.WriteObjectStart();
				_writer.WriteKey("physicalLocation");
				WritePhysicalLocation(additionalLocation);
				_writer.WriteObjectEnd();
			}
		}
		_writer.WriteArrayEnd();
	}

	private void WritePhysicalLocation(Location location)
	{
		FileLinePositionSpan lineSpan = location.GetLineSpan();
		_writer.WriteObjectStart();
		_writer.Write("uri", GetUri(lineSpan.Path));
		_writer.WriteObjectStart("region");
		_writer.Write("startLine", lineSpan.StartLinePosition.Line + 1);
		_writer.Write("startColumn", lineSpan.StartLinePosition.Character + 1);
		_writer.Write("endLine", lineSpan.EndLinePosition.Line + 1);
		_writer.Write("endColumn", lineSpan.EndLinePosition.Character + 1);
		_writer.WriteObjectEnd();
		_writer.WriteObjectEnd();
	}

	private static bool HasPath(Location location)
	{
		return !string.IsNullOrEmpty(location.GetLineSpan().Path);
	}

	private static string GetUri(string path)
	{
		if (Uri.TryCreate(path, UriKind.Absolute, out Uri result))
		{
			return result.AbsoluteUri;
		}
		if (Uri.TryCreate(path, UriKind.Relative, out result))
		{
			return _fileRoot.MakeRelativeUri(new Uri(_fileRoot, result)).ToString();
		}
		return WebUtility.UrlEncode(path);
	}

	private void WriteProperties(Diagnostic diagnostic)
	{
		if (diagnostic.WarningLevel <= 0 && diagnostic.Properties.Count <= 0)
		{
			return;
		}
		_writer.WriteObjectStart("properties");
		if (diagnostic.WarningLevel > 0)
		{
			_writer.Write("warningLevel", diagnostic.WarningLevel);
		}
		if (diagnostic.Properties.Count > 0)
		{
			_writer.WriteObjectStart("customProperties");
			foreach (KeyValuePair<string, string> item in diagnostic.Properties.OrderBy((KeyValuePair<string, string> x) => x.Key, StringComparer.Ordinal))
			{
				_writer.Write(item.Key, item.Value);
			}
			_writer.WriteObjectEnd();
		}
		_writer.WriteObjectEnd();
	}

	private void WriteRules()
	{
		if (_descriptors.Count <= 0)
		{
			return;
		}
		_writer.WriteObjectStart("rules");
		foreach (KeyValuePair<string, DiagnosticDescriptor> item in _descriptors.ToSortedList())
		{
			DiagnosticDescriptor value = item.Value;
			_writer.WriteObjectStart(item.Key);
			_writer.Write("id", value.Id);
			string value2 = value.Title.ToString(_culture);
			if (!string.IsNullOrEmpty(value2))
			{
				_writer.Write("shortDescription", value2);
			}
			string value3 = value.Description.ToString(_culture);
			if (!string.IsNullOrEmpty(value3))
			{
				_writer.Write("fullDescription", value3);
			}
			_writer.Write("defaultLevel", GetLevel(value.DefaultSeverity));
			if (!string.IsNullOrEmpty(value.HelpLinkUri))
			{
				_writer.Write("helpUri", value.HelpLinkUri);
			}
			_writer.WriteObjectStart("properties");
			if (!string.IsNullOrEmpty(value.Category))
			{
				_writer.Write("category", value.Category);
			}
			_writer.Write("isEnabledByDefault", value.IsEnabledByDefault);
			if (value.CustomTags.Any())
			{
				_writer.WriteArrayStart("tags");
				foreach (string customTag in value.CustomTags)
				{
					_writer.Write(customTag);
				}
				_writer.WriteArrayEnd();
			}
			_writer.WriteObjectEnd();
			_writer.WriteObjectEnd();
		}
		_writer.WriteObjectEnd();
	}

	private static string GetLevel(DiagnosticSeverity severity)
	{
		return severity switch
		{
			DiagnosticSeverity.Info => "note", 
			DiagnosticSeverity.Error => "error", 
			_ => "warning", 
		};
	}

	public void Dispose()
	{
		_writer.WriteArrayEnd();
		WriteRules();
		_writer.WriteObjectEnd();
		_writer.WriteArrayEnd();
		_writer.WriteObjectEnd();
		_writer.Dispose();
	}
}
