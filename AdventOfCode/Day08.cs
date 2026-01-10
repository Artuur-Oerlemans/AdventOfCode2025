using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day08 : BaseDay
{
    private readonly List<Coor3D> _input;
    private readonly Dictionary<(long, long), char> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    public Day08()
    {
        Regex re = new(@"^(\d+),(\d+),(\d+)$");
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split("\r\n")
            .Select(s => re.Match(s).Groups)
            .Select(g => new Coor3D(long.Parse(g[1].Value), long.Parse(g[2].Value), long.Parse(g[3].Value)))
,
        ];

    }
    public override ValueTask<string> Solve_1()
    {
        long result = 1;
        List<(Coor3D p1, Coor3D p2, long dist)> distances = new();
        for (int index1 = 0; index1 < _input.Count; index1++)
        {
            for (int index2 = index1 + 1; index2 < _input.Count; index2++)
            {
                distances.Add((_input[index1], _input[index2], CalcSquaredDist(_input[index1], _input[index2])));
            }
        }

        Dictionary<Coor3D, List<Coor3D>> circuits = _input.ToDictionary(p => p, p => new List<Coor3D>() { p });
        List<(Coor3D p1, Coor3D p2, long dist)> shortestConnections = distances.OrderBy(d => d.dist)
            .ToList();

        int numberOfConnections = _input.Count < 1000 ? 10 : 1000;
        for (int j = 0; j < numberOfConnections; j++)
        {
            (Coor3D p1, Coor3D p2, long dist) = shortestConnections[j];
            if (circuits[p1] != circuits[p2])
            {
                circuits[p1].AddRange(circuits[p2]);
                foreach (Coor3D p in circuits[p2])
                {
                    circuits[p] = circuits[p1];
                }
            }
        }

        circuits.Values.Distinct().Select(l => l.Count).OrderByDescending(l => l)
            .Take(3).ToList().ForEach(l => result *= l);

        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 1;
        List<(Coor3D p1, Coor3D p2, long dist)> distances = new();
        for (int index1 = 0; index1 < _input.Count; index1++)
        {
            for (int index2 = index1 + 1; index2 < _input.Count; index2++)
            {
                distances.Add((_input[index1], _input[index2], CalcSquaredDist(_input[index1], _input[index2])));
            }
        }

        Dictionary<Coor3D, List<Coor3D>> circuits = _input.ToDictionary(p => p, p => new List<Coor3D>() { p });
        List<(Coor3D p1, Coor3D p2, long dist)> shortestConnections = distances.OrderBy(d => d.dist)
            .ToList();

        int numberOfConnections = _input.Count < 1000 ? 10 : 1000;
        for (int j = 0; true; j++)
        {
            (Coor3D p1, Coor3D p2, long dist) = shortestConnections[j];
            if (circuits[p1] != circuits[p2])
            {
                circuits[p1].AddRange(circuits[p2]);
                foreach (Coor3D p in circuits[p2])
                {
                    circuits[p] = circuits[p1];
                }
            }
            if (circuits[_input[0]].Count == _input.Count)
            {
                return new ValueTask<string>($"Solution to {ClassPrefix} {p1.i * p2.i}, part 2");
            }
        }
    }

    private record Coor3D(long i, long j, long k);

    private long CalcSquaredDist(Coor3D a, Coor3D b)
    {
        return ((a.i - b.i) * (a.i - b.i)) + ((a.j - b.j) * (a.j - b.j)) + ((a.k - b.k) * (a.k - b.k));
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
