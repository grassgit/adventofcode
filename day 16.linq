<Query Kind="Program" />

void Main()
{
	var input =
		File.ReadAllText(Util.CurrentQueryPath + ".input")
		//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
		//File.ReadAllText(Util.CurrentQueryPath + ".sample2")
		;

	var mapping = new Dictionary<char, Action> {
		{ '.', Action.None },
		{ '|', Action.SplitUD },
		{ '-', Action.SplitLR },
		{ '/', Action.RotateCW },
		{ '\\', Action.RotateCCW },
		{ '0', Action.Stop },
	};
	var mapInv = mapping.ToDictionary(x => x.Value, x => x.Key);

	var lines = input.Split("\r\n");
	var initialMap = new[] { new string('0', lines[0].Length) }.Concat(lines).Concat(new[] { new string('0', lines[0].Length) })
		.SelectMany((l, y) => $"0{l}0".Select((c, x) => new Cell(x, y, mapping[c])))
		.ToDictionary(x => x.P)

		//.GroupBy(x=>x.Key.x)
		//.Select(x=>string.Join("", x.Select(y=>(int)y.Value)))
		//.Dump()
		;

	var state = new Dictionary<(int X, int Y), dynamic>();
	IEnumerable<Move> Change(Action action, Move move)
	{
		//$"{move.P}) {action} {move.Direction} {(((int)move.Direction & 1) == 1 ? "Vert" : "Hor")}".Dump();
		return new int[(int)action & 3]
			.Select((_, x) => (x, action, move.Direction, ((int)move.Direction & 1) == 1) switch
			{
				(0, Action.None, Direction.Down, _) or
				(0, Action.SplitUD, Direction.Down, _) or
				(0, Action.SplitUD, _, false) => move.Next(y: 1, change: Direction.Down),

				(0, Action.None, Direction.Up, _) or
				(0, Action.SplitUD, Direction.Up, _) or
				(1, Action.SplitUD, _, false) => move.Next(y: -1, change: Direction.Up),

				(0, Action.None, Direction.Right, _) or
				(0, Action.SplitLR, Direction.Right, _) or
				(0, Action.SplitLR, _, true) => move.Next(x: 1, change: Direction.Right),

				(0, Action.None, Direction.Left, _) or
				(0, Action.SplitLR, Direction.Left, _) or
				(1, Action.SplitLR, _, true) => move.Next(x: -1, change: Direction.Left),

				(0, Action.RotateCW, Direction.Left, _) or
				(0, Action.RotateCCW, Direction.Right, _) => move.Next(y: 1, change: Direction.Down),

				(0, Action.RotateCW, Direction.Right, _) or
				(0, Action.RotateCCW, Direction.Left, _) => move.Next(y: -1, change: Direction.Up),

				(0, Action.RotateCW, Direction.Down, _) or
				(0, Action.RotateCCW, Direction.Up, _) => move.Next(x: -1, change: Direction.Left),

				(0, Action.RotateCW, Direction.Up, _) or
				(0, Action.RotateCCW, Direction.Down, _) => move.Next(x: 1, change: Direction.Right),

				(_, Action.SplitUD, _, true) or
				(_, Action.SplitLR, _, false)
					 => null,

				_ => throw new InvalidOperationException($"{x} {action} {move.Direction} {(((int)move.Direction & 1) == 1 ? "Vert" : "Hor")}")
			})
			.Where(x => x != null)
			.ToArray();
	}

	(Move, int, int, IEnumerable<Cell>) Run(Dictionary<(int X, int Y), Cell> map, Move start)
	{
		map = map.ToDictionary(x => x.Key, x => new Cell(x.Value.X, x.Value.Y, x.Value.Action));
		map[start.P].Used[start.Direction] = true;

		var moves = Recurse(new[] { start })
			.SelectMany(x => x
				.More(
					Change(map[x.Item.P].Action, x.Item)
						.Select(z => z with { Invert = Flip(x.Item.Direction) })
						.Where(y => !map[y.P].Used[y.Direction] && !map[y.P].Reversed[y.Direction]))
					.Select(y => map
						.For(l => l[y.P].Used, m => m.Set(y.Direction, _ => true))
						.For(l => l[x.Item.P].Reversed, m => m.Set(Flip(y.Direction), _ => true))
						== null ? y : y)
			)
			.ToArray()
			//.Dump()
			;
		var count = map.Values
		.Where(x => x.Action != Action.Stop)
		.Count(x => x.Used.Any(z => z.Value));
		//$"Moved: {moves.Length} from {start.P} direction {start.Direction} - has {count} covered".Dump();

		return (start, moves.Length, count, map.Values);
	}

	var start = new Move(1, 1, Direction.Down);
	var (_, _, _, result) = Run(initialMap, start);
	result
		.Where(x => x.Action != Action.Stop)
		.GroupBy(x => x.Y)
		.Select(x => string.Join("", x.Select(c => c.Used.Any(z => z.Value) ? '#' : '.')))
		.Dump();
	initialMap
		.Where(x => x.Value.Action != Action.Stop)
		.Select(x => x.Key)
		.Where(x => x.X == 1 || x.X == lines[0].Length)
		.Select(x => new Move(x.X, x.Y, x.X == 1 ? Direction.Right : Direction.Left))
		.Union(initialMap
			.Where(x => x.Value.Action != Action.Stop)
			.Select(x => x.Key)
			.Where(x => x.Y == 1 || x.Y == lines.Length)
			.Select(x => new Move(x.X, x.Y, x.Y == 1 ? Direction.Down : Direction.Up)))
		.AsParallel() // > reduces only by 50%
		.Select(x => Run(initialMap, x))
		.Select(x => new { x.Item1.X, x.Item1.Y, x.Item1.Direction, initialMap[x.Item1.P].Action, Moves = x.Item2, Covered = x.Item3 })
		.OrderBy(x => x.Covered)
		.Last()
		.Dump();
	// part 1: 6994 
	// part 2: 7488
}

