using System;

namespace Microsoft.Dynamics.Nav.Types;

public interface ITestPage : ITestFilter, IDisposable
{
	int ValidationErrorCount { get; }

	FormResult FormResult { get; }

	string Name { get; }

	string Caption { get; }

	int PageId { get; }

	Guid FormHandle { get; }

	bool Creatable { get; }

	bool IsExpanded { get; }

	bool RuntimeEditable { get; }

	ITestAction GetAction(int id);

	ITestField GetField(int id);

	ITestPage GetPage(int id);

	void SetSelection(bool value);

	ITestAction GetBuiltInAction(FormResult formResult);

	ITestFilter GetDataItemFilter(int id);

	bool IsOpened();

	void Close();

	void InsertEmptyRow(bool beforeCurrent);

	bool MoveNext();

	bool MovePrevious();

	bool MoveFirst();

	bool MoveLast();

	string GetValidationError(int index);

	bool FindRowFromTableFieldValues(int[] fieldIds, object[] values, bool forward);

	bool FindRowFromControlFieldValue(int fieldId, object value, bool forward);

	object GetBookmark();

	bool GoToBookmark(object bookmark);

	object[] GetTableFieldValues(int[] fieldIds);

	ITestAction Edit();

	ITestAction View();

	bool Expand(bool doExpand);
}
