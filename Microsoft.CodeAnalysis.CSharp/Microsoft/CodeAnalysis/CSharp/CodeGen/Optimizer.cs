using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal class Optimizer
{
	public static BoundStatement Optimize(BoundStatement src, bool debugFriendly, out HashSet<LocalSymbol> stackLocals)
	{
		PooledDictionary<LocalSymbol, LocalDefUseInfo> instance = PooledDictionary<LocalSymbol, LocalDefUseInfo>.GetInstance();
		src = (BoundStatement)StackOptimizerPass1.Analyze(src, instance, debugFriendly);
		FilterValidStackLocals(instance);
		BoundStatement result;
		if (instance.Count == 0)
		{
			stackLocals = null;
			result = src;
		}
		else
		{
			stackLocals = new HashSet<LocalSymbol>(instance.Keys);
			result = StackOptimizerPass2.Rewrite(src, instance);
		}
		foreach (LocalDefUseInfo value in instance.Values)
		{
			value.Free();
		}
		instance.Free();
		return result;
	}

	private static void FilterValidStackLocals(Dictionary<LocalSymbol, LocalDefUseInfo> info)
	{
		ArrayBuilder<LocalDefUseInfo> instance = ArrayBuilder<LocalDefUseInfo>.GetInstance();
		LocalSymbol[] array = info.Keys.ToArray();
		foreach (LocalSymbol localSymbol in array)
		{
			LocalDefUseInfo localDefUseInfo = info[localSymbol];
			if (localSymbol.SynthesizedKind == SynthesizedLocalKind.OptimizerTemp)
			{
				instance.Add(localDefUseInfo);
				info.Remove(localSymbol);
			}
			else if (localDefUseInfo.CannotSchedule)
			{
				localDefUseInfo.Free();
				info.Remove(localSymbol);
			}
		}
		if (info.Count != 0)
		{
			RemoveIntersectingLocals(info, instance);
		}
		foreach (LocalDefUseInfo item in instance)
		{
			item.Free();
		}
		instance.Free();
	}

	private static void RemoveIntersectingLocals(Dictionary<LocalSymbol, LocalDefUseInfo> info, ArrayBuilder<LocalDefUseInfo> dummies)
	{
		ArrayBuilder<LocalDefUseSpan> instance = ArrayBuilder<LocalDefUseSpan>.GetInstance(dummies.Count);
		foreach (LocalDefUseInfo dummy2 in dummies)
		{
			foreach (LocalDefUseSpan localDef in dummy2.LocalDefs)
			{
				if (localDef.Start != localDef.End)
				{
					instance.Add(localDef);
				}
			}
		}
		int count = instance.Count;
		foreach (var item in from _003C_003Eh__TransparentIdentifier0 in info.SelectMany((KeyValuePair<LocalSymbol, LocalDefUseInfo> i) =>
			{
				KeyValuePair<LocalSymbol, LocalDefUseInfo> keyValuePair = i;
				return keyValuePair.Value.LocalDefs;
			}, (KeyValuePair<LocalSymbol, LocalDefUseInfo> i, LocalDefUseSpan d2) => new
			{
				i = i,
				d = d2
			})
			orderby _003C_003Eh__TransparentIdentifier0.d.End - _003C_003Eh__TransparentIdentifier0.d.Start, _003C_003Eh__TransparentIdentifier0.d.End
			select new
			{
				i = _003C_003Eh__TransparentIdentifier0.i.Key,
				d = _003C_003Eh__TransparentIdentifier0.d
			})
		{
			if (!info.ContainsKey(item.i))
			{
				continue;
			}
			LocalDefUseSpan d = item.d;
			int count2 = instance.Count;
			bool flag;
			if (count2 > 5000)
			{
				flag = true;
			}
			else
			{
				flag = false;
				for (int num = 0; num < count; num++)
				{
					LocalDefUseSpan dummy = instance[num];
					if (d.ConflictsWithDummy(dummy))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					for (int num2 = count; num2 < count2; num2++)
					{
						LocalDefUseSpan other = instance[num2];
						if (d.ConflictsWith(other))
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (flag)
			{
				info[item.i].LocalDefs.Free();
				info.Remove(item.i);
			}
			else
			{
				instance.Add(d);
			}
		}
		instance.Free();
	}
}
