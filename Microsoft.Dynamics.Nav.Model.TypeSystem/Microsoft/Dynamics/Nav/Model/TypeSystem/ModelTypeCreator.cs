namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public abstract class ModelTypeCreator
{
	private static IModelTypeCreatorFromString genericTypeCreatorFromString = new GenericTypeCreatorFromString();

	private static IModelTypeCreatorFromString invariantTypeCreatorFromString = new InvariantTypeCreatorFromString();

	public static IModelTypeCreatorFromString GenericTypeCreatorFromString => genericTypeCreatorFromString;

	public static IModelTypeCreatorFromString InvariantTypeCreatorFromString => invariantTypeCreatorFromString;
}
