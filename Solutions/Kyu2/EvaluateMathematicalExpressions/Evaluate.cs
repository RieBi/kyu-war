namespace Evaluation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

public class Evaluate
{
    public string eval(string expression)
    {
        try
        {
            var tokenizer = new Tokenizer(expression);
            var tokens = tokenizer.Tokenize();

            var evaluator = new Evaluator(tokens);
            var result = evaluator.Evaluate();

            return result;
        }
        catch (Exception exc)
        {
            return $"ERROR: {exc.Message}";
        }
    }
}

public enum TokenKind
{
    BinaryOperator,
    Number,
    Function,
    OpeningParenthese,
    ClosingParenthese
}

public record Token
{
    public TokenKind Kind { get; }
    public string Value { get; }

    public Token(TokenKind kind, string value)
    {
        this.Kind = kind;
        this.Value = value;
    }
}

public record TokenNumber : Token
{
    public double Number { get; }

    public TokenNumber(TokenKind kind, string value) : base(kind, value)
    {
        this.Number = double.Parse(value);
    }
}

public record TokenParenthese : Token
{
    public LinkedListNode<Token> Matched { get; }

    public TokenParenthese(TokenKind kind, string value, LinkedListNode<Token> matched) : base(kind, value)
    {
        this.Matched = matched;
    }
}

public class Tokenizer
{
    private static readonly ImmutableHashSet<char> numberSign = ImmutableHashSet.Create('+', '-' );
    private static readonly ImmutableHashSet<char> binaryOperator = ImmutableHashSet.Create('+', '-', '*', '/', '&');
    private static readonly ImmutableHashSet<char> scientificNotationSeparator = ImmutableHashSet.Create('e', 'E');
    private static readonly ImmutableHashSet<string> function =
        ImmutableHashSet.Create("log", "ln", "exp", "sqrt", "abs", "atan", "acos", "asin", "sinh", "cosh", "tanh", "tan", "sin", "cos");

    const char decimalSeparator = '.';
    const char openingParenthese = '(';
    const char closingParenthese = ')';

    private string _source;
    private int _pos = 0;
    private readonly List<Token> _tokens = new();

    public Tokenizer(string source)
    {
        this._source = source;
    }

    public List<Token> Tokenize()
    {
        while (!IsEof())
        {
            if (IsSkippable(_source[_pos]))
            {
                _pos++;
                continue;
            }

            Token? current = ParseNumber();
            current ??= ParseParentheses();
            current ??= ParseBinaryOperator();
            current ??= ParseFunction();

            if (current is null)
                throw new InvalidOperationException("Unknown token detected");
            else
                _tokens.Add(current);
        }

        return _tokens;
    }

    public Token? ParseNumber()
    {
        if (!IsDigit(At()))
            return null;

        var start = _pos;
        var pos = _pos;
        while (IsDigit(At(pos)))
            pos++;

        var result = new Token(TokenKind.Number, _source[start..pos]);
        _pos = pos;

        if (At(pos) == decimalSeparator)
        {
            pos++;

            if (!IsDigit(At(pos)))
                return result;

            while (IsDigit(At(pos)))
                pos++;

            result = new Token(TokenKind.Number, _source[start..pos]);
            _pos = pos;
        }

        if (!scientificNotationSeparator.Contains(At(pos)))
            return result;

        pos++;

        if (numberSign.Contains(At(pos)))
            pos++;

        if (!IsDigit(At(pos)))
            return result;

        while (IsDigit(At(pos)))
            pos++;

        result = new Token(TokenKind.Number, _source[start..pos]);
        _pos = pos;
        return result;
    }

    public Token? ParseParentheses()
    {
        if (At() == openingParenthese)
        {
            _pos++;
            return new(TokenKind.OpeningParenthese, openingParenthese.ToString());
        }
        else if (At() == closingParenthese)
        {
            _pos++;
            return new(TokenKind.ClosingParenthese, closingParenthese.ToString());
        }

        return null;
    }

    public Token? ParseBinaryOperator()
    {
        if (binaryOperator.Contains(At()))
        {
            var result = new Token(TokenKind.BinaryOperator, At().ToString());
            _pos++;
            return result;
        }

        return null;
    }

