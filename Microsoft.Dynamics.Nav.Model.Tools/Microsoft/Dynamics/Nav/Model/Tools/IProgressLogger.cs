using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public interface IProgressLogger
{
	void Increment();

	void SetProgress(int newProgress);

	void HandleConfirmationRequests(Cmdlet cmdlet);

	bool ShouldProcess(string file);

	bool ShouldContinue(string file);
}
