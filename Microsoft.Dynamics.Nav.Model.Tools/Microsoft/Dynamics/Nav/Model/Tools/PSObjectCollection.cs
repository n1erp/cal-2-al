using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class PSObjectCollection : IEnumerable<PSObject>, IEnumerable
{
	private readonly IEnumerable<PSObject> objects;

	private readonly string summary;

	public string Summary => summary;

	public PSObjectCollection(IEnumerable<PSObject> objects, string summary, PSHost host)
	{
		this.objects = objects;
		try
		{
			this.summary = FormatSummary(summary, host.UI.RawUI.BufferSize.Width);
		}
		catch (HostException)
		{
			this.summary = summary;
		}
	}

	private static string FormatSummary(string s, int width)
	{
		width--;
		if (width < 79)
		{
			return s;
		}
		List<string> list = s.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None).ToList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Length > width)
			{
				int num2 = list[num].IndexOf(" – ", StringComparison.Ordinal);
				num2 = ((num2 <= 0) ? (list[num].Length - list[num].TrimStart().Length) : (num2 + 3));
				int num3 = width - num2;
				string text = list[num];
				list[num] = list[num].Substring(0, width);
				int i = width;
				int num4 = num + 1;
				for (; i < text.Length; i += num3)
				{
					list.Insert(num4++, new string(' ', num2) + ((i + num3 > text.Length) ? text.Substring(i) : text.Substring(i, num3)));
				}
			}
		}
		return string.Join(Environment.NewLine, list);
	}

	public IEnumerator<PSObject> GetEnumerator()
	{
		return objects.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return objects.GetEnumerator();
	}

	public override string ToString()
	{
		return Summary;
	}
}
