using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day07 : BaseDay
{
    private readonly List<string> _input;
    private readonly Dictionary<(long, long), char> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    public Day07()
    {
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split("\r\n")
,
        ];

        maxI = _input.Count;
        maxJ = _input[0].Length;
        _dict = [];
        for (int i = 0; i < _input.Count; i++)
        {
            for (int j = 0; j < _input[i].Length; j++)
            {
                if (_input[i][j] != '.')
                {
                    _dict.Add((i, j), _input[i][j]);
                }
            }
        }

    }
    public override ValueTask<string> Solve_1()
    {
        long result = 0;
        List<bool> beams = Enumerable.Repeat(false, _input[0].Length).ToList();
        foreach (string s in _input)
        {
            List<bool> newBeams = Enumerable.Repeat(false, _input[0].Length).ToList();

            for (int i = 0; i < newBeams.Count; i++)
            {
                if (s[i] == 'S')
                {
                    newBeams[i] = true;
                }
                if (s[i] == '^' && beams[i])
                {
                    result++;
                    if (i > 0)
                    {
                        newBeams[i - 1] = true;
                    }
                    if (i < newBeams.Count - 2)
                    {
                        newBeams[i + 1] = true;
                    }
                }
                if (s[i] == '.' && beams[i])
                {
                    newBeams[i] = true;
                }
            }
            beams = newBeams;
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 0;
        List<long> beams = Enumerable.Repeat(0L, _input[0].Length).ToList();
        foreach (string s in _input)
        {
            List<long> newBeams = Enumerable.Repeat(0L, _input[0].Length).ToList();

            for (int i = 0; i < _input[0].Length; i++)
            {
                if (s[i] == 'S')
                {
                    newBeams[i] = 1;
                }
                if (s[i] == '^' && beams[i] > 0)
                {
                    if (i > 0)
                    {
                        newBeams[i - 1] += beams[i];
                    }
                    if (i < newBeams.Count - 1)
                    {
                        newBeams[i + 1] += beams[i];
                    }
                }
                if (s[i] == '.' && beams[i] > 0)
                {
                    newBeams[i] += beams[i];
                }
            }
            beams = newBeams;
            //PrintBreams(beams);
        }
        foreach (long numberOfTimelines in beams)
        {
            result += numberOfTimelines;
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 2");
    }

    private void PrintBreams(List<long> beams)
    {
        foreach (long numberOfTimelines in beams)
        {
            if (numberOfTimelines > 0)
            {
                Console.Write(numberOfTimelines.ToString("D2") + " ");
            }
            else
            {
                Console.Write("   ");
            }
        }
        Console.WriteLine();
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
