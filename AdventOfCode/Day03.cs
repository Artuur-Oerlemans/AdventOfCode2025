using Spectre.Console;
using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day03 : BaseDay
{
    private readonly List<string> _input;
    private readonly Dictionary<(long, long), int> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    public Day03()
    {
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split("\r\n")
,
        ];


    }
    public override ValueTask<string> Solve_1()
    {
        long result = 0;
        for (int i = 0; i < _input.Count; i++)
        {
            int maxJolt = 0;
            for (int jIni = 0; jIni < _input[i].Length; jIni++)
            {
                for (int jSec = jIni + 1; jSec < _input[i].Length; jSec++)
                {
                    int joltage = int.Parse(_input[i][jIni].ToString() + _input[i][jSec].ToString());
                    if (joltage > maxJolt)
                    {
                        maxJolt = joltage;
                    }
                }
            }
            result += maxJolt;
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 0;
        for (int i = 0; i < _input.Count; i++)
        {
            result += GetMaxJolts("", 12, _input[i]);
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 2");
    }

    private long GetMaxJolts(string prefix, int remainingDigits, string remainingString)
    {
        if (remainingDigits == 0)
        {
            return long.Parse(prefix);
        }
        int bestDigit = 0;
        for (int j = 1; j < remainingString.Length - remainingDigits + 1; j++)
        {
            if (int.Parse(remainingString.Substring(j, 1)) > int.Parse(remainingString.Substring(bestDigit, 1)))
            {
                bestDigit = j;
            }
        }
        return GetMaxJolts(prefix + remainingString[bestDigit].ToString(), remainingDigits - 1, remainingString.Substring(bestDigit + 1));
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
