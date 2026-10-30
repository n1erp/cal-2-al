using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public abstract class Diagnostic : IEquatable<Diagnostic>, IFormattable
{
	internal sealed class SimpleDiagnostic : Diagnostic
	{
		private readonly DiagnosticDescriptor _descriptor;

		private readonly DiagnosticSeverity _severity;

		private readonly int _warningLevel;

		private readonly Location _location;

		private readonly IReadOnlyList<Location> _additionalLocations;

		private readonly object[] _messageArgs;

		private readonly ImmutableDictionary<string, string> _properties;

		private readonly bool _isSuppressed;

		public override DiagnosticDescriptor Descriptor => _descriptor;

		public override string Id => _descriptor.Id;

		internal override IReadOnlyList<object> Arguments => _messageArgs;

		public override DiagnosticSeverity Severity => _severity;

		public override bool IsSuppressed => _isSuppressed;

		public override int WarningLevel => _warningLevel;

		public override Location Location => _location;

		public override IReadOnlyList<Location> AdditionalLocations => _additionalLocations;

		public override ImmutableDictionary<string, string> Properties => _properties;

		private SimpleDiagnostic(DiagnosticDescriptor descriptor, DiagnosticSeverity severity, int warningLevel, Location location, IEnumerable<Location> additionalLocations, object[] messageArgs, ImmutableDictionary<string, string> properties, bool isSuppressed)
		{
			if ((warningLevel == 0 && severity != DiagnosticSeverity.Error) || (warningLevel != 0 && severity == DiagnosticSeverity.Error))
			{
				throw new ArgumentException(string.Format("{0} ({1}) and {2} ({3}) are not compatible.", "warningLevel", warningLevel, "severity", severity), "warningLevel");
			}
			_descriptor = descriptor ?? throw new ArgumentNullException("descriptor");
			_severity = severity;
			_warningLevel = warningLevel;
			_location = location ?? Location.None;
			ImmutableArray<Location>? immutableArray = additionalLocations?.ToImmutableArray();
			IReadOnlyList<Location> additionalLocations2;
			if (!immutableArray.HasValue)
			{
				additionalLocations2 = SpecializedCollections.EmptyReadOnlyList<Location>();
			}
			else
			{
				IReadOnlyList<Location> readOnlyList = immutableArray.GetValueOrDefault();
				additionalLocations2 = readOnlyList;
			}
			_additionalLocations = additionalLocations2;
			_messageArgs = messageArgs ?? Array.Empty<object>();
			_properties = properties ?? ImmutableDictionary<string, string>.Empty;
			_isSuppressed = isSuppressed;
		}

		internal static SimpleDiagnostic Create(DiagnosticDescriptor descriptor, DiagnosticSeverity severity, int warningLevel, Location location, IEnumerable<Location> additionalLocations, object[] messageArgs, ImmutableDictionary<string, string> properties, bool isSuppressed = false)
		{
			return new SimpleDiagnostic(descriptor, severity, warningLevel, location, additionalLocations, messageArgs, properties, isSuppressed);
		}

		internal static SimpleDiagnostic Create(string id, LocalizableString title, string category, LocalizableString message, LocalizableString description, string helpLink, DiagnosticSeverity severity, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, int warningLevel, Location location, IEnumerable<Location> additionalLocations, IEnumerable<string> customTags, ImmutableDictionary<string, string> properties, bool isSuppressed = false)
		{
			return new SimpleDiagnostic(new DiagnosticDescriptor(id, title, message, category, defaultSeverity, isEnabledByDefault, description, helpLink, customTags.ToImmutableArrayOrEmpty()), severity, warningLevel, location, additionalLocations, null, properties, isSuppressed);
		}

		public override string GetMessage(IFormatProvider formatProvider = null)
		{
			if (_messageArgs.Length == 0)
			{
				return _descriptor.MessageFormat.ToString(formatProvider);
			}
			string text = _descriptor.MessageFormat.ToString(formatProvider);
			try
			{
				return string.Format(formatProvider, text, _messageArgs);
			}
			catch (Exception)
			{
				return text;
			}
		}

		public override bool Equals(Diagnostic obj)
		{
			if (!(obj is SimpleDiagnostic simpleDiagnostic))
			{
				return false;
			}
			if (AnalyzerExecutor.IsAnalyzerExceptionDiagnostic(this))
			{
				return AnalyzerExecutor.AreEquivalentAnalyzerExceptionDiagnostics(this, simpleDiagnostic);
			}
			if (_descriptor.Equals(simpleDiagnostic._descriptor) && _messageArgs.SequenceEqual(simpleDiagnostic._messageArgs, (object a, object b) => a == b) && _location == simpleDiagnostic._location && _severity == simpleDiagnostic._severity)
			{
				return _warningLevel == simpleDiagnostic._warningLevel;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Diagnostic);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_descriptor, Hash.CombineValues(_messageArgs, Hash.Combine(_warningLevel, Hash.Combine(_location, (int)_severity))));
		}

		internal override Diagnostic WithLocation(Location location)
		{
			if (location == null)
			{
				throw new ArgumentNullException("location");
			}
			if (location != _location)
			{
				return new SimpleDiagnostic(_descriptor, _severity, _warningLevel, location, _additionalLocations, _messageArgs, _properties, _isSuppressed);
			}
			return this;
		}

		internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
		{
			if (Severity != severity)
			{
				int defaultWarningLevel = GetDefaultWarningLevel(severity);
				return new SimpleDiagnostic(_descriptor, severity, defaultWarningLevel, _location, _additionalLocations, _messageArgs, _properties, _isSuppressed);
			}
			return this;
		}

		internal override Diagnostic WithIsSuppressed(bool isSuppressed)
		{
			if (IsSuppressed != isSuppressed)
			{
				return new SimpleDiagnostic(_descriptor, _severity, _warningLevel, _location, _additionalLocations, _messageArgs, _properties, isSuppressed);
			}
			return this;
		}
	}

	internal const string CompilerDiagnosticCategory = "Compiler";

	internal const int HighestValidWarningLevel = 4;

	public abstract DiagnosticDescriptor Descriptor { get; }

	public abstract string Id { get; }

	internal virtual string Category => Descriptor.Category;

	public virtual DiagnosticSeverity DefaultSeverity => Descriptor.DefaultSeverity;

	public abstract DiagnosticSeverity Severity { get; }

	public abstract int WarningLevel { get; }

	public abstract bool IsSuppressed { get; }

	internal virtual bool IsEnabledByDefault => Descriptor.IsEnabledByDefault;

	public bool IsWarningAsError
	{
		get
		{
			if (DefaultSeverity == DiagnosticSeverity.Warning)
			{
				return Severity == DiagnosticSeverity.Error;
			}
			return false;
		}
	}

	public abstract Location Location { get; }

	public abstract IReadOnlyList<Location> AdditionalLocations { get; }

	internal virtual IReadOnlyList<string> CustomTags => (IReadOnlyList<string>)Descriptor.CustomTags;

	public virtual ImmutableDictionary<string, string> Properties => ImmutableDictionary<string, string>.Empty;

	internal virtual int Code => 0;

	internal virtual IReadOnlyList<object> Arguments => SpecializedCollections.EmptyReadOnlyList<object>();

	public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, params object[] messageArgs)
	{
		return Create(descriptor, location, null, null, messageArgs);
	}

	public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, ImmutableDictionary<string, string> properties, params object[] messageArgs)
	{
		return Create(descriptor, location, null, properties, messageArgs);
	}

	public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, IEnumerable<Location> additionalLocations, params object[] messageArgs)
	{
		return Create(descriptor, location, additionalLocations, null, messageArgs);
	}

	public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, IEnumerable<Location> additionalLocations, ImmutableDictionary<string, string> properties, params object[] messageArgs)
	{
		return Create(descriptor, location, descriptor.DefaultSeverity, additionalLocations, properties, messageArgs);
	}

	public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, DiagnosticSeverity effectiveSeverity, IEnumerable<Location> additionalLocations, ImmutableDictionary<string, string> properties, params object[] messageArgs)
	{
		if (descriptor == null)
		{
			throw new ArgumentNullException("descriptor");
		}
		int defaultWarningLevel = GetDefaultWarningLevel(effectiveSeverity);
		return SimpleDiagnostic.Create(descriptor, effectiveSeverity, defaultWarningLevel, location ?? Location.None, additionalLocations, messageArgs, properties);
	}

	public static Diagnostic Create(string id, string category, LocalizableString message, DiagnosticSeverity severity, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, int warningLevel, LocalizableString title = null, LocalizableString description = null, string helpLink = null, Location location = null, IEnumerable<Location> additionalLocations = null, IEnumerable<string> customTags = null, ImmutableDictionary<string, string> properties = null)
	{
		return Create(id, category, message, severity, defaultSeverity, isEnabledByDefault, warningLevel, isSuppressed: false, title, description, helpLink, location, additionalLocations, customTags, properties);
	}

	public static Diagnostic Create(string id, string category, LocalizableString message, DiagnosticSeverity severity, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, int warningLevel, bool isSuppressed, LocalizableString title = null, LocalizableString description = null, string helpLink = null, Location location = null, IEnumerable<Location> additionalLocations = null, IEnumerable<string> customTags = null, ImmutableDictionary<string, string> properties = null)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (category == null)
		{
			throw new ArgumentNullException("category");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		return SimpleDiagnostic.Create(id, title ?? ((LocalizableString)string.Empty), category, message, description ?? ((LocalizableString)string.Empty), helpLink ?? string.Empty, severity, defaultSeverity, isEnabledByDefault, warningLevel, location ?? Location.None, additionalLocations, customTags, properties, isSuppressed);
	}

	internal static Diagnostic Create(CommonMessageProvider messageProvider, int errorCode)
	{
		return Create(new DiagnosticInfo(messageProvider, errorCode));
	}

	internal static Diagnostic Create(CommonMessageProvider messageProvider, int errorCode, params object[] arguments)
	{
		return Create(new DiagnosticInfo(messageProvider, errorCode, arguments));
	}

	internal static Diagnostic Create(DiagnosticInfo info)
	{
		return new DiagnosticWithInfo(info, Location.None);
	}

	public abstract string GetMessage(IFormatProvider formatProvider = null);

	public SuppressionInfo GetSuppressionInfo(Compilation compilation)
	{
		if (!IsSuppressed)
		{
			return null;
		}
		if (!new SuppressMessageAttributeState(compilation).IsDiagnosticSuppressed(this, out var suppressingAttribute))
		{
			suppressingAttribute = null;
		}
		return new SuppressionInfo(Id, suppressingAttribute);
	}

	string IFormattable.ToString(string ignored, IFormatProvider formatProvider)
	{
		return DiagnosticFormatter.Instance.Format(this, formatProvider);
	}

	public override string ToString()
	{
		return DiagnosticFormatter.Instance.Format(this, CultureInfo.CurrentUICulture);
	}

	public abstract override bool Equals(object obj);

	public abstract override int GetHashCode();

	public abstract bool Equals(Diagnostic obj);

	private string GetDebuggerDisplay()
	{
		return Severity switch
		{
			(DiagnosticSeverity)(-1) => "Unresolved diagnostic at " + Location, 
			(DiagnosticSeverity)(-2) => "Void diagnostic at " + Location, 
			_ => ToString(), 
		};
	}

	internal abstract Diagnostic WithLocation(Location location);

	internal abstract Diagnostic WithSeverity(DiagnosticSeverity severity);

	internal abstract Diagnostic WithIsSuppressed(bool isSuppressed);

	internal bool HasIntersectingLocation(SyntaxTree tree, TextSpan? filterSpanWithinTree = null)
	{
		foreach (Location item in GetDiagnosticLocationsWithinTree(tree))
		{
			if (!filterSpanWithinTree.HasValue || filterSpanWithinTree.Value.IntersectsWith(item.SourceSpan))
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerable<Location> GetDiagnosticLocationsWithinTree(SyntaxTree tree)
	{
		if (Location.SourceTree == tree)
		{
			yield return Location;
		}
		if (AdditionalLocations == null)
		{
			yield break;
		}
		foreach (Location additionalLocation in AdditionalLocations)
		{
			if (additionalLocation.SourceTree == tree)
			{
				yield return additionalLocation;
			}
		}
	}

	internal Diagnostic WithReportDiagnostic(ReportDiagnostic reportAction)
	{
		return reportAction switch
		{
			ReportDiagnostic.Suppress => null, 
			ReportDiagnostic.Error => WithSeverity(DiagnosticSeverity.Error), 
			ReportDiagnostic.Default => this, 
			ReportDiagnostic.Warn => WithSeverity(DiagnosticSeverity.Warning), 
			ReportDiagnostic.Info => WithSeverity(DiagnosticSeverity.Info), 
			ReportDiagnostic.Hidden => WithSeverity(DiagnosticSeverity.Hidden), 
			_ => throw ExceptionUtilities.UnexpectedValue(reportAction), 
		};
	}

	internal static int GetDefaultWarningLevel(DiagnosticSeverity severity)
	{
		if (severity != DiagnosticSeverity.Warning && severity == DiagnosticSeverity.Error)
		{
			return 0;
		}
		return 1;
	}

	internal virtual bool IsNotConfigurable()
	{
		return AnalyzerManager.HasNotConfigurableTag(CustomTags);
	}
}
