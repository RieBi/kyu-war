namespace Evaluation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class Evaluate
{
    public string eval(string expression)
    {
        string result = "0"; //calculated expression (double converted to string) or Errormessage starting with "ERROR" (+ optional Errormessage)
        return result;
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

public record Token(TokenKind Kind, string Value);

public class Tokenizer(string source)
{
    private static readonly ImmutableHashSet<char> numberSign = ['+', '-'];
    private static readonly ImmutableHashSet<char> binaryOperator = ['+', '-', '*', '/', '&'];
    private static readonly ImmutableHashSet<char> scientificNotationSeparator = ['e', 'E'];
    private static readonly ImmutableHashSet<string> function = ["log", "ln", "exp", "sqrt", "abs", "atan", "acos", "asin", "sinh", "cosh", "tanh", "tan", "sin", "cos"];
    const char decimalSeparator = '.';
    const char openingParenthese = '(';
    const char closingParenthese = ')';

    private string _source = source;
    private int _pos = 0;
    private readonly List<Token> _tokens = [];

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
                _pos++;
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
            if (!char.IsAsciiLetter(At(pos)))
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

    private static bool IsDigit(char ch) => char.IsAsciiDigit(ch);

    private static bool IsSkippable(char ch)
    {
        HashSet<char> skippable = [' ', '\n', '\r', '\t'];

        return skippable.Contains(ch);
    }
}