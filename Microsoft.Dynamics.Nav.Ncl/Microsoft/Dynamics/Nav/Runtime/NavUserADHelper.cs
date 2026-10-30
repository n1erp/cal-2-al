using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavUserADHelper
{
	private const string ObjectSid = "objectSid";

	private const string ObjectSidFilter = "(&(objectClass=user)({0}={1}))";

	private const string RootDirectoryTemplate = "GC://{0}";

	private const string DistinguishedName = "DistinguishedName";

	private const string ActiveDirectoryNotSupported = "ActiveDirectory not supported.";

	private static object domainLock = new object();

	private static string rootDomain;

	private static bool isForest;

	public static bool IsADForest
	{
		get
		{
			FindRootAndCurrentDomains();
			return isForest;
		}
	}

	public static string GetDistinguishedName(string userSid)
	{
		FindRootAndCurrentDomains();
		using DirectoryEntry searchRoot = new DirectoryEntry(string.Format(CultureInfo.InvariantCulture, "GC://{0}", rootDomain));
		using DirectorySearcher directorySearcher = new DirectorySearcher(searchRoot);
		directorySearcher.SearchScope = SearchScope.Subtree;
		directorySearcher.Filter = string.Format(CultureInfo.InvariantCulture, "(&(objectClass=user)({0}={1}))", "objectSid", userSid);
		directorySearcher.ReferralChasing = ReferralChasingOption.None;
		directorySearcher.PropertiesToLoad.AddRange(new string[1] { "DistinguishedName" });
		return directorySearcher.FindOne()?.Properties["DistinguishedName"][0].ToString();
	}

	private static void FindRootAndCurrentDomains()
	{
		if (rootDomain != null)
		{
			if (string.IsNullOrEmpty(rootDomain))
			{
				throw new NotSupportedException("ActiveDirectory not supported.");
			}
			return;
		}
		lock (domainLock)
		{
			if (rootDomain != null)
			{
				if (string.IsNullOrEmpty(rootDomain))
				{
					throw new NotSupportedException("ActiveDirectory not supported.");
				}
				return;
			}
			try
			{
				DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain);
				using Domain domain = Domain.GetDomain(context);
				rootDomain = domain.Forest?.RootDomain?.Name ?? domain.Name;
				isForest = !domain.Name.Equals(rootDomain);
			}
			catch (Exception ex) when (ex is ActiveDirectoryObjectNotFoundException || ex is ActiveDirectoryOperationException)
			{
				rootDomain = string.Empty;
				throw new NotSupportedException("ActiveDirectory not supported.", ex);
			}
		}
	}
}
