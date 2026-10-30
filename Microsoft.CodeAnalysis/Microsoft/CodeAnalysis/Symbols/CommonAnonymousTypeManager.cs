namespace Microsoft.CodeAnalysis.Symbols;

internal abstract class CommonAnonymousTypeManager
{
	private ThreeState _templatesSealed = ThreeState.False;

	internal bool AreTemplatesSealed => _templatesSealed == ThreeState.True;

	protected void SealTemplates()
	{
		_templatesSealed = ThreeState.True;
	}
}
