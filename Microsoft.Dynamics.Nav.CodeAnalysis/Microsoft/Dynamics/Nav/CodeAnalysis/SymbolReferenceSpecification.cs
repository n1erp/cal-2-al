using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerDisplay("{ToDisplayString(), nq}")]
public class SymbolReferenceSpecification : IEquatable<SymbolReferenceSpecification>
{
	private class InternalVersionComparer : IComparer<SymbolReferenceSpecification>
	{
		public int Compare(SymbolReferenceSpecification x, SymbolReferenceSpecification y)
		{
			return x.Version.CompareTo(y.Version);
		}
	}

	private class InternalVersionlessEqualityComparer : IEqualityComparer<SymbolReferenceSpecification>
	{
		public bool Equals(SymbolReferenceSpecification x, SymbolReferenceSpecification y)
		{
			if (string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Publisher, y.Publisher, StringComparison.OrdinalIgnoreCase))
			{
				return x.AppId == y.AppId;
			}
			return false;
		}

		public int GetHashCode(SymbolReferenceSpecification specification)
		{
			return Hash.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(specification.Name ?? string.Empty), StringComparer.OrdinalIgnoreCase.GetHashCode(specification.Publisher ?? string.Empty), specification.AppId.GetHashCode());
		}
	}

	public const string PublisherName = "Microsoft";

	public const string PlatformName = "System";

	public const string ApplicationName = "Application";

	public const string TestFrameworkName = "Test";

	public const string DefaultPublisher = "Default Publisher";

	public const string DefaultVersion = "1.0.0.0";

	public string Name { get; }

	public string Publisher { get; }

	public Version Version { get; }

	public Guid AppId { get; }

	public bool Exact { get; }

	public bool IsValid { get; }

	public static IComparer<SymbolReferenceSpecification> VersionComparer { get; } = new InternalVersionComparer();

	public static IEqualityComparer<SymbolReferenceSpecification> VersionLessEqualityComparer { get; } = new InternalVersionlessEqualityComparer();

	public SymbolReferenceSpecification(string publisher, string name, Version version, bool exact = false, Guid? appId = null)
	{
		Publisher = publisher;
		Name = name;
		Version = version;
		Exact = exact;
		AppId = appId ?? Guid.Empty;
		IsValid = !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Publisher) && Version != null;
	}

	public bool IsApplicationSymbolReference()
	{
		return IsApplicationSymbolReference(Name, Publisher);
	}

	public bool IsPlatformSymbolReference()
	{
		return IsPlatformSymbolReference(Name, Publisher);
	}

	public bool IsTestSymbolReference()
	{
		return IsTestSymbolReference(Name, Publisher);
	}

	public SymbolReferenceSpecification WithPublisher(string publisher)
	{
		return new SymbolReferenceSpecification(publisher, Name, Version, Exact, AppId);
	}

	public SymbolReferenceSpecification WithName(string name)
	{
		return new SymbolReferenceSpecification(Publisher, name, Version, Exact, AppId);
	}

	public SymbolReferenceSpecification WithVersion(Version version)
	{
		return new SymbolReferenceSpecification(Publisher, Name, version, Exact, AppId);
	}

	public SymbolReferenceSpecification WithExact(bool exact)
	{
		return new SymbolReferenceSpecification(Publisher, Name, Version, exact, AppId);
	}

	public static SymbolReferenceSpecification ApplicationReference(Version version)
	{
		return new SymbolReferenceSpecification("Microsoft", "Application", version);
	}

	public static SymbolReferenceSpecification PlatformReference(Version version)
	{
		return new SymbolReferenceSpecification("Microsoft", "System", version);
	}

	public static SymbolReferenceSpecification TestReference(Version version)
	{
		return new SymbolReferenceSpecification("Microsoft", "Test", version);
	}

	public static bool IsApplicationSymbolReference(string applicationName, string publisherName)
	{
		if (publisherName.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
		{
			return applicationName.Equals("Application", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsPlatformSymbolReference(string applicationName, string publisherName)
	{
		if (publisherName.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
		{
			return applicationName.Equals("System", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsTestSymbolReference(string applicationName, string publisherName)
	{
		if (publisherName.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
		{
			return applicationName.Equals("Test", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private bool IsApplicable(string publisher, string appName, Version version, bool exact)
	{
		if (Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase) && Name.Equals(appName, StringComparison.OrdinalIgnoreCase))
		{
			if (!(Version >= version) || exact)
			{
				return Version == version;
			}
			return true;
		}
		return false;
	}

	public bool IsApplicable(SymbolReferenceSpecification requestedReference)
	{
		return IsApplicable(requestedReference.Publisher, requestedReference.Name, requestedReference.Version, requestedReference.Exact);
	}

	public bool IsSatisfiedBy(string publisher, string appName, Version version)
	{
		bool flag = Version == null || version == null;
		if (string.Equals(Publisher, publisher, StringComparison.OrdinalIgnoreCase) && string.Equals(Name, appName, StringComparison.OrdinalIgnoreCase) && !flag)
		{
			if (!(Version <= version) || Exact)
			{
				return object.Equals(Version, version);
			}
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SymbolReferenceSpecification)obj);
	}

	public bool Equals(SymbolReferenceSpecification other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(Publisher, other.Publisher, StringComparison.OrdinalIgnoreCase) && object.Equals(Version, other.Version) && AppId == other.AppId)
		{
			return Exact == other.Exact;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(Name ?? string.Empty), StringComparer.OrdinalIgnoreCase.GetHashCode(Publisher ?? string.Empty), Version?.GetHashCode() ?? 0, AppId.GetHashCode(), Exact.GetHashCode());
	}

	internal void ValidateReferenceModuleSpecification(Guid appId, IList<Diagnostic> diagnostics)
	{
		if (!IsApplicationSymbolReference() && !IsPlatformSymbolReference() && !IsTestSymbolReference() && appId != AppId)
		{
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1042, AppId, Publisher, Name, Version, appId));
		}
	}

	public string ToDisplayString()
	{
		return string.Concat(Name, " by ", Publisher, " (", Version, ")");
	}
}
