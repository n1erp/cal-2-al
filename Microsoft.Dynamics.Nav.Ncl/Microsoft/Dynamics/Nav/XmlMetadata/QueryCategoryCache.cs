using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

internal class QueryCategoryCache
{
	private class QueryCategoryInfo : IDisposable
	{
		private ConcurrentDictionary<string, List<int>> queryCategoryList;

		private readonly ConcurrentDictionary<int, string> queriesWithCategoryDictionary = new ConcurrentDictionary<int, string>();

		private bool disposedValue;

		private static char[] categoryTrimChars => new char[2] { ' ', '\t' };

		internal int Count
		{
			get
			{
				if (queryCategoryList == null)
				{
					return 0;
				}
				return queryCategoryList.Count;
			}
		}

		internal static QueryCategoryInfo Create()
		{
			QueryCategoryInfo queryCategoryInfo = new QueryCategoryInfo();
			queryCategoryInfo.LoadQueryCategoriesFromMetadata();
			return queryCategoryInfo;
		}

		private QueryCategoryInfo()
		{
		}

		internal void Clear()
		{
			queryCategoryList?.Clear();
			queryCategoryList = null;
			queriesWithCategoryDictionary.Clear();
		}

		internal void Add(IList<int> addItems)
		{
			if (addItems == null)
			{
				return;
			}
			foreach (int addItem in addItems)
			{
				MetaQuery queryMetadata = NavGlobal.MetadataProvider.GetQueryMetadata(addItem);
				AddQuryToInfo(queryMetadata.Id, queryMetadata.QueryCategory);
			}
		}

		internal void Remove(IList<int> itemsToRemove)
		{
			if (itemsToRemove == null || queryCategoryList == null || queryCategoryList.Count == 0)
			{
				return;
			}
			lock (queryCategoryList)
			{
				foreach (int item in itemsToRemove)
				{
					if (!queriesWithCategoryDictionary.TryRemove(item, out var value))
					{
						continue;
					}
					string[] array = SplitAndTrimCategories(value);
					string[] array2 = array;
					foreach (string key in array2)
					{
						if (queryCategoryList.TryGetValue(key, out var value2))
						{
							value2.Remove(item);
							if (value2.Count == 0)
							{
								queryCategoryList.TryRemove(key, out value2);
							}
						}
					}
				}
			}
		}

		private void LoadQueryCategoriesFromMetadata()
		{
			NavCurrentThread.Session?.Diagnostics?.SendTraceTag("000080E", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Load Query category from metadata for App group: {0}", NavCurrentThread.Session.NavAppGroup.GroupId);
			queryCategoryList = new ConcurrentDictionary<string, List<int>>();
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000142);
			navRecord.ALSetFilter(9, "<>''");
			if (!navRecord.ALFind(DataError.TrapError, "-"))
			{
				return;
			}
			do
			{
				string text = navRecord.GetFieldValue(9).ToString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					int queryId = navRecord.GetFieldValue(1).ToInt32();
					AddQuryToInfo(queryId, text);
				}
			}
			while (navRecord.ALNext() > 0);
		}

		private void AddQuryToInfo(int queryId, string rawCategoryString)
		{
			if (queryCategoryList == null)
			{
				return;
			}
			string[] array = SplitAndTrimCategories(rawCategoryString);
			if (array.Length == 0)
			{
				return;
			}
			string filteredCategoryList = string.Join(",", array);
			queriesWithCategoryDictionary.AddOrUpdate(queryId, filteredCategoryList, (int num2, string oldText) => filteredCategoryList);
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (!queryCategoryList.TryGetValue(key, out var value))
				{
					queryCategoryList.TryAdd(key, new List<int> { queryId });
				}
				else
				{
					value.Add(queryId);
				}
			}
		}

		internal IList<MetaQuery> QueriesWithCategory(string category)
		{
			category = category.Trim(categoryTrimChars);
			if (string.IsNullOrEmpty(category))
			{
				return new List<MetaQuery>();
			}
			if (queryCategoryList == null)
			{
				LoadQueryCategoriesFromMetadata();
			}
			if (queryCategoryList.TryGetValue(category, out var value))
			{
				List<MetaQuery> list = new List<MetaQuery>();
				{
					foreach (int item in value.Distinct())
					{
						try
						{
							MetaQuery queryMetadata = NavGlobal.MetadataProvider.GetQueryMetadata(item);
							list.Add(queryMetadata);
						}
						catch (NavMetadataNotFoundException)
						{
						}
					}
					return list;
				}
			}
			return new List<MetaQuery>();
		}

		private static string[] SplitAndTrimCategories(string rawCategoryString)
		{
			char[] separator = new char[1] { ',' };
			if (string.IsNullOrEmpty(rawCategoryString))
			{
				return new string[0];
			}
			List<string> list = new List<string>();
			string[] array = rawCategoryString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text in array2)
			{
				list.Add(text.Trim(categoryTrimChars));
			}
			return list.ToArray();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					queryCategoryList.Clear();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}

	private ConcurrentDictionary<int, QueryCategoryInfo> queryCategoryCacheDictionary = new ConcurrentDictionary<int, QueryCategoryInfo>();

	internal IList<MetaQuery> QueryList(int appId, string category)
	{
		if (!queryCategoryCacheDictionary.TryGetValue(appId, out var value))
		{
			value = QueryCategoryInfo.Create();
			queryCategoryCacheDictionary.TryAdd(appId, value);
		}
		return value.QueriesWithCategory(category);
	}

	internal void Remove(int appGroupdId, IList<int> itemsToRemove = null)
	{
		QueryCategoryInfo value2;
		if (appGroupdId == 0)
		{
			RemoveInAllAppGroups(itemsToRemove);
		}
		else if (itemsToRemove == null)
		{
			if (queryCategoryCacheDictionary.TryRemove(appGroupdId, out var value))
			{
				value.Clear();
			}
		}
		else if (queryCategoryCacheDictionary.TryGetValue(appGroupdId, out value2))
		{
			value2.Remove(itemsToRemove);
			if (value2.Count == 0)
			{
				queryCategoryCacheDictionary.TryRemove(appGroupdId, out value2);
			}
		}
	}

	private void RemoveInAllAppGroups(IList<int> itemsToRemove)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, QueryCategoryInfo> item in queryCategoryCacheDictionary)
		{
			item.Value.Remove(itemsToRemove);
			if (item.Value.Count == 0)
			{
				list.Add(item.Key);
			}
		}
		foreach (int item2 in list)
		{
			queryCategoryCacheDictionary.TryRemove(item2, out var _);
		}
	}

	internal void Update(int groupId, IList<int> updateItems)
	{
		Remove(groupId, updateItems);
		Add(groupId, updateItems);
	}

	internal void Add(int groupdId, IList<int> addItems)
	{
		if (groupdId == 0)
		{
			foreach (KeyValuePair<int, QueryCategoryInfo> item in queryCategoryCacheDictionary)
			{
				item.Value.Add(addItems);
			}
			return;
		}
		if (queryCategoryCacheDictionary.TryGetValue(groupdId, out var value))
		{
			value.Add(addItems);
		}
	}
}
