using System;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Resources;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlIndexAndChanges
{
	private readonly NCLMetaKey oldSqlIndex;

	private readonly NCLMetaKey oldSiftIndex;

	private readonly IndexChangeTypes changes;

	private readonly NCLMetaKey newKey;

	public NCLMetaKey OldSqlIndex => oldSqlIndex;

	public NCLMetaKey OldSiftIndex => oldSiftIndex;

	public IndexChangeTypes Changes => changes;

	public NCLMetaKey NewKey => newKey;

	public bool NotChanged => changes == IndexChangeTypes.None;

	public bool Missing => (changes & IndexChangeTypes.Missing) != 0;

	public bool Excessive => (changes & IndexChangeTypes.Excessive) != 0;

	public bool ClusterIndex
	{
		get
		{
			if (changes != IndexChangeTypes.None)
			{
				if (oldSqlIndex == null || !oldSqlIndex.Clustered)
				{
					if (newKey != null)
					{
						return newKey.Clustered;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool PrimaryKey
	{
		get
		{
			if (changes != IndexChangeTypes.None)
			{
				if (oldSqlIndex == null || !oldSqlIndex.IsPrimaryKey)
				{
					if (newKey != null)
					{
						return newKey.IsPrimaryKey;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool IndexFields => (changes & IndexChangeTypes.IndexFields) != 0;

	public bool UniqueChanged => (changes & IndexChangeTypes.Unique) != 0;

	public bool SiftExcessive => (changes & IndexChangeTypes.SiftExcessive) != 0;

	public bool SiftMissing => (changes & IndexChangeTypes.SiftMissing) != 0;

	public bool SumFields => (changes & IndexChangeTypes.SumFields) != 0;

	public string IndexChangeTypeDescription
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = changes.ToString().Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				IndexChangeTypes result = (Enum.TryParse<IndexChangeTypes>(text, out result) ? result : IndexChangeTypes.None);
				string text2 = SqlIndexAndChangeDescriptions.ResourceManager.GetString(result.ToString());
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append((!string.IsNullOrEmpty(text2)) ? text2 : text);
			}
			return stringBuilder.ToString();
		}
	}

	public bool IsPotentialUniqueConstraintViolation
	{
		get
		{
			if ((Missing || UniqueChanged || IndexFields) && NewKey.Unique)
			{
				return true;
			}
			return false;
		}
	}

	public NavSqlIndexAndChanges(NCLMetaKey oldSqlIndex, NCLMetaKey oldSiftIndex, IndexChangeTypes changes, NCLMetaKey newKey)
	{
		this.oldSqlIndex = oldSqlIndex;
		this.oldSiftIndex = oldSiftIndex;
		this.changes = changes;
		this.newKey = newKey;
	}
}
