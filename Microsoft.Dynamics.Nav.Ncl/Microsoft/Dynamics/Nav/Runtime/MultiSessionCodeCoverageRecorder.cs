using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class MultiSessionCodeCoverageRecorder(NavSession owningSession, ALCodeEnvironment codeEnvironment) : CodeCoverageRecorder(owningSession, codeEnvironment)
{
	private readonly object syncRoot = new object();

	private SingleSessionCodeCoverageRecorder archive;

	private SwitchDictionary<SingleSessionCodeCoverageRecorder> recorderDictionary = new SwitchDictionary<SingleSessionCodeCoverageRecorder>();

	public override IEnumerable<CodeCoverageScopeCounter> ScopeCounters
	{
		get
		{
			lock (syncRoot)
			{
				SwitchDictionary<SingleSessionCodeCoverageRecorder> switchDictionary = recorderDictionary;
				recorderDictionary = new SwitchDictionary<SingleSessionCodeCoverageRecorder>();
				foreach (SingleSessionCodeCoverageRecorder item in switchDictionary)
				{
					item.Dispose();
				}
				IEnumerable<CodeCoverageScopeCounter> result;
				if (archive != null)
				{
					result = archive.ScopeCounters;
				}
				else
				{
					IEnumerable<CodeCoverageScopeCounter> enumerable = new CodeCoverageScopeCounter[0];
					result = enumerable;
				}
				return result;
			}
		}
	}

	public override void ProcessExit(NavMethodScope methodScope)
	{
		GetOrCreateRecorderForConnection(methodScope.Session).ProcessExit(methodScope);
	}

	public override void ProcessStatementHit(NavMethodScope methodScope)
	{
		GetOrCreateRecorderForConnection(methodScope.Session).ProcessStatementHit(methodScope);
	}

	public override void SetHitCounts(Dictionary<int, CodeCoverageScopeCounter> scopeCounters)
	{
		lock (syncRoot)
		{
			archive = new SingleSessionCodeCoverageRecorder(OwningSession, CodeEnvironment, (SingleSessionCodeCoverageRecorder rec) =>
			{
			});
			archive.SetHitCounts(scopeCounters);
			recorderDictionary = new SwitchDictionary<SingleSessionCodeCoverageRecorder>();
		}
	}

	public override void StopRecording()
	{
		ExecutionListener.RemoveListener(this);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			foreach (SingleSessionCodeCoverageRecorder item in recorderDictionary)
			{
				item.Dispose();
			}
			archive.Dispose();
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private SingleSessionCodeCoverageRecorder GetOrCreateRecorderForConnection(NavSession session)
	{
		if (!recorderDictionary.TryGetValue(session.Id, out var item))
		{
			return CreateRecorderForConnectionId(session);
		}
		return item;
	}

	private SingleSessionCodeCoverageRecorder CreateRecorderForConnectionId(NavSession session)
	{
		lock (syncRoot)
		{
			if (recorderDictionary.TryGetValue(session.Id, out var item))
			{
				return item;
			}
			item = new SingleSessionCodeCoverageRecorder(session, CodeEnvironment, OnSingleSessionRecorderStopping);
			recorderDictionary = recorderDictionary.Add(session.Id, item);
			return item;
		}
	}

	private void OnSingleSessionRecorderStopping(SingleSessionCodeCoverageRecorder recorder)
	{
		lock (syncRoot)
		{
			archive = ((archive == null) ? recorder : DestructiveMergeSingleSessionRecordersForArchive(archive, recorder));
			if (recorder.OwningSession != null)
			{
				recorderDictionary = recorderDictionary.RemoveKey(recorder.OwningSession.Id);
			}
		}
	}

	private static SingleSessionCodeCoverageRecorder DestructiveMergeSingleSessionRecordersForArchive(SingleSessionCodeCoverageRecorder recorder1, SingleSessionCodeCoverageRecorder recorder2)
	{
		if (recorder1 == null)
		{
			return recorder2;
		}
		if (recorder2 == null)
		{
			return recorder1;
		}
		SingleSessionCodeCoverageRecorder singleSessionCodeCoverageRecorder;
		SingleSessionCodeCoverageRecorder singleSessionCodeCoverageRecorder2;
		if (recorder1.ScopeCounters.Count() > recorder2.ScopeCounters.Count())
		{
			singleSessionCodeCoverageRecorder = recorder1;
			singleSessionCodeCoverageRecorder2 = recorder2;
		}
		else
		{
			singleSessionCodeCoverageRecorder = recorder2;
			singleSessionCodeCoverageRecorder2 = recorder1;
		}
		foreach (CodeCoverageScopeCounter scopeCounter in singleSessionCodeCoverageRecorder2.ScopeCounters)
		{
			if (singleSessionCodeCoverageRecorder.ScopeCounterLookup.TryGetValue(scopeCounter.ScopeId, out var value))
			{
				value.DestructiveBulkAddHitCounts(scopeCounter.StatementHitCounts);
			}
			else
			{
				singleSessionCodeCoverageRecorder.ScopeCounterLookup.Add(scopeCounter.ScopeId, scopeCounter);
			}
		}
		return singleSessionCodeCoverageRecorder;
	}
}
