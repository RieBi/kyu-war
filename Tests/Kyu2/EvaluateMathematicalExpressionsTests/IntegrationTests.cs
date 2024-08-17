using Evaluation;

namespace Tests.Kyu2.EvaluateMathematicalExpressionsTests;
public class IntegrationTests
{
    [Theory]
    [InlineData("2*3&2", "18")]
    [InlineData("(-(2 + 3)* (1 + 2)) * 4 & 2", "-240")]
    [InlineData("-5&3&2*2-1", "-3906251")]
    [InlineData("abs(-(-1+(2*(4--3)))&2)", "169")]
    [InlineData("(Abs(1+(2--1)-3)* sin(3 + -1) / 2.1) -12.3453* 0.45+2.4e3", "2394.877613774679")]
    [InlineData("(1+-2+2*-2) & 1e1", "9765625")]
    [InlineData("2+5*5*5", "127")]
    [InlineData("1+2+18-3-2-1--1", "16")]
    [InlineData("2&2&3", "256")]
    [InlineData("2+5*5/5", "7")]
    public void Evaluation_ReturnsAccurateResult(string input, string expected)
    {
        var evaluate = new Evaluate();

        var actual = evaluate.eval(input);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("sqrt(-2)*2\")+\"     ")]
    [InlineData("2*5/0")]
    [InlineData("sqrt(-5&(-3+1--1+--3))")]
    [InlineData("si  n(15)*3")]
    [InlineData("(1+9+3)))")]
    public void Evaluation_ResultsInError(string input)
    {
        var evaluate = new Evaluate();

        var actual = evaluate.eval(input);

        Assert.Equal("ERROR", actual.Substring(0, 5).ToUpperInvariant());
    }
}
