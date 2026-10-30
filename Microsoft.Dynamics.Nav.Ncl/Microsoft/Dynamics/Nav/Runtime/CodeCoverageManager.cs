using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class CodeCoverageManager
{
	internal static bool IsCodeCoverageEnabledForConnection(NavSession session)
	{
		return CodeCoverageRecorderForSession(session) != null;
	}

	internal static void StartSingleSessionCodeCoverage(NavSession session, ALCodeEnvironment codeEnvironment)
	{
		StartCodeCoverage(new SingleSessionCodeCoverageRecorder(session, codeEnvironment, ExecutionListener.RemoveListener));
	}

	internal static void StartMultiSessionCodeCoverage(NavSession session, ALCodeEnvironment codeEnvironment)
	{
		StartCodeCoverage(new MultiSessionCodeCoverageRecorder(session, codeEnvironment));
	}

	private static void StartCodeCoverage(CodeCoverageRecorder coverageRecorder)
	{
		CodeCoverageRecorderForSession(coverageRecorder.OwningSession)?.Dispose();
		ExecutionListener.AddListener(coverageRecorder);
	}

	public static void StopCodeCoverageRecording(NavSession session)
	{
		using CodeCoverageRecorder codeCoverageRecorder = CodeCoverageRecorderForSession(session);
		if (codeCoverageRecorder != null)
		{
			codeCoverageRecorder.StopRecording();
			LoadDataIntoVirtualTable(session, codeCoverageRecorder);
		}
	}

	public static void RefreshTable(NavSession session)
	{
		CodeCoverageRecorder codeCoverageRecorder = CodeCoverageRecorderForSession(session);
		if (codeCoverageRecorder != null)
		{
			LoadDataIntoVirtualTable(session, codeCoverageRecorder);
		}
	}

	private static void LoadDataIntoVirtualTable(NavSession session, CodeCoverageRecorder coverageRecorder)
	{
		CodeCoverageDataProvider instanceForSession = CodeCoverageDataProvider.GetInstanceForSession(session);
		IEnumerable<CodeCoverageScopeCounter> scopeCounters = coverageRecorder.ScopeCounters.Select((CodeCoverageScopeCounter sc) => sc.Clone());
		IEnumerable<ApplicationObjectId> anyIncludesDefinedForSession = CodeCoverageObjectIncludes.GetAnyIncludesDefinedForSession(session);
		instanceForSession.SetScopeCounters(scopeCounters, anyIncludesDefinedForSession);
	}

	public static void LoadTableDataIntoCounters(NavSession session)
	{
		CodeCoverageRecorder codeCoverageRecorder = CodeCoverageRecorderForSession(session);
		if (codeCoverageRecorder == null)
		{
			throw new NavALException(Lang.CodeCoverageMustBeEnabledBeforeLoadCall);
		}
		CodeCoverageDataProvider instanceForSession = CodeCoverageDataProvider.GetInstanceForSession(session);
		IEnumerable<CodeCoverageScopeCounter> scopeCounters = instanceForSession.GetScopeCounters();
		codeCoverageRecorder.SetHitCounts(scopeCounters.ToDictionary((CodeCoverageScopeCounter sc) => sc.ScopeId));
	}

	public static void EnsureApplicationObjectsAreIncludedInCoverageTable(NavRecord objectTableRecord)
	{
		if (objectTableRecord == null)
		{
			throw new ArgumentNullException("objectTableRecord");
		}
		NavSession session = objectTableRecord.Session;
		CodeCoverageObjectIncludes.RemoveAnyOnSession(session);
		CodeCoverageObjectIncludes.CreateAndAdd(objectTableRecord);
	}

	private static CodeCoverageRecorder CodeCoverageRecorderForSession(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		return ExecutionListener.AllRegisteredListeners.OfType<CodeCoverageRecorder>().FirstOrDefault((CodeCoverageRecorder recorder) => recorder.OwningSession == session);
	}
}
