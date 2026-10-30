using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestPageBase : NavApplicationObjectBase, ITreeSharedObject, ITreeObject
{
	private enum InitialMove
	{
		First,
		Next,
		Previous
	}

	private ApplicationObjectId pageUnderTestId;

	private ITestPage testPage;

	private Dictionary<int, NavTestField> fields = new Dictionary<int, NavTestField>();

	private Dictionary<int, NavTestAction> actions = new Dictionary<int, NavTestAction>();

	private Dictionary<ViewMode, NavTestAction> viewModeActions = new Dictionary<ViewMode, NavTestAction>();

	private Dictionary<int, NavTestPart> parts = new Dictionary<int, NavTestPart>();

	private Dictionary<FormResult, NavTestAction> builtInActions = new Dictionary<FormResult, NavTestAction>();

	private Dictionary<int, NavTestFilter> dataItems = new Dictionary<int, NavTestFilter>();

	private NavTestFilter testFilter;

	private PageDefinition metaPage;

	private NavForm serverform;

	private int[] primaryKeyFields;

	public string ALCaption
	{
		get
		{
			CheckPageOpened();
			return TestPage.Caption;
		}
	}

	public NavTestFilter ALFilter
	{
		get
		{
			CheckPageOpened();
			if (testFilter == null)
			{
				testFilter = new NavTestFilter(testPage);
			}
			return testFilter;
		}
	}

	public bool ALIsExpanded => TestPage.IsExpanded;

	public bool ALEditable => TestPage.RuntimeEditable;

	internal PageDefinition MetaPage
	{
		get
		{
			if (metaPage == null)
			{
				metaPage = LoadMetadata();
			}
			return metaPage;
		}
	}

	internal NavForm ServerForm
	{
		get
		{
			if (serverform == null)
			{
				serverform = Session.Company.GetRegisteredForm(TestPage.FormHandle);
			}
			return serverform;
		}
	}

	private int[] PrimaryKeyFields
	{
		get
		{
			if (primaryKeyFields == null)
			{
				NCLMetaTable metaTable = GetMetaTable();
				if (metaTable != null)
				{
					primaryKeyFields = new int[metaTable.PrimaryKey.KeyFieldsList.Count];
					int num = 0;
					foreach (NCLMetaField keyFields in metaTable.PrimaryKey.KeyFieldsList)
					{
						primaryKeyFields[num++] = keyFields.FieldNo;
					}
				}
				else
				{
					primaryKeyFields = new int[0];
				}
			}
			return primaryKeyFields;
		}
	}

	protected ITestPage TestPage
	{
		get
		{
			CheckPageOpened();
			return testPage;
		}
	}

	internal FormResult FormResult => testPage.FormResult;

	public string Name => testPage.Name;

	public override Type HandleType => typeof(NavTestPageHandle);

	internal override NCLMetaApplicationObject MetaApplicationObject
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override string ObjectName => MetaPage.Name;

	public override NavNclType NclType => NavNclType.NavTestPage;

	internal bool IsTestPageDisposed => testPage == null;

	internal bool IsOpened
	{
		get
		{
			if (testPage != null)
			{
				return testPage.IsOpened();
			}
			return false;
		}
	}

	public int ALValidationErrorCount()
	{
		CheckPageOpened();
		return TestPage.ValidationErrorCount;
	}

	public string ALGetValidationError()
	{
		CheckPageOpened();
		if (ALValidationErrorCount() == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ALValidationErrorCount(); i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(TestPage.GetValidationError(i));
		}
		return stringBuilder.ToString();
	}

	public string ALGetValidationError(int index)
	{
		CheckPageOpened();
		try
		{
			return TestPage.GetValidationError(index - 1);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
	}

	public void ALNew()
	{
		CheckPageOpened();
		if (!TestPage.Creatable)
		{
			throw new NavInsertDeniedPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.TestPageNewMethodNotAllowed, TestPage.Caption, TestPage.PageId));
		}
		TestPage.InsertEmptyRow(beforeCurrent: true);
	}

	public bool ALNext()
	{
		CheckPageOpened();
		return TestPage.MoveNext();
	}

	public NavTestAction ALView()
	{
		CheckPageOpened();
		if (!viewModeActions.TryGetValue(ViewMode.View, out var value))
		{
			value = new NavTestAction(TestPage.View());
			viewModeActions.Add(ViewMode.View, value);
		}
		return value;
	}

	public NavTestAction ALEdit()
	{
		CheckPageOpened();
		if (!viewModeActions.TryGetValue(ViewMode.Edit, out var value))
		{
			value = new NavTestAction(TestPage.Edit());
			viewModeActions.Add(ViewMode.Edit, value);
		}
		return value;
	}

	public bool ALPrevious()
	{
		CheckPageOpened();
		return TestPage.MovePrevious();
	}

	public bool ALFirst()
	{
		CheckPageOpened();
		return TestPage.MoveFirst();
	}

	public bool ALLast()
	{
		CheckPageOpened();
		return TestPage.MoveLast();
	}

	public bool ALGoToRecord(DataError errorLevel, NavRecord record)
	{
		if (MetaPage.Properties.SourceObject.SourceTable != record.TableID)
		{
			throw new NavTestSourceTableMismatchException();
		}
		NCLMetaTable metaTableById = record.Session.NCLMetadata.GetMetaTableById(MetaPage.Properties.SourceObject.SourceTable);
		NavValue[] array = new NavValue[metaTableById.PrimaryKey.KeyFieldsList.Count];
		for (int i = 0; i < PrimaryKeyFields.Length; i++)
		{
			array[i] = record.GetFieldValue(PrimaryKeyFields[i]);
		}
		return ALGoToKey(errorLevel, array);
	}

	public bool ALGoToKey(DataError errorLevel, params NavValue[] values)
	{
		if (PrimaryKeyFields.Length != values.Length)
		{
			throw new NavTestInvalidNumberOfKeyFieldValuesException();
		}
		object[] array = new object[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			array[i] = values[i].ClientObject;
		}
		try
		{
			TestPage.SetSelection(value: false);
			return InternalFindRowFromTableFieldValues(errorLevel, PrimaryKeyFields, array, forward: true);
		}
		finally
		{
			TestPage.SetSelection(value: true);
		}
	}

	public bool ALFindFirstField(DataError errorLevel, int fieldNo, NavValue value)
	{
		return InternalFindRowFromControlFieldValue(errorLevel, fieldNo, value.ClientObject, InitialMove.First);
	}

	public bool ALFindNextField(DataError errorLevel, int fieldNo, NavValue value)
	{
		return InternalFindRowFromControlFieldValue(errorLevel, fieldNo, value.ClientObject, InitialMove.Next);
	}

	public bool ALFindPreviousField(DataError errorLevel, int fieldNo, NavValue value)
	{
		return InternalFindRowFromControlFieldValue(errorLevel, fieldNo, value.ClientObject, InitialMove.Previous);
	}

	public bool ALExpand(bool expand)
	{
		return TestPage.Expand(expand);
	}

	internal NavTestPageBase(ITreeObject parent, ApplicationObjectId pageUnderTestId)
		: base(parent, pageUnderTestId)
	{
		this.pageUnderTestId = pageUnderTestId;
		if (pageUnderTestId.ObjectNumber != 0)
		{
			metaPage = LoadMetadata();
		}
	}

	internal NavTestPageBase(ITreeObject parent, ApplicationObjectId pageUnderTestId, ITestPage testPage)
		: this(parent, pageUnderTestId)
	{
		this.testPage = testPage;
	}

	protected virtual PageDefinition LoadMetadata()
	{
		return NavGlobal.MetadataProvider.GetPageDefinition(pageUnderTestId.ObjectNumber);
	}

	protected internal void CheckPageOpened()
	{
		if (testPage == null || !testPage.IsOpened())
		{
			throw new NavTestPageNotOpenedException();
		}
	}

	private NCLMetaTable GetMetaTable()
	{
		if (MetaPage.Properties.SourceObject.SourceTable == 0)
		{
			return null;
		}
		return NavGlobal.NCLMetadata.GetMetaTableById(MetaPage.Properties.SourceObject.SourceTable);
	}

	private void InternalClear()
	{
		testPage = null;
		fields.Clear();
		actions.Clear();
		parts.Clear();
		builtInActions.Clear();
		dataItems.Clear();
		testFilter = null;
		metaPage = null;
		serverform = null;
	}

	internal void Close()
	{
		if (testPage != null && testPage.IsOpened())
		{
			testPage.Close();
		}
		InternalClear();
	}

	public override void Clear()
	{
		Close();
		base.Clear();
	}

	public NavTestAction GetAction(int id)
	{
		CheckPageOpened();
		if (!actions.TryGetValue(id, out var value))
		{
			ITestAction action = testPage.GetAction(id);
			if (action == null)
			{
				throw NavTestActionNotFoundException.Create(CultureInfo.CurrentCulture, id);
			}
			value = new NavTestAction(action);
			actions.Add(id, value);
		}
		return value;
	}

	public NavTestField GetField(int id)
	{
		return GetField(id, throwIfNotFound: true);
	}

	public NavTestField GetField(int id, bool throwIfNotFound)
	{
		CheckPageOpened();
		if (!fields.TryGetValue(id, out var value))
		{
			ITestField field = testPage.GetField(id);
			if (field == null)
			{
				if (!throwIfNotFound)
				{
					return null;
				}
				throw NavTestFieldNotFoundException.Create(CultureInfo.CurrentCulture, id);
			}
			value = new NavTestField(this, field);
			fields.Add(id, value);
		}
		return value;
	}

	public NavTestPart GetPart(int id)
	{
		return GetPart(id, throwIfNotFound: true);
	}

	internal NavTestPart GetPart(int id, bool throwIfNotFound)
	{
		CheckPageOpened();
		if (!parts.TryGetValue(id, out var value))
		{
			ITestPage page = testPage.GetPage(id);
			if (page == null)
			{
				if (!throwIfNotFound)
				{
					return null;
				}
				throw NavTestPartNotFoundException.Create(CultureInfo.CurrentCulture, id);
			}
			value = new NavTestPart(this, page);
			parts.Add(id, value);
		}
		return value;
	}

	private ITestAction FindBuiltInAction(params FormResult[] formResults)
	{
		foreach (FormResult formResult in formResults)
		{
			ITestAction builtInAction = testPage.GetBuiltInAction(formResult);
			if (builtInAction != null)
			{
				return builtInAction;
			}
		}
		return null;
	}

	public NavTestAction GetBuiltInAction(FormResult formResult)
	{
		CheckPageOpened();
		if (!builtInActions.TryGetValue(formResult, out var value))
		{
			ITestAction testAction = formResult switch
			{
				FormResult.OK => FindBuiltInAction(FormResult.OK, FormResult.LookupOK), 
				FormResult.Cancel => FindBuiltInAction(FormResult.Cancel, FormResult.LookupCancel), 
				_ => testPage.GetBuiltInAction(formResult), 
			};
			if (testAction == null)
			{
				throw NavTestActionNotFoundException.Create(CultureInfo.CurrentCulture, formResult);
			}
			value = new NavTestAction(testAction);
			builtInActions.Add(formResult, value);
		}
		return value;
	}

	public NavTestFilter GetDataItem(int id)
	{
		CheckPageOpened();
		if (!dataItems.TryGetValue(id, out var value))
		{
			ITestFilter dataItemFilter = testPage.GetDataItemFilter(id);
			if (dataItemFilter == null)
			{
				throw NavTestPartNotFoundException.Create(CultureInfo.CurrentCulture, id);
			}
			value = new NavTestFilter(dataItemFilter);
			dataItems.Add(id, value);
		}
		return value;
	}

	private bool InternalFindRowFromControlFieldValue(DataError errorLevel, int fieldNo, object value, InitialMove initialMove)
	{
		object bookmark = TestPage.GetBookmark();
		object[] tableFieldValues = TestPage.GetTableFieldValues(PrimaryKeyFields);
		switch (initialMove)
		{
		case InitialMove.First:
			TestPage.MoveFirst();
			break;
		case InitialMove.Next:
			if (!TestPage.MoveNext())
			{
				return false;
			}
			break;
		case InitialMove.Previous:
			if (!TestPage.MovePrevious())
			{
				return false;
			}
			break;
		}
		bool flag = TestPage.FindRowFromControlFieldValue(fieldNo, value, initialMove != InitialMove.Previous);
		if (!flag)
		{
			if (!TestPage.GoToBookmark(bookmark) && tableFieldValues != null)
			{
				ALFirst();
				TestPage.FindRowFromTableFieldValues(PrimaryKeyFields, tableFieldValues, forward: true);
			}
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavTestRowNotFoundException();
			}
		}
		return flag;
	}

	private bool InternalFindRowFromTableFieldValues(DataError errorLevel, int[] fieldNos, object[] values, bool forward)
	{
		object bookmark = TestPage.GetBookmark();
		object[] tableFieldValues = TestPage.GetTableFieldValues(PrimaryKeyFields);
		TestPage.MoveFirst();
		bool flag = TestPage.FindRowFromTableFieldValues(fieldNos, values, forward);
		if (!flag)
		{
			if (!TestPage.GoToBookmark(bookmark) && tableFieldValues != null)
			{
				ALFirst();
				TestPage.FindRowFromTableFieldValues(fieldNos, tableFieldValues, forward: true);
			}
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavTestRowNotFoundException();
			}
		}
		return flag;
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		throw new NotSupportedException();
	}

	public override int GetHashCode()
	{
		return pageUnderTestId.GetHashCode();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && testPage != null)
		{
			testPage.Dispose();
		}
		base.Dispose(disposing);
	}

	internal string GetFilter()
	{
		CheckPageOpened();
		if (ServerForm.SourceTable == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NavFilter item in TestPage.GetFilter())
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(ServerForm.SourceTable.MetaTable.GetFieldByNo(item.FilterField).FieldName);
			stringBuilder.Append('=');
			stringBuilder.Append(item.FilterValue);
		}
		return stringBuilder.ToString();
	}

	protected virtual void Open(ViewMode mode)
	{
		if (IsOpened)
		{
			throw new NavTestPageAlreadyOpenException();
		}
		InternalClear();
	}

	internal void Attach(ITestPage value)
	{
		testPage = value;
	}

	public override string ToString()
	{
		return "TestPage " + ObjectId.ObjectNumber.ToString(CultureInfo.InvariantCulture);
	}
}
