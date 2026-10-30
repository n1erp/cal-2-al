using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Client;

public abstract class ServiceConnectionBase : ServerInvocationHandler
{
	protected NavDiagnostics diagnostics = NavDiagnostics.CreateSectionDiagnostics(NavDiagnostics.GlobalInstance, MonitorType.None, singleThreadMonitorAccess: true);

	public const int InvokeCodeUnitMessageId = 3;

	public abstract long MetadataToken { get; }

	public Func<Guid> GetTelemetryClientSessionId { get; set; }

	public Func<Guid> GetTelemetryClientActivityId { get; set; }

	public NavDataSet GetPage(PageRequestDefinition pageRequestDefinition, ref NavRecordState state)
	{
		NavRecordState localState = state;
		DataSetResponse dataSetResponse = CallServer((IAsyncNavService server) => server.BeginGetPage(pageRequestDefinition, localState, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndGetPage(result));
		state = dataSetResponse.State;
		return dataSetResponse.DataSet;
	}

	public byte[] GetMediaContent(Guid mediaHandle, ref bool allowCaching, ref string mimeType)
	{
		allowCaching = true;
		mimeType = string.Empty;
		return null;
	}

	public Image GetMediaThumbnail(Guid mediaHandle, int width, int height, ref bool allowCaching)
	{
		allowCaching = false;
		if (mediaHandle == Guid.Empty)
		{
			return null;
		}
		bool allowCachingInternal = false;
		string mimeType = string.Empty;
		byte[] array = CallServer((IAsyncNavService server) => server.BeginGetScaledMedia(mediaHandle, width, height, ref allowCachingInternal, ref mimeType, null, null), (IAsyncNavService server, IAsyncResult serverResult) => server.EndGetScaledMedia(ref allowCachingInternal, ref mimeType, serverResult));
		if (array == null || array.Length == 0)
		{
			return null;
		}
		allowCaching = allowCachingInternal;
		if (!NavMediaImageTypes.IsSupportedMimeType(mimeType))
		{
			throw new NavImageLoadErrorException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidMediaContentType, mimeType));
		}
		try
		{
			return Image.FromStream(new MemoryStream(array));
		}
		catch (ArgumentException innerException)
		{
			throw new NavImageLoadErrorException(Lang.MediaImageLoadError, innerException);
		}
	}

	public void OpenForm(ref NavOpenFormArguments mainForm, ref NavOpenFormArguments[] subForms)
	{
		diagnostics.SendTraceTag("0000007", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Invoking OpenForm");
		OpenFormRequest openFormRequest = new OpenFormRequest
		{
			VersionNumber = MetadataToken
		};
		int num;
		if (mainForm == null)
		{
			num = 0;
			openFormRequest.HasMainForm = false;
		}
		else
		{
			num = 1;
			openFormRequest.HasMainForm = true;
		}
		int num2 = num;
		if (subForms != null)
		{
			num += subForms.Length;
		}
		openFormRequest.ControlIds = new string[num];
		openFormRequest.States = new NavRecordState[num];
		openFormRequest.DataSet = null;
		int num3 = -1;
		if (openFormRequest.HasMainForm)
		{
			openFormRequest.ControlIds[0] = null;
			openFormRequest.States[0] = mainForm.State;
			num3 = mainForm.State.FormId;
		}
		for (int i = num2; i < num; i++)
		{
			openFormRequest.ControlIds[i] = subForms[i - num2].ControlId;
			openFormRequest.States[i] = subForms[i - num2].State;
		}
		FormOpenResponse formOpenResponse = null;
		using (diagnostics.CreateTraceScope("0000008", DiagnosticParameter.SystemMetadata("Invoking server side OpenForm"), MonitorType.None, Category.Runtime))
		{
			formOpenResponse = CallServer((IAsyncNavService server) => server.BeginOpenForm(openFormRequest, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndOpenForm(result));
		}
		if (formOpenResponse == null)
		{
			if (openFormRequest.VersionNumber != MetadataToken)
			{
				throw new NavPageBuildErrorException(string.Format(CultureInfo.CurrentCulture, Lang.PageMetadataExpired, num3));
			}
			throw new InvalidOperationException();
		}
		if (formOpenResponse.DataSet == null || formOpenResponse.States == null || formOpenResponse.States.Length != num)
		{
			throw new InvalidOperationException();
		}
		NavRecordState[] states = formOpenResponse.States;
		if (mainForm != null)
		{
			mainForm.State = formOpenResponse.States[0];
			ExtractTablesFromDataSet(mainForm, formOpenResponse, 0);
			mainForm.TableCaptions = formOpenResponse.TableCaptions[0];
			mainForm.FormProperties = formOpenResponse.FormProperties[0];
		}
		for (int num4 = num2; num4 < num; num4++)
		{
			ExtractTablesFromDataSet(subForms[num4 - num2], formOpenResponse, num4);
			subForms[num4 - num2].TableCaptions = formOpenResponse.TableCaptions[num4];
			subForms[num4 - num2].FormProperties = formOpenResponse.FormProperties[num4];
			subForms[num4 - num2].State = states[num4];
		}
		if (openFormRequest.VersionNumber != MetadataToken)
		{
			throw new NavPageBuildErrorException(string.Format(CultureInfo.CurrentCulture, Lang.PageMetadataExpired, num3));
		}
	}

	public void CloseForm(ref NavCloseFormArguments mainForm, ref NavCloseFormArguments[] subForms, bool force)
	{
		CloseFormRequest closeFormRequest = new CloseFormRequest();
		int num;
		int num2;
		if (mainForm == null)
		{
			num = 0;
			num2 = 0;
			closeFormRequest.HasMainForm = false;
		}
		else
		{
			num = 1;
			num2 = 1;
			closeFormRequest.HasMainForm = true;
		}
		if (subForms != null)
		{
			num += subForms.Length;
		}
		closeFormRequest.States = new NavRecordState[num];
		closeFormRequest.ExitActions = new int[num];
		NavDataSet navDataSet = new NavDataSet
		{
			Locale = CultureInfo.InvariantCulture
		};
		if (closeFormRequest.HasMainForm)
		{
			closeFormRequest.States[0] = mainForm.State;
			closeFormRequest.ExitActions[0] = (int)mainForm.ExitAction;
			if (mainForm.DataSet != null && mainForm.DataSet.Tables.Count >= 1)
			{
				NavDataTable navDataTable = mainForm.DataSet.Tables[0];
				mainForm.DataSet.Tables.Clear();
				navDataTable.TableName = string.Format(CultureInfo.InvariantCulture, "{0}", 0);
				navDataSet.Tables.Add(navDataTable);
			}
		}
		for (int i = num2; i < num; i++)
		{
			closeFormRequest.States[i] = subForms[i - num2].State;
			closeFormRequest.ExitActions[i] = (int)subForms[i - num2].ExitAction;
			if (subForms[i - num2].DataSet != null && subForms[i - num2].DataSet.Tables.Count >= 1)
			{
				NavDataTable navDataTable2 = subForms[i - num2].DataSet.Tables[0];
				subForms[i - num2].DataSet.Tables.Clear();
				navDataTable2.TableName = string.Format(CultureInfo.InvariantCulture, "{0}", i);
				navDataSet.Tables.Add(navDataTable2);
			}
		}
		closeFormRequest.Force = force;
		closeFormRequest.DataSet = navDataSet;
		FormOpenCloseResponse formOpenCloseResponse = CallServer((IAsyncNavService server) => server.BeginCloseForm(closeFormRequest, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndCloseForm(result));
		if (formOpenCloseResponse.States == null || formOpenCloseResponse.States.Length != num)
		{
			throw new InvalidOperationException();
		}
		NavRecordState[] states = formOpenCloseResponse.States;
		if (mainForm != null)
		{
			mainForm.State = formOpenCloseResponse.States[0];
		}
		for (int num3 = num2; num3 < num; num3++)
		{
			subForms[num3 - num2].State = states[num3];
		}
	}

	public object InvokeApplicationMethod(ApplicationCodeType objectType, int objectId, string methodName, ref NavDataSet dataSet, ref NavRecordState state, ref object[] args)
	{
		if (dataSet == null)
		{
			throw new ArgumentNullException("dataSet");
		}
		NotifyAboutCallServerNameBase(3, string.Format(CultureInfo.CurrentCulture, "{0} {1}", objectType, methodName), null);
		ApplicationMethodRequest request = new ApplicationMethodRequest
		{
			ApplicationCodeType = objectType,
			ObjectId = objectId,
			MethodName = methodName,
			DataSet = dataSet,
			DataSetState = state,
			Args = args
		};
		CodeUnitResponse codeUnitResponse = CallServer((IAsyncNavService server) => server.BeginInvokeApplicationMethod(request, request.DataSetState, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndInvokeApplicationMethod(result));
		if (codeUnitResponse == null)
		{
			throw new NotImplementedException("Unexpected object returned.");
		}
		args = codeUnitResponse.Args;
		dataSet = codeUnitResponse.DataSet;
		state = codeUnitResponse.DataSetState;
		return codeUnitResponse.ReturnValue;
	}

	public NavDataSet ValidateField(ref NavRecordState state, NavDataSet recDataSet, int controlId)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		FieldRequest fieldRequest = new FieldRequest();
		fieldRequest.TableId = state.TableId;
		fieldRequest.ControlId = controlId;
		fieldRequest.RecDataSet = recDataSet;
		NavRecordState localState = state;
		CodeUnitResponse codeUnitResponse = CallServer((IAsyncNavService server) => server.BeginValidateField(fieldRequest, localState, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndValidateField(result));
		state = codeUnitResponse.DataSetState;
		return codeUnitResponse.DataSet;
	}

	public NavDataSet LookupField(ref NavRecordState state, NavDataSet recDataSet, int controlId, ref string lookupValue)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		FieldRequest fieldRequest = new FieldRequest();
		fieldRequest.TableId = state.TableId;
		fieldRequest.ControlId = controlId;
		fieldRequest.RecDataSet = recDataSet;
		fieldRequest.Args = new object[1] { lookupValue };
		NavRecordState localState = state;
		CodeUnitResponse codeUnitResponse = CallServer((IAsyncNavService server) => server.BeginLookupField(fieldRequest, localState, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndLookupField(result));
		state = codeUnitResponse.DataSetState;
		if (codeUnitResponse.Args != null && codeUnitResponse.Args.Length != 0)
		{
			lookupValue = codeUnitResponse.Args[0].ToString();
		}
		return codeUnitResponse.DataSet;
	}

	public NavDataSet DrilldownField(ref NavRecordState state, NavDataSet recDataSet, int controlId)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		FieldRequest request = new FieldRequest();
		request.TableId = state.TableId;
		request.ControlId = controlId;
		request.RecDataSet = recDataSet;
		request.DataSetState = state;
		CodeUnitResponse codeUnitResponse = CallServer((IAsyncNavService server) => server.BeginDrillDownField(request, request.DataSetState, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndDrillDownField(result));
		state = codeUnitResponse.DataSetState;
		return codeUnitResponse.DataSet;
	}

	public NavDataSet AssistEditField(ref NavRecordState state, NavDataSet recDataSet, int controlId)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		FieldRequest fieldRequest = new FieldRequest();
		fieldRequest.TableId = state.TableView.TableId;
		fieldRequest.ControlId = controlId;
		fieldRequest.RecDataSet = recDataSet;
		NavRecordState localState = state;
		CodeUnitResponse codeUnitResponse = CallServer((IAsyncNavService server) => server.BeginAssistEdit(fieldRequest, localState, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndAssistEdit(result));
		state = codeUnitResponse.DataSetState;
		return codeUnitResponse.DataSet;
	}

	public NavDataSet ActionField(ref NavRecordState state, NavDataSet recDataSet, int controlId, int controlEventId)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		FieldRequest fieldRequest = new FieldRequest();
		fieldRequest.TableId = state.TableView.TableId;
		fieldRequest.ControlId = controlId;
		fieldRequest.ControlEventId = controlEventId;
		fieldRequest.RecDataSet = recDataSet;
		NavRecordState localState = state;
		CodeUnitResponse codeUnitResponse = CallServer((IAsyncNavService server) => server.BeginActionField(fieldRequest, localState, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndActionField(result));
		state = codeUnitResponse.DataSetState;
		return codeUnitResponse.DataSet;
	}

	public string EvaluateFilter(string toEvaluate, ObjectType objectType, int objectId, int fieldId)
	{
		return CallServer((IAsyncNavService server) => server.BeginEvaluateFilter(toEvaluate, objectType, objectId, fieldId, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndEvaluateFilter(result));
	}

	public string EvaluateQueryFilterByPosition(string toEvaluate, int objectId, int columnPosition)
	{
		return CallServer((IAsyncNavService server) => server.BeginEvaluateQueryFilterByPosition(toEvaluate, objectId, columnPosition, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndEvaluateQueryFilterByPosition(result));
	}

	public EvaluateBag Evaluate(string toEvaluate, NavType navType)
	{
		return CallServer((IAsyncNavService server) => server.BeginEvaluate(toEvaluate, navType, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndEvaluate(result));
	}

	public int EvaluateRelation(NavRecordState state, NavDataSet dataSet, int fieldId)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (dataSet == null)
		{
			throw new ArgumentNullException("dataSet");
		}
		FieldRequest fieldRequest = new FieldRequest();
		fieldRequest.TableId = state.TableId;
		fieldRequest.FieldId = fieldId;
		fieldRequest.RecDataSet = dataSet;
		return CallServer((IAsyncNavService service) => service.BeginEvaluateRelation(fieldRequest, state, null, null), (IAsyncNavService service, IAsyncResult result) => service.EndEvaluateRelation(result)).RelationIndex;
	}

	public bool GetAggregatesAsync(NavRecordState state, ref NavDataSet aggregateData)
	{
		if (aggregateData == null)
		{
			throw new ArgumentNullException("aggregateData");
		}
		NavDataSet localAggregateData = aggregateData;
		bool result = CallServer((IAsyncNavService server) => server.BeginGetAggregates(state, ref localAggregateData, null, null), (IAsyncNavService server, IAsyncResult result2) => server.EndGetAggregates(ref localAggregateData, result2), ServerCallOptions.AggregateDataCall);
		aggregateData = localAggregateData;
		return result;
	}

	public bool GetAggregates(NavRecordState state, ref NavDataSet aggregateData)
	{
		if (aggregateData == null)
		{
			throw new ArgumentNullException("aggregateData");
		}
		NavDataSet localAggregateData = aggregateData;
		bool result = CallServer((IAsyncNavService server) => server.BeginGetAggregates(state, ref localAggregateData, null, null), (IAsyncNavService server, IAsyncResult result2) => server.EndGetAggregates(ref localAggregateData, result2));
		aggregateData = localAggregateData;
		return result;
	}

	public int RecordCountApprox(int tableId)
	{
		return CallServer((IAsyncNavService server) => server.BeginRecordCountApprox(tableId, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndRecordCountApprox(result));
	}

	public NavDataSet InsertRecord(ref NavRecordState state, NavDataSet recDataSet, bool belowXRec, bool isEmptyMultipleLine)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		RecordRequest recordRequest = new RecordRequest();
		recordRequest.TableId = state.TableView.TableId;
		recordRequest.BelowXRec = belowXRec;
		recordRequest.RecDataSet = recDataSet;
		NavRecordState localState = state;
		DataSetResponse dataSetResponse = CallServer((IAsyncNavService server) => server.BeginInsertRecord(localState, recordRequest, belowXRec, isEmptyMultipleLine, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndInsertRecord(result));
		state = dataSetResponse.State;
		return dataSetResponse.DataSet;
	}

	public NavDataSet ModifyRecord(ref NavRecordState state, NavDataSet recDataSet)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		RecordRequest recordRequest = new RecordRequest
		{
			TableId = state.TableId,
			RecDataSet = recDataSet
		};
		NavRecordState localState = state;
		DataSetResponse dataSetResponse = CallServer((IAsyncNavService server) => server.BeginModifyRecord(localState, recordRequest, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndModifyRecord(result));
		state = dataSetResponse.State;
		return dataSetResponse.DataSet;
	}

	public NavDataSet RenameRecord(ref NavRecordState state, NavDataSet recDataSet)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (recDataSet == null)
		{
			throw new ArgumentNullException("recDataSet");
		}
		RecordRequest recordRequest = new RecordRequest
		{
			TableId = state.TableId,
			RecDataSet = recDataSet
		};
		NavRecordState localState = state;
		DataSetResponse dataSetResponse = CallServer((IAsyncNavService server) => server.BeginRenameRecord(localState, recordRequest, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndRenameRecord(result));
		state = dataSetResponse.State;
		return dataSetResponse.DataSet;
	}

	public byte[][] DeleteRecord(ref NavRecordState state, NavDataSet recDataSet, bool deleteSelectedRecords)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		RecordRequest recordRequest = new RecordRequest
		{
			TableId = state.TableView.TableId,
			RecDataSet = recDataSet
		};
		NavRecordState localState = state;
		RecordDeletedResponse recordDeletedResponse = CallServer((IAsyncNavService server) => server.BeginDeleteRecord(localState, recordRequest, deleteSelectedRecords, null, null), (IAsyncNavService server, IAsyncResult result) => server.EndDeleteRecord(result));
		state = recordDeletedResponse.State;
		return recordDeletedResponse.DeletedRecords;
	}

	public void EndClientCall(object result)
	{
		CallServer((IAsyncNavService server) => server.BeginEndClientCall(result, null, null), (IAsyncNavService server, IAsyncResult asyncResult) =>
		{
			server.EndEndClientCall(asyncResult);
			return (object)null;
		}, ServerCallOptions.Synchronous);
	}

	public void EndClientCall(AutomationMethodResponse result)
	{
		CallServer((IAsyncNavService server) => server.BeginEndClientAutomationMethodCall(result, null, null), (IAsyncNavService server, IAsyncResult asyncResult) =>
		{
			server.EndEndClientAutomationMethodCall(asyncResult);
			return (object)null;
		}, ServerCallOptions.Synchronous);
	}

	public void EndClientCall(FileActionResponse result)
	{
		CallServer((IAsyncNavService server) => server.BeginEndClientFileActionCall(result, null, null), (IAsyncNavService server, IAsyncResult asyncResult) =>
		{
			server.EndEndClientFileActionCall(asyncResult);
			return (object)null;
		}, ServerCallOptions.Synchronous);
	}

	public void EndClientCall(FormResult result)
	{
		CallServer((IAsyncNavService server) => server.BeginEndClientFormRunModalCall(result, null, null), (IAsyncNavService server, IAsyncResult asyncResult) =>
		{
			server.EndEndClientFormRunModalCall(asyncResult);
			return (object)null;
		}, ServerCallOptions.Synchronous);
	}

	private static void ExtractTablesFromDataSet(NavOpenFormArguments form, FormOpenCloseResponse formResponse, int tableNumber)
	{
		string tableName = string.Format(CultureInfo.InvariantCulture, "{0}", tableNumber);
		string tableName2 = ServerResponse.GenerateNonRowTableName(tableNumber);
		if (formResponse.DataSet.Tables.Contains(tableName))
		{
			NavDataSet navDataSet = new NavDataSet
			{
				Locale = formResponse.DataSet.Locale
			};
			NavDataTable table = formResponse.DataSet.Tables[tableName];
			formResponse.DataSet.Tables.Remove(tableName);
			navDataSet.Tables.Add(table);
			if (formResponse.DataSet.Tables.Contains(tableName2))
			{
				table = formResponse.DataSet.Tables[tableName2];
				formResponse.DataSet.Tables.Remove(tableName2);
				navDataSet.Tables.Add(table);
			}
			form.DataSet = navDataSet;
		}
		else
		{
			form.DataSet = null;
		}
	}

	protected virtual void NotifyAboutCallServerNameBase(int id, string name, string information)
	{
	}
}
