using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public sealed class ApplicationObjectLanguageInfo
{
	internal class ApplicationObjectLanguageInfoComparer : IComparer<ApplicationObjectLanguageInfo>
	{
		public int Compare(ApplicationObjectLanguageInfo x, ApplicationObjectLanguageInfo y)
		{
			if (x == y)
			{
				return 0;
			}
			int result;
			if ((result = x.ObjectType - y.ObjectType) != 0)
			{
				return result;
			}
			if ((result = x.Id - y.Id) != 0)
			{
				return result;
			}
			if ((result = x.LCID - y.LCID) != 0)
			{
				return result;
			}
			return 0;
		}
	}

	private static readonly ApplicationObjectLanguageInfoComparer comparer = new ApplicationObjectLanguageInfoComparer();

	private readonly SortedList<string, TranslateLine> translateLines = new SortedList<string, TranslateLine>();

	internal static ApplicationObjectLanguageInfoComparer Comparer => comparer;

	public ElementType ObjectType { get; private set; }

	public int Id { get; private set; }

	public int LCID { get; private set; }

	public IList<TranslateLine> TranslateLines => translateLines.Values;

	internal IEnumerable<string> GetLines()
	{
		return TranslateLines.Select((TranslateLine l) => l.Line);
	}

	internal ApplicationObjectLanguageInfo(ElementType objectType, int id, int lcid)
	{
		ObjectType = objectType;
		Id = id;
		LCID = lcid;
	}

	internal void AddLine(string key, string value)
	{
		if (!translateLines.ContainsKey(key))
		{
			translateLines.Add(key, new TranslateLine
			{
				Key = key,
				Value = value
			});
		}
	}
}
