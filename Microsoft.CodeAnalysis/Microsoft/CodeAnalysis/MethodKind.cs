namespace Microsoft.CodeAnalysis;

public enum MethodKind
{
	AnonymousFunction = 0,
	LambdaMethod = AnonymousFunction,
	Constructor = 1,
	Conversion = 2,
	DelegateInvoke = 3,
	Destructor = 4,
	EventAdd = 5,
	EventRaise = 6,
	EventRemove = 7,
	ExplicitInterfaceImplementation = 8,
	UserDefinedOperator = 9,
	Ordinary = 10,
	PropertyGet = 11,
	PropertySet = 12,
	ReducedExtension = 13,
	StaticConstructor = 14,
	SharedConstructor = StaticConstructor,
	BuiltinOperator = 15,
	DeclareMethod = 16,
	LocalFunction = 17
}
