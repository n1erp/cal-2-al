using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class HttpHeadersScaffolding : Scaffolding<NavHttpHeaders>
{
	internal class HttpHeaderValueBinder : ValueBinder
	{
		private readonly string name;

		public HttpHeaderValueBinder(string name)
		{
			this.name = name ?? throw new ArgumentNullException("name");
		}

		public override object GetValue(object parent)
		{
			IEnumerable<string> enumerable = (parent as NavHttpHeaders)?.Headers.GetValues(name);
			if (enumerable == null)
			{
				return null;
			}
			return string.Join(";", enumerable);
		}

		public override bool TrySetValue(object parentInstance, object newChildValue)
		{
			return false;
		}

		public override Expression BuildGetValueExpression(Expression input)
		{
			return input;
		}
	}

	public override string TypeString => ScaffoldingHelper.GetTypeString(NavNclType.NavHttpHeaders, null);

	protected override bool ProbablyHasChildren => true;

	public HttpHeadersScaffolding(string name, Type targetType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, targetType, valueBinder, true, promotedChildren)
	{
	}

	protected override string GetStringValue(NavHttpHeaders instance)
	{
		return null;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavHttpHeaders instance)
	{
		foreach (KeyValuePair<string, IEnumerable<string>> header in instance.Headers)
		{
			yield return ScaffoldingFactory.GetScaffolding(header.Key, typeof(string), new HttpHeaderValueBinder(header.Key), instance, false);
		}
	}

	protected override void ReadMetadata(NavHttpHeaders instance)
	{
		base.ReadMetadata(instance);
	}
}
