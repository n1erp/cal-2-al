using System;

namespace Microsoft.Dynamics.Nav.Types;

public interface IClientCallbackHandler : IClientContract
{
	void Message(string displayMessage, Guid automationId);

	void Hyperlink(string link, Guid automationId);

	void FormRun(FormRunRequest formRequest);

	void SendNotification(NotificationInfo notification);

	void SendSessionUpdateRequest(SessionSettingsInfo sessionSettingsInfo);

	void CompanyInformationChanged(CompanyInformationChanges companyInformationChanges);

	void WorkDateChanged(DateTime workDate);
}
