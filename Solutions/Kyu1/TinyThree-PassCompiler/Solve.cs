namespace Solutions.Kyu1.TinyThree_PassCompiler;

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Compiler
{
    public Ast pass1(string prog)
    {
        List<string> tokens = tokenize(prog);

        var parser = new Parser(tokens);
        return parser.Parse();
    }

    public Ast pass2(Ast ast)
    {
        return ast.Simplify();
    }


    public List<string> pass3(Ast ast)
    {
        return null;
    }

    private List<string> tokenize(string input)
    {
        List<string> tokens = new List<string>();
        Regex rgxMain = new Regex(@"\[|\]|[-+*/=()]|[A-Za-z_][A-Za-z0-9_]*|[0-9]*(?:\.?[0-9]+)");
        MatchCollection matches = rgxMain.Matches(input);
        foreach (Match m in matches) tokens.Add(m.Groups[0].Value);
        return tokens;
    }
}

public class Parser
{
    private int pos = 0;
    private readonly List<string> tokens;
    private Dictionary<string, int> parameters;
    private int paramCount;

    public Parser(List<string> tokens)
    {
        this.tokens = tokens;
    }

    public Ast Parse()
    {
        pos = 0;
        parameters = new();
        paramCount = 0;

        return ParseFunctionDeclaration();
    }

    public Ast ParseBasicExpression()
    {
        var token = At();

        if (token == "(")
        {
            Expect("(");
            var expression = ParseAdditionExpression();
            Expect(")");

            return expression;
        }
        else if (int.TryParse(token, out var number))
        {
            return new ImmOp(number);
        }
        else
        {
            if (parameters.TryGetValue(token, out var argument))
            {
                return new ArgOp(argument);
            }

            throw new InvalidOperationException($"Parameter '{token}' does not exist.");
        }
    }

    public Ast ParseMultiplicationExpression()
    {
        var left = ParseBasicExpression();

        while (At() == "*" || At() == "/")
        {
            var op = Next();

            var right = ParseMultiplicationExpression();
            left = new BinOp(op, left, right);
        }

        return left;
    }

    public Ast ParseAdditionExpression()
    {
        var left = ParseMultiplicationExpression();

        while (At() == "+" || At() == "-")
        {
            var op = Next();

            var right = ParseAdditionExpression();
            left = new BinOp(op, left, right);
        }

        return left;
    }

    public Ast ParseFunctionDeclaration()
    {
        Expect("[");

        while (At() != "]")
        {
            parameters[Next()] = paramCount++;
        }

        Expect("]");

        return ParseAdditionExpression();
    }

    private string At() => pos < tokens.Count ? tokens[pos] : string.Empty;

    private string Next() => pos < tokens.Count ? tokens[pos++] : string.Empty;

    private void Expect(string token)
    {
        var next = Next();
        if (next != token)
            throw new InvalidOperationException($"Expected '{token}', got: '{next}'.");
    }
}

public class Ast
{
    public string Operation { get; set; }

    public Ast(string operation)
    {
        Operation = operation;
    }

    public string op() => Operation;

    public virtual Ast Simplify() => this;
}

public class BinOp : Ast
{
    public Ast LeftChild { get; set; }
    public Ast RightChild { get; set; }

    public BinOp(string operation, Ast leftChild, Ast rightChild) : base(operation)
    {
        LeftChild = leftChild;
        RightChild = rightChild;
    }

    public Ast a() => LeftChild;

    public Ast b() => RightChild;

    public override Ast Simplify()
    {
        LeftChild = LeftChild.Simplify();
        RightChild = RightChild.Simplify();

        if (LeftChild is ImmOp left && RightChild is ImmOp right)
        {
            var result = Operation switch
            {
                "+" => left.Value + right.Value,
                "-" => left.Value - right.Value,
                "*" => left.Value * right.Value,
                "/" => left.Value / right.Value,
                _ => throw new InvalidOperationException($"Can't simplify unknown operation '{Operation}'")
            };

            var newNode = new ImmOp(result);
            return newNode;
        }

        return this;
    }
}

public abstract class UnOp : Ast
{
    protected UnOp(string operation) : base(operation) { }

    public abstract int n();
}

public class ArgOp : UnOp
{
    public int ArgumentIndex { get; set; }

    public ArgOp(int argumentIndex) : base("arg")
    {
        ArgumentIndex = argumentIndex;
    }

    public override int n() => ArgumentIndex;
}

public class ImmOp : UnOp
{
    public int Value { get; set; }

    public ImmOp(int value) : base("imm")
    {
        Value = value;
    }

    public override int n() => Value;
}
