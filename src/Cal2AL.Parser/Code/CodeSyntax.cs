namespace Cal2AL.Parser.Code;

/// <summary>Base class of all C/AL syntax nodes. Every node knows the first and last token it covers.</summary>
public abstract class SyntaxNode
{
    public required CodeToken FirstToken { get; init; }
    public required CodeToken LastToken { get; init; }

    public int Line => FirstToken.Line;

    public abstract IEnumerable<SyntaxNode> Children { get; }

    /// <summary>This node and all its descendants, depth first.</summary>
    public IEnumerable<SyntaxNode> DescendantsAndSelf()
    {
        var stack = new Stack<SyntaxNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            foreach (var child in node.Children.Reverse())
            {
                stack.Push(child);
            }
        }
    }
}

// ---- Expressions ---------------------------------------------------------------------------------

public abstract class ExpressionSyntax : SyntaxNode;

public sealed class LiteralExpression : ExpressionSyntax
{
    public required CodeToken Token { get; init; }
    public override IEnumerable<SyntaxNode> Children => [];
}

public sealed class NameExpression : ExpressionSyntax
{
    public required CodeToken Name { get; init; }
    public string Identifier => Name.IdentifierName;
    public override IEnumerable<SyntaxNode> Children => [];
}

/// <summary><c>Target.Name</c></summary>
public sealed class MemberAccessExpression : ExpressionSyntax
{
    public required ExpressionSyntax Target { get; init; }
    public required CodeToken Name { get; init; }
    public string MemberName => Name.IdentifierName;
    public override IEnumerable<SyntaxNode> Children => [Target];
}

/// <summary><c>Target::Name</c> (option value, object ID such as <c>DATABASE::Customer</c>).</summary>
public sealed class OptionAccessExpression : ExpressionSyntax
{
    public required ExpressionSyntax Target { get; init; }
    public required CodeToken Name { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Target];
}

public sealed class InvocationExpression : ExpressionSyntax
{
    public required ExpressionSyntax Target { get; init; }
    public required IReadOnlyList<ExpressionSyntax> Arguments { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Target, .. Arguments];
}

public sealed class IndexExpression : ExpressionSyntax
{
    public required ExpressionSyntax Target { get; init; }
    public required IReadOnlyList<ExpressionSyntax> Indices { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Target, .. Indices];
}

public sealed class UnaryExpression : ExpressionSyntax
{
    public required CodeToken Operator { get; init; }
    public required ExpressionSyntax Operand { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Operand];
}

public sealed class BinaryExpression : ExpressionSyntax
{
    public required ExpressionSyntax Left { get; init; }
    public required CodeToken Operator { get; init; }
    public required ExpressionSyntax Right { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Left, Right];
}

/// <summary><c>Low..High</c> inside a set literal or a CASE label; either side may be missing.</summary>
public sealed class RangeExpression : ExpressionSyntax
{
    public ExpressionSyntax? Low { get; init; }
    public ExpressionSyntax? High { get; init; }
    public override IEnumerable<SyntaxNode> Children => new[] { Low, High }.Where(e => e is not null).Cast<SyntaxNode>();
}

/// <summary><c>[a, b..c]</c> used with IN.</summary>
public sealed class SetExpression : ExpressionSyntax
{
    public required IReadOnlyList<ExpressionSyntax> Elements { get; init; }
    public override IEnumerable<SyntaxNode> Children => Elements;
}

public sealed class ParenthesizedExpression : ExpressionSyntax
{
    public required ExpressionSyntax Inner { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Inner];
}

// ---- Statements ----------------------------------------------------------------------------------

public abstract class StatementSyntax : SyntaxNode;

public sealed class BlockStatement : StatementSyntax
{
    public required IReadOnlyList<StatementSyntax> Statements { get; init; }
    public override IEnumerable<SyntaxNode> Children => Statements;
}

public sealed class EmptyStatement : StatementSyntax
{
    public override IEnumerable<SyntaxNode> Children => [];
}

public sealed class AssignmentStatement : StatementSyntax
{
    public required ExpressionSyntax Target { get; init; }
    public required CodeToken Operator { get; init; }
    public required ExpressionSyntax Value { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Target, Value];
}

public sealed class ExpressionStatement : StatementSyntax
{
    public required ExpressionSyntax Expression { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Expression];
}

public sealed class IfStatement : StatementSyntax
{
    public required ExpressionSyntax Condition { get; init; }
    public required StatementSyntax Then { get; init; }
    public StatementSyntax? Else { get; init; }
    public override IEnumerable<SyntaxNode> Children => new SyntaxNode?[] { Condition, Then, Else }.Where(n => n is not null).Cast<SyntaxNode>();
}

public sealed class CaseBranch : SyntaxNode
{
    public required IReadOnlyList<ExpressionSyntax> Labels { get; init; }
    public required StatementSyntax Body { get; init; }
    public override IEnumerable<SyntaxNode> Children => [.. Labels, Body];
}

public sealed class CaseStatement : StatementSyntax
{
    public required ExpressionSyntax Selector { get; init; }
    public required IReadOnlyList<CaseBranch> Branches { get; init; }
    public IReadOnlyList<StatementSyntax>? ElseStatements { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Selector, .. Branches, .. ElseStatements ?? []];
}

public sealed class WhileStatement : StatementSyntax
{
    public required ExpressionSyntax Condition { get; init; }
    public required StatementSyntax Body { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Condition, Body];
}

public sealed class RepeatStatement : StatementSyntax
{
    public required IReadOnlyList<StatementSyntax> Statements { get; init; }
    public required ExpressionSyntax Condition { get; init; }
    public override IEnumerable<SyntaxNode> Children => [.. Statements, Condition];
}

public sealed class ForStatement : StatementSyntax
{
    public required ExpressionSyntax Variable { get; init; }
    public required ExpressionSyntax From { get; init; }
    public required bool DownTo { get; init; }
    public required ExpressionSyntax To { get; init; }
    public required StatementSyntax Body { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Variable, From, To, Body];
}

public sealed class ForEachStatement : StatementSyntax
{
    public required ExpressionSyntax Variable { get; init; }
    public required ExpressionSyntax Collection { get; init; }
    public required StatementSyntax Body { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Variable, Collection, Body];
}

public sealed class WithStatement : StatementSyntax
{
    public required ExpressionSyntax Record { get; init; }
    public required StatementSyntax Body { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Record, Body];
}

public sealed class ExitStatement : StatementSyntax
{
    public ExpressionSyntax? Value { get; init; }
    public override IEnumerable<SyntaxNode> Children => Value is null ? [] : [Value];
}

public sealed class AssertErrorStatement : StatementSyntax
{
    public required StatementSyntax Statement { get; init; }
    public override IEnumerable<SyntaxNode> Children => [Statement];
}

/// <summary>The statements of one trigger or procedure body.</summary>
public sealed class CodeBodySyntax
{
    public required IReadOnlyList<StatementSyntax> Statements { get; init; }
    public required IReadOnlyList<CodeToken> Tokens { get; init; }

    public IEnumerable<SyntaxNode> AllNodes() => Statements.SelectMany(s => s.DescendantsAndSelf());
}
