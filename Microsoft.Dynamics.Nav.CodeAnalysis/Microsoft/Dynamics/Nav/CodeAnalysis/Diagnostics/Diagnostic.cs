using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public abstract class Diagnostic : IEquatable<Diagnostic>, IFormattable
{
	internal sealed class SimpleDiagnostic : Diagnostic
	{
		private class ObjectComparer : IEqualityComparer<object>
		{
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				return x == y;
			}

			public int GetHashCode(object obj)
			{
				return 0;
			}
		}

		private readonly DiagnosticDescriptor descriptor;

		private readonly DiagnosticSeverity severity;

		private readonly int warningLevel;

		private readonly Location location;

		private readonly IReadOnlyList<Location> additionalLocations;

		private readonly object[] messageArgs;

		private readonly ImmutableDictionary<string, string> properties;

		private readonly bool isSuppressed;

		public override DiagnosticDescriptor Descriptor => descriptor;

		public override string Id => descriptor.Id;

		internal override IReadOnlyList<object> Arguments => messageArgs;

		public override DiagnosticSeverity Severity => severity;

		public override int WarningLevel => warningLevel;

		public override Location Location => location;

		public override IReadOnlyList<Location> AdditionalLocations => additionalLocations;

		public override ImmutableDictionary<string, string> Properties => properties;

		public override bool IsSuppressed => isSuppressed;

		private SimpleDiagnostic(DiagnosticDescriptor descriptor, DiagnosticSeverity severity, int warningLevel, Location location, IEnumerable<Location> additionalLocations, object[] messageArgs, ImmutableDictionary<string, string> properties, bool isSuppressed)
		{
			if ((warningLevel == 0 && severity != DiagnosticSeverity.Error) || (warningLevel != 0 && severity == DiagnosticSeverity.Error))
			{
				throw new ArgumentException("warningLevel");
			}
			if (descriptor == null)
			{
				throw new ArgumentNullException("descriptor");
			}
			this.descriptor = descriptor;
			this.severity = severity;
			this.warningLevel = warningLevel;
			this.location = location ?? Location.None;
			IReadOnlyList<Location> readOnlyList;
			if (additionalLocations == null)
			{
				readOnlyList = SpecializedCollections.EmptyReadOnlyList<Location>();
			}
			else
			{
				IReadOnlyList<Location> readOnlyList2 = additionalLocations.ToArray();
				readOnlyList = readOnlyList2;
			}
			this.additionalLocations = readOnlyList;
			this.messageArgs = messageArgs ?? SpecializedCollections.EmptyArray<object>();
			this.properties = properties ?? ImmutableDictionary<string, string>.Empty;
			this.isSuppressed = isSuppressed;
		}

		internal static SimpleDiagnostic Create(DiagnosticDescriptor descriptor, DiagnosticSeverity severity, int warningLevel, Location location, IEnumerable<Location> additionalLocations, object[] messageArgs, ImmutableDictionary<string, string> properties, bool isSuppressed = false)
		{
			return new SimpleDiagnostic(descriptor, severity, warningLevel, location, additionalLocations, messageArgs, properties, isSuppressed);
		}

		internal static SimpleDiagnostic Create(string id, LocalizableString title, string category, LocalizableString message, LocalizableString description, string helpLink, DiagnosticSeverity severity, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, int warningLevel, Location location, IEnumerable<Location> additionalLocations, IEnumerable<string> customTags, ImmutableDictionary<string, string> properties, bool isSuppressed = false)
		{
			return new SimpleDiagnostic(new DiagnosticDescriptor(id, title, message, category, defaultSeverity, isEnabledByDefault, description, helpLink, customTags.ToArray()), severity, warningLevel, location, additionalLocations, null, properties, isSuppressed);
		}

		public override string GetMessage(IFormatProvider formatProvider = null)
		{
			if (messageArgs.Length == 0)
			{
				return descriptor.MessageFormat.ToString(formatProvider);
			}
			string format = descriptor.MessageFormat.ToString(formatProvider);
			return string.Format(formatProvider, format, messageArgs);
		}

		public override bool Equals(Diagnostic obj)
		{
			if (!(obj is SimpleDiagnostic simpleDiagnostic))
			{
				return false;
			}
			if (descriptor.Equals(simpleDiagnostic.descriptor) && messageArgs.SequenceEqual(simpleDiagnostic.messageArgs, new ObjectComparer()) && location == simpleDiagnostic.location && severity == simpleDiagnostic.severity)
			{
				return warningLevel == simpleDiagnostic.warningLevel;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Diagnostic);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(descriptor.GetHashCode(), Hash.CombineValues(messageArgs, Hash.Combine(warningLevel, Hash.Combine(location.GetHashCode(), (int)severity))));
		}

		internal override Diagnostic WithLocation(Location locationParameter)
		{
			if (locationParameter == null)
			{
				throw new ArgumentNullException("locationParameter");
			}
			if (locationParameter != Location)
			{
				return new SimpleDiagnostic(descriptor, severity, warningLevel, locationParameter, additionalLocations, messageArgs, properties, isSuppressed);
			}
			return this;
		}

		internal override Diagnostic WithSeverity(DiagnosticSeverity severityParameter)
		{
			if (Severity != severityParameter)
			{
				int defaultWarningLevel = GetDefaultWarningLevel(severityParameter);
				return new SimpleDiagnostic(descriptor, severityParameter, defaultWarningLevel, location, additionalLocations, messageArgs, properties, isSuppressed);
			}
			return this;
		}

		internal override Diagnostic WithIsSuppressed(bool isSuppressedParameter)
		{
			if (IsSuppressed != isSuppressedParameter)
			{
				return new SimpleDiagnostic(descriptor, severity, warningLevel, location, additionalLocations, messageArgs, properties, isSuppressedParameter);
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
		if (descriptor == null)
		{
			throw new ArgumentNullException("descriptor");
		}
		int defaultWarningLevel = GetDefaultWarningLevel(descriptor.DefaultSeverity);
		return SimpleDiagnostic.Create(descriptor, descriptor.DefaultSeverity, defaultWarningLevel, location ?? Location.None, additionalLocations, messageArgs, properties);
	}

	public static Diagnostic Create(string id, string category, LocalizableString message, DiagnosticSeverity severity, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, int warningLevel, LocalizableString title = null, LocalizableString description = null, string helpLink = null, Location location = null, IEnumerable<Location> additionalLocations = null, IEnumerable<string> customTags = null, ImmutableDictionary<string, string> properties = null)
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
		return SimpleDiagnostic.Create(id, title ?? ((LocalizableString)string.Empty), category, message, description ?? ((LocalizableString)string.Empty), helpLink ?? string.Empty, severity, defaultSeverity, isEnabledByDefault, warningLevel, location ?? Location.None, additionalLocations, customTags, properties);
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

	string IFormattable.ToString(string ignored, IFormatProvider formatProvider)
	{
		return DiagnosticFormatter.Instance.Format(this, formatProvider);
	}

	public override string ToString()
	{
		return DiagnosticFormatter.Instance.Format(this, CultureInfo.CurrentCulture);
	}

	public abstract override bool Equals(object obj);

	public abstract override int GetHashCode();

	public abstract bool Equals(Diagnostic other);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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

	internal virtual bool IsNotConfigurable()
	{
		return AnalyzerManager.HasNotConfigurableTag(CustomTags);
	}

	internal bool ContainsLocation(SyntaxTree tree, TextSpan? filterSpanWithinTree = null)
	{
		if (Location.SourceTree != tree)
		{
			return false;
		}
		if (filterSpanWithinTree.HasValue)
		{
			return filterSpanWithinTree.Value.Contains(Location.SourceSpan);
		}
		return true;
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
			_ => throw new NotSupportedException(reportAction.ToString()), 
		};
	}

	internal static int GetDefaultWarningLevel(DiagnosticSeverity severity)
	{
		return severity switch
		{
			DiagnosticSeverity.Error => 0, 
			DiagnosticSeverity.Warning => 1, 
			_ => 4, 
		};
	}
}
