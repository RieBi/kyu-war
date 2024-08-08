using Evaluation;

namespace Tests.Kyu2.EvaluateMathematicalExpressionsTests;
public class TokenizerTests
{
    [Theory]
    [InlineData("123 text", "123")]
    [InlineData("123.0123", "123.0123")]
    [InlineData("123.+123", "123")]
    [InlineData("123e10 text", "123e10")]
    [InlineData("123e+10text", "123e+10")]
    [InlineData("555.77e-7 else", "555.77e-7")]
    [InlineData("1e5.8", "1e5")]
    public void ParseNumber_ReturnsOnlyNumber(string input, string expected)
    {
        var tokenizer = new Tokenizer(input);

        var actual = tokenizer.ParseNumber();

        Assert.Equal(expected, actual?.Value);
        Assert.Equal(TokenKind.Number, actual?.Kind);
    }

    [Theory]
    [InlineData("(8131)", "(")]
    [InlineData(")dsds", ")")]
    [InlineData("123()", null)]
    public void ParseParentheses_ReturnsOnlyParenthese(string input, string? expected)
    {
        var tokenizer = new Tokenizer(input);

        var actual = tokenizer.ParseParentheses();

        Assert.Equal(expected, actual?.Value);
        Assert.True(expected == null || actual?.Kind == TokenKind.OpeningParenthese || actual?.Kind == TokenKind.ClosingParenthese);
    }

    [Theory]
    [InlineData("+123", "+")]
    [InlineData("-", "-")]
    [InlineData("**", "*")]
    [InlineData("/123", "/")]
    [InlineData("&123", "&")]
    [InlineData("123", null)]
    public void ParseBinaryOperator_ReturnsOnlyOperator(string input, string? expected)
    {
        var tokenizer = new Tokenizer(input);

        var actual = tokenizer.ParseBinaryOperator();

        Assert.Equal(expected, actual?.Value);
        Assert.True(expected == null || actual?.Kind == TokenKind.BinaryOperator);
    }

    [Theory]
    [InlineData("logn", "log")]
    [InlineData("Ln", "ln")]
    [InlineData("ExP", "exp")]
    [InlineData("Sq_rt", null)]
    [InlineData("ABStraction", "abs")]
    [InlineData("sIn", "sin")]
    [InlineData("AsiN", "asin")]
    public void ParseFunction_ReturnsOnlyFunction(string input, string? expected)
    {
        var tokenizer = new Tokenizer(input);

        var actual = tokenizer.ParseFunction();

        Assert.Equal(expected, actual?.Value);
        Assert.True(expected == null || actual?.Kind == TokenKind.Function);
    }

    [Theory]
    [MemberData(nameof(GetTokenParseData))]
    public void ParseSource_ReturnsTokens(string input, List<Token> expected)
    {
        var tokenizer = new Tokenizer(input);

        var actual = tokenizer.Tokenize();

        Assert.Collection(actual, expected.Select((_, i) =>
        {
            Action<Token> action = (f) =>
            {
                Assert.Equal(f.Value, expected[i].Value);
                Assert.Equal(f.Kind, expected[i].Kind);
            };

            return action;
        }).ToArray());
    }

    public static TheoryData<string, List<Token>> GetTokenParseData()
    {
        TheoryData<string, List<Token>> result = new()
        {
            {
                "5 + 7",
                new()
                {
                    new(TokenKind.Number, "5"),
                    new(TokenKind.BinaryOperator, "+"),
                    new(TokenKind.Number, "7")
                }
            },
            {
                "Tan(4 * (6 - 7) / 10) * 0",
                new()
                {
                    new(TokenKind.Function, "tan"),
                    new(TokenKind.OpeningParenthese, "("),
                    new(TokenKind.Number, "4"),
                    new(TokenKind.BinaryOperator, "*"),
                    new(TokenKind.OpeningParenthese, "("),
                    new(TokenKind.Number, "6"),
                    new(TokenKind.BinaryOperator, "-"),
                    new(TokenKind.Number, "7"),
                    new(TokenKind.ClosingParenthese, ")"),
                    new(TokenKind.BinaryOperator, "/"),
                    new(TokenKind.Number, "10"),
                    new(TokenKind.ClosingParenthese, ")"),
                    new(TokenKind.BinaryOperator, "*"),
                    new(TokenKind.Number, "0")
                }
            },
            {
                "2.2e2-(3.111e-8/0)",
                new()
                {
                    new(TokenKind.Number, "2.2e2"),
                    new(TokenKind.BinaryOperator, "-"),
                    new(TokenKind.OpeningParenthese, "("),
                    new(TokenKind.Number, "3.111e-8"),
                    new(TokenKind.BinaryOperator, "/"),
                    new(TokenKind.Number, "0"),
                    new(TokenKind.ClosingParenthese, ")")
                }
            }
        };

        return result;
    }
}
