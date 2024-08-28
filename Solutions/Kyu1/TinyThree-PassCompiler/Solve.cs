namespace Solutions.Kyu1.TinyThree_PassCompiler;

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Compiler
{
    public Ast pass1(string prog)
    {
        List<string> tokens = tokenize(prog);
        return null;
    }

    public Ast pass2(Ast ast)
    {
        return null;
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

public class Ast
{
    public string Operation { get; set; }

    public Ast(string operation)
    {
        Operation = operation;
    }

    public string op() => Operation;
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