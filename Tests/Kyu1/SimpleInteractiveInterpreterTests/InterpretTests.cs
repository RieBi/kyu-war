using InterpreterKata;

namespace Tests.Kyu1.SimpleInteractiveInterpreterTests;
public class InterpretTests
{
    [Theory]
    [InlineData("10 + 3 + 7", "20")]
    [InlineData("1", "1")]
    [InlineData("2+3+5*7+4", "44")]
    [InlineData("1 + 20 / 5 * 4 - 1 + 8 * 2 % 7", "18")]
    [InlineData("a = 2 + 3", "5")]
    [InlineData("a = b = c = 5", "5")]
    [InlineData("a = 5 + (b = 77 - (c = 11 + 2))", "69")]
    [InlineData("x = 13 + (y = 3)", "16")]
    [InlineData("(2+2)*2", "8")]
    [InlineData("(((2-1))+1)/1", "2")]
    public void Interpreter_EvaluatesExpressionLineCorrectly(string input, string expected)
    {
        var interpreter = new Interpreter();

        var actual = interpreter.input(input);

        Assert.Equal(expected, actual.ToString());
    }
}
