using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavValueMetadata : INavValueMetadata
{
	private NavNclType nclType;

	private static NCLOptionMetadata defaultOptionMetadata = NCLOptionMetadata.Default;

	public NavType NavType => NavNclTypeHelper.GetNavTypeFromNclType(nclType);

	public NavNclType NclType => nclType;

	public bool HasMetadata
	{
		get
		{
			switch (nclType)
			{
			case NavNclType.NavOption:
			case NavNclType.NavOemText:
			case NavNclType.NavOemCode:
			case NavNclType.NavText:
			case NavNclType.NavCode:
				return true;
			default:
				return false;
			}
		}
	}

	public virtual int NavDefinedLengthMetadata
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public virtual NCLOptionMetadata NavOptionMetadata
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public static NavValueMetadata DefaultMetadata(NavNclType nclType)
	{
		return nclType switch
		{
			NavNclType.NavOemCode => new NavValueDefinedLengthMetadata(NavNclType.NavOemCode, 2048), 
			NavNclType.NavOemText => new NavValueDefinedLengthMetadata(NavNclType.NavOemText, 2048), 
			NavNclType.NavText => new NavValueDefinedLengthMetadata(NavNclType.NavText, int.MaxValue), 
			NavNclType.NavCode => new NavValueDefinedLengthMetadata(NavNclType.NavCode, 2048), 
			NavNclType.NavOption => new NavValueOptionMetadata(defaultOptionMetadata), 
			_ => new NavValueMetadata(nclType), 
		};
	}

	public static NavValueMetadata DefaultMetadata(NavType navType)
	{
		return DefaultMetadata(NavNclTypeHelper.GetNclTypeFromNavType(navType));
	}

	public static INavValueMetadata CalcMetadataFromObject(object value)
	{
		if (value == null)
		{
			return DefaultMetadata(NavNclType.None);
		}
		NavValue navValue = value as NavValue;
		while (navValue != null)
		{
			if (navValue.IsWrapper && navValue.IsInitialized)
			{
				navValue = navValue.InnerValue;
				continue;
			}
			return DefaultMetadata(navValue.NavType);
		}
		NavNclType nclTypeFromObject = NavNclTypeHelper.GetNclTypeFromObject(value);
		return DefaultMetadata(nclTypeFromObject);
	}

	public static INavValueMetadata CalcMetadataFromDotNetObject(object value)
	{
		if (value == null)
		{
			return DefaultMetadata(NavNclType.None);
		}
		NavValue navValue = value as NavValue;
		while (navValue != null)
		{
			if (navValue.IsWrapper && navValue.IsInitialized)
			{
				navValue = navValue.InnerValue;
				continue;
			}
			return DefaultMetadata(navValue.NavType);
		}
		NavNclType nclTypeFromObject = NavNclTypeHelper.GetNclTypeFromObject(value, forDotNet: true);
		return DefaultMetadata(nclTypeFromObject);
	}

	public static bool CompareTypeAndMetadata(INavValueMetadata left, INavValueMetadata right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (left.NclType != right.NclType)
		{
			return false;
		}
		switch (left.NclType)
		{
		case NavNclType.NavOemText:
		case NavNclType.NavOemCode:
		case NavNclType.NavText:
		case NavNclType.NavCode:
			return left.NavDefinedLengthMetadata == right.NavDefinedLengthMetadata;
		case NavNclType.NavOption:
			return left.NavOptionMetadata == right.NavOptionMetadata;
		default:
			return true;
		}
	}

	protected NavValueMetadata(NavNclType nclType)
	{
		this.nclType = nclType;
	}

	public override int GetHashCode()
	{
		return (int)NclType;
	}
}
