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
        Regex rgxMain = new Regex("\\[|\\]|[-+*/=\\(\\)]|[A-Za-z_][A-Za-z0-9_]*|[0-9]*(\\.?[0-9]+)");
        MatchCollection matches = rgxMain.Matches(input);
        foreach (Match m in matches) tokens.Add(m.Groups[0].Value);
        return tokens;
    }
}

public class Ast;
