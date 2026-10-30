using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlProcessingInstruction : NavXmlNodeGeneric<XProcessingInstruction>
{
	public new static NavXmlProcessingInstruction Default => new NavXmlProcessingInstruction(null);

	public override NavNclType NclType => NavNclType.NavXmlProcessingInstruction;

	public static NavXmlProcessingInstruction ALCreate(string target, string data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlProcessingInstruction(new XProcessingInstruction(target, data)));
	}

	public bool ALGetData(DataError errorLevel, ByRef<NavText> result)
	{
		return GetData(errorLevel, result);
	}

	public bool ALSetData(DataError errorLevel, string value)
	{
		return SetData(errorLevel, value);
	}

	public bool ALGetTarget(DataError errorLevel, ByRef<NavText> result)
	{
		return GetTarget(errorLevel, result);
	}

	public bool ALSetTarget(DataError errorLevel, string value)
	{
		return SetTarget(errorLevel, value);
	}

	public NavXmlProcessingInstruction(XProcessingInstruction value)
		: base(value)
	{
	}

	private bool GetData(DataError errorLevel, ByRef<NavText> result)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			result.Value = new NavText(result.Value.MaxLength, Value.Data);
		});
	}

	private bool SetData(DataError errorLevel, string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Value.Data = value;
		});
	}

	private bool GetTarget(DataError errorLevel, ByRef<NavText> result)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			result.Value = new NavText(result.Value.MaxLength, Value.Target);
		});
	}

	private bool SetTarget(DataError errorLevel, string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Value.Target = value;
		});
	}
}
