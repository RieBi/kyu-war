using InterpreterKata;

namespace Tests.Kyu1.SimpleInteractiveInterpreterTests;
public class InterpretTests
{
    public static TheoryData<List<string>, List<string>> GetVariablesAssignmentData()
    {
        return new()
        {
            {
                new()
                {
                    "a = 2",
                    "b = 3",
                    "a + b"
                },
                new()
                {
                    "2",
                    "3",
                    "5"
                }
            },
            {
                new()
                {
                    "x = 7",
                    "x + 6"
                },
                new()
                {
                    "7",
                    "13"
                }
            },
            {
                new()
                {
                    "x = 0",
                    "x = x + 7",
                    "x = x + 6",
                    "x"
                },
                new()
                {
                    "0",
                    "7",
                    "13",
                    "13"
                }
            },
            {
                new()
                {
                    "x = y = 2 + 2",
                    "(x * y)"
                },
                new()
                {
                    "4",
                    "16"
                }
            }
        };
    }

    public static TheoryData<List<string>, List<string>> GetFunctionsEvaluationData()
    {
        return new()
        {
            {
                new()
                {
                    "fn avg x y => (x + y) / 2",
                    "a = 2",
                    "a = 4",
                    "avg a b"
                },
                new()
                {
                    string.Empty,
                    "2",
                    "4",
                    "3"
                }
            },
            {
                new()
                {
                    "fn echo x => x",
                    "fn add x y => x + y",
                    "add echo 4 echo 3"
                },
                new()
                {
                    string.Empty,
                    string.Empty,
                    "7"
                }
            },
            {
                new()
                {
                    "fn inc x => x + 1",
                    "a = 0",
                    "a = inc a",
                    "fn inc x => x + 2",
                    "a = inc a"
                },
                new()
                {
                    string.Empty,
                    "0",
                    "1",
                    string.Empty,
                    "3"
                }
            }
        };
    }

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
    [InlineData("8 * 8 2 + 2", "64")]
    [InlineData("2 + 2 8 * 8", "4")]
    public void Interpreter_EvaluatesExpressionLineCorrectly(string input, string expected)
    {
        var interpreter = new Interpreter();

        var actual = interpreter.input(input);

        Assert.Equal(expected, actual.ToString());
    }

    [Theory]
    [MemberData(nameof(GetVariablesAssignmentData))]
    [MemberData(nameof(GetFunctionsEvaluationData))]
    public void Intepreter_UsesState(List<string> inputs, List<string> expected)
    {
        var interpreter = new Interpreter();
        var results = new List<string>();

        foreach (var line in inputs)
            results.Add(interpreter.input(line).ToString() ?? string.Empty);

        Assert.Equal(expected, results);
    }
}
