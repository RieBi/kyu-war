namespace Evaluation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

public record Token(TokenKind Kind, string value);

public class Tokenizer(string source)
{
    private static ImmutableHashSet<char> numberSign = ['+', '-'];

    private string _source = source;
    private int _pos = 0;
    private readonly List<Token> _tokens = [];

    public List<Token> Tokenize()
    {
        while (_pos < _source.Length)
        {
            if (IsSkippable(_source[_pos]))
            {
                _pos++;
                continue;
            }


        }

        return _tokens;
    }

    public Token? ParseNumber()
    {
        const char decimalSeparator = '.';
        ImmutableHashSet<char> scientificNotationSeparator = ['e', 'E'];

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