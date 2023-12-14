<Query Kind="Program" />


void Main()
{
	var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

	//┌┐└┘
	var movement = new[] {
		new Map('|', Position.Top, 1, 0, Position.Top, '│'),
		new Map('|', Position.Bottom, -1, 0, Position.Bottom,'│'),
		new Map('-', Position.Left, 0, 1, Position.Left,'─'),
		new Map('-', Position.Right, 0, -1, Position.Right,'─'),
		new Map('L', Position.Top, 0, 1, Position.Left,'└'),
		new Map('L', Position.Right, -1, 0, Position.Bottom,'└'),
		new Map('J', Position.Top, 0, -1, Position.Right, '┘' ),
		new Map('J', Position.Left, -1, 0, Position.Bottom,'┘'),
		new Map('F', Position.Bottom, 0, 1, Position.Left,'┌'),
		new Map('F', Position.Right, 1, 0, Position.Top,'┌'),
		new Map('7', Position.Bottom, 0, -1, Position.Right,'┐'),
		new Map('7', Position.Left, 1, 0, Position.Top, '┐'),
		Map.StartPosition
	}
	.ToDictionary(x => (x.Char, x.Start));

	var lines = input.Split("\r\n");
	var map = lines
		.SelectMany((l, x) => l.Select((c, y) => new { X = x, Y = y, Char = c }).Where(x => x.Char != '.'))
		.ToDictionary(x => (x.X, x.Y));
	//.Dump()
	;
	var size = new { X = lines.Length, Y = lines.Max(x => x.Length) };

	string CreateMap(IEnumerable<(int x, int y, char c)> points)
	{
		var index = points.ToDictionary(x => (x.x, x.y), x => x.c);
		return string.Join("\r\n", new int[size.X].Select((_, x) => string.Join("", new int[size.Y].Select((_, y) => index.TryGetValue((x, y), out var c) ? c : ' '))));
	}
	var start = map.Values.Where(x => x.Char == 'S').Single();

	var init = new Me(start.X, start.Y, Position.Top);
	var result = new[] {
		new Me(start.X + 1, start.Y, Position.Top),
		new Me(start.X, start.Y + 1, Position.Left),
		new Me(start.X - 1, start.Y, Position.Bottom),
		new Me(start.X, start.Y - 1, Position.Right),
	}
	.Select(start => Accumulate(
			map, new { Position = start, From = init, Step = 0, Arrow = 'S', },
			(current, _) => !map.TryGetValue((current.Position.X, current.Position.Y), out var position) || !movement.TryGetValue((position.Char, current.Position.Side), out var move) ? null : new { Position = move.Apply(current.Position), From = current.Position, Step = current.Step + 1, move.Arrow, },
			(p, a) => p != null && p.Arrow != 'S' && !(p.Position.X == start.X && p.Position.Y == start.Y))
			)
		//.Select(x=>CreateMap(x.Select(y=>(y.From.X, y.From.Y, y.Arrow))).Dump() == null ? x :x)
		.Select(x => new { Path = x, End = x.LastOrDefault(), Steps = x.Count() })
		.Select(x => new { x.Path, x.Steps, x.End, Complete = x.End.Position.X == start.X && x.End.Position.Y == start.Y })
		//.Dump()
		.Where(x => x.Complete)
		.Select(x => new { x.Steps, Path = x.Path.Select(y => y.Arrow == 'S' ? y with { Arrow = movement.Values.Where(z => z.Start == x.End.Position.Side && z.End == x.Path.First().Position.Side).Single().Arrow } : y).ToArray() })
		//.Dump()
		.GroupBy(x => x.Steps)
		.Single().First();

	//CreateMap(result.Path.Select(y => (y.From.X, y.From.Y, y.Arrow))).Dump();

	var pathIndex = result.Path.ToDictionary(x => (x.From.X, x.From.Y), x => x.Arrow);

	var horizontal = new int[size.X]
		.SelectMany((_, x) => 
			Accumulate(new int[size.Y].Select((_, y) => y),
				new { X = -1, Y = -1, Char = ' ', Buffer = '\0', Enclose = 0 },
				(a, y) => new
				{
					X = x,
					Y = y,
					Char = pathIndex.TryGetValue((x, y), out var c) ? c : '\0',
					Buffer = c == '┌' || c == '└' ? c : c == '┘' || c == '┐' ? '\0' : a.Buffer,
					Enclose = a.Enclose + (c == '│' || (c== '┘' && a.Buffer =='┌') || (c== '┐' && a.Buffer =='└') ? 1 : 0)
				},
				(_, _) => true)
			.Where(x => x.Enclose % 2 == 1 && x.Char == '\0')
			)
		//.Dump()
		;
		
		horizontal.Count().Dump();

	CreateMap(
		result.Path.Select(y => (y.From.X, y.From.Y, y.Arrow))
		.Union(horizontal.Select(x=>(x.X,x.Y,'0')))
		).Dump();


	//movement.Values.Where(x=>x.Start == result.Path.Last().From.Side && x.End == result.Path.First().From.Side).Single().Dump();

	// part 1: 6649
	// part 2: 601
}

// You can define other methods, fields, classes and namespaces here

enum Position { Top, Right, Bottom, Left }

record Me(int X, int Y, Position Side)
{
	public bool SameCoordinate(Me me)
	{
		return me.X == X && me.Y == Y;
	}
}
//record Move(int Horizontal, int Vertical)
//{
//	public static Move Up = new Move(-1, 0);
//	public static Move Down = new Move(1, 0);
//	public static Move Right = new Move(1, 1);
//	public static Move Left = new Move(-1, 0);
//}
record Map(Char Char, Position Start, int Horizontal, int Vertical, Position End, char Arrow)
{
	public static Map StartPosition { get; } = new Map('S', Position.Top, Int32.MaxValue, Int32.MaxValue, Position.Top, 'S');

	public Me Apply(Me current)
	{
		if (current.Side != Start)
			throw new InvalidOperationException();
		//$"{current} > {this} > {new Me(current.X + Horizontal, current.Y + Vertical, End)}".Dump();
		return new Me(current.X + Horizontal, current.Y + Vertical, End);
	}
}

public static IEnumerable<TAcc> Accumulate<TSource, TAcc>(IEnumerable<TSource> source,
	TAcc init,
	Func<TAcc, TSource, TAcc> func,
	Func<TAcc, IEnumerable<TAcc>, bool> predicate)
{
	var all = new List<TAcc>();
	if (init != null)
		all.Add(init);
	using (IEnumerator<TSource> e = source.GetEnumerator())
	{
		var result = init;
		while (e.MoveNext())
		{
			var next = func(result, e.Current);
			if (!predicate(next, all))
				break;
			result = next;
			all.Add(result);
		}
	}
	return all;
}
