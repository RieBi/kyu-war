﻿using InterpreterKata;

namespace Tests.Kyu1.SimpleInteractiveInterpreterTests;
public class InterpretTests
{
    [Theory]
    [InlineData("10 + 3 + 7", "20")]
    [InlineData("1", "1")]
    [InlineData("2+3+5*7+4", "44")]
    [InlineData("1 + 20 / 5 * 4 - 1 + 8 * 2 % 7", "18")]
    public void Interpreter_EvaluatesExpressionLineCorrectly(string input, string expected)
    {
        var interpreter = new Interpreter();

        var actual = interpreter.input(input);

        Assert.Equal(expected, actual.ToString());
    }
}
