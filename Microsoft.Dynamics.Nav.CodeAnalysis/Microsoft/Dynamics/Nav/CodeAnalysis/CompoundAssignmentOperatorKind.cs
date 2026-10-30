using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
internal enum CompoundAssignmentOperatorKind
{
	TypeMask = 0xFF,
	OpMask = 0xFF00,
	Multiplication = 0x1000,
	Addition = 0x1100,
	Subtraction = 0x1200,
	Division = 0x1300,
	Remainder = 0x1400,
	IDivision = 0x1F00
}
