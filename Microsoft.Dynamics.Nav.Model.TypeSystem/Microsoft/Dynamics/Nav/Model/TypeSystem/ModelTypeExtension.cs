using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public static class ModelTypeExtension
{
	public static void AssertCompiledState(this IModelType modelType, CompiledState compileStateRequired)
	{
		if (modelType.CompiledState < compileStateRequired)
		{
			throw new InvalidOperationException($"Type {modelType.GetType()} has not been compiled");
		}
	}

	public static string ValueAsString(this IModelType modelType)
	{
		return modelType.ValueAsString(ModelTypeFormatter.InvariantFormatter);
	}

	public static string ValueAsString(this IModelType modelType, IModelTypeFormatter formatter)
	{
		return (formatter ?? ModelTypeFormatter.InvariantFormatter).Format(modelType);
	}
}
