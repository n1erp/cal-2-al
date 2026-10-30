using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlCommandCache : TreeObject
{
	private NavSqlCommand cachedCommands;

	private NavSqlCommand lastCachedCommand;

	private readonly int maxNumberOfCachedCommands;

	private readonly int maxNumberOfCachedCommandsSharedSchema;

	private Dictionary<NavSqlCommandCacheKey, NavSqlCommand> cachedCommandDictionary;

	private NavSqlCommandCache(NavSqlConnection parentConnection, int maxNumberOfCachedCommands, int maxNumberOfCachedCommandsSharedSchema)
		: base(parentConnection)
	{
		Initialize();
		this.maxNumberOfCachedCommands = maxNumberOfCachedCommands;
		this.maxNumberOfCachedCommandsSharedSchema = maxNumberOfCachedCommandsSharedSchema;
	}

	public static NavSqlCommandCache Create(NavSqlConnection parentConnection, int maxNumberOfCachedCommands, int maxNumberOfCachedCommandsSharedSchema)
	{
		return new NavSqlCommandCache(parentConnection, maxNumberOfCachedCommands, maxNumberOfCachedCommandsSharedSchema);
	}

	internal IEnumerable<NavSqlCommand> CachedCommands()
	{
		NavSqlCommand command = cachedCommands;
		while (command != null && cachedCommands != null)
		{
			yield return command;
			command = command.NextSqlCommand;
		}
	}

	internal void SetLastUsedCachedCommand(NavSqlCommand command)
	{
		command.UnlinkFromList(ref cachedCommands, ref lastCachedCommand);
		command.LinkToList(ref cachedCommands, ref lastCachedCommand);
	}

	public void Initialize()
	{
		CloseAllReadersOnTransactionEnd(recreateCommand: false);
		cachedCommands = null;
		lastCachedCommand = null;
		cachedCommandDictionary = new Dictionary<NavSqlCommandCacheKey, NavSqlCommand>();
	}

	public void CloseAllReadersOnTransactionEnd(bool recreateCommand = true)
	{
		foreach (NavSqlCommand item in CachedCommands())
		{
			try
			{
				item.CloseReader(forceClose: true, recreateCommand);
			}
			catch (ObjectDisposedException)
			{
			}
		}
	}

	public void AddCachedCommand(NavSqlCommandCacheKey key, NavSqlCommand command)
	{
		if (command.CanCacheCommand)
		{
			bool? flag = command.Connection.Database.GetTenantDatabasePropertiesIfLoaded()?.PerCompanySchema;
			if (((!flag.HasValue || flag.Value) && cachedCommandDictionary.Count >= maxNumberOfCachedCommands) || (flag.HasValue && flag == false && cachedCommandDictionary.Count >= maxNumberOfCachedCommandsSharedSchema))
			{
				NavSqlCommand navSqlCommand = lastCachedCommand;
				navSqlCommand.UnlinkFromList(ref cachedCommands, ref lastCachedCommand);
				cachedCommandDictionary.Remove(navSqlCommand.CommandKey);
				navSqlCommand.Dispose();
			}
			RemoveCachedCommand(key);
			command.CommandKey = key;
			command.LinkToList(ref cachedCommands, ref lastCachedCommand);
			cachedCommandDictionary.Add(key, command);
		}
	}

	internal void RemoveCachedCommand(NavSqlCommandCacheKey key)
	{
		if (cachedCommandDictionary.TryGetValue(key, out var value))
		{
			cachedCommandDictionary.Remove(key);
			value.UnlinkFromList(ref cachedCommands, ref lastCachedCommand);
			value.Dispose();
		}
	}

	public NavSqlCommand GetCachedCommand(NavSqlCommandCacheKey key)
	{
		bool tryGetResult = false;
		NavSqlCommand value = null;
		if (cachedCommandDictionary.TryGetValue(key, out value))
		{
			if (value.Stale)
			{
				cachedCommandDictionary.Remove(key);
				value.UnlinkFromList(ref cachedCommands, ref lastCachedCommand);
				value.Dispose();
				value = null;
			}
			else
			{
				tryGetResult = true;
				SetLastUsedCachedCommand(value);
				value.SetCommandTimeout();
			}
		}
		UpdatePerformanceCountersWithCommandCacheRequestResult(tryGetResult);
		return value;
	}

	private static void UpdatePerformanceCountersWithCommandCacheRequestResult(bool tryGetResult)
	{
		NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithCommandCacheRequestResult(tryGetResult);
	}
}
