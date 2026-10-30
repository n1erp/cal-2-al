using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ObjectAlSourceCodeInfo : IEquatable<ObjectAlSourceCodeInfo>
{
	private readonly AppQualifiedApplicationObjectId appQualifiedObjectId;

	private readonly string sourceCode;

	private readonly string name;

	public AppQualifiedApplicationObjectId AppQualifiedObjectId
	{
		[DebuggerStepThrough]
		get
		{
			return appQualifiedObjectId;
		}
	}

	public string SourceCode
	{
		[DebuggerStepThrough]
		get
		{
			return sourceCode;
		}
	}

	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return name;
		}
	}

	public bool IsNavAppApplicationObjectSource { get; }

	public ObjectAlSourceCodeInfo(AppQualifiedApplicationObjectId appQualifiedObjectId, NavAppGroup appGroup)
	{
		this.appQualifiedObjectId = appQualifiedObjectId;
		(sourceCode, IsNavAppApplicationObjectSource) = ALCodeEnvironment.GetSourceCodeContext(this.appQualifiedObjectId, NavCurrentThread.Session, appGroup);
		try
		{
			name = NavCurrentThread.Session.GetObjectName(this.appQualifiedObjectId.UnqualifiedId);
		}
		catch (NavNCLObjectNotFoundException)
		{
		}
	}

	public override int GetHashCode()
	{
		return appQualifiedObjectId.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is ObjectAlSourceCodeInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(ObjectAlSourceCodeInfo other)
	{
		return AppQualifiedObjectId == other.AppQualifiedObjectId;
	}
}
