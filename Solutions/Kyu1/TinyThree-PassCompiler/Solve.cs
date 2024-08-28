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
        return ast.Compile();
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
            Next();
            return new UnOp("imm", number);
        }
        else
        {
            if (parameters.TryGetValue(token, out var argument))
            {
                Next();
                return new UnOp("arg", argument);
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

public static class ExtensionMethods
{
    public static Ast Simplify(this Ast node)
    {
        if (node is UnOp)
            return node;

        var binOp = node as BinOp ?? throw new InvalidOperationException();

        var LeftChild = binOp.a().Simplify();
        var RightChild = binOp.b().Simplify();

        if (LeftChild is UnOp left && RightChild is UnOp right && left.op() == "imm" && right.op() == "imm")
        {
            var result = binOp.op() switch
            {
                "+" => left.n() + right.n(),
                "-" => left.n() - right.n(),
                "*" => left.n() * right.n(),
                "/" => left.n() / right.n(),
                _ => throw new InvalidOperationException($"Can't simplify unknown operation '{binOp.op()}'")
            };

            var newNode = new UnOp("imm", result);
            return newNode;
        }

        return binOp;
    }

    public static List<string> Compile(this Ast node)
    {
        if (node is UnOp unOp)
            return new() { $"{(unOp.op() == "arg" ? "AR" : "IM")} {unOp.n()}" };

        var binOp = node as BinOp ?? throw new InvalidOperationException();

        var result = new List<string>();

        result.AddRange(binOp.a().Compile());
        result.Add("PU");
        result.AddRange(binOp.b().Compile());
        result.Add("SW");
        result.Add("PO");

        var operationDirective = binOp.op() switch
        {
            "+" => "AD",
            "-" => "SU",
            "*" => "MU",
            "/" => "DI",
            _ => throw new InvalidOperationException($"Can't compile unknown operator '{binOp.op()}'")
        };

        result.Add(operationDirective);

        return result;
    }
}

public abstract class Ast
{
    protected string Operation { get; set; }

    protected Ast(string operation)
    {
        Operation = operation;
    }

    public string op() => Operation;
}

public class BinOp : Ast
{
    private Ast LeftChild { get; set; }
    private Ast RightChild { get; set; }

    public BinOp(string operation, Ast leftChild, Ast rightChild) : base(operation)
    {
        LeftChild = leftChild;
        RightChild = rightChild;
    }

    public Ast a() => LeftChild;

    public Ast b() => RightChild;
}

public class UnOp : Ast
{
    private readonly int Value;

    public UnOp(string operation, int value) : base(operation)
    {
        Value = value;
    }

    public int n() => Value;
}
