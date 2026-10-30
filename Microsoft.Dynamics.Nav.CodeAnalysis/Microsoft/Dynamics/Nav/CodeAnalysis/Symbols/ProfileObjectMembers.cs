using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ProfileObjectMembers : ObjectMembers<ProfileTypeSymbol>
{
	internal ProfileObjectMembers(ProfileTypeSymbol containingSymbol, ProfileSyntax declaration)
		: base(containingSymbol, (ObjectSyntax)declaration)
	{
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		CheckMissingProperties(diagnostics);
		GetProperty(PropertyKind.RoleCenter)?.ValidateRoleCenterPageReference(ContainingSymbol.DeclaringCompilation, diagnostics);
		CheckPageCustomizations(diagnostics);
	}

	private void CheckPageCustomizations(DiagnosticBag diagnostics)
	{
		ContainingSymbol.GetBoundPropertyValue<BoundIdentifierListPropertyValue>(PropertyKind.Customizations)?.Value.Where((BoundExpression exp) => exp.Kind == BoundKind.ApplicationObjectAccess).Cast<BoundApplicationObjectAccess>().GroupBy((BoundApplicationObjectAccess appObjAccess) => ((PageCustomizationTypeSymbol)appObjAccess.ApplicationObjectTypeSymbol).Target?.Name)
			.Where((IGrouping<string, BoundApplicationObjectAccess> group) => group.Count() > 1 && group.Key != null)
			.Do((IGrouping<string, BoundApplicationObjectAccess> group) =>
			{
				group.Do((BoundApplicationObjectAccess appObjAccess) =>
				{
					diagnostics.Add(ErrorCode.ERR_MultiplePageCustomizationsSpecifiedPerProfilePerTargetPage, appObjAccess.Syntax.GetLocation(), group.Key);
				});
			});
	}

	private void CheckMissingProperties(DiagnosticBag diagnostics)
	{
		if (GetProperty(PropertyKind.Description) == null)
		{
			diagnostics.Add(ErrorCode.ERR_MissingObjectProperty, ContainingSymbol.GetLocation(), PropertyKind.Description.ToString());
		}
		if (GetProperty(PropertyKind.RoleCenter) == null)
		{
			diagnostics.Add(ErrorCode.ERR_MissingObjectProperty, ContainingSymbol.GetLocation(), PropertyKind.RoleCenter.ToString());
		}
	}
}
