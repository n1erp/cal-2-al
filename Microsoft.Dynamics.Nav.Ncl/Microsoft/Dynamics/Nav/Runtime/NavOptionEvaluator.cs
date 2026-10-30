using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavOptionEvaluator : NavValueEvaluator<NavOption>
{
	private readonly bool skipOptionCaption;

	private static readonly NavOptionEvaluator instance = new NavOptionEvaluator();

	internal static NavOptionEvaluator Instance => instance;

	private bool SkipOptionCaption
	{
		get
		{
			if (skipOptionCaption && NavCurrentThread.Session != null)
			{
				return NavCurrentThread.Session.ClientConnectionType == ConnectionType.WebServiceClient;
			}
			return false;
		}
	}

	internal NavOptionEvaluator()
	{
		skipOptionCaption = ServerUserSettings.Instance.ServicesOptionFormat.Value == ServicesOptionFormat.OptionString;
	}

	protected override bool InternalEvaluate(DataError dataError, out NavOption value, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		int num;
		if (SkipOptionCaption || (num = metadata.NavOptionMetadata.GetIndexFromCaption(source)) < 0)
		{
			num = metadata.NavOptionMetadata.GetIndexFromOption(source);
			if (num < 0)
			{
				if (dataError == DataError.TrapError)
				{
					value = GetDefaultValue(metadata);
					return false;
				}
				throw NavNCLInvalidOptionStringException.Create(CultureInfo.CurrentCulture, source, metadata.NavOptionMetadata.OptionCaption);
			}
		}
		value = NavOption.Create(metadata.NavOptionMetadata, num);
		return true;
	}

	protected override NavOption GetDefaultValue(INavValueMetadata metadata)
	{
		return NavOption.Create(metadata.NavOptionMetadata, 0);
	}
}
