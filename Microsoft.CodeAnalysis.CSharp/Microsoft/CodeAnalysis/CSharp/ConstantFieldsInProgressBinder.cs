namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ConstantFieldsInProgressBinder : Binder
{
	private readonly ConstantFieldsInProgress _inProgress;

	internal override ConstantFieldsInProgress ConstantFieldsInProgress => _inProgress;

	internal ConstantFieldsInProgressBinder(ConstantFieldsInProgress inProgress, Binder next)
		: base(next, BinderFlags.FieldInitializer | next.Flags)
	{
		_inProgress = inProgress;
	}
}
