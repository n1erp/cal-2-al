using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class NavValueScaffolding : Scaffolding<NavValue>
{
	private readonly NavNclType nclType;

	private string typeString;

	private bool isNull;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => false;

	protected override bool SetValueSupported => true;

	private bool CanFormatAndEvaluate
	{
		get
		{
			switch (nclType)
			{
			case NavNclType.NavBoolean:
			case NavNclType.NavByte:
			case NavNclType.NavInteger:
			case NavNclType.NavBigInteger:
			case NavNclType.NavDecimal:
			case NavNclType.NavOption:
			case NavNclType.NavDuration:
			case NavNclType.NavOemText:
			case NavNclType.NavOemCode:
			case NavNclType.NavTextConstant:
			case NavNclType.NavBigText:
			case NavNclType.NavText:
			case NavNclType.NavChar:
			case NavNclType.NavCode:
			case NavNclType.NavDate:
			case NavNclType.NavTime:
			case NavNclType.NavDateTime:
			case NavNclType.NavGuid:
			case NavNclType.NavRecordId:
			case NavNclType.NavDateFormula:
			case NavNclType.NavTextBuilder:
			case NavNclType.NavDotNet:
				return true;
			default:
				return false;
			}
		}
	}

	public NavValueScaffolding(string name, Type type, NavNclType nclType, ValueBinder valueBinder)
		: base(name, type, valueBinder, false, false)
	{
		this.nclType = nclType;
	}

	protected override string GetStringValue(NavValue instance)
	{
		if (instance != null)
		{
			return StringHelper(instance);
		}
		return "<Uninitialized>";
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance == null)
		{
			return isNull;
		}
		if (instance is NavValue navValue)
		{
			return navValue.NclType == nclType;
		}
		return false;
	}

	protected override bool TryParse(string newValueAsString, out object newValueAsObject)
	{
		if (CanFormatAndEvaluate)
		{
			newValueAsObject = ALSystemVariable.EvaluateIntoNavValue(DataError.TrapError, NavValueMetadata.DefaultMetadata(nclType), newValueAsString, 0);
			return newValueAsObject != null;
		}
		newValueAsObject = null;
		return false;
	}

	protected override void ReadMetadata(NavValue instance)
	{
		if (instance == null)
		{
			isNull = true;
		}
		typeString = ScaffoldingHelper.GetTypeString(nclType, instance);
		base.ReadMetadata(instance);
	}

	private string StringHelper(NavValue instance)
	{
		if (CanFormatAndEvaluate)
		{
			switch (instance.NclType)
			{
			case NavNclType.NavByte:
				return ScaffoldingHelper.ByteToStringValue(instance.ToByte());
			case NavNclType.NavOemText:
			case NavNclType.NavOemCode:
			case NavNclType.NavBigText:
			case NavNclType.NavText:
			case NavNclType.NavChar:
			case NavNclType.NavCode:
			case NavNclType.NavTextBuilder:
				return "'" + Scaffolding.TruncateIfNeeded(instance.ToString()) + "'";
			case NavNclType.NavTime:
			case NavNclType.NavDateTime:
				return NavFormatEvaluateHelper.Format(instance, 0, 1);
			default:
				return instance.ToString();
			}
		}
		return ScaffoldingHelper.GetTypeString(nclType, instance);
	}
}
