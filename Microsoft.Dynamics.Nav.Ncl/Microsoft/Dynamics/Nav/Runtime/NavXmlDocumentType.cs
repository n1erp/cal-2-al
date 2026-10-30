using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlDocumentType : NavXmlNodeGeneric<XDocumentType>
{
	public new static NavXmlDocumentType Default => new NavXmlDocumentType(null);

	public override NavNclType NclType => NavNclType.NavXmlDocumentType;

	public static NavXmlDocumentType ALCreate(string name)
	{
		return ALCreate(name, null, null, null);
	}

	public static NavXmlDocumentType ALCreate(string name, string publicId)
	{
		return ALCreate(name, publicId, null, null);
	}

	public static NavXmlDocumentType ALCreate(string name, string publicId, string systemId)
	{
		return ALCreate(name, publicId, systemId, null);
	}

	public static NavXmlDocumentType ALCreate(string name, string publicId, string systemId, string internalSubset)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlDocumentType(new XDocumentType(name, publicId, systemId, internalSubset)));
	}

	public bool ALGetName(DataError errorLevel, ByRef<NavText> result)
	{
		return GetName(errorLevel, result);
	}

	public bool ALSetName(DataError errorLevel, string value)
	{
		return SetName(errorLevel, value);
	}

	public bool ALGetSystemId(DataError errorLevel, ByRef<NavText> result)
	{
		return GetSystemId(errorLevel, result);
	}

	public bool ALSetSystemId(DataError errorLevel, string value)
	{
		return SetSystemId(errorLevel, value);
	}

	public bool ALGetInternalSubset(DataError errorLevel, ByRef<NavText> result)
	{
		return GetInternalSubset(errorLevel, result);
	}

	public bool ALSetInternalSubset(DataError errorLevel, string value)
	{
		return SetInternalSubset(errorLevel, value);
	}

	public bool ALGetPublicId(DataError errorLevel, ByRef<NavText> result)
	{
		return GetPublicId(errorLevel, result);
	}

	public bool ALSetPublicId(DataError errorLevel, string value)
	{
		return SetPublicId(errorLevel, value);
	}

	internal NavXmlDocumentType(XDocumentType value)
		: base(value)
	{
	}

	private bool GetName(DataError errorLevel, ByRef<NavText> result)
	{
		result.Value = new NavText(result.Value.MaxLength, Value.Name);
		return true;
	}

	private bool SetName(DataError errorLevel, string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Value.Name = value;
		});
	}

	private bool GetSystemId(DataError errorLevel, ByRef<NavText> result)
	{
		if (Value.SystemId == null)
		{
			return TrappableXmlOperationExecutor.RaiseMemberNotInitialized(errorLevel);
		}
		result.Value = new NavText(result.Value.MaxLength, Value.SystemId);
		return true;
	}

	private bool SetSystemId(DataError errorLevel, string value)
	{
		Value.SystemId = value;
		return true;
	}

	private bool GetInternalSubset(DataError errorLevel, ByRef<NavText> result)
	{
		if (Value.InternalSubset == null)
		{
			return TrappableXmlOperationExecutor.RaiseMemberNotInitialized(errorLevel);
		}
		result.Value = new NavText(result.Value.MaxLength, Value.InternalSubset);
		return true;
	}

	private bool SetInternalSubset(DataError errorLevel, string value)
	{
		Value.InternalSubset = value;
		return true;
	}

	private bool GetPublicId(DataError errorLevel, ByRef<NavText> result)
	{
		if (Value.PublicId == null)
		{
			return TrappableXmlOperationExecutor.RaiseMemberNotInitialized(errorLevel);
		}
		result.Value = new NavText(result.Value.MaxLength, Value.PublicId);
		return true;
	}

	private bool SetPublicId(DataError errorLevel, string value)
	{
		Value.PublicId = value;
		return true;
	}
}
