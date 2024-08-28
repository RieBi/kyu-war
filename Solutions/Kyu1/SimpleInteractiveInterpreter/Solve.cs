namespace Solutions.Kyu1.SimpleInteractiveInterpreter;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Interpreter
{
    private readonly Parser _parser;

    public Interpreter()
    {
        _parser = new Parser();
    }

    public double? input(string input)
    {
        var tokens = tokenize(input);
        var result = _parser.CalculateResult(tokens);

        return double.IsNaN(result) ? null : result;
    }

    private List<string> tokenize(string input)
    {
        var tokens = new List<string>();
        var rgxMain = new Regex(@"=>|[-+*/%=\\(\\)]|[A-Za-z_][A-Za-z0-9_]*|[0-9]+(\.?[0-9]+)?");
        MatchCollection matches = rgxMain.Matches(input);
        foreach (Match m in matches) tokens.Add(m.Groups[0].Value);
        return tokens;
    }
}

public class Parser
{
    private List<string> _tokens = new();
    private int _pos = 0;

    private readonly HashSet<string> multiplicationOperators = new() { "*", "/", "%" };
    private readonly HashSet<string> additionOperators = new() { "+", "-" };

    private readonly State state = new();

    public double CalculateResult(List<string> tokens)
    {
        _tokens = tokens;
        _pos = 0;

        var result = ParseAnyExpression();

        if (_pos != _tokens.Count)
            throw new InvalidOperationException("Unused trailing tokens detected");

        return result.GetResult(state);
    }

    private IParserNode ParseParentheses()
    {
        var node = ParseAnyExpression();

        if (At() != ")")
            throw new InvalidOperationException("Unmatched parenthese found");

        Next();

        return node;
    }

    private IParserNode ParseIdentifier(string token)
    {
        if (state.funcs.TryGetValue(token, out var func))
        {
            var paramsCount = func.Parameters.Count;

            var arguments = new List<IParserNode>();
            for (int i = 0; i < paramsCount; i++)
                arguments.Add(ParseAssignmentExpression());

            var funcInvocation = new FunctionInvocationNode(token, arguments);
            return funcInvocation;
        }
        else
            return new IdentifierNode(token);
    }

    private IParserNode ParseConstantExpression()
    {
        var token = Next();

        if (double.TryParse(token, out var result))
        {
            return new ValueNode() { Value = result };
        }
        else if (token == "(")
        {
            return ParseParentheses();
        }
        else
        {
            return ParseIdentifier(token);
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

    private IParserNode ParseFunctionDeclaration()
    {
        if (At() != "fn")
            return ParseAssignmentExpression();

        Next();

        var funcName = Next();
        var parameters = new List<string>();

        while (At() != "=>")
            parameters.Add(Next());

        if (Next() != "=>")
            throw new InvalidOperationException("Function declaration missed => operator");

        var funcBody = ParseAssignmentExpression();

        var funcNode = new FunctionDeclarationNode(funcName, parameters, funcBody);
        return funcNode;
    }

    private IParserNode ParseAnyExpression() => ParseFunctionDeclaration();

    private string At() => _pos < _tokens.Count ? _tokens[_pos] : string.Empty;

    private string Next() => _pos < _tokens.Count ? _tokens[_pos++] : string.Empty;
}

public class State
{
    internal Dictionary<string, double> values = new();
    internal Dictionary<string, FunctionDeclarationNode> funcs = new();

    public void AssignVariable(string identifier, double value)
    {
        if (funcs.ContainsKey(identifier))
            throw new InvalidOperationException("Cannot assign variable: function already exists");

        values[identifier] = value;
    }

    public void AssignFunction(FunctionDeclarationNode function)
    {
        if (values.ContainsKey(function.Identifier))
            throw new InvalidOperationException("Cannot assign function: variable already exists");

        funcs[function.Identifier] = function;
    }

    public double GetVariableValue(string identifier)
    {
        if (!values.TryGetValue(identifier, out var value))
            throw new InvalidOperationException("Variable is not declared");

        return value;
    }

    public double GetMethodResult(string identifier, List<double> arguments)
    {
        if (!funcs.TryGetValue(identifier, out var value))
            throw new InvalidOperationException("Function is not declared");

        var localState = new State
        {
            funcs = funcs
        };

        for (int i = 0; i < value.Parameters.Count; i++)
            localState.AssignVariable(value.Parameters[i], arguments[i]);

        return value.BodyNode.GetResult(localState);
    }
}

public interface IParserNode
{
    double GetResult(State state);
}

public class ValueNode : IParserNode
{
    public double Value { get; set; }

    public double GetResult() => Value;

    public double GetResult(State state) => GetResult();
}

public class IdentifierNode : IParserNode
{
    public IdentifierNode(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; set; }

    public double GetResult(State state) => state.GetVariableValue(Identifier);
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

    public double GetResult(State state)
    {
        var leftValue = LeftChild.GetResult(state);
        var rightValue = RightChild.GetResult(state);

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

    public double GetResult(State state)
    {
        var rightResult = RightHandSide.GetResult(state);
        state.AssignVariable(Identifier.Identifier, rightResult);

        return rightResult;
    }
}

public class FunctionDeclarationNode : IParserNode
{
    public FunctionDeclarationNode(string identifier, List<string> parameters, IParserNode bodyNode)
    {
        Identifier = identifier;
        Parameters = parameters;
        BodyNode = bodyNode;
    }

    public string Identifier { get; set; }
    public List<string> Parameters { get; set; }
    public IParserNode BodyNode { get; set; }

    public double GetResult(State state)
    {
        state.AssignFunction(this);

        return double.NaN;
    }
}

public class FunctionInvocationNode : IParserNode
{
    public string FunctionIdentifier { get; set; }
    public List<IParserNode> Arguments { get; set; }

    public FunctionInvocationNode(string functionIdentifier, List<IParserNode> arguments)
    {
        FunctionIdentifier = functionIdentifier;
        Arguments = arguments;
    }

    public double GetResult(State state)
    {
        var processedArguments = Arguments.Select(f => f.GetResult(state)).ToList();
        return state.GetMethodResult(FunctionIdentifier, processedArguments);
    }
}