using System;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class DeltaSyntaxGenFactory
{
	internal static DeltaSyntaxGen GetSyntaxGenerator(PersonalizationV1DeltaType personalizationV1DeltaType)
	{
		return personalizationV1DeltaType switch
		{
			PersonalizationV1DeltaType.Add => (DeltaSyntaxGen)new AddDeltaSyntaxGen(new AddDeltaValidator()), 
			PersonalizationV1DeltaType.Delete => new DeleteDeltaSyntaxGen(new DeleteDeltaValidator()), 
			PersonalizationV1DeltaType.Move => new MoveDeltaSyntaxGen(new MoveDeltaValidator()), 
			PersonalizationV1DeltaType.Order => new OrderDeltaSyntaxGen(new OrderDeltaValidator()), 
			PersonalizationV1DeltaType.Update => new UpdateDeltaSyntaxGen(new UpdateDeltaValidator()), 
			_ => throw new InvalidOperationException("The operation is not supported right now!"), 
		};
	}
}