    public Token? ParseFunction()
    {
        var start = _pos;
        var maxLength = function.Max(f => f.Length);

        var pos = _pos;
        for (int i = 0; i < maxLength; i++)
        {
            if (!char.IsLetter(At(pos)))
                return null;

            pos++;
            var substr = _source[start..pos].ToLowerInvariant();
            if (function.Contains(substr))
            {
                _pos = pos;
                return new(TokenKind.Function, substr);
            }
        }

        return null;
    }

    private char At() => _pos >= _source.Length ? char.MinValue : _source[_pos];

    private char At(int index) => index >= _source.Length ? char.MinValue : _source[index];

    private bool IsEof() => _pos >= _source.Length;

    private bool IsEof(int index) => index >= _source.Length;

    private static bool IsDigit(char ch) => char.IsDigit(ch);

    private static bool IsSkippable(char ch)
    {
        ImmutableHashSet<char> skippable = ImmutableHashSet.Create(' ', '\n', '\r', '\t');

        return skippable.Contains(ch);
    }
}

public class Evaluator
{
    List<Token> _tokens;

    public Evaluator(List<Token> tokens)
    {
        this._tokens = tokens;
    }

    public string Evaluate()
    {
        var linkedList = new LinkedList<Token>(_tokens);
        MatchParentheses(linkedList);
        NumberizeNumbers(linkedList);

        var startParenthese = new Token(TokenKind.OpeningParenthese, "(");
        var endParenthese = new Token(TokenKind.ClosingParenthese, ")");

        linkedList.AddFirst(startParenthese);
        linkedList.AddLast(endParenthese);

        CalculateParentheses(linkedList.First!);
        return linkedList.First!.Value.Value;
    }

    private void CalculateParentheses(LinkedListNode<Token> node)
    {
        if (node.Value.Kind != TokenKind.OpeningParenthese)
            return;

        var start = node;
        node = node.Next!;

        while (node.Value.Kind != TokenKind.ClosingParenthese)
        {
            var list = node.List!;
            var prev = node.Previous;

            var token = node.Value;
            if (token.Kind == TokenKind.BinaryOperator)
                node = CalculateExpression(node, 0)!;
            else if (token.Kind == TokenKind.OpeningParenthese)
                CalculateParentheses(node);
            else
                CalculateValue(node);

            if (node.List is null)
                node = prev ?? list.First!;

            node = node.Next!;
        }

        var linkedList = start.List!;
        var actualValue = node.Previous!;
        linkedList.Remove(node);

        start.Value = actualValue.Value;
        linkedList.Remove(actualValue);
    }

    private void CalculateValue(LinkedListNode<Token> node)
    {
        var token = node.Value;
        if (token.Kind == TokenKind.Number)
            return;

        CalculateParentheses(node);
        CalculateUnaryMinuses(node);

        if (token.Kind != TokenKind.Function)
            return;

        var functionStart = node.Next!;

        CalculateParentheses(functionStart);
        var numToken = (functionStart.Value as TokenNumber)!;
        var numValue = numToken.Number;

        Func<double, double> function = token.Value switch
        {
            "log" => Math.Log10,
            "ln" => Math.Log,
            "exp" => Math.Exp,
            "sqrt" => Math.Sqrt,
            "abs" => Math.Abs,
            "atan" => Math.Atan,
            "acos" => Math.Acos,
            "asin" => Math.Asin,
            "sinh" => Math.Sinh,
            "cosh" => Math.Cosh,
            "tanh" => Math.Tanh,
            "tan" => Math.Tan,
            "sin" => Math.Sin,
            "cos" => Math.Cos,
            _ => throw new InvalidOperationException("Function not supported")
        };

        numValue = function(numValue);

        ValidateNumericValue(numValue);

        node.Value = new TokenNumber(TokenKind.Number, numValue.ToString());

        node.List!.Remove(functionStart);
    }