record Cell(int X, int Y, Action Action)
{
	public (int X, int Y) P => (X, Y);
	public Dictionary<Direction, bool> Used { get; } = Enum.GetValues<Direction>().ToDictionary(x => x, _ => false);
	public Dictionary<Direction, bool> Reversed { get; } = Enum.GetValues<Direction>().ToDictionary(x => x, _ => false);

	public Direction Start { get; init; }
}

record Move(int X, int Y, Direction Direction)
{
	public Direction? Invert { get; set; }
	public (int X, int Y) P => (X, Y);
	public Move Next(Direction change, int x = 0, int y = 0)
	{
		//$"At {(X, Y)} moving {Direction} > Go to {(X + x, Y + y)} moving {change}".Dump();

		return new Move(X + x, Y + y, change);
	}
}

enum Action
{
	None = 1,
	SplitUD = 2,
	SplitLR = 6,
	RotateCW = 9,
	RotateCCW = 13,
	Stop = 0
}


enum Direction
{
	None = 0,
	Down = 1,
	Up = 5,
	Left = 4,
	Right = 6
}


Direction Flip(Direction direction) => direction switch
{
	Direction.Left => Direction.Right,
	Direction.Right => Direction.Left,
	Direction.Up => Direction.Down,
	Direction.Down => Direction.Up,
	_ => throw new InvalidOperationException()
};
IEnumerable<(T Item, Func<IEnumerable<T>, IEnumerable<T>> More)> Recurse<T>(IEnumerable<T> init)
{
	var list = init.ToList();
	var index = 0;
	IEnumerable<T> More(IEnumerable<T> items)
	{
		list.AddRange(items);
		return items;
	}
	while (list.Count > index && index < 5000000)
		yield return (list[index++], More);

	//index.Dump("Loop exit");
}
public static class Ext
{
	public static T For<T, TChild>(this T item, Func<T, TChild> select, Action<TChild> action)
	{
		var child = select(item);
		action(child);
		return item;
	}
	public static T Do<T>(this T item, Action<T> action)
	{
		action(item);
		return item;
	}
	public static Dictionary<TKey, TValue> Set<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, Func<TValue, TValue> set)
	{
		source[key] = set(source[key]);
		return source;
	}
	public static bool Append<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue value)
	{
		source.Add(key, value);
		return true;
	}
}