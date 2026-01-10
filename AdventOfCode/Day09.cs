using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day09 : BaseDay
{
    private readonly List<(long i, long j)> _input;
    private readonly Dictionary<(long, long), char> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    public Day09()
    {
        Regex re = new(@"(\d+),(\d+)$");
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
//, "square"
//, "L"
//, "plus"
, ".txt")
                        ).Split("\r\n")
            .Select(s => re.Match(s).Groups)
            .Select(g => (long.Parse(g[1].Value), long.Parse(g[2].Value)))
,
        ];

        //maxI = _input.Count;
        //maxJ = _input[0].Length;
        //_dict = [];
        //for (int i = 0; i < _input.Count; i++)
        //{
        //    for (int j = 0; j < _input[i].Length; j++)
        //    {
        //        if (_input[i][j] != '.')
        //        {
        //            _dict.Add((i, j), _input[i][j]);
        //        }
        //    }
        //}

    }
    public override ValueTask<string> Solve_1()
    {
        long result = 0;
        foreach ((long i1, long j1) in _input)
        {
            foreach ((long i2, long j2) in _input)
            {
                result = Math.Max(result, (Math.Abs(i1 - i2) + 1) * (Math.Abs(j1 - j2) + 1));
            }
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    private enum Corner
    {
        lt,
        rt,
        lb,
        rb,
        x, // no previous corner
        vl, // vertical line, no corner
        vr, // vertical line, no corner
        ilt,
        irt,
        ilb,
        irb
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 0;
        List<Line> lines = new();
        for (int index = 0; index < _input.Count; index++)
        {
            lines.Add(new Line(_input[index], _input[(index + 1) % _input.Count]));
        }

        foreach ((long i1, long j1) cornera in _input)
        {
            (long i1, long j1) = cornera;
            foreach ((long i2, long j2) cornerb in _input)
            {
                (long i2, long j2) = cornerb;
                if (!Intersect(cornera, cornerb, lines))
                {
                    result = Math.Max(result, (Math.Abs(i1 - i2) + 1) * (Math.Abs(j1 - j2) + 1));
                }
            }
        }

        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 2");
    }

    private bool Intersect((long i, long j) cornera, (long i, long j) cornerb, List<Line> lines)
    {
        long minI = Math.Min(cornera.i, cornerb.i);
        long maxI = Math.Max(cornera.i, cornerb.i);
        long minJ = Math.Min(cornera.j, cornerb.j);
        long maxJ = Math.Max(cornera.j, cornerb.j);

        foreach (Line line in lines)
        {
            if (Intersect1D(minI, maxI, line.a.i, line.b.i) && Intersect1D(minJ, maxJ, line.a.j, line.b.j))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Intersect1D(long minJ, long maxJ, long j1, long j2)
    {
        long minLine = Math.Min(j1, j2);
        long maxLine = Math.Max(j1, j2);
        return minJ < maxLine && minLine < maxJ;
    }

    /// <summary>
    /// I really should have read the exercise better. I didn't notice all points are in order, so I reused my
    /// calculate entire area of green and red tile code to determine the edges. After having made the simpler version,
    /// I have verified that this code works.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private List<Line> ReallyShittyApproachToDetermineLines()
    {
        Dictionary<long, List<(long i, long j)>> dict = new();
        foreach ((long i1, long j1) in _input)
        {
            if (dict.ContainsKey(j1))
            {
                dict[j1].Add((i1, j1));
            }
            else
            {
                dict[j1] = new List<(long i1, long j1)>() { (i1, j1) };
            }
        }

        List<long> iSegments = new();

        List<(long sharedJ, List<(long i, long j)> points)> orderedInput = dict
            .Select(kv => (kv.Key, kv.Value.OrderBy(p => p.i).ToList()))
            .OrderBy(t => t.Key)
            .ToList();

        List<Line> lines = new();

        long? prevJ = null;
        foreach ((long sharedJ, List<(long i, long j)> points) in orderedInput)
        {
            int posPoints = 0;
            int posSegment = 0;
            long? prevI = null;
            List<long> newISegments = new();
            Corner prevCorner = Corner.x;
            while (posPoints < points.Count || posSegment < iSegments.Count)
            {
                Corner curCorner;
                if (posPoints < points.Count && (posSegment == iSegments.Count || points[posPoints].i < iSegments[posSegment]))
                {
                    long curI = points[posPoints].i;
                    newISegments.Add(curI);
                    curCorner = PointCorner(prevCorner);
                    lines.AddRange(ToAddLines(curCorner, prevI, curI, prevJ, sharedJ));
                    prevI = curI;
                    posPoints++;
                }
                else if (posPoints == points.Count || iSegments[posSegment] < points[posPoints].i)
                {
                    long curI = iSegments[posSegment];
                    newISegments.Add(curI);
                    curCorner = SegmentCorner(prevCorner);
                    lines.AddRange(ToAddLines(curCorner, prevI, curI, prevJ, sharedJ));
                    prevI = curI;
                    posSegment++;
                }
                else if (iSegments[posSegment] == points[posPoints].i)
                {
                    long curI = iSegments[posSegment];
                    curCorner = matchCorner(prevCorner);
                    lines.AddRange(ToAddLines(curCorner, prevI, curI, prevJ, sharedJ));
                    prevI = curI;
                    posSegment++;
                    posPoints++;
                }
                else
                {
                    throw new Exception("This should be impossible");
                }
                prevCorner = curCorner;
            }
            iSegments = newISegments;
            prevJ = sharedJ;
        }

        return lines;
    }

    private record Line((long i, long j) a, (long i, long j) b);

    private static List<Line> ToAddLines(Corner curCorner, long? prevI, long curI, long? prevJ, long curJ)
    {
        (long, long) curPos = (curI, curJ);
        switch (curCorner)
        {
            case Corner.rb:
            case Corner.ilt:
                return new List<Line>() { new((prevI.Value, curJ), curPos), new((curI, prevJ.Value), curPos) };
            case Corner.lt:
            case Corner.vl:
            case Corner.irb:
            case Corner.vr:
                return new List<Line>();
            case Corner.rt:
            case Corner.ilb:
                return new List<Line>() { new((prevI.Value, curJ), curPos) };
            case Corner.irt:
            case Corner.lb:
                return new List<Line>() { new((curI, prevJ.Value), curPos) };
            case Corner.x:
            default:
                throw new Exception();
        }
    }


    /// <summary>
    /// I didn't read the question thoroughly enough. This calculates the entire are of all the tiles, not sure if it works a 100%.
    /// One assumption, the coordinates are corners, so each corner is only connected to two sides of the final shape.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private long TotalTileAreaCalculator()
    {
        long result = 0;
        Dictionary<long, List<(long i, long j)>> dict = new();
        foreach ((long i1, long j1) in _input)
        {
            if (dict.ContainsKey(j1))
            {
                dict[j1].Add((i1, j1));
            }
            else
            {
                dict[j1] = new List<(long i1, long j1)>() { (i1, j1) };
            }
        }
        // Determine if there are any weird extrusions.
        bool x = dict.Values.Where(l => l.Count % 2 == 1).Any();

        List<long> iSegments = new();

        List<(long sharedJ, List<(long i, long j)> points)> orderedInput = dict
            .Select(kv => (kv.Key, kv.Value.OrderBy(p => p.i).ToList()))
            .OrderBy(t => t.Key)
            .ToList();

        long? prevJ = null;
        foreach ((long sharedJ, List<(long i, long j)> points) in orderedInput)
        {
            int posPoints = 0;
            int posSegment = 0;
            long? prevI = null;
            List<long> newISegments = new();
            Corner prevCorner = Corner.x;
            while (posPoints < points.Count || posSegment < iSegments.Count)
            {
                Corner curCorner;
                if (posPoints < points.Count && (posSegment == iSegments.Count || points[posPoints].i < iSegments[posSegment]))
                {
                    long curI = points[posPoints].i;
                    newISegments.Add(curI);
                    curCorner = PointCorner(prevCorner);
                    result += ToAdd(curCorner, prevI, curI, prevJ, sharedJ);
                    prevI = curI;
                    posPoints++;
                }
                else if (posPoints == points.Count || iSegments[posSegment] < points[posPoints].i)
                {
                    long curI = iSegments[posSegment];
                    newISegments.Add(curI);
                    curCorner = SegmentCorner(prevCorner);
                    result += ToAdd(curCorner, prevI, curI, prevJ, sharedJ);
                    prevI = curI;
                    posSegment++;
                }
                else if (iSegments[posSegment] == points[posPoints].i)
                {
                    long curI = iSegments[posSegment];
                    curCorner = matchCorner(prevCorner);
                    result += ToAdd(curCorner, prevI, curI, prevJ, sharedJ);
                    prevI = curI;
                    posSegment++;
                    posPoints++;
                }
                else
                {
                    throw new Exception("This should be impossible");
                }
                prevCorner = curCorner;
            }
            iSegments = newISegments;
            prevJ = sharedJ;
        }
        return result;
    }

    private static long ToAdd(Corner curCorner, long? prevI, long curI, long? prevJ, long sharedJ)
    {
        switch (curCorner)
        {
            case Corner.rb:
            case Corner.irt:
            case Corner.irb:
            case Corner.ilb:
            case Corner.vr:
                return (curI - prevI.Value) * (sharedJ - prevJ.Value);
            case Corner.lt:
                return 1;
            case Corner.rt:
                return curI - prevI.Value;
            case Corner.lb:
            case Corner.vl:
                return sharedJ - prevJ.Value;
            case Corner.ilt:
                return curI - prevI.Value + (sharedJ - prevJ.Value) - 1;
            case Corner.x:
            default:
                throw new Exception();
        }
    }

    private static Corner matchCorner(Corner prevCorner)
    {
        Corner curCorner;
        switch (prevCorner)
        {
            case Corner.rb:
            case Corner.rt:
            case Corner.vr:
            case Corner.x:
                curCorner = Corner.lb;
                break;
            case Corner.lb:
            case Corner.irb:
                curCorner = Corner.rb;
                break;
            case Corner.lt:
            case Corner.irt:
                curCorner = Corner.ilt;
                break;
            case Corner.vl:
            case Corner.ilt:
            case Corner.ilb:
                curCorner = Corner.irt;
                break;
            default:
                throw new Exception();
        }

        return curCorner;
    }

    private static Corner SegmentCorner(Corner prevCorner)
    {
        Corner curCorner;
        switch (prevCorner)
        {
            case Corner.vl:
            case Corner.ilt:
            case Corner.ilb:
                curCorner = Corner.vr;
                break;
            case Corner.rb:
            case Corner.rt:
            case Corner.vr:
            case Corner.x:
                curCorner = Corner.vl;
                break;
            case Corner.lt:
            case Corner.lb:
            case Corner.irt:
            case Corner.irb:
            default:
                throw new Exception();
        }

        return curCorner;
    }

    private static Corner PointCorner(Corner prevCorner)
    {
        Corner curCorner;
        switch (prevCorner)
        {
            case Corner.rb:
            case Corner.rt:
            case Corner.vr:
            case Corner.x:
                curCorner = Corner.lt;
                break;
            case Corner.lt:
            case Corner.irt:
                curCorner = Corner.rt;
                break;
            case Corner.lb:
            case Corner.irb:
                curCorner = Corner.ilb;
                break;
            case Corner.vl:
            case Corner.ilb:
                curCorner = Corner.irb;
                break;
            case Corner.ilt:
            default:
                throw new Exception();
        }

        return curCorner;
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
