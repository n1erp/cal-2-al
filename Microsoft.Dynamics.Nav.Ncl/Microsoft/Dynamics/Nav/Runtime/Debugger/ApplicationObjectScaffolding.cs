using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class ApplicationObjectScaffolding<T> : Scaffolding<T> where T : NavApplicationObjectBase
{
	private class GlobalsScaffolding : Scaffolding<T>
	{
		private ApplicationObjectScaffolding<T> applicationObjectScaffolding;

		public override string TypeString => string.Empty;

		protected override bool ProbablyHasChildren => true;

		public GlobalsScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren, ApplicationObjectScaffolding<T> applicationObjectScaffolding)
			: base(name, type, valueBinder, false, promotedChildren)
		{
			this.applicationObjectScaffolding = applicationObjectScaffolding;
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(T instance)
		{
			return applicationObjectScaffolding.CreateGlobalChildren(instance);
		}

		protected override string GetStringValue(T instance)
		{
			return string.Empty;
		}
	}

	private static readonly Regex recRegex = new Regex("^(x?Rec|CurrFieldNo)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	private Type type;

	private string typeString;

	private readonly bool hideGlobalsNode;

	public ApplicationObjectId ObjectId { get; protected set; }

	public override string TypeString => typeString;

	protected bool HideGlobalsNode => hideGlobalsNode;

	protected override bool ProbablyHasChildren => true;

	public ApplicationObjectScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren, bool hideGlobalsNode)
		: base(name, type, valueBinder, true, promotedChildren)
	{
		this.hideGlobalsNode = hideGlobalsNode;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavApplicationObjectBase navApplicationObjectBase && navApplicationObjectBase.ObjectId == ObjectId)
		{
			return instance.GetType() == type;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(T instance)
	{
		if (hideGlobalsNode)
		{
			return CreateGlobalChildren(instance);
		}
		return new GlobalsScaffolding[1]
		{
			new GlobalsScaffolding("<Globals>", instance.GetType(), new IdentityValueBinder(), PromotedChildren, this)
		};
	}

	protected override string GetStringValue(T instance)
	{
		return null;
	}

	protected override void ReadMetadata(T instance)
	{
		if (instance != null)
		{
			ObjectId = instance.ObjectId;
			type = instance.GetType();
			typeString = ScaffoldingHelper.GetTypeString(instance);
		}
		else
		{
			typeString = string.Empty;
		}
	}

	protected virtual IEnumerable<Scaffolding> CreateGlobalChildren(NavApplicationObjectBase instance)
	{
		instance.EnsureGlobalVariablesInitialized();
		if (DebugRuntime.IsDebuggingDisabledForApplicationObject(instance.MetaApplicationObject))
		{
			return ImmutableArray<Scaffolding>.Empty;
		}
		IEnumerable<Scaffolding> enumerable = from propInfo in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			let name = propInfo.Name
			where recRegex.IsMatch(name) && propInfo.IsDebuggerAvailable()
			let valueBinder = new PropertyInfoValueBinder(propInfo)
			select (!typeof(NavRecord).IsAssignableFrom(propInfo.PropertyType)) ? ScaffoldingFactory.GetScaffolding(name, propInfo.PropertyType, valueBinder, instance, false) : new RecordScaffolding(name, propInfo.PropertyType, valueBinder, name == "Rec", !(instance is NavRecord));
		IEnumerable<Scaffolding> second = from fieldInfo in (instance.GetType() == typeof(NavRecord)) ? new FieldInfo[0] : instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
			where fieldInfo.IsDebuggerAvailable()
			let name = fieldInfo.AlName()
			let valueBinder = new FieldInfoValueBinder(fieldInfo)
			select ScaffoldingFactory.GetScaffolding(name, fieldInfo.FieldType, valueBinder, instance, false);
		if (ShowTextConstants)
		{
			TextConstantsScaffolding textConstantsScaffolding = new TextConstantsScaffolding("<Global Text Constants>", instance.GetType(), new IdentityValueBinder());
			textConstantsScaffolding.Expand(instance);
			if (textConstantsScaffolding.Children.Count > 0)
			{
				textConstantsScaffolding.Collapse();
				return new TextConstantsScaffolding[1] { textConstantsScaffolding }.Concat(enumerable).Concat(second);
			}
		}
		return enumerable.Concat(second);
	}
}
