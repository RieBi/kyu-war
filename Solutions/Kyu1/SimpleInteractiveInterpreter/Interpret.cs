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
