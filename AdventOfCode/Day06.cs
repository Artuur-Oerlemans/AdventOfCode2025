using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day06 : BaseDay
{
    private readonly List<List<string>> _input;
    private readonly List<List<string>> _inputWithSpaces;
    private readonly Dictionary<(long, long), char> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    public Day06()
    {
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split("\r\n")
                        .Select( x => x.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList() )
        ];
        List<string> strings =
            [.. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split("\r\n")];
        string operators = strings[strings.Count - 1];
        bool operatorFound = true;
        int indexOpe = 0;
        _inputWithSpaces = new List<List<string>>();
        while (operatorFound)
        {
            int indexNextOpe = indexOpe + 1;
            while (indexNextOpe < operators.Length && operators.Substring(indexNextOpe, 1) == " ")
            {
                indexNextOpe++;
            }
            operatorFound = indexNextOpe < operators.Length;
            List<string> numberStrings = strings.Take(strings.Count - 1)
                .Select(s => s.Substring(indexOpe, indexNextOpe - indexOpe))
                .ToList();
            _inputWithSpaces.Add(numberStrings);
            indexOpe = indexNextOpe;
        }

    }
    public override ValueTask<string> Solve_1()
    {
        long result = 0;
        for (int j = 0; j < _input[0].Count; j++)
        {
            string operation = _input[_input.Count - 1][j];
            List<long> numbers = _input.Take(_input.Count - 1)
                .Select(l => l[j])
                .Select(long.Parse)
                .ToList();
            result += ApplyOperator(operation, numbers);
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    private static long ApplyOperator(string operation, List<long> numbers)
    {
        if (operation == "+")
        {
            return numbers.Sum(x => x);
        }
        else
        {
            long product = 1;
            foreach (long num in numbers)
            {
                product *= num;
            }
            return product;
        }
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 0;
        for (int j = 0; j < _input[0].Count; j++)
        {
            string operation = _input[_input.Count - 1][j];
            List<string> strings = _inputWithSpaces[j];
            List<long> numbers = new();
            for (int index = 0; index < strings[0].Length; index++)
            {
                string newNumber = "";
                foreach (string s in strings)
                {
                    string cha = s.Substring(index, 1);
                    if (cha != " ")
                    {
                        newNumber += cha;
                    }
                }

                bool newNumberFound = newNumber != "";
                if (newNumberFound)
                {
                    numbers.Add(long.Parse(newNumber));
                }
            }

            result += ApplyOperator(operation, numbers);
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 2");
    }

    private string DictToString(Dictionary<(int, int), char> dict)
    {
        string r = "";
        for (int i = 0; i < maxI; i++)
        {
            for (int j = 0; j < maxJ; j++)
            {
                if (dict.ContainsKey((i, j)))
                {
                    r += dict[(i, j)];
                }
                else
                {
                    r += '.';
                }
            }
            r += "\r\n";
        }
        return r;
    }

    private string LocationSetToString(HashSet<(int, int)> energized)
    {
        string r = "";
        for (int i = 0; i < maxI; i++)
        {
            for (int j = 0; j < maxJ; j++)
            {
                if (energized.Contains((i, j)))
                {
                    r += "#";
                }
                else
                {
                    r += '.';
                }
            }
            r += "\r\n";
        }
        return r;
    }

    private void L(object o)
    {
        Console.WriteLine(o.ToString());
    }

    private void RegexYouKnow()
    {
        string temps =
    @"move 1 to A
move 2 to B
move 3 to C";
        Regex re = new(@"^move (\d) to ([A-Z])$");
        temps.Split("\r\n")
            .Select(s => re.Match(s).Groups)
            .Select(g => (int.Parse(g[1].Value), g[2].Value))
            .ToList().ForEach(t => Console.WriteLine(t.Value));
    }

    private (long, long) Add((long, long) a, (long, long) b)
    {
        return (a.Item1 + b.Item1, a.Item2 + b.Item2);
    }

    private (long, long) Mul((long, long) a, long x)
    {
        return (a.Item1 * x, a.Item2 * x);
    }
    private bool InRange((long, long) a)
    {
        return a.Item1 >= 0 && a.Item2 >= 0 && a.Item1 < maxI && a.Item2 < maxJ;
    }

    private (long, long) Neg((long, long) p)
    {
        return (-p.Item1, -p.Item2);
    }
}
