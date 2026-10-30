using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal abstract class Scaffolding
{
	private enum State
	{
		Created,
		MetadataRead,
		ChildrenCreated
	}

	protected const string UninitializedValueString = "<Uninitialized>";

	protected const string ErrorValueString = "<System Error retrieving value>";

	private readonly List<Scaffolding> children;

	private readonly bool childrenTypesCanChange;

	private readonly string name;

	private readonly bool promotedChildren;

	private readonly Type targetType;

	private LocalNode lastLocalNode;

	private State state;

	private ValueBinder valueBinder;

	private List<ScaffoldingValueCoercerBase> valueCoercers;

	private const int MaxLength = 1024;

	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return name;
		}
	}

	public bool PromotedChildren => promotedChildren;

	public abstract string TypeString { get; }

	public ValueBinder ValueBinder
	{
		[DebuggerStepThrough]
		get
		{
			return valueBinder;
		}
	}

	public IList<ScaffoldingValueCoercerBase> ValueCoercers => valueCoercers ?? (valueCoercers = new List<ScaffoldingValueCoercerBase>());

	internal List<Scaffolding> Children
	{
		get
		{
			if (state != State.ChildrenCreated)
			{
				throw new InvalidOperationException("Node must be expanded before the Children property can be accessed.");
			}
			return children;
		}
	}

	protected abstract bool ProbablyHasChildren { get; }

	protected virtual bool SetValueSupported => false;

	protected Type TargetType => targetType;

	internal bool ShowTextConstants { get; set; }

	protected Scaffolding(string name, Type targetType, ValueBinder valueBinder, bool childrenTypesCanChange, bool promotedChildren)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		if (valueBinder == null)
		{
			throw new ArgumentNullException("valueBinder");
		}
		this.name = name;
		this.targetType = targetType;
		this.childrenTypesCanChange = childrenTypesCanChange;
		this.promotedChildren = promotedChildren;
		this.valueBinder = valueBinder;
		children = new List<Scaffolding>();
		state = State.Created;
		ShowTextConstants = true;
	}

	public static object ApplyValueCoercers(object instance, IList<ScaffoldingValueCoercerBase> coercers)
	{
		if (coercers != null)
		{
			for (int num = coercers.Count - 1; num >= 0; num--)
			{
				instance = coercers[num].CoerceValue(instance);
			}
		}
		return instance;
	}

	public void Collapse()
	{
		if (state == State.ChildrenCreated)
		{
			children.Clear();
			state = State.MetadataRead;
		}
	}

	public LocalNode GetLocalNode(object instance, bool forceExpand = false)
	{
		EnsureMetadataRead(instance);
		string stringValue = GetStringValue(instance);
		LocalNodeChangeState localNodeChangeState = ((lastLocalNode == null) ? LocalNodeChangeState.New : ((!(lastLocalNode.Summary == stringValue)) ? LocalNodeChangeState.ValueChanged : LocalNodeChangeState.Unchanged));
		if (localNodeChangeState == LocalNodeChangeState.ValueChanged && ProbablyHasChildren)
		{
			Collapse();
			ExpandAll(instance);
		}
		else if (forceExpand && state != State.ChildrenCreated && ProbablyHasChildren)
		{
			ExpandAll(instance);
		}
		IEnumerable<LocalNode> enumerable = ((state == State.ChildrenCreated) ? GetChildrenLocalNodes(instance) : null);
		return lastLocalNode = new LocalNode(Name, TypeString, stringValue, ProbablyHasChildren, localNodeChangeState, enumerable);
	}

	public object GetValue(object parentInstance)
	{
		return CorceValue(GetValueNoCoercion(parentInstance));
	}

	public bool IsCompatible(object instance)
	{
		if (valueCoercers != null)
		{
			for (int num = valueCoercers.Count - 1; num >= 0; num--)
			{
				ScaffoldingValueCoercerBase scaffoldingValueCoercerBase = valueCoercers[num];
				if (!scaffoldingValueCoercerBase.IsCompatible(instance))
				{
					return false;
				}
				instance = scaffoldingValueCoercerBase.CoerceValue(instance);
			}
		}
		return IsCompatibleOverride(instance);
	}

	public void ReplaceValueBinder(ValueBinder newValueBinder)
	{
		if (newValueBinder == null)
		{
			throw new ArgumentNullException("newValueBinder");
		}
		valueBinder = newValueBinder;
	}

	public bool TryParseAndSetValue(object parentInstance, string newValueAsString)
	{
		bool flag = TryParse(newValueAsString, out var newValueAsObject);
		if (flag)
		{
			flag = TrySetValue(parentInstance, newValueAsObject);
		}
		return flag;
	}

	internal object CorceValue(object instance)
	{
		return ApplyValueCoercers(instance, valueCoercers);
	}

	internal bool Expand(object instance)
	{
		EnsureMetadataRead(instance);
		if (state != State.ChildrenCreated)
		{
			children.AddRange(CreateChildren(instance));
			state = State.ChildrenCreated;
			return true;
		}
		return false;
	}

	internal void ExpandAll(object instance)
	{
		HashSet<object> expandedInstances = new HashSet<object>();
		ExpandAll(instance, expandedInstances);
	}

	private void ExpandAll(object instance, HashSet<object> expandedInstances)
	{
		if (expandedInstances.Contains(instance))
		{
			return;
		}
		if (Expand(instance))
		{
			expandedInstances.Add(instance);
		}
		foreach (Scaffolding child in children)
		{
			child.ShowTextConstants = ShowTextConstants;
			child.ExpandAll(child.GetValue(instance), expandedInstances);
		}
	}

	internal IEnumerable<LocalNode> GetChildrenLocalNodes(object parentInstance)
	{
		bool flag = Expand(parentInstance);
		if (childrenTypesCanChange && !flag)
		{
			List<LocalNode> list = new List<LocalNode>(children.Count);
			for (int i = 0; i < children.Count; i++)
			{
				Scaffolding scaffolding = children[i];
				object value = scaffolding.GetValue(parentInstance);
				scaffolding.EnsureMetadataRead(value);
				if (!scaffolding.IsCompatibleOverride(value))
				{
					try
					{
						scaffolding = ScaffoldingFactory.GetScaffolding(scaffolding.Name, scaffolding.targetType, scaffolding.valueBinder, parentInstance, promotedChildren);
						children[i] = scaffolding;
						value = scaffolding.GetValue(parentInstance);
					}
					catch (NavNCLNotSupportedTypeException)
					{
						continue;
					}
				}
				list.Add(scaffolding.GetLocalNode(value));
			}
			return list;
		}
		return children.Select((Scaffolding c) => c.GetLocalNode(c.GetValue(parentInstance)));
	}

	protected internal abstract string GetStringValue(object instance);

	protected virtual IEnumerable<Scaffolding> CreateChildren(object instance)
	{
		yield break;
	}

	protected object GetValueNoCoercion(object parentInstance)
	{
		return valueBinder.GetValue(parentInstance);
	}

	protected abstract bool IsCompatibleOverride(object instance);

	private void EnsureMetadataRead(object instance)
	{
		if (state == State.Created)
		{
			ReadMetadata(instance);
			state = State.MetadataRead;
		}
	}

	protected virtual void ReadMetadata(object instance)
	{
	}

	protected virtual bool TryParse(string newValueAsString, out object newValueAsObject)
	{
		newValueAsObject = null;
		return false;
	}

	protected bool TrySetValue(object parentInstance, object newValueAsObject)
	{
		if (SetValueSupported && ValueCoercers.Count <= 1)
		{
			foreach (ScaffoldingValueCoercerBase valueCoercer in ValueCoercers)
			{
				object valueNoCoercion = GetValueNoCoercion(parentInstance);
				if (!valueCoercer.TryReverseCoerce(valueNoCoercion, newValueAsObject))
				{
					return false;
				}
				newValueAsObject = valueNoCoercion;
			}
			return ValueBinder.TrySetValue(parentInstance, newValueAsObject);
		}
		return false;
	}

	protected static string TruncateIfNeeded(string substring)
	{
		if (substring.Length > 1024)
		{
			substring = substring.Substring(0, 1023) + "…";
		}
		return substring;
	}
}
internal abstract class Scaffolding<T> : Scaffolding
{
	protected Scaffolding(string name, Type targetType, ValueBinder valueBinder, bool childrenTypesCanChange, bool promotedChildren)
		: base(name, targetType, valueBinder, childrenTypesCanChange, promotedChildren)
	{
	}

	protected internal sealed override string GetStringValue(object instance)
	{
		if (instance == null || instance is T)
		{
			return GetStringValue((T)instance);
		}
		return "<System Error retrieving value>";
	}

	protected sealed override IEnumerable<Scaffolding> CreateChildren(object instance)
	{
		if (instance == null || instance is T)
		{
			return CreateChildren((T)instance);
		}
		return new Scaffolding[0];
	}

	protected virtual IEnumerable<Scaffolding> CreateChildren(T instance)
	{
		yield break;
	}

	protected abstract string GetStringValue(T instance);

	protected sealed override void ReadMetadata(object instance)
	{
		if (instance == null || instance is T)
		{
			ReadMetadata((T)instance);
		}
	}

	protected virtual void ReadMetadata(T instance)
	{
	}

	private static string AssertTypeString(object instance)
	{
		return string.Format(CultureInfo.InvariantCulture, "Debugger Scaffolding Error - Expected type : {0}, Actual: {1}", typeof(T).Name, instance.GetType().Name);
	}
}
