using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class XmlPortScaffolding : ApplicationObjectScaffolding<NavXmlPort>
{
	public class UninitializedXmlPortRecordScaffolding : Scaffolding
	{
		public override string TypeString => string.Empty;

		protected override bool ProbablyHasChildren => false;

		public UninitializedXmlPortRecordScaffolding(string name, Type targetType, ValueBinder valueBinder)
			: base(name, targetType, valueBinder, childrenTypesCanChange: false, promotedChildren: false)
		{
		}

		protected internal override string GetStringValue(object instance)
		{
			return "<Uninitialized>";
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return instance == null;
		}
	}

	private const string RecPropertyName = "Rec";

	private const string XRecPropertyName = "xRec";

	public XmlPortScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren, bool hideGlobalsNode)
		: base(name, type, valueBinder, promotedChildren, hideGlobalsNode)
	{
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavXmlPort instance)
	{
		IEnumerable<Scaffolding> result = base.CreateChildren(instance);
		if (HideGlobalsNode)
		{
			result = ReplaceRecAndXRec(instance, result);
		}
		return result;
	}

	protected override IEnumerable<Scaffolding> CreateGlobalChildren(NavApplicationObjectBase instance)
	{
		IEnumerable<Scaffolding> enumerable = base.CreateGlobalChildren(instance);
		return ReplaceRecAndXRec((NavXmlPort)instance, enumerable);
	}

	internal static IEnumerable<Scaffolding> ReplaceRecAndXRec(NavXmlPort instance, IEnumerable<Scaffolding> children)
	{
		if (instance.Rec != null)
		{
			return children;
		}
		DelegateValueBinder delegateValueBinder = new DelegateValueBinder((object r) => ((NavXmlPort)r).Rec, typeof(NavRecord));
		UninitializedXmlPortRecordScaffolding uninitializedXmlPortRecordScaffolding = new UninitializedXmlPortRecordScaffolding("Rec", typeof(NavXmlPort), delegateValueBinder);
		UninitializedXmlPortRecordScaffolding uninitializedXmlPortRecordScaffolding2 = new UninitializedXmlPortRecordScaffolding("xRec", typeof(NavXmlPort), delegateValueBinder);
		return new UninitializedXmlPortRecordScaffolding[2] { uninitializedXmlPortRecordScaffolding, uninitializedXmlPortRecordScaffolding2 }.Concat(children.Where((Scaffolding c) => c.Name != "Rec" && c.Name != "xRec"));
	}
}
