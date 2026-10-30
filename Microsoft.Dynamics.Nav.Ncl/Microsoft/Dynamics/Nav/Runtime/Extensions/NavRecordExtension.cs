using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Extensions;

public class NavRecordExtension : NavApplicationObjectBase
{
	private NCLTableExtension nclTableExtension;

	private bool globalVariablesInitialized;

	public override NavNclType NclType { get; } = NavNclType.NavRecordExtension;

	public override Type HandleType => typeof(NavRecordExtension);

	internal override NCLMetaApplicationObject MetaApplicationObject => NclTableExtension;

	public override string ObjectName => ObjectType.TableExtension.ToString() + " " + ObjectId.ObjectNumber;

	protected internal NavRecord ParentObject { get; private set; }

	protected int CurrFieldNo
	{
		get
		{
			return ParentObject.CurrFieldNo;
		}
		set
		{
			ParentObject.CurrFieldNo = value;
		}
	}

	public NCLTableExtension NclTableExtension
	{
		get
		{
			if (nclTableExtension == null)
			{
				nclTableExtension = (NCLTableExtension)Session.NCLMetadata.GetExtensionApplicationObject(ObjectId, OriginalAppGroupId);
			}
			return nclTableExtension;
		}
	}

	public NavRecordExtension(ITreeObject parent, int objectNumber)
		: base(parent, new ApplicationObjectId(ObjectType.TableExtension, objectNumber))
	{
		ParentObject = parent as NavRecord;
	}

	public override int GetHashCode()
	{
		return 0xF ^ ObjectId.ObjectNumber;
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		return new NavRecordHandle(parent, ParentObject.ObjectId.ObjectNumber, temporary: false, ParentObject.RecordImplementation.SecurityFiltering)
		{
			Target = ParentObject
		};
	}

	public virtual void InitializeGlobalVariables()
	{
	}

	protected internal override void EnsureGlobalVariablesInitialized()
	{
		if (!globalVariablesInitialized)
		{
			InitializeGlobalVariables();
			globalVariablesInitialized = true;
		}
	}

	public new virtual void OnClear()
	{
	}

	public NavValue GetFieldValueSafe(int fieldNo, NavType expectedType)
	{
		return ParentObject.GetFieldValueSafe(fieldNo, expectedType);
	}

	public virtual NavValue GetFieldValueSafe(int extensionObjectId, int fieldNo, NavType expectedType)
	{
		return ParentObject.GetFieldValueSafe(extensionObjectId, fieldNo, expectedType);
	}

	public virtual string GetAutoFormatString(int fieldNumber, out bool handled)
	{
		handled = false;
		return string.Empty;
	}

	public virtual string GetDecimalPlaces(int fieldNumber, out bool handled)
	{
		handled = false;
		return string.Empty;
	}

	public virtual string EvaluateCaptionClass(int fieldNumber, out bool handled)
	{
		handled = false;
		return string.Empty;
	}

	public virtual string EvaluateCaptionClass(int extensionObjectId, int fieldNumber, out bool handled)
	{
		handled = false;
		return string.Empty;
	}

	public virtual void OnBeforeInsert()
	{
	}

	public virtual void OnInsert()
	{
	}

	public virtual void OnAfterInsert()
	{
	}

	public virtual void OnBeforeModify()
	{
	}

	public virtual void OnModify()
	{
	}

	public virtual void OnAfterModify()
	{
	}

	public virtual void OnBeforeDelete()
	{
	}

	public virtual void OnDelete()
	{
	}

	public virtual void OnAfterDelete()
	{
	}

	public virtual void OnBeforeRename()
	{
	}

	public virtual void OnRename()
	{
	}

	public virtual void OnAfterRename()
	{
	}
}
