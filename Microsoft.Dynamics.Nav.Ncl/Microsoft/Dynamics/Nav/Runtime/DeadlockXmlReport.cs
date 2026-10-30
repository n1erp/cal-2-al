using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DeadlockXmlReport
{
	public class Process
	{
		public string Id { get; set; }

		public long OwnerId { get; set; }

		public Process(string id, long ownerid)
		{
			Id = id;
			OwnerId = ownerid;
		}
	}

	private const string VictimListTag = "victim-list";

	private const string VictimProcessTag = "victimProcess";

	private const string ProcessListTag = "process-list";

	private const string ProcessTag = "process";

	private const string IdAttr = "id";

	private const string OwnerIdAttr = "ownerId";

	private readonly List<Process> processList = new List<Process>();

	private readonly HashSet<string> victimList = new HashSet<string>();

	public string SourceReport { get; private set; }

	public ISet<string> VictimList => victimList;

	public IReadOnlyList<Process> ProcessList => processList;

	public void Load(string xml)
	{
		using (XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(xml)))
		{
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.Name == "victim-list")
				{
					ReadVictimList(xmlTextReader);
				}
				else if (xmlTextReader.Name == "process-list")
				{
					ReadProcessList(xmlTextReader);
				}
			}
		}
		SourceReport = xml;
	}

	public override string ToString()
	{
		return SourceReport;
	}

	private void ReadProcessList(XmlReader reader)
	{
		ReadUntilEndTag(reader, "process-list", (XmlReader r) =>
		{
			if (r.Name == "process")
			{
				ReadProcess(r);
			}
		});
	}

	private void ReadProcess(XmlReader reader)
	{
		string attribute = reader.GetAttribute("id");
		if (long.TryParse(reader.GetAttribute("ownerId"), out var result))
		{
			processList.Add(new Process(attribute, result));
		}
	}

	private static void ReadUntilEndTag(XmlReader reader, string endTag, Action<XmlReader> action)
	{
		while (reader.Read())
		{
			if (reader.IsStartElement())
			{
				action(reader);
			}
			if (reader.Name == endTag && !reader.IsStartElement())
			{
				break;
			}
		}
	}

	private void ReadVictimList(XmlReader reader)
	{
		ReadUntilEndTag(reader, "victim-list", (XmlReader r) =>
		{
			if (r.Name == "victimProcess")
			{
				victimList.Add(r.GetAttribute("id"));
			}
		});
	}
}
