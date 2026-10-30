using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class InitializeAppGroupProfiler
{
	private readonly Dictionary<ObjectType, InitializeAppGroupProfilerEntry> overallEntriesByType = new Dictionary<ObjectType, InitializeAppGroupProfilerEntry>();

	private int totalGroups;

	private TimeSpan totalTime;

	public static readonly ObjectType[] SupportedTypes = new ObjectType[8]
	{
		ObjectType.CodeUnit,
		ObjectType.Page,
		ObjectType.PageExtension,
		ObjectType.Query,
		ObjectType.Report,
		ObjectType.Table,
		ObjectType.TableExtension,
		ObjectType.XmlPort
	};

	public InitializeAppGroupProfiler()
	{
		ObjectType[] supportedTypes = SupportedTypes;
		foreach (ObjectType objectType in supportedTypes)
		{
			overallEntriesByType.Add(objectType, new InitializeAppGroupProfilerEntry(objectType));
		}
	}

	public void ProcessCompletedSession(InitializeAppGroupProfilerSession session)
	{
		foreach (InitializeAppGroupProfilerEntry entry in session.Entries)
		{
			InitializeAppGroupProfilerEntry initializeAppGroupProfilerEntry = overallEntriesByType[entry.ObjectType];
			initializeAppGroupProfilerEntry.Constructed += entry.Constructed;
			initializeAppGroupProfilerEntry.Referenced += entry.Referenced;
			initializeAppGroupProfilerEntry.ProcessingTime += entry.ProcessingTime;
		}
		totalTime += session.TotalTime;
		totalGroups++;
	}

	public string GenerateOverallTelemetry()
	{
		List<string> list = new List<string>();
		list.Add("Groups:" + totalGroups);
		AddPerTypeInfo(list, overallEntriesByType.Values);
		list.Add("Total:" + totalTime.ToStringWithMilliseconds());
		return string.Join("; ", list);
	}

	public static string GenerateTelemetryForSession(NavAppGroup group, InitializeAppGroupProfilerSession session)
	{
		List<string> list = new List<string>();
		list.Add("GroupId:" + group.GroupId);
		list.Add("Extensions:" + string.Join(" | ", group.OrderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.TechnicalDisplayName)));
		AddPerTypeInfo(list, session.Entries);
		list.Add("Total:" + session.TotalTime.ToStringWithMilliseconds());
		return string.Join("; ", list);
	}

	private static void AddPerTypeInfo(List<string> toJoin, IEnumerable<InitializeAppGroupProfilerEntry> entries)
	{
		foreach (InitializeAppGroupProfilerEntry item2 in entries.OrderBy((InitializeAppGroupProfilerEntry e) => e.ObjectType))
		{
			string text = Abbreviate(item2.ObjectType);
			string item = string.Format(CultureInfo.InvariantCulture, "{0}_NEW:{1}; {0}_REF:{2}; {0}_TIME:{3}", text, item2.Constructed, item2.Referenced, item2.ProcessingTime.ToStringWithMilliseconds());
			toJoin.Add(item);
		}
	}

	private static string Abbreviate(ObjectType type)
	{
		return type switch
		{
			ObjectType.CodeUnit => "C", 
			ObjectType.Page => "P", 
			ObjectType.PageExtension => "PE", 
			ObjectType.Query => "Q", 
			ObjectType.Report => "R", 
			ObjectType.Table => "T", 
			ObjectType.TableExtension => "TE", 
			ObjectType.XmlPort => "X", 
			_ => null, 
		};
	}
}
