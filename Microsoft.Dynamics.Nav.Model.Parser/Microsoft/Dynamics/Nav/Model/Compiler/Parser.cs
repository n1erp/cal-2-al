using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.Compiler;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Parser
{
	private Token token;

	private readonly Scanner scan;

	private readonly bool ignoreParseExceptions;

	internal ALParseException ALException { get; private set; }

	public Parser(Scanner scan, bool ignoreParseExceptions = false)
	{
		this.scan = scan;
		this.ignoreParseExceptions = ignoreParseExceptions;
	}

	public static AstNode ParseFunctionBody(string[] lines)
	{
		TryParseFunctionBody(null, ignoreParseExceptions: false, lines, out var node, out var _);
		return node;
	}

	public static bool TryParseFunctionBody(CancellationTokenSource token, bool ignoreParseExceptions, string[] lines, out AstNode node, out ALParseException exception)
	{
		exception = null;
		using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(string.Join(Environment.NewLine, lines))))
		{
			Parser parser = new Parser(new Scanner(stream, Encoding.Unicode), ignoreParseExceptions);
			if (!parser.TryParseFunctionBody(token, out node))
			{
				exception = parser.ALException;
			}
		}
		return node != null;
	}

	public AstNode ParseFunctionBody()
	{
		TryParseFunctionBody(null, out var node);
		return node;
	}

	private bool TryParseFunctionBody(CancellationTokenSource cancellationToken, out AstNode node)
	{
		node = null;
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		if (token == Token.t_endofinput)
		{
			return false;
		}
		Block block = new Block(new List<Statement>());
		try
		{
			while (token != Token.t_endofinput && ALException == null)
			{
				if (cancellationToken != null && cancellationToken.IsCancellationRequested)
				{
					cancellationToken.Token.ThrowIfCancellationRequested();
				}
				Statement statement = ParseStatement();
				if (statement != Statement.ErrorStatement)
				{
					block.Stmts.Add(statement);
					if (token == Token.t_semi)
					{
						token = scan.NextToken();
					}
					continue;
				}
				break;
			}
		}
		catch (ALParseException aLException)
		{
			if (!ignoreParseExceptions)
			{
				throw;
			}
			ALException = aLException;
		}
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		block.Position = CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn);
		node = block;
		return ALException == null;
	}

	private void SkipToken(Token skipToken)
	{
		if (token != skipToken)
		{
			if (ALException != null)
			{
				HandleError<Error>("Syntax error, expected: " + skipToken);
			}
		}
		else
		{
			token = scan.NextToken();
		}
	}

	private Statement ParseStatement()
	{
		Statement result = null;
		try
		{
			switch (token)
			{
			case Token.t_begin:
				result = ParseBlock();
				break;
			case Token.t_if:
				result = ParseIf();
				break;
			case Token.t_case:
				result = ParseCase();
				break;
			case Token.t_while:
				result = ParseWhile();
				break;
			case Token.t_with:
				result = ParseWith();
				break;
			case Token.t_for:
				result = ParseFor();
				break;
			case Token.t_foreach:
				result = ParseForeach();
				break;
			case Token.t_repeat:
				result = ParseRepeat();
				break;
			case Token.t_exit:
				result = ParseExit();
				break;
			case Token.t_semi:
			{
				Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
				Tuple<int, int> end = new Tuple<int, int>(currentScanStartLineAndColumn.Item1 + 1, currentScanStartLineAndColumn.Item2 + 1);
				result = new EmptyStatement(CreatePosition(currentScanStartLineAndColumn, end));
				break;
			}
			case Token.t_asserterror:
				result = ParseAssertError();
				break;
			default:
				result = ParseAssign();
				break;
			}
		}
		catch (Exception) when (ALException == null || ignoreParseExceptions)
		{
			HandleError<Error>("Error: expected an AL statement, got: " + scan.TokenVal);
		}
		if (!ignoreParseExceptions)
		{
			return null;
		}
		return result;
	}

	private List<Statement> ParseStatementList(Token toToken)
	{
		List<Statement> list = new List<Statement>();
		while (token != toToken && ALException == null)
		{
			Statement statement = ParseStatement();
			if (statement != null)
			{
				list.Add(statement);
			}
			if (token == Token.t_semi)
			{
				token = scan.NextToken();
			}
		}
		return list;
	}

	private Statement ParseAssign()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		Expression expression = ParseVar();
		Token token = this.token;
		Tuple<int, int> currentScanEndLineAndColumn;
		if ((uint)(token - 19) <= 3u || token == Token.t_assign)
		{
			Token op = this.token;
			this.token = scan.NextToken();
			Expression source = ParseValue();
			currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			return new Assign(expression, op, source, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
		}
		currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new ExpressionStatement(expression, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private If ParseIf()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		Expression ifExpr = ParseValue();
		SkipToken(Token.t_then);
		Statement ifStmt = null;
		Statement elseStmt = null;
		if (token != Token.t_endofinput)
		{
			ifStmt = ParseStatement();
			if (token == Token.t_else)
			{
				token = scan.NextToken();
				elseStmt = ParseStatement();
			}
		}
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new If(ifExpr, ifStmt, elseStmt, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private Case ParseCase()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		Expression caseExpr = ParseValue();
		SkipToken(Token.t_of);
		List<CaseLine> list = new List<CaseLine>();
		while (token != Token.t_end && token != Token.t_else && ALException == null)
		{
			List<Expression> list2 = new List<Expression>();
			while (ALException == null)
			{
				Tuple<int, int> currentScanStartLineAndColumn2 = GetCurrentScanStartLineAndColumn();
				Expression expression = ParseValue();
				if (token == Token.t_range)
				{
					token = scan.NextToken();
					Expression rhs = ParseValue();
					Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
					expression = new Binary(expression, rhs, Token.t_range, CreatePosition(currentScanStartLineAndColumn2, currentScanEndLineAndColumn));
				}
				list2.Add(expression);
				if (token != Token.t_comma)
				{
					break;
				}
				token = scan.NextToken();
			}
			SkipToken(Token.t_colon);
			Statement stmt = ParseStatement();
			list.Add(new CaseLine(list2, stmt));
			if (token != Token.t_semi)
			{
				break;
			}
			token = scan.NextToken();
		}
		List<Statement> elseStmts = null;
		if (token == Token.t_else)
		{
			token = scan.NextToken();
			elseStmts = ParseStatementList(Token.t_end);
		}
		SkipToken(Token.t_end);
		Tuple<int, int> currentScanEndLineAndColumn2 = GetCurrentScanEndLineAndColumn();
		return new Case(caseExpr, list, elseStmts, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn2));
	}

	private Repeat ParseRepeat()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		List<Statement> stmts = ParseStatementList(Token.t_until);
		SkipToken(Token.t_until);
		Expression untilExpr = ParseValue();
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new Repeat(stmts, untilExpr, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private While ParseWhile()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		Expression whileExpr = ParseValue();
		SkipToken(Token.t_do);
		Statement stmt = ParseStatement();
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new While(whileExpr, stmt, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private For ParseFor()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		Expression var = ParseVar();
		SkipToken(Token.t_assign);
		Expression init = ParseValue();
		Token op = token;
		if (token != Token.t_to && token != Token.t_downto)
		{
			return HandleError<For>("Error: expected TO or DOWNTO, got: " + scan.TokenVal);
		}
		token = scan.NextToken();
		Expression to = ParseValue();
		SkipToken(Token.t_do);
		Statement body = ParseStatement();
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new For(var, init, op, to, body, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private Foreach ParseForeach()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		Expression var = ParseVar();
		SkipToken(Token.t_in);
		Expression collection = ParseValue();
		SkipToken(Token.t_do);
		Statement body = ParseStatement();
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new Foreach(var, collection, body, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private Exit ParseExit()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		if (token == Token.t_left)
		{
			token = scan.NextToken();
			Expression exitExpr = ParseValue();
			SkipToken(Token.t_right);
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			return new Exit(exitExpr, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
		}
		Tuple<int, int> currentScanEndLineAndColumn2 = GetCurrentScanEndLineAndColumn();
		return new Exit(null, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn2));
	}

	private Block ParseBlock()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		List<Statement> stmts = ParseStatementList(Token.t_end);
		SkipToken(Token.t_end);
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new Block(stmts, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private With ParseWith()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		Expression withId = ParseVar();
		SkipToken(Token.t_do);
		Statement stmt = ParseStatement();
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new With(withId, stmt, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private Statement ParseAssertError()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		token = scan.NextToken();
		Statement stmt = ParseStatement();
		Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
		return new AssertErrorStatement(stmt, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
	}

	private List<Expression> ParseExpressionList()
	{
		List<Expression> list = new List<Expression>();
		Expression expression = ParseValue();
		if (expression != null)
		{
			list.Add(expression);
			while (token == Token.t_comma && ALException == null)
			{
				token = scan.NextToken();
				list.Add(ParseValue());
			}
			return list;
		}
		return null;
	}

	private Expression ParseValue()
	{
		return ParseExpr();
	}

	private Expression ParseExpr()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		Expression expression = ParseSum();
		while ((token == Token.t_compeq || token == Token.t_complt || token == Token.t_compgt || token == Token.t_comple || token == Token.t_compge || token == Token.t_compne || token == Token.t_in) && ALException == null)
		{
			if (token == Token.t_in)
			{
				token = scan.NextToken();
				SkipToken(Token.t_left2);
				List<Expression> list = new List<Expression>();
				while (ALException == null)
				{
					Tuple<int, int> currentScanStartLineAndColumn2 = GetCurrentScanStartLineAndColumn();
					Expression expression2 = ParseValue();
					if (token == Token.t_range)
					{
						token = scan.NextToken();
						Expression rhs = ParseValue();
						Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
						expression2 = new Binary(expression2, rhs, Token.t_range, CreatePosition(currentScanStartLineAndColumn2, currentScanEndLineAndColumn));
					}
					list.Add(expression2);
					if (token != Token.t_comma)
					{
						break;
					}
					token = scan.NextToken();
				}
				SkipToken(Token.t_right2);
				Tuple<int, int> currentScanEndLineAndColumn2 = GetCurrentScanEndLineAndColumn();
				expression = new InList(expression, list, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn2));
			}
			else
			{
				Token op = token;
				token = scan.NextToken();
				Expression expression3 = ParseSum();
				expression = new Binary(expression, expression3, op, CreatePosition(currentScanStartLineAndColumn, new Tuple<int, int>(expression3.Position.EndLine, expression3.Position.EndCol)));
			}
			currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		}
		return expression;
	}

	private Expression ParseSum()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		Expression expression = ParseProd();
		while ((token == Token.t_plus || token == Token.t_minus || token == Token.t_or || token == Token.t_xor) && ALException == null)
		{
			Token op = token;
			token = scan.NextToken();
			Expression expression2 = ParseProd();
			expression = new Binary(expression, expression2, op, CreatePosition(currentScanStartLineAndColumn, new Tuple<int, int>(expression2.Position.EndLine, expression2.Position.EndCol)));
			currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		}
		return expression;
	}

	private Expression ParseProd()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		Expression expression = ParseFactor();
		while ((token == Token.t_mult || token == Token.t_idiv || token == Token.t_rdiv || token == Token.t_mod || token == Token.t_and) && ALException == null)
		{
			Token op = token;
			token = scan.NextToken();
			Expression expression2 = ParseFactor();
			expression = new Binary(expression, expression2, op, CreatePosition(currentScanStartLineAndColumn, new Tuple<int, int>(expression2.Position.EndLine, expression2.Position.EndCol)));
			currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		}
		return expression;
	}

	private Expression ParseFactor()
	{
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		Expression result;
		switch (token)
		{
		case Token.t_plus:
		case Token.t_minus:
		case Token.t_not:
		{
			Token op = token;
			token = scan.NextToken();
			result = ParseFactor();
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new Unary(op, result, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			break;
		}
		case Token.t_id:
			result = ParseVar();
			break;
		case Token.t_left:
		{
			token = scan.NextToken();
			result = ParseExpr();
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			SkipToken(Token.t_right);
			result = new ParenthesesExpr(result, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			break;
		}
		case Token.t_false:
		case Token.t_true:
		{
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new LiteralBool(token == Token.t_true, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		case Token.t_s32:
		{
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new LiteralInt32(int.Parse(scan.TokenVal, CultureInfo.InvariantCulture), CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		case Token.t_s64:
		{
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			string tokenVal2 = scan.TokenVal;
			result = new LiteralInt64(long.Parse(tokenVal2.Substring(0, tokenVal2.Length - 1), CultureInfo.InvariantCulture), CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		case Token.t_date:
		{
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new LiteralDate(DateTime.MinValue, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		case Token.t_time:
		{
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new LiteralTime(DateTime.MinValue, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		case Token.t_datetime:
		{
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new LiteralDateTime(DateTime.MinValue, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		case Token.t_string:
		{
			string tokenVal = scan.TokenVal;
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new LiteralText(tokenVal, null, CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		case Token.t_bcd:
		{
			Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
			result = new LiteralDecimal(decimal.Parse(scan.TokenVal, CultureInfo.InvariantCulture), CreatePosition(currentScanStartLineAndColumn, currentScanEndLineAndColumn));
			token = scan.NextToken();
			break;
		}
		default:
			return HandleError<Expression>(string.Format(CultureInfo.InvariantCulture, "Unspecified token : '{0}'", token));
		}
		return result;
	}

	private Expression ParseVar()
	{
		if (!Scanner.IsTokenIdentifier(token))
		{
			return HandleError<Expression>("Error: variable expected, got: " + scan.TokenVal);
		}
		Tuple<int, int> currentScanStartLineAndColumn = GetCurrentScanStartLineAndColumn();
		TextPosition position = CreatePosition(currentScanStartLineAndColumn, new Tuple<int, int>(currentScanStartLineAndColumn.Item1, currentScanStartLineAndColumn.Item2 + scan.Length));
		Expression expression = new Identifier(scan.TokenVal, null, position);
		token = scan.NextToken();
		if (token == Token.t_left)
		{
			List<Expression> arguments = null;
			token = scan.NextToken();
			if (token != Token.t_right)
			{
				arguments = ParseExpressionList();
			}
			position = CreatePosition(expression.Position);
			SkipToken(Token.t_right);
			expression = new MethodCall(expression, arguments, null, position);
		}
		while (ALException == null)
		{
			switch (token)
			{
			case Token.t_period:
			{
				token = scan.NextToken();
				Tuple<int, int> currentScanStartLineAndColumn3 = GetCurrentScanStartLineAndColumn();
				if (!Scanner.IsTokenIdentifier(token))
				{
					return HandleError<Expression>("Error: variable expected, got: " + scan.TokenVal);
				}
				Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
				Expression member = new Identifier(scan.TokenVal, null, CreatePosition(currentScanStartLineAndColumn3, currentScanEndLineAndColumn));
				position = new TextPosition(expression.Position.StartLine, expression.Position.StartCol, currentScanEndLineAndColumn.Item1, currentScanEndLineAndColumn.Item2);
				expression = new MemberAccess(expression, member, position);
				token = scan.NextToken();
				if (token == Token.t_left)
				{
					List<Expression> arguments2 = null;
					token = scan.NextToken();
					if (token != Token.t_right)
					{
						arguments2 = ParseExpressionList();
					}
					position = CreatePosition(expression.Position);
					SkipToken(Token.t_right);
					expression = new MethodCall(expression, arguments2, null, position);
				}
				break;
			}
			case Token.t_left2:
			{
				token = scan.NextToken();
				List<Expression> indices = ParseExpressionList();
				position = CreatePosition(expression.Position);
				SkipToken(Token.t_right2);
				expression = new ArrayIndex(expression, indices, position);
				break;
			}
			case Token.t_strsetval:
			{
				token = scan.NextToken();
				Tuple<int, int> currentScanStartLineAndColumn2 = GetCurrentScanStartLineAndColumn();
				if (!Scanner.IsTokenIdentifier(token))
				{
					return HandleError<Expression>("Error: variable expected, got: " + scan.TokenVal);
				}
				Tuple<int, int> currentScanEndLineAndColumn = GetCurrentScanEndLineAndColumn();
				Expression ident = new Identifier(scan.TokenVal, null, CreatePosition(currentScanStartLineAndColumn2, currentScanEndLineAndColumn));
				position = new TextPosition(expression.Position.StartLine, expression.Position.StartCol, currentScanEndLineAndColumn.Item1, currentScanEndLineAndColumn.Item2);
				expression = new OptionAccess(expression, ident, position);
				token = scan.NextToken();
				return expression;
			}
			case Token.t_left:
				return HandleError<Expression>("Error: " + scan.TokenVal + "is not a function");
			default:
				return expression;
			}
		}
		return Expression.ErrorExpression;
	}

	private T HandleError<T>(string message) where T : AstNode, new()
	{
		if (ALException == null)
		{
			ALException = new ALParseException(scan.LineNo, scan.LinePos, message);
		}
		if (!ignoreParseExceptions)
		{
			throw ALException;
		}
		TextPosition position = new TextPosition(scan.LineNo, scan.LinePos, scan.LineNo, scan.LinePos + 1);
		return new T
		{
			Position = position
		};
	}

	private Tuple<int, int> GetCurrentScanStartLineAndColumn()
	{
		return new Tuple<int, int>(scan.LineNo, scan.LinePos - scan.Length);
	}

	private Tuple<int, int> GetCurrentScanEndLineAndColumn()
	{
		return new Tuple<int, int>(scan.LineNo, scan.LinePos);
	}

	private TextPosition CreatePosition(TextPosition startPosition)
	{
		return new TextPosition(startPosition.StartLine, startPosition.StartCol, scan.LineNo, scan.LinePos);
	}

	private static TextPosition CreatePosition(Tuple<int, int> start, Tuple<int, int> end)
	{
		return new TextPosition(start.Item1, start.Item2, end.Item1, end.Item2);
	}
}
