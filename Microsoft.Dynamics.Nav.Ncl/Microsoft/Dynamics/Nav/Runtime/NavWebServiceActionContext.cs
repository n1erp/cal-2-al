using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavWebServiceActionContext : NavValue
{
	internal WebServiceActionContext ActionContext;

	public static NavWebServiceActionContext Default => new NavWebServiceActionContext();

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public override NavNclType NclType => NavNclType.NavWebServiceActionContext;

	public bool ALAddEntityKey(DataError errorLevel, int fieldId, NavValue value)
	{
		return AddEntityKey(errorLevel, fieldId, value);
	}

	public void ALSetObjectType(NavObjectType objectType)
	{
		ActionContext.SetNavObjectType(objectType);
	}

	public NavObjectType ALGetObjectType()
	{
		return ActionContext.NavObjectType;
	}

	public int ALGetObjectId()
	{
		return ActionContext.NavObjectId;
	}

	public void ALSetObjectId(int objectId)
	{
		ActionContext.NavObjectId = objectId;
	}

	public WebServiceActionResultCode ALGetResultCode()
	{
		return (WebServiceActionResultCode)Enum.Parse(typeof(WebServiceActionResultCode), ActionContext.ResultCode.ToString());
	}

	public void ALSetResultCode(WebServiceActionResultCode resultCode)
	{
		ActionContext.ResultCode = (WebServiceActionContext.StatusCode)Enum.Parse(typeof(WebServiceActionContext.StatusCode), resultCode.ToString());
	}

	public void ALClear()
	{
		Clear();
	}

	internal NavWebServiceActionContext()
	{
		ActionContext = new WebServiceActionContext();
	}

	public override int GetHashCode()
	{
		return ActionContext.EntityKeys?.GetHashCode() ?? 0;
	}

	private bool AddEntityKey(DataError errorLevel, int fieldId, NavValue value)
	{
		return TrappableOperationExecutor.Execute(errorLevel, () =>
		{
			ActionContext.EntityKeys.Add(fieldId, value.ValueAsObject);
			return true;
		}, (Exception exception, DataError error) =>
		{
			if (exception is ArgumentException || exception is ArgumentNullException)
			{
				throw new NavNCLArgumentException();
			}
		});
	}

	private void Clear()
	{
		ActionContext = new WebServiceActionContext();
	}
}
