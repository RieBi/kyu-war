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

        Assert.Equal(expected, actual?.value);
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

        Assert.Equal(expected, actual?.value);
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

        Assert.Equal(expected, actual?.value);
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

        Assert.Equal(expected, actual?.value);
        Assert.True(expected == null || actual?.Kind == TokenKind.Function);
    }
}
