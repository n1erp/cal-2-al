using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public struct NavFormUpdateParameters
{
	private Guid serverFormHandle;

	private NavFormUpdateTypes updateType;

	[NonSerialized]
	private NavRecordState state;

	[DataMember]
	public Guid ServerFormHandle
	{
		[DebuggerStepThrough]
		get
		{
			return serverFormHandle;
		}
		[DebuggerStepThrough]
		set
		{
			serverFormHandle = value;
		}
	}

	[DataMember]
	public NavFormUpdateTypes UpdateType
	{
		[DebuggerStepThrough]
		get
		{
			return updateType;
		}
		[DebuggerStepThrough]
		set
		{
			updateType = value;
		}
	}

	[DataMember]
	public NavRecordState State
	{
		[DebuggerStepThrough]
		get
		{
			return state;
		}
		[DebuggerStepThrough]
		set
		{
			state = value;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		NavFormUpdateParameters navFormUpdateParameters = (NavFormUpdateParameters)obj;
		if (serverFormHandle == navFormUpdateParameters.serverFormHandle)
		{
			return updateType == navFormUpdateParameters.updateType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return serverFormHandle.GetHashCode() ^ updateType.GetHashCode();
	}

	public static bool operator ==(NavFormUpdateParameters value1, object value2)
	{
		return value1.Equals(value2);
	}

	public static bool operator !=(NavFormUpdateParameters value1, object value2)
	{
		return !value1.Equals(value2);
	}
}
