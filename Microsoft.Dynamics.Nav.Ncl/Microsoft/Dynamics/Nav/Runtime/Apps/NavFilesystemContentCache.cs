using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavFilesystemContentCache : INavFilesystemContentCache
{
	private class CacheKeyEqualityComparer : IEqualityComparer<NavFilesystemContentCacheKey>
	{
		public bool Equals(NavFilesystemContentCacheKey x, NavFilesystemContentCacheKey y)
		{
			if (x.ContentHash.Equals(y.ContentHash))
			{
				return x.FileExtension.Equals(y.FileExtension);
			}
			return false;
		}

		public int GetHashCode(NavFilesystemContentCacheKey obj)
		{
			return obj.ContentHash.GetHashCode() ^ obj.FileExtension.GetHashCode();
		}
	}

	private readonly object lockObj = new object();

	private readonly string cacheRoot;

	private readonly string currentVersionDirectory;

	private HashSet<NavFilesystemContentCacheKey> knownCacheKeys = new HashSet<NavFilesystemContentCacheKey>(new CacheKeyEqualityComparer());

	public NavFilesystemContentCache(string cacheRoot, string cacheVersion)
	{
		this.cacheRoot = cacheRoot;
		currentVersionDirectory = Path.Combine(this.cacheRoot, cacheVersion);
	}

	public byte[] GetContent(NavFilesystemContentCacheKey cacheKey)
	{
		if (cacheKey == null)
		{
			throw new ArgumentNullException("cacheKey");
		}
		lock (lockObj)
		{
			if (knownCacheKeys.Contains(cacheKey))
			{
				string path = BuildContentPath(cacheKey);
				if (File.Exists(path))
				{
					return ReadFile(path);
				}
			}
		}
		return null;
	}

	public string GetPath(NavFilesystemContentCacheKey cacheKey)
	{
		if (cacheKey == null)
		{
			throw new ArgumentNullException("cacheKey");
		}
		lock (lockObj)
		{
			if (knownCacheKeys.Contains(cacheKey))
			{
				string text = BuildContentPath(cacheKey);
				if (File.Exists(text))
				{
					return text;
				}
			}
		}
		return null;
	}

	public void PutContent(NavFilesystemContentCacheKey cacheKey, byte[] content)
	{
		if (cacheKey == null)
		{
			throw new ArgumentNullException("cacheKey");
		}
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		lock (lockObj)
		{
			if (!knownCacheKeys.Contains(cacheKey))
			{
				string path = BuildSubfolderPath(cacheKey);
				string path2 = BuildContentPath(cacheKey);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				if (!File.Exists(path2))
				{
					WriteFile(path2, content);
				}
				knownCacheKeys.Add(cacheKey);
			}
		}
	}

	public bool HasContent(NavFilesystemContentCacheKey cacheKey)
	{
		if (cacheKey == null)
		{
			throw new ArgumentNullException("cacheKey");
		}
		lock (lockObj)
		{
			return knownCacheKeys.Contains(cacheKey);
		}
	}

	public void PurgeUnreferencedContent(IEnumerable<Sha256Hash> referencedHashes)
	{
		if (referencedHashes == null)
		{
			throw new ArgumentNullException("referencedHashes");
		}
		lock (lockObj)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			int count = knownCacheKeys.Count;
			HashSet<Sha256Hash> referencedAsHashSet = referencedHashes.ToHashSet();
			List<NavFilesystemContentCacheKey> list = knownCacheKeys.Where((NavFilesystemContentCacheKey known) => !referencedAsHashSet.Contains(known.ContentHash)).ToList();
			list.ForEach(DeleteContent);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000072A", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Content cache purge -- Path:{0}; OrigFiles:{1}; PurgedFiles:{2}; RemainingFiles:{3}; Elapsed:{4}", currentVersionDirectory, count, list.Count, count - list.Count, stopwatch.Elapsed.ToStringWithMilliseconds());
		}
	}

	public void Initialize()
	{
		lock (lockObj)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			if (!Directory.Exists(cacheRoot))
			{
				Directory.CreateDirectory(cacheRoot);
			}
			DeleteDirectoriesForOldVersions();
			if (!Directory.Exists(currentVersionDirectory))
			{
				Directory.CreateDirectory(currentVersionDirectory);
			}
			string[] files = Directory.GetFiles(currentVersionDirectory, "*.*", SearchOption.AllDirectories);
			knownCacheKeys = (from file in files
				select new
				{
					BaseName = Path.GetFileNameWithoutExtension(file),
					Extension = Path.GetExtension(file).Replace(".", "")
				} into split
				select new NavFilesystemContentCacheKey(Sha256Hash.CreateFromHexString(split.BaseName), split.Extension)).ToHashSet(new CacheKeyEqualityComparer());
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000072B", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Content cache init -- Path:{0}; Files:{1}; Elapsed:{2}", currentVersionDirectory, knownCacheKeys.Count, stopwatch.Elapsed.ToStringWithMilliseconds());
		}
	}

	private void DeleteContent(NavFilesystemContentCacheKey cacheKey)
	{
		if (cacheKey == null)
		{
			throw new ArgumentNullException("cacheKey");
		}
		string path = BuildContentPath(cacheKey);
		knownCacheKeys.Remove(cacheKey);
		DeleteFileSafe(path);
	}

	private void DeleteDirectoriesForOldVersions()
	{
		string[] directories = Directory.GetDirectories(cacheRoot);
		string[] array = directories;
		foreach (string text in array)
		{
			if (text != currentVersionDirectory)
			{
				DeleteDirectorySafe(text, recursive: true);
			}
		}
	}

	private string BuildSubfolderPath(NavFilesystemContentCacheKey cacheKey)
	{
		return Path.Combine(currentVersionDirectory, cacheKey.ContentHash.HexString.Substring(0, 2));
	}

	private string BuildContentPath(NavFilesystemContentCacheKey cacheKey)
	{
		return Path.Combine(BuildSubfolderPath(cacheKey), cacheKey.ContentHash.HexString + "." + cacheKey.FileExtension);
	}

	private static void DeleteDirectorySafe(string path, bool recursive)
	{
		try
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive);
			}
		}
		catch
		{
		}
	}

	private static void DeleteFileSafe(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
		}
		catch
		{
		}
	}

	private static byte[] ReadFile(string path)
	{
		return File.ReadAllBytes(path);
	}

	private static void WriteFile(string path, byte[] content)
	{
		File.WriteAllBytes(path, content);
		File.SetAttributes(path, FileAttributes.ReadOnly);
	}
}
