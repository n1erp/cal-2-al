using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Extensions;

public class NavFormExtension : NavApplicationObjectBase
{
	private NCLPageExtension nclPageExtension;

	protected NavRecord Record { get; set; }

	protected internal NavForm ParentObject { get; private set; }

	public override NavNclType NclType { get; } = NavNclType.NavFormExtension;

	public virtual NCLPageExtension NclPageExtension
	{
		get
		{
			if (nclPageExtension == null)
			{
				nclPageExtension = (NCLPageExtension)Session.NCLMetadata.GetExtensionApplicationObject(ObjectId, OriginalAppGroupId);
			}
			return nclPageExtension;
		}
	}

	public override string ObjectName => ObjectType.PageExtension.ToString() + " " + ObjectId.ObjectNumber;

	public override Type HandleType => typeof(NavFormExtension);

	internal override NCLMetaApplicationObject MetaApplicationObject => NclPageExtension;

	public NavFormExtension(ITreeObject parent, int objectNumber, NavRecord record)
		: base(parent, new ApplicationObjectId(ObjectType.PageExtension, objectNumber))
	{
		ParentObject = parent as NavForm;
		Record = record;
	}

	protected void RegisterAutoFormatExpression(int controlId, int autoFormatType, Func<string> getAutoFormatStringFunc)
	{
		ParentObject?.RegisterAutoFormatExpression(controlId, autoFormatType, getAutoFormatStringFunc);
	}

	protected void RegisterGetDecimalStringExpression(int controlId, INavRecordHandle record, int fieldId)
	{
		ParentObject?.RegisterGetDecimalStringExpression(controlId, record, fieldId);
	}

	protected void RegisterDynamicCaptionExpression(int controlId)
	{
		ParentObject?.RegisterDynamicCaptionExpression(controlId);
	}

	protected void RegisterFormatStringExpression(int controlId, string formatString)
	{
		ParentObject?.RegisterFormatStringExpression(controlId, formatString);
	}

	protected internal void RegisterSourceExpression(string id, string name, Type type, SourceExpressionType expressionType, NavFormSourceExpressionGetter get, NavFormSourceExpressionSetter set)
	{
		ParentObject?.RegisterSourceExpression(id, name, type, expressionType, get, set);
	}

	protected void RegisterUIPart(string uiPartName, int uiPartFormId)
	{
		ParentObject.RegisterUIPart(uiPartName, uiPartFormId);
	}

	public virtual void InitializeComponent()
	{
	}

	public virtual void OnMetadataLoaded()
	{
	}

	public new virtual void OnClear()
	{
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

	public NavForm GetPart(int partId)
	{
		return ParentObject.GetPart(partId);
	}

	public string GetDecimalString(INavRecordHandle record, int fieldId, int controlId)
	{
		return ParentObject.GetDecimalString(record, fieldId, controlId);
	}

	public override int GetHashCode()
	{
		return 0xE ^ ObjectId.ObjectNumber;
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		return new NavFormHandle(parent, ParentObject);
	}

	public virtual void OnOpenPage()
	{
	}

	public virtual void OnClosePage()
	{
	}

	public virtual void OnAfterGetRecord()
	{
	}

	public virtual void OnNewRecord(bool belowXRec)
	{
	}

	public virtual bool OnInsertRecord(bool belowXRec)
	{
		return true;
	}

	public virtual bool OnModifyRecord()
	{
		return true;
	}

	public virtual bool OnDeleteRecord()
	{
		return true;
	}

	public virtual bool OnQueryClosePage(FormResult closeAction)
	{
		return true;
	}

	public virtual void OnAfterGetCurrRecord()
	{
	}

	public virtual void OnPageBackgroundTaskCompleted(int taskId, NavDictionary<NavText, NavText> result)
	{
	}

	public virtual void OnPageBackgroundTaskError(int taskId, NavText errorCode, NavText errorText, NavText errorCallstack)
	{
	}
}
