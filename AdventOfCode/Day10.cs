using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day10 : BaseDay
{
    private const int _largerThanAnyPossibleSolution = 1000000;
    private readonly List<Machine> _input;
    private readonly Dictionary<(long, long), char> _dict;
    private readonly List<(long, long)> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsAll = [(1, -1), (-1, -1), (1, 1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)];
    private readonly List<(long, long)> dirsDiag = [(1, -1), (-1, -1), (1, 1), (-1, 1)];
    private readonly long maxI;
    private readonly long maxJ;

    private record Machine(List<bool> Lights, List<List<int>> Buttons, List<long> Jolts);

    public Day10()
    {
        Regex re = new(@"^\[([.#]+)\] (.+) {(.+)}$");
        _input =
        [
            .. File.ReadAllText(string.Concat(InputFilePath.AsSpan(0, InputFilePath.Length - 4)
//, "test"
, ".txt")
                        ).Split("\r\n")
            .Select(s => re.Match(s).Groups)
            .Select(g => new Machine(ToLights(g[1].Value), ToButtons(g[2].Value), ToJoltage(g[3].Value)))
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

    private static List<bool> ToLights(string s)
    {
        return s.Select(c => c == '#')
            .ToList();
    }

    private static List<List<int>> ToButtons(string s)
    {
        return s.Split(' ')
            .Select(bs => bs[1..(bs.Length - 1)]
                .Split(",")
                .Select(int.Parse)
                .ToList()
            )
            .ToList();
    }

    private static List<long> ToJoltage(string s)
    {
        return s.Split(",")
                .Select(long.Parse)
                .ToList();
    }
    public override ValueTask<string> Solve_1()
    {
        long result = 0;
        foreach (Machine machine in _input)
        {
            result += LeastButtonPressesRequiredForLights(machine);
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 1");
    }

    private static long LeastButtonPressesRequiredForLights(Machine machine)
    {
        long minimalButtonPresses = int.MaxValue;
        for (int i = 0; i < Math.Pow(2, machine.Buttons.Count); i++)
        {
            string onButtons = Convert.ToString(i, 2);
            List<bool> curLights = OffLightsMachine(machine);
            long buttonsOn = TurnOnLights(machine, onButtons, curLights);
            if (curLights.SequenceEqual(machine.Lights))
            {
                minimalButtonPresses = Math.Min(minimalButtonPresses, buttonsOn);
            }
        }
        return minimalButtonPresses;
    }

    private static long TurnOnLights(Machine machine, string onButtons, List<bool> curLights)
    {
        long buttonsOn = 0;
        for (int j = 0; j < onButtons.Length; j++)
        {
            int buttonId = machine.Buttons.Count - j - 1;
            if (onButtons[onButtons.Length - j - 1] == '1')
            {
                buttonsOn++;
                foreach (int lightId in machine.Buttons[buttonId])
                {
                    curLights[lightId] = !curLights[lightId];
                }
            }
        }

        return buttonsOn;
    }

    private static List<bool> OffLightsMachine(Machine machine)
    {
        return Enumerable.Repeat(false, machine.Lights.Count).ToList();
    }

    public override ValueTask<string> Solve_2()
    {
        long result = 0;
        for (int i = 0; i < _input.Count; i++)
        {
            Machine machine = _input[i];
            Console.WriteLine($"Machine number {i + 1} {DateTime.Now}");
            result += LeastButtonPressesRequiredForJoltage(machine);
        }
        return new ValueTask<string>($"Solution to {ClassPrefix} {result}, part 2");
    }

    public record Equation(List<int> ButtonIds, long Sum);

    private static long LeastButtonPressesRequiredForJoltage(Machine machine)
    {
        List<Equation> equations = [];

        for (int joltageId = 0; joltageId < machine.Jolts.Count; joltageId++)
        {
            List<int> buttonIds = [];
            for (int buttonId = 0; buttonId < machine.Buttons.Count; buttonId++)
            {
                if (machine.Buttons[buttonId].Contains(joltageId))
                {
                    buttonIds.Add(buttonId);
                }
            }
            equations.Add(new Equation(buttonIds, machine.Jolts[joltageId]));
        }

        return GetLeastButtonPresses(equations);
    }

    private static long GetLeastButtonPresses(List<Equation> equations)
    {
        if (equations.Count == 0)
        {
            return 0;
        }

        try
        {
            equations = SubstractEquationsOfEquations(equations);
        }
        catch (Exception ex)
        {
            return _largerThanAnyPossibleSolution;
        }

        foreach (Equation equation in equations)
        {
            if (equation.ButtonIds.Count == 1)
            {
                return SubstituteEquations(equation.ButtonIds[0], equation.Sum, equations);
            }
        }

        int toRemoveButtonId = equations
            .OrderBy(e => e.ButtonIds.Count)
            .First()
            .ButtonIds[0];

        long maxButtonValue = GetMaxButtonValue(equations, toRemoveButtonId);
        long lowestPresses = _largerThanAnyPossibleSolution;

        for (long buttonValue = 0; buttonValue <= maxButtonValue; buttonValue++)
        {
            lowestPresses = Math.Min(lowestPresses, SubstituteEquations(toRemoveButtonId, buttonValue, equations));
        }

        return lowestPresses;
    }

    private static List<Equation> SubstractEquationsOfEquations(List<Equation> equations)
    {
        return equations
            .Select(e => SubstractEquationsOfEquation(equations, e))
            .ToList();
    }

    private static Equation SubstractEquationsOfEquation(List<Equation> equations, Equation equation)
    {
        foreach (Equation childEquation in equations)
        {
            if (childEquation.ButtonIds.Count < equation.ButtonIds.Count && !childEquation.ButtonIds.Except(equation.ButtonIds).Any())
            {
                List<int> newButtonIds = equation.ButtonIds
                    .Where(bi => !childEquation.ButtonIds.Contains(bi))
                    .ToList();
                long newSum = equation.Sum - childEquation.Sum;
                if (newSum < 0)
                {
                    throw new Exception();
                }
                return new Equation(newButtonIds, newSum);
            }
        }
        return equation;
    }

    private static long GetMaxButtonValue(List<Equation> equations, int buttonId)
    {
        long maxButtonValue = long.MaxValue;
        foreach (Equation e in equations)
        {
            if (e.ButtonIds.Contains(buttonId))
            {
                maxButtonValue = Math.Min(maxButtonValue, e.Sum);
            }
        }
        if (maxButtonValue == long.MaxValue)
        {
            throw new Exception("There is now equation with buttonId?");
        }
        return maxButtonValue;
    }

    private static long SubstituteEquations(int buttonId, long value, List<Equation> equations)
    {
        List<Equation> simplifiedEquations = [];
        foreach (Equation equation in equations)
        {
            if (equation.ButtonIds.Contains(buttonId))
            {
                if (equation.ButtonIds.Count == 1)
                {
                    if (equation.Sum != value)
                    {
                        return _largerThanAnyPossibleSolution;
                    }
                }
                else
                {
                    if (equation.Sum < value)
                    {
                        return _largerThanAnyPossibleSolution;
                    }
                    else
                    {
                        List<int> newButtonIds = equation.ButtonIds
                            .Where(x => x != buttonId)
                            .ToList();
                        simplifiedEquations.Add(new Equation(newButtonIds, equation.Sum - value));
                    }
                }
            }
            else
            {
                simplifiedEquations.Add(equation);
            }
        }
        //Console.WriteLine();
        //PrintEquations(equations);
        //Console.WriteLine($"Substitute x{buttonId} for {value}");
        //PrintEquations(simplifiedEquations);
        return value + GetLeastButtonPresses(simplifiedEquations);
    }

    private static void PrintEquations(List<Equation> equations)
    {
        Console.WriteLine(string.Join(" | ", equations.Select(e => string.Join("+", e.ButtonIds.Select(b => "x" + b)) + " = " + e.Sum)));
    }

    private record JoltProgress(List<long> Jolts, long Presses);

    // My code couldn't handle 10 dimension ):
    private static long FirstAttemptLeastButtonPressesRequiredForJoltage(Machine machine)
    {
        PriorityQueue<JoltProgress, long> priorityQueue = new();
        HashSet<string> reachedJolts = new();

        priorityQueue.Enqueue(new JoltProgress(NoJoltageMachine(machine), 0), 0);
        while (true)
        {
            JoltProgress jp = priorityQueue.Dequeue();

            if (jp.Jolts.SequenceEqual(machine.Jolts))
            {
                return jp.Presses;
            }

            if (IsTooMuchJolts(jp.Jolts, machine.Jolts))
            {
                continue;
            }

            foreach (List<int> button in machine.Buttons)
            {
                List<long> newJolts = PressButton(jp.Jolts, button);
                long newPresses = jp.Presses + 1;
                long newPriority = (newPresses * machine.Jolts.Count) - newJolts.Sum(x => x);

                if (IsTooMuchJolts(newJolts, machine.Jolts))
                {
                    continue;
                }

                string joltsString = ToJoltString(newJolts);

                if (reachedJolts.Contains(joltsString))
                {
                    continue;
                }
                else
                {
                    reachedJolts.Add(joltsString);
                }
                priorityQueue.Enqueue(new JoltProgress(newJolts, jp.Presses + 1), newPriority);
            }
        }
    }

    private static List<long> PressButton(List<long> jolts, List<int> button)
    {
        List<long> newJolts = jolts.Select(l => l).ToList();
        foreach (int joltId in button)
        {
            newJolts[joltId]++;
        }
        return newJolts;
    }

    private static string ToJoltString(List<long> jolts)
    {
        return string.Join(", ", jolts);
    }

    private static bool IsTooMuchJolts(List<long> jolts, List<long> maxJolts)
    {
        for (int i = 0; i < maxJolts.Count; i++)
        {
            if (jolts[i] > maxJolts[i])
            {
                return true;
            }
        }
        return false;
    }

    private static List<long> NoJoltageMachine(Machine machine)
    {
        return Enumerable.Repeat(0L, machine.Jolts.Count).ToList();
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
        Regex re = new(@"^move (\d+) to ([A-Z])$");
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
