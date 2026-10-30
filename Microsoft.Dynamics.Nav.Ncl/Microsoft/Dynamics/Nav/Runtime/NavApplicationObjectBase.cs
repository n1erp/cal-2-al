using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavApplicationObjectBase : NavComplexValue, ISessionProvider
{
	internal static uint[] EmptyIndirectPermissionList = new uint[2];

	private readonly NavSession session;

	private readonly ApplicationObjectId objectId;

	private ApplicationObjectId effectiveObjectId;

	private int runtimeAppGroupId;

	private int originalAppGroupId;

	private bool isStale;

	private int localSetupVersion;

	private bool? executePermissionsValidated;

	public NavSession Session => session;

	public int OriginalAppGroupId
	{
		get
		{
			return originalAppGroupId;
		}
		set
		{
			originalAppGroupId = value;
		}
	}

	public int RuntimeAppGroupId
	{
		get
		{
			return runtimeAppGroupId;
		}
		set
		{
			runtimeAppGroupId = value;
		}
	}

	public bool IsStale
	{
		get
		{
			return isStale;
		}
		set
		{
			if (!value)
			{
				throw new InvalidOperationException();
			}
			isStale = true;
		}
	}

	public override int GetBytesSize => 0;

	public abstract Type HandleType { get; }

	public ApplicationObjectId ObjectId => objectId;

	public ApplicationObjectId EffectiveObjectId
	{
		get
		{
			if (effectiveObjectId.ObjectNumber != 0)
			{
				return effectiveObjectId;
			}
			if (ObjectId.ObjectType != ObjectType.Page)
			{
				effectiveObjectId = ObjectId;
			}
			else
			{
				Type type = GetType();
				Type declaringType = type.DeclaringType;
				effectiveObjectId = ObjectId;
				if (declaringType != null && declaringType.IsSubclassOf(typeof(NavReport)))
				{
					effectiveObjectId = new ApplicationObjectId(ObjectType.Report, ObjectId.ObjectNumber);
				}
				if (declaringType != null && declaringType.IsSubclassOf(typeof(NavXmlPort)))
				{
					effectiveObjectId = new ApplicationObjectId(ObjectType.XmlPort, ObjectId.ObjectNumber);
				}
			}
			return effectiveObjectId;
		}
	}

	public abstract string ObjectName { get; }

	internal virtual bool ExecutePermissionsValidated
	{
		get
		{
			return ExecutePermissionsValidatedEx == true;
		}
		set
		{
			ExecutePermissionsValidatedEx = value;
		}
	}

	internal bool? ExecutePermissionsValidatedEx
	{
		get
		{
			if (localSetupVersion < Session.Database.PermissionSetupMonitor.SetupVersion)
			{
				executePermissionsValidated = null;
			}
			return executePermissionsValidated;
		}
		set
		{
			executePermissionsValidated = value;
			localSetupVersion = Session.Database.PermissionSetupMonitor.SetupVersion;
		}
	}

	protected virtual uint[] IndirectPermissionList => EmptyIndirectPermissionList;

	internal abstract NCLMetaApplicationObject MetaApplicationObject { get; }

	protected NavApplicationObjectBase(ITreeObject parent, ApplicationObjectId objectId)
		: base(parent)
	{
		this.objectId = objectId;
		session = Tree.Session;
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup(session);
		originalAppGroupId = navAppGroup.GroupId;
		runtimeAppGroupId = navAppGroup.GroupId;
		if (session != null)
		{
			localSetupVersion = session.Database.PermissionSetupMonitor.SetupVersion;
		}
	}

	public override void Clear()
	{
		EnsureGlobalVariablesInitialized();
		ClearAll();
	}

	public void ClearApplicationMemberVariables()
	{
		EnsureGlobalVariablesInitialized();
		ClearAll();
	}

	public abstract NavComplexValue CreateHandle(ITreeObject parent);

	internal virtual NavApplicationObjectBase ResolveTriggerHandler(string triggerMethodName, object[] args)
	{
		return this;
	}

	public object Invoke(int methodId, object[] arguments)
	{
		EnsureGlobalVariablesInitialized();
		return OnInvoke(methodId, arguments);
	}

	internal static NavApplicationObjectBase FindApplicationObject(ITreeObject treeObject)
	{
		ITreeObject treeObject2 = treeObject;
		if (IsBasedOnSharedTable(treeObject2))
		{
			FindParentNonSharedTableBased(treeObject2);
		}
		while (treeObject2 != null)
		{
			if (treeObject2 is NavApplicationObjectBase result)
			{
				return result;
			}
			if (treeObject2 is NavMethodScope navMethodScope)
			{
				return navMethodScope.ApplicationObject;
			}
			treeObject2 = FindParentNonSharedTableBased(treeObject2);
		}
		return null;
	}

	internal static NavApplicationObjectBase FindDeclaringApplicationObject(ITreeObject treeObject)
	{
		ITreeObject treeObject2 = FindParentNonSharedTableBased(treeObject);
		return FindApplicationObject(treeObject2);
	}

	private static bool IsBasedOnSharedTable(ITreeObject treeObject)
	{
		if (!(treeObject is NavRecord navRecord))
		{
			return false;
		}
		return navRecord.IsBasedOnSharedTable;
	}

	private static ITreeObject FindParentNonSharedTableBased(ITreeObject treeObject)
	{
		ITreeObject parent = treeObject.Tree.Parent;
		if (IsBasedOnSharedTable(treeObject))
		{
			parent = parent.Tree.Parent;
		}
		return parent;
	}

	internal uint[] GetIndirectPermissionList()
	{
		return IndirectPermissionList;
	}

	internal uint GetIndirectPermissionMask(int tableID)
	{
		uint[] indirectPermissionList = IndirectPermissionList;
		if (indirectPermissionList == null || indirectPermissionList.Length < 2)
		{
			return 0u;
		}
		for (int i = 0; i < indirectPermissionList.Length - 1; i += 2)
		{
			if (tableID == indirectPermissionList[i])
			{
				return indirectPermissionList[i + 1];
			}
		}
		return 0u;
	}

	protected virtual void OnClear()
	{
	}

	protected virtual void ClearAll()
	{
		OnClear();
		CallOnClearExtensionMethod();
	}

	protected virtual void CallOnClearExtensionMethod()
	{
	}

	protected virtual object OnInvoke(int methodId, object[] arguments)
	{
		throw new NavNCLMissingMethodException(CultureInfo.InvariantCulture, methodId);
	}

	protected internal virtual void EnsureGlobalVariablesInitialized()
	{
	}

	public static bool TryInvoke(Action method)
	{
		try
		{
			using (NavCurrentThread.Session.CurrentMethodScope.GetTryMethodScope())
			{
				method();
				return true;
			}
		}
		catch (NavBaseException ex) when (!(ex is NavNclTryFunctionWriteException))
		{
			return false;
		}
	}
}
