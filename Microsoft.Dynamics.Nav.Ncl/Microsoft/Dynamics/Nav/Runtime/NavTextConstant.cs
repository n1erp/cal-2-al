using System;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTextConstant : NavStringValue
{
	private static NavTextConstant defaultValue = new NavTextConstant(new int[1] { DefaultLCId }, new string[1] { string.Empty });

	private readonly NavTextConstantCaptionResolver captionResolver;

	private readonly NavAppNCLCaptionStrings captionStrings = new NavAppNCLCaptionStrings();

	public static int DefaultLCId => NavResourceManager.DefaultLanguageId;

	public static NavTextConstant Default => defaultValue;

	public override string Value
	{
		get
		{
			NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
			int groupId = navAppGroup.GroupId;
			NCLCaptionStrings nCLCaptionStrings = captionStrings.GetCaptionStrings(groupId, captionResolver.ResolveCaptionStrings);
			return nCLCaptionStrings.GetValueOrDefault();
		}
	}

	public override int MaxLength => 2048;

	public override NavNclType NclType => NavNclType.NavTextConstant;

	public override object ValueAsObject => Value;

	public override int GetBytesSize => GetBytes().Length;

	public NavTextConstant(int[] lcIds, string[] values)
	{
		if (lcIds == null)
		{
			throw new ArgumentNullException("lcIds");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		captionResolver = new NavTextConstantCaptionResolver(lcIds, values);
	}

	public NavTextConstant(int objType, int objId, int txtId, int[] lcIds, string[] values)
		: this(objType, objId, -1, txtId, -1, 0, isRequestPage: false, lcIds, values)
	{
	}

	public NavTextConstant(int objType, int objId, int codeId, int txtId, int fieldId, int ctrlType, bool isRequestPage, int[] lcIds, string[] values)
	{
		if (lcIds == null)
		{
			throw new ArgumentNullException("lcIds");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		captionResolver = new NavTextConstantCaptionResolver((ObjectType)objType, objId, codeId, txtId, fieldId, ctrlType, isRequestPage, lcIds, values);
	}

	public override int ToInt32()
	{
		return int.Parse(Value, NavCurrentThread.Session.WindowsCulture);
	}

	public override long ToInt64()
	{
		return long.Parse(Value, NavCurrentThread.Session.WindowsCulture);
	}

	public override Decimal18 ToDecimal()
	{
		return Decimal18.Parse(Value, NavCurrentThread.Session.WindowsCulture);
	}

	public override byte[] GetBytes()
	{
		return NCLManagedAdapter.TextToByte(Value);
	}
}
