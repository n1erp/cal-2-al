using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavValueFilterToken : FilterToken
{
	private readonly NavValue navValue;

	public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(navValue) + SizableObject.GetApproximateByteSize(1);

	public NavValue NavValue
	{
		[DebuggerStepThrough]
		get
		{
			return navValue;
		}
	}

	public override string Value => navValue.ToString();

	public NavValueFilterToken(NavValue navValue)
		: base(FilterTokenType.BinaryValue)
	{
		if (navValue == null)
		{
			throw new ArgumentNullException("navValue");
		}
		this.navValue = navValue;
	}
}
