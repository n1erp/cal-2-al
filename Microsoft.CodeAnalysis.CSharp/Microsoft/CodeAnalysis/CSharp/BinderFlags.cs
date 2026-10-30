using System;

namespace Microsoft.CodeAnalysis.CSharp;

[Flags]
internal enum BinderFlags : uint
{
	None = 0u,
	SuppressConstraintChecks = 1u,
	SuppressObsoleteChecks = 2u,
	ConstructorInitializer = 4u,
	FieldInitializer = 8u,
	ObjectInitializerMember = 0x10u,
	CollectionInitializerAddMethod = 0x20u,
	AttributeArgument = 0x40u,
	GenericConstraintsClause = 0x80u,
	Cref = 0x100u,
	CrefParameterOrReturnType = 0x200u,
	UnsafeRegion = 0x400u,
	SuppressUnsafeDiagnostics = 0x800u,
	SemanticModel = 0x1000u,
	EarlyAttributeBinding = 0x2000u,
	CheckedRegion = 0x4000u,
	UncheckedRegion = 0x8000u,
	InLockBody = 0x10000u,
	InCatchBlock = 0x20000u,
	InFinallyBlock = 0x40000u,
	InTryBlockOfTryCatch = 0x80000u,
	InCatchFilter = 0x100000u,
	InNestedFinallyBlock = 0x200000u,
	IgnoreAccessibility = 0x400000u,
	ParameterDefaultValue = 0x800000u,
	AllowManagedAddressOf = 0x1000000u,
	AllowAwaitInUnsafeContext = 0x2000000u,
	IgnoreCorLibraryDuplicatedTypes = 0x4000000u,
	InScriptUsing = 0x8000000u,
	InLoadedSyntaxTree = 0x10000000u,
	AllClearedAtExecutableCodeBoundary = InLockBody | InCatchBlock | InFinallyBlock | InTryBlockOfTryCatch | InCatchFilter | InNestedFinallyBlock
}
