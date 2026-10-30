using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public sealed class DiagnosticDescriptor : IEquatable<DiagnosticDescriptor>
{
	public string Id { get; }

	public LocalizableString Title { get; }

	public LocalizableString Description { get; }

	public string HelpLinkUri { get; }

	public LocalizableString MessageFormat { get; }

	public string Category { get; }

	public DiagnosticSeverity DefaultSeverity { get; }

	public bool IsEnabledByDefault { get; }

	public IEnumerable<string> CustomTags { get; private set; }

	public DiagnosticDescriptor(string id, LocalizableString title, LocalizableString messageFormat, string category, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, LocalizableString description = null, string helpLinkUri = null, params string[] customTags)
		: this(id, title, messageFormat, category, defaultSeverity, isEnabledByDefault, description, helpLinkUri, customTags.AsImmutableOrEmpty())
	{
	}

	internal DiagnosticDescriptor(string id, LocalizableString title, LocalizableString messageFormat, string category, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, LocalizableString description, string helpLinkUri, IEnumerable<string> customTags)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException(CodeAnalysisResources.DiagnosticIdCantBeNullOrWhitespace, "id");
		}
		if (messageFormat == null)
		{
			throw new ArgumentNullException("messageFormat");
		}
		if (category == null)
		{
			throw new ArgumentNullException("category");
		}
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		Id = id;
		Title = title;
		Category = category;
		MessageFormat = messageFormat;
		DefaultSeverity = defaultSeverity;
		IsEnabledByDefault = isEnabledByDefault;
		Description = description ?? ((LocalizableString)string.Empty);
		HelpLinkUri = helpLinkUri ?? string.Empty;
		CustomTags = customTags.ToArray();
	}

	public bool Equals(DiagnosticDescriptor other)
	{
		if (other != null && Category == other.Category && DefaultSeverity == other.DefaultSeverity && Description.Equals(other.Description) && HelpLinkUri == other.HelpLinkUri && Id == other.Id && IsEnabledByDefault == other.IsEnabledByDefault && MessageFormat.Equals(other.MessageFormat))
		{
			return Title.Equals(other.Title);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as DiagnosticDescriptor);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Category.GetHashCode(), Hash.Combine(DefaultSeverity.GetHashCode(), Hash.Combine(Description.GetHashCode(), Hash.Combine(HelpLinkUri.GetHashCode(), Hash.Combine(Id.GetHashCode(), Hash.Combine(IsEnabledByDefault.GetHashCode(), Hash.Combine(MessageFormat.GetHashCode(), Title.GetHashCode())))))));
	}
}
