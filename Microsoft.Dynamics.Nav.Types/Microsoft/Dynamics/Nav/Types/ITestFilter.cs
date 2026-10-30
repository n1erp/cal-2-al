using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types;

public interface ITestFilter
{
	bool Ascending { get; set; }

	string CurrentKey { get; }

	void SetFilter(int fieldId, string filterValue);

	IEnumerable<NavFilter> GetFilter();

	string GetFilter(int fieldId);

	void SetCurrentKeyFields(int[] fields);

	int[] GetCurrentKeyFields();
}
