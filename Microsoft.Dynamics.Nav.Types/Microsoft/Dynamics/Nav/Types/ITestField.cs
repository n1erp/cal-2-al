namespace Microsoft.Dynamics.Nav.Types;

public interface ITestField
{
	string Value { get; set; }

	string Name { get; }

	string Caption { get; }

	NavType FieldType { get; }

	int ValidationErrorCount { get; }

	long LastUsedValidationErrorId { get; }

	long MaxValidationErrorId { get; }

	object ObjectValue { get; }

	int OptionCount { get; }

	bool Enabled { get; }

	bool Editable { get; }

	bool Visible { get; }

	bool HideValue { get; }

	bool ShowMandatory { get; }

	string GetValidationError(int index);

	void Activate();

	void Lookup();

	void AssistEdit();

	void Drilldown();

	void Invoke();

	string ValueToString(object value);

	string GetOption(int index);
}
