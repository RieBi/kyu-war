namespace InterpreterKata;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class Interpreter
{
    public double? input(string input)
    {
        var tokens = tokenize(input);
        var parser = new Parser(tokens);
        return parser.CalculateResult();
    }

    private List<string> tokenize(string input)
    {
        var tokens = new List<string>();
        var rgxMain = MyRegex();
        MatchCollection matches = rgxMain.Matches(input);
        foreach (Match m in matches) tokens.Add(m.Groups[0].Value);
        return tokens;
    }

    [GeneratedRegex(@"=>|[-+*/%=\\(\\)]|[A-Za-z_][A-Za-z0-9_]*|[0-9]+(\.?[0-9]+)?")]
    private static partial Regex MyRegex();
}

public class Parser
{
    private readonly List<string> _tokens;
    private int _pos = 0;

    private readonly HashSet<string> multiplicationOperators = new() { "*", "/", "%" };
    private readonly HashSet<string> additionOperators = new() { "+", "-" };

    public Parser(List<string> tokens)
    {
        _tokens = tokens;
    }

    public double CalculateResult()
    {
        _pos = 0;
        var result = ParseAnyExpression();
        return result.GetResult();
    }

    private IParserNode ParseConstantExpression()
    {
        var token = Next();

        if (double.TryParse(token, out var result))
        {
            return new ValueNode() { Value = result };
        }
        else
        {
            return new IdentifierNode(token);
        }
    }

    private IParserNode ParseMultiplicationBinaryExpression()
    {
        IParserNode left = ParseConstantExpression();
        
        while (multiplicationOperators.Contains(At()))
        {
            var op = Next();
            var right = ParseConstantExpression();

            left = new BinaryExpressionNode(left, right, op);
        }

        return left;
    }

    private IParserNode ParseAdditionBinaryExpression()
    {
        var left = ParseMultiplicationBinaryExpression();

        while (additionOperators.Contains(At()))
        {
            var op = Next();
            var right = ParseMultiplicationBinaryExpression();

            left = new BinaryExpressionNode(left, right, op);
        }

        return left;
    }

    private IParserNode ParseAssignmentExpression()
    {
        var left = ParseAdditionBinaryExpression();

        if (At() == "=" && left is IdentifierNode identifierLeft)
        {
            Next();

            var right = ParseAssignmentExpression();

            left = new AssignmentNode(identifierLeft, right);
        }

        return left;
    }

    private IParserNode ParseAnyExpression() => ParseAssignmentExpression();

    private bool IsNumber(string token, out double result) => double.TryParse(token, out result);

    private string At() => _pos < _tokens.Count ? _tokens[_pos] : string.Empty;

    private string Next() => _pos < _tokens.Count ? _tokens[_pos++] : string.Empty;
}

public interface IParserNode
{
    double GetResult();
}

public class ValueNode : IParserNode
{
    public double Value { get; set; }

    public double GetResult() => Value;
}

public class IdentifierNode : IParserNode
{
    public IdentifierNode(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; set; }

    public double GetResult() => throw new NotImplementedException();
}

public class BinaryExpressionNode : IParserNode
{
    public BinaryExpressionNode(IParserNode leftChild, IParserNode rightChild, string binaryOperator)
    {
        LeftChild = leftChild;
        RightChild = rightChild;
        Operator = binaryOperator;
    }

    public IParserNode LeftChild { get; set; }
    public IParserNode RightChild { get; set; }
    public string Operator { get; set; }

    public double GetResult()
    {
        var leftValue = LeftChild.GetResult();
        var rightValue = RightChild.GetResult();

        var result = Operator switch
        {
            "+" => leftValue + rightValue,
            "-" => leftValue - rightValue,
            "*" => leftValue * rightValue,
            "/" => leftValue / rightValue,
            "%" => leftValue % rightValue,
            _ => double.NaN
        };

        return result;
    }
}

public class AssignmentNode : IParserNode
{
    public AssignmentNode(IdentifierNode identifier, IParserNode rightHandSide)
    {
        Identifier = identifier;
        RightHandSide = rightHandSide;
    }

    public IdentifierNode Identifier { get; set; }
    public IParserNode RightHandSide { get; set; }

    public double GetResult() => RightHandSide.GetResult();
}
