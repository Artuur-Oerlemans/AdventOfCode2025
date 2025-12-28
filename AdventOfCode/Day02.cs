using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day02 : BaseDay
{
    private readonly List<string> _input;
    private readonly List<(long start, long end)> _interpretted;
    private readonly Dictionary<(long, long), char> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    public Day02()
    {
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split(",")
,
        ];

        Regex re = new(@"^(\d+)-(\d+)");
        _interpretted = _input
            .Select(s => re.Match(s).Groups)
            .Select(g => (long.Parse(g[1].Value), long.Parse(g[2].Value)))
            .ToList();
    }
    public override ValueTask<string> Solve_1()
    {
        long result = 0;
        foreach ((long start, long end) in _interpretted)
        {
            for (long i = start; i <= end; i++)
            {
                if (isDoubleNumber(i))
                {
                    result += i;
                }

            }
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    private bool isDoubleNumber(long i)
    {
        string s = i.ToString();
        if (s.Length % 2 == 1)
        {
            return false;
        }
        return s.Substring(0, s.Length / 2) == s.Substring(s.Length / 2, s.Length / 2);
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 0;
        foreach ((long start, long end) in _interpretted)
        {
            for (long i = start; i <= end; i++)
            {
                if (isRepeatingNumber(i))
                {
                    result += i;
                }

            }
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 2");
    }

    private bool isRepeatingNumber(long i)
    {
        string s = i.ToString();
        for (int div = 1; div < s.Length; div++)
        {
            if (s.Length % div != 0)
            {
                continue;
            }
            for (int start = 0; start < s.Length - div; start += div)
            {
                if (s.Substring(start, div) != s.Substring(start + div, div))
                {
                    break;
                }
                if (start == s.Length - (2 * div))
                {
                    return true;
                }
            }
        }
        return false;
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
