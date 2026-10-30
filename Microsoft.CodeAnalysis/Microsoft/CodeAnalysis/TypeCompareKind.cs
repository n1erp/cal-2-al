using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum TypeCompareKind
{
	ConsiderEverything = 0,
	IgnoreCustomModifiersAndArraySizesAndLowerBounds = 1,
	IgnoreDynamic = 2,
	IgnoreTupleNames = 4,
	IgnoreDynamicAndTupleNames = IgnoreDynamic | IgnoreTupleNames,
	AllIgnoreOptions = IgnoreDynamicAndTupleNames | IgnoreCustomModifiersAndArraySizesAndLowerBounds,
	AllIgnoreOptionsForVB = 5
}
