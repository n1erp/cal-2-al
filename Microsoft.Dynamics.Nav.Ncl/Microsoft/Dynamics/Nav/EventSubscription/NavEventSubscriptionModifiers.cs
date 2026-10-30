using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventSubscriptionModifiers
{
	private static readonly NavEventExclusionFilter[] emptyFilters = new NavEventExclusionFilter[0];

	private static readonly NavEventErrorReporter[] emptyReporters = new NavEventErrorReporter[0];

	private readonly Dictionary<Tuple<ApplicationObjectId, string>, NavEventExclusionFilter[]> exclusionFiltersByPublisherMethod;

	private readonly Dictionary<Tuple<ApplicationObjectId, string>, NavEventSubscriptionSettingsEntry[]> settingsEntriesByPublisherMethod;

	private readonly Dictionary<Tuple<ApplicationObjectId, string>, NavEventErrorReporter[]> errorReportersByPublisherMethod;

	public NavEventSubscriptionModifiers(IEnumerable<NavEventExclusionFilter> exclusionFilters, IEnumerable<NavEventSubscriptionSettingsEntry> settingsEntries, IEnumerable<NavEventErrorReporter> errorReporters)
	{
		exclusionFiltersByPublisherMethod = (from f in exclusionFilters
			group f by new { f.PublisherObject, f.PublisherMethod }).ToDictionary(f => Tuple.Create(f.Key.PublisherObject, f.Key.PublisherMethod), f => f.ToArray());
		settingsEntriesByPublisherMethod = (from e in settingsEntries
			group e by new { e.PublisherObject, e.PublisherMethod }).ToDictionary(f => Tuple.Create(f.Key.PublisherObject, f.Key.PublisherMethod), f => f.ToArray());
		errorReportersByPublisherMethod = (from r in errorReporters
			group r by new { r.PublisherObject, r.PublisherMethod }).ToDictionary(r => Tuple.Create(r.Key.PublisherObject, r.Key.PublisherMethod), r => r.ToArray());
	}

	public NavEventExclusionFilter[] GetExclusionFiltersForPublisher(ApplicationObjectId objectId, string method)
	{
		if (exclusionFiltersByPublisherMethod.TryGetValue(Tuple.Create(objectId, method), out var value))
		{
			return value;
		}
		return emptyFilters;
	}

	public INavErrorReporter[] GetErrorReportersForPublisher(ApplicationObjectId objectId, string method)
	{
		if (errorReportersByPublisherMethod.TryGetValue(Tuple.Create(objectId, method), out var value))
		{
			return value;
		}
		return emptyReporters;
	}

	public NavEventSubscriptionSettings GetSettingsForPublisher(ApplicationObjectId objectId, string method)
	{
		NavEventSubscriptionSettings navEventSubscriptionSettings = NavEventSubscriptionSettings.None;
		if (settingsEntriesByPublisherMethod.TryGetValue(Tuple.Create(objectId, method), out var value))
		{
			NavEventSubscriptionSettingsEntry[] array = value;
			foreach (NavEventSubscriptionSettingsEntry navEventSubscriptionSettingsEntry in array)
			{
				navEventSubscriptionSettings |= navEventSubscriptionSettingsEntry.Settings;
			}
		}
		return navEventSubscriptionSettings;
	}
}