    private LinkedListNode<Token>? CalculateExpression(LinkedListNode<Token>? node, int prevPriority)
    {
        if (node is null || node.Value.Kind == TokenKind.ClosingParenthese)
            return null;

        CalculateUnaryMinuses(node);
        CalculateDuplicateMinuses(node);

        var priority = GetOperatorPriority(node.Value.Value);

        if (priority < 3 && priority <= prevPriority)
            return null;

        while (node is not null && GetOperatorPriority(node.Value.Value) == priority)
        {
            var rightPart = node.Next!;
            CalculateValue(rightPart);
            rightPart = node.Next!;
            CalculateExpression(rightPart.Next, priority);

            rightPart = node.Next!;
            var leftPart = node.Previous!;

            var leftToken = leftPart.Value as TokenNumber
                ?? throw new InvalidOperationException("Cannot perform binary operation on null value");
            var rightToken = rightPart.Value as TokenNumber
                ?? throw new InvalidOperationException("Cannot perform binary operation on null value");


            var resultVal = node.Value.Value switch
            {
                "+" => leftToken.Number + rightToken.Number,
                "-" => leftToken.Number - rightToken.Number,
                "*" => leftToken.Number * rightToken.Number,
                "/" => leftToken.Number / rightToken.Number,
                "&" => Math.Pow(leftToken.Number, rightToken.Number),
                _ => throw new InvalidOperationException("Invalid binary operator")
            };

            ValidateNumericValue(resultVal);

            var resultToken = new TokenNumber(TokenKind.Number, resultVal.ToString());

            node.Value = resultToken;

            var linkedList = node.List!;
            linkedList.Remove(leftPart);
            linkedList.Remove(rightPart);

            node = node.Next;
        }

        return node?.Previous!;
    }

    private void CalculateUnaryMinuses(LinkedListNode<Token> node)
    {
        var prev = node.Previous;

        if (node.Value.Value != "-" || prev?.Value.Kind == TokenKind.Number)
            return;

        var list = node.List!;
        var count = 1;

        while (node.Next?.Value.Value == "-")
        {
            list.Remove(node.Next);
            count++;
        }

        var tokenNum = count % 2 == 0 ? "1" : "-1";
        var prevToken = new TokenNumber(TokenKind.Number, tokenNum);
        var curToken = new Token(TokenKind.BinaryOperator, "*");

        node.Value = curToken;
        list.AddBefore(node, prevToken);
    }

    private void CalculateDuplicateMinuses(LinkedListNode<Token> node)
    {
        var prev = node.Previous;
        var next = node.Next;

        if (node.Value.Value != "-" || prev?.Value.Kind != TokenKind.Number || next?.Value.Value != "-")
            return;

        var list = node.List!;
        var count = 1;

        while (node.Next?.Value.Value == "-")
        {
            list.Remove(node.Next);
            count++;
        }

        if (count % 2 == 0)
            node.Value = new(TokenKind.BinaryOperator, "+");
    }

    private int GetOperatorPriority(string op) => op switch
    {
        "+" or "-" => 1,
        "*" or "/" => 2,
        "&" => 3,
        _ => 10
    };

    private void ValidateNumericValue(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("Resulting number is not finite");
    }

    private void MatchParentheses(LinkedList<Token> linkedList)
    {
        var cur = linkedList.First;
        var stack = new Stack<LinkedListNode<Token>>();

        while (cur != null)
        {
            var token = cur.Value;
            if (token.Value == "(")
                stack.Push(cur);

            if (token.Value == ")")
            {
                if (stack.Count == 0)
                    throw new InvalidOperationException("Unmatched parentheses detected");

                var matchedStart = stack.Pop();
                var startToken = matchedStart.Value;

                var newStart = new LinkedListNode<Token>(null!);
                var newEnd = new LinkedListNode<Token>(null!);

                newStart.Value = new TokenParenthese(TokenKind.OpeningParenthese, "(", newEnd);
                newEnd.Value = new TokenParenthese(TokenKind.ClosingParenthese, ")", newStart);

                var midStart = matchedStart.Next!;
                linkedList.Remove(matchedStart);
                linkedList.AddBefore(midStart, newStart);

                var midEnd = cur.Previous!;
                linkedList.Remove(cur);
                linkedList.AddAfter(midEnd, newEnd);

                cur = newEnd;
            }

            cur = cur.Next;
        }
    }

    private void NumberizeNumbers(LinkedList<Token> linkedList)
    {
        var cur = linkedList.First;

        while (cur is not null)
        {
            if (cur.Value.Kind == TokenKind.Number)
            {
                var val = cur.Value.Value;

                var newToken = new TokenNumber(TokenKind.Number, val);

                if (cur.Next is not null)
                {
                    var next = cur.Next;
                    linkedList.Remove(cur);
                    cur = linkedList.AddBefore(next, newToken);
                }
                else
                {
                    var newLast = linkedList.AddLast(newToken);
                    linkedList.Remove(cur);
                    cur = newLast;
                }
            }

            cur = cur.Next;
        }
    }
}