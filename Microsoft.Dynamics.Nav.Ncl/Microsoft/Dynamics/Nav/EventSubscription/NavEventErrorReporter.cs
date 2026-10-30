using System;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventErrorReporter : INavErrorReporter
{
	private readonly Action<string, Exception> reportingAction;

	public ApplicationObjectId PublisherObject { get; }

	public string PublisherMethod { get; }

	public NavEventErrorReporter(ApplicationObjectId publisherObject, string publisherMethod, Action<string, Exception> reportingAction)
	{
		if (publisherMethod == null)
		{
			throw new ArgumentNullException("publisherMethod");
		}
		PublisherObject = publisherObject;
		PublisherMethod = publisherMethod;
		this.reportingAction = reportingAction;
	}

	public void ReportError(string errorMessage, Exception exception)
	{
		reportingAction?.Invoke(errorMessage, exception);
	}
}
