namespace InterpreterKata;

using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class Interpreter
{
    public double? input(string input)
    {
        var tokens = tokenize(input);
        return null;
    }

    private List<string> tokenize(string input)
    {
        var tokens = new List<string>();
        var rgxMain = MyRegex();
        MatchCollection matches = rgxMain.Matches(input);
        foreach (Match m in matches) tokens.Add(m.Groups[0].Value);
        return tokens;
    }

    [GeneratedRegex(@"=>|[-+*/%=\\(\\)]|[A-Za-z_][A-Za-z0-9_]*|[0-9]+\.?[0-9]+")]
    private static partial Regex MyRegex();
}

public class Parser
{
    private readonly List<string> _tokens;
    private int _pos = 0;

    public Parser(List<string> tokens)
    {
        _tokens = tokens;
    }

    public void ParseExpression()
    {
        var cur = _tokens[_pos++];

        IParserNode curNode;

        if (IsNumber(cur, out var value))
        {
            var node = new ValueNode();
            node.Value = value;
            curNode = node;
        }
    }

    private bool IsNumber(string token, out double result) => double.TryParse(token, out result);

    private string At() => _tokens[_pos];

    private string Next() => _tokens[_pos++];
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
    public AssignmentNode(string identifier, IParserNode rightHandSide)
    {
        Identifier = identifier;
        RightHandSide = rightHandSide;
    }

    public string Identifier { get; set; }
    public IParserNode RightHandSide { get; set; }

    public double GetResult() => RightHandSide.GetResult();
}
