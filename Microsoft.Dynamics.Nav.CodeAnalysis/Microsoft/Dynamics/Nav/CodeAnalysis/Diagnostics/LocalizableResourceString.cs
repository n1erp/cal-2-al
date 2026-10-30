using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public sealed class LocalizableResourceString : LocalizableString
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

	private readonly string nameOfLocalizableResource;

	private readonly ResourceManager resourceManager;

	private readonly Type resourceSource;

	private readonly string[] formatArguments;

	public LocalizableResourceString(string nameOfLocalizableResource, ResourceManager resourceManager, Type resourceSource)
		: this(nameOfLocalizableResource, resourceManager, resourceSource, SpecializedCollections.EmptyArray<string>())
	{
	}

	public LocalizableResourceString(string nameOfLocalizableResource, ResourceManager resourceManager, Type resourceSource, params string[] formatArguments)
	{
		if (nameOfLocalizableResource == null)
		{
			throw new ArgumentNullException("nameOfLocalizableResource");
		}
		if (resourceManager == null)
		{
			throw new ArgumentNullException("resourceManager");
		}
		if (resourceSource == null)
		{
			throw new ArgumentNullException("resourceSource");
		}
		if (formatArguments == null)
		{
			throw new ArgumentNullException("formatArguments");
		}
		this.resourceManager = resourceManager;
		this.nameOfLocalizableResource = nameOfLocalizableResource;
		this.resourceSource = resourceSource;
		this.formatArguments = formatArguments;
	}

	protected override string GetText(IFormatProvider formatProvider)
	{
		CultureInfo cultureInfo = (formatProvider as CultureInfo) ?? CultureInfo.CurrentUICulture;
		string text = resourceManager.GetString(nameOfLocalizableResource, cultureInfo);
		if (text == null)
		{
			return string.Empty;
		}
		if (formatArguments.Length == 0)
		{
			return text;
		}
		object[] args = formatArguments;
		return string.Format(cultureInfo, text, args);
	}

	protected override bool AreEqual(object other)
	{
		LocalizableResourceString localizableResourceString = other as LocalizableResourceString;
		if (other != null && nameOfLocalizableResource == localizableResourceString.nameOfLocalizableResource && resourceManager == localizableResourceString.resourceManager && resourceSource == localizableResourceString.resourceSource)
		{
			return formatArguments.SequenceEqual(localizableResourceString.formatArguments, new ObjectComparer());
		}
		return false;
	}

	protected override int GetHash()
	{
		int hashCode = nameOfLocalizableResource.GetHashCode();
		int hashCode2 = resourceManager.GetHashCode();
		int hashCode3 = resourceSource.GetHashCode();
		object[] values = formatArguments;
		return Hash.Combine(hashCode, Hash.Combine(hashCode2, Hash.Combine(hashCode3, Hash.CombineValues(values))));
	}
}
