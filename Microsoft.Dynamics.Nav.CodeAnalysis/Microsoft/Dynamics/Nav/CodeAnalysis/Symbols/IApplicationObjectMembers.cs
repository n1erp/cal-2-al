namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal interface IApplicationObjectMembers : IObjectMembers
{
	int Id { get; }

	string Name { get; }
}
