using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class ReportScaffolding : ApplicationObjectScaffolding<NavReport>
{
	public class UninitializedReportRecordScaffolding : Scaffolding
	{
		public override string TypeString => string.Empty;

		protected override bool ProbablyHasChildren => false;

		public UninitializedReportRecordScaffolding(string name, Type targetType, ValueBinder valueBinder)
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

	public ReportScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren, bool hideGlobalsNode)
		: base(name, type, valueBinder, promotedChildren, hideGlobalsNode)
	{
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavReport instance)
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
		return ReplaceRecAndXRec((NavReport)instance, enumerable);
	}

	internal static IEnumerable<Scaffolding> ReplaceRecAndXRec(NavReport instance, IEnumerable<Scaffolding> children)
	{
		if (instance.Rec != null)
		{
			return children;
		}
		DelegateValueBinder delegateValueBinder = new DelegateValueBinder((object r) => ((NavReport)r).Rec, typeof(NavRecord));
		UninitializedReportRecordScaffolding uninitializedReportRecordScaffolding = new UninitializedReportRecordScaffolding("Rec", typeof(NavRecord), delegateValueBinder);
		UninitializedReportRecordScaffolding uninitializedReportRecordScaffolding2 = new UninitializedReportRecordScaffolding("xRec", typeof(NavRecord), delegateValueBinder);
		return new UninitializedReportRecordScaffolding[2] { uninitializedReportRecordScaffolding, uninitializedReportRecordScaffolding2 }.Concat(children.Where((Scaffolding c) => c.Name != "Rec" && c.Name != "xRec"));
	}
}
