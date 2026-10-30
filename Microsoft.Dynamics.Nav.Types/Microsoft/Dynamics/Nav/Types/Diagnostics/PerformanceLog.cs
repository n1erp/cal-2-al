using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types.Diagnostics;

public class PerformanceLog
{
	private class EventStats
	{
		public int HitCount { get; private set; }

		public TimeSpan Elapsed { get; private set; }

		public void RecordHit(TimeSpan elapsed)
		{
			HitCount++;
			Elapsed += elapsed;
		}
	}

	private readonly string title;

	private readonly List<Tuple<string, long>> linearStepTimings = new List<Tuple<string, long>>();

	private string currentLinearStep;

	private Stopwatch linearStepSw = new Stopwatch();

	private readonly List<string> messages = new List<string>();

	private readonly Dictionary<string, EventStats> eventStats = new Dictionary<string, EventStats>();

	public string Report
	{
		get
		{
			FinalizeLinearSteps();
			if (linearStepTimings.Any() || eventStats.Any() || messages.Any())
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "PERFORMANCE LOG FOR: {0}{1}", title, Environment.NewLine);
				stringBuilder.AppendLine();
				if (linearStepTimings.Any())
				{
					stringBuilder.AppendLine("ORDERED STEPS");
					foreach (Tuple<string, long> linearStepTiming in linearStepTimings)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "  Step:{0}; ElapsedMs:{1}{2}", linearStepTiming.Item1, linearStepTiming.Item2, Environment.NewLine);
					}
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "  Step:TOTAL; ElapsedMs:{0}{1}", linearStepTimings.Sum((Tuple<string, long> t) => t.Item2), Environment.NewLine);
					stringBuilder.AppendLine();
				}
				if (eventStats.Any())
				{
					stringBuilder.AppendLine("EVENT STATS");
					foreach (KeyValuePair<string, EventStats> item in eventStats.OrderBy((KeyValuePair<string, EventStats> kvp) => kvp.Key))
					{
						double num = Math.Round(item.Value.Elapsed.TotalMilliseconds, 0);
						double num2 = Math.Round(num / (double)item.Value.HitCount, 0);
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "  Event:{0}; Hits:{1}; ElapsedMs:{2}; AverageMs:{3}{4}", item.Key, item.Value.HitCount, num, num2, Environment.NewLine);
					}
					stringBuilder.AppendLine();
				}
				if (messages.Any())
				{
					stringBuilder.AppendLine("MESSAGES");
					foreach (string message in messages)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "  {0}{1}", message, Environment.NewLine);
					}
					stringBuilder.AppendLine();
				}
				return stringBuilder.ToString();
			}
			return null;
		}
	}

	public PerformanceLog(string title)
	{
		this.title = title;
		BeginLinearTimingStep("InitialAutoStep");
	}

	public void RecordMessage(string message)
	{
		messages.Add(message);
	}

	public void RecordTimedEvent(string eventName, TimeSpan elapsed)
	{
		if (!eventStats.TryGetValue(eventName, out var value))
		{
			value = new EventStats();
			eventStats.Add(eventName, value);
		}
		value.RecordHit(elapsed);
	}

	public void TimeAndRecordAction(string eventName, Action action)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			action();
		}
		finally
		{
			RecordTimedEvent(eventName, stopwatch.Elapsed);
		}
	}

	public void BeginLinearTimingStep(string stepName)
	{
		if (linearStepSw != null)
		{
			if (currentLinearStep != null)
			{
				linearStepTimings.Add(Tuple.Create(currentLinearStep, linearStepSw.ElapsedMilliseconds));
			}
			currentLinearStep = stepName;
			linearStepSw.Restart();
		}
	}

	public void BeginUnnamedTimingStep()
	{
		BeginLinearTimingStep("*UNNAMED*");
	}

	private void FinalizeLinearSteps()
	{
		if (linearStepSw != null)
		{
			linearStepSw.Stop();
			if (currentLinearStep != null)
			{
				linearStepTimings.Add(Tuple.Create(currentLinearStep, linearStepSw.ElapsedMilliseconds));
				currentLinearStep = null;
			}
			linearStepSw = null;
		}
	}
}
