using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day01 : BaseDay
{
    private readonly List<string> _input;
    private readonly List<(string dir, int dist)> _interpretted;
    private readonly Dictionary<(long, long), char> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    public Day01()
    {
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split("\r\n")
,
        ];

        Regex re = new(@"^([A-Z])(\d+)$");
        _interpretted = _input
            .Select(s => re.Match(s).Groups)
            .Select(g => (g[1].Value, int.Parse(g[2].Value)))
            .ToList();

    }
    public override ValueTask<string> Solve_1()
    {
        long result = 0;
        int position = 50;
        foreach ((string dir, int dist) in _interpretted)
        {
            if (dir == "L")
            {
                position = (position - dist) % 100;
            }
            else if (dir == "R")
            {
                position = (position + dist) % 100;
            }

            if (position == 0)
            {
                result++;
            }
        }

        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 0;
        int position = 50;
        int prevPos;
        foreach ((string dir, int dist) in _interpretted)
        {
            int sign = dir == "R" ? 1 : -1;
            for (int i = 0; i < dist; i++)
            {
                position = (position + sign + 10000) % 100;
                if (position == 0)
                {
                    result++;
                }
            }
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
        Regex re = new(@"^([A-Z])(\d)+$");
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
