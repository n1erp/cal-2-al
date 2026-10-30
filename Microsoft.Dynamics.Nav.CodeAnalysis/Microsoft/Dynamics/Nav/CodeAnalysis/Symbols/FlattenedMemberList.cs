using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class FlattenedMemberList<T>
{
	private ImmutableArray<T> lazyMembers;

	private readonly ImmutableArray<T> memberHierarchy;

	private readonly Func<T, ImmutableArray<T>> getChildren;

	internal ImmutableArray<T> FlattenedList
	{
		get
		{
			if (lazyMembers != null)
			{
				return lazyMembers;
			}
			ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
			foreach (T item in memberHierarchy)
			{
				AddChildMembers(instance, item);
			}
			ImmutableInterlocked.InterlockedExchange(ref lazyMembers, instance.ToImmutableAndFree());
			return lazyMembers;
		}
	}

	public FlattenedMemberList(ImmutableArray<T> memberHierarchy, Func<T, ImmutableArray<T>> getChildren)
	{
		this.memberHierarchy = memberHierarchy;
		this.getChildren = getChildren;
	}

	private void AddChildMembers(ArrayBuilder<T> members, T member)
	{
		members.Add(member);
		foreach (T item in getChildren(member))
		{
			AddChildMembers(members, item);
		}
	}
}
