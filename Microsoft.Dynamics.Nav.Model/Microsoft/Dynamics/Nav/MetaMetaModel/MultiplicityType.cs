using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaMetaModel;

public enum MultiplicityType
{
	[Name("0..1")]
	ZeroToOne,
	[Name("*")]
	ZeroToMany,
	[Name("1")]
	One,
	[Name("1..*")]
	OneToMany
}
