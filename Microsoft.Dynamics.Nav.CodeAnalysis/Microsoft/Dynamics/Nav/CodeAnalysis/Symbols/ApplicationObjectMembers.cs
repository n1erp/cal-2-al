using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal class ApplicationObjectMembers<T> : ObjectMembers<T>, IApplicationObjectMembers, IObjectMembers where T : ObjectTypeSymbol
{
	internal new ApplicationObjectSyntax Declaration => (ApplicationObjectSyntax)base.Declaration;

	public int Id { get; }

	internal ApplicationObjectMembers(T containingSymbol, ApplicationObjectSyntax declaration)
		: base(containingSymbol, (ObjectSyntax)declaration)
	{
		Id = declaration.ObjectId.GetObjectIdOrDefault();
	}

	protected override void CheckObject(DiagnosticBag diagnostics)
	{
		if (Name.Length > 30)
		{
			diagnostics.Add(ErrorCode.ERR_ObjectIdentifierTooLong, Declaration.Name.GetLocation(), 30);
		}
	}
}
