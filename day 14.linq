<Query Kind="Program" />

void Main()
{

	var input =
		File.ReadAllText(Util.CurrentQueryPath + ".input")
		//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
		;

	input.Dump();
	var lines = input.Split("\r\n")
		.Select((x, i) => new
		{
			Index = i,
			Line = x,
			Row = new Row(
				x.Select((c, i) => c == 'O' ? 1 : 0).ToArray(),
				x.Select((c, i) => c == '#').ToArray())
		})
		//.Dump()
		.ToList();


	Row[] Roll(Row[] rows)
	{
		//rows = new[] { new Row(-1, default(int[]), new bool[rows[0].Movable.Length].Select(x => true).ToArray()) }.Concat(rows).Reverse().ToArray();

		var width = rows[0].Movable.Length;
		var height = rows.Length;

		var parallel = new (int, bool)[rows[0].Movable.Length][]
			.Select((_, c) => rows.Select((x, i) => (x.Movable[c], i == 0 ? true : rows[i - 1].Fixed[c]))).ToArray()
			.ToArray();

		var moved = parallel
		.AsParallel()
		.Select(col =>
			col.Reverse()
				.Aggregate(
					new { Hold = 0, Drop = new int[0] },
					(a, x) => new
					{
						Hold = x.Item2 ? 0 : a.Hold + x.Item1,
						Drop = a.Drop.Concat(new[] { x.Item2 ? a.Hold + x.Item1 : 0 }).ToArray()
					})
				.Drop
				.Reverse()
				.Aggregate(
					new { Drop = new int[0], Hold = 0 },
					(a, x) => new
					{
						Drop = a.Drop.Concat(new[] { x + a.Hold > 0 ? 1 : 0 }).ToArray(),
						Hold = x + a.Hold > 0 ? x + a.Hold - 1 : 0
					})
				.Drop)
				.ToArray();
		moved = moved[0].Select((_, i) => moved.Select(x => x[i]).ToArray()).ToArray();

		return moved.Zip(rows, (m, r) => new Row(m, r.Fixed)).ToArray();
	}
	var rows = lines.Select(x => x.Row).ToArray();

	var diff = new Dictionary<string, (int, int)>();
	int predict = 0;
	diff.Clear();
	var target = 1000000000;
	for (var i = 1; i <= target; i++)
	{
		for (var d = 0; d < 4; d++)
		{
			rows = Roll(rows).ToArray();
			rows = Rotate(rows).ToArray();
		}
		var map = string.Join("", rows.Select(x => string.Join("", x.Movable.Select(y => y == 1 ? 'O' : '.'))));
		if (diff.TryGetValue(map, out var m))
		{
			//$"{i} Same as {m.Item1}".Dump();
			var di = m.Item1;
			if (predict == 0)
			{
				predict = ((target - di) % (i - di)) + di;
				$"{i} Same as {di} {i - di} - Prediction: {predict}".Dump();

				if (target > 5000)
					break;
			}
			//}
		}
		else
		{
			diff.Add(map, (i, Weight(rows)));
		}
	}
	//Draw(rows);

	$"{target}: {diff.Values.Single(x=>x.Item1 == predict)}".Dump();
	if (target <= 5000)
		$"{target}: {diff[string.Join("", rows.Select(x => string.Join("", x.Movable.Select(y => y == 1 ? 'O' : '.'))))]}".Dump();

	diff.Dump();
	//var a = new[] {
	//	new Row(new[] { 1, 2, 3 }, new bool[3]),
	//	new Row(new[] { 4, 5, 6 }, new bool[3]),
	//	new Row(new[] { 7, 8, 9 }, new bool[3])
	//	};
	//a.Select(x => string.Join(" ", x.Movable)).Dump();
	//Rotate(new[] {
	//	new Row(new[] { 1, 2, 3 }, new bool[3]),
	//	new Row(new[] { 4, 5, 6 }, new bool[3]),
	//	new Row(new[] { 7, 8, 9 }, new bool[3])
	//	})
	//	.Select(x => string.Join(" ", x.Movable))
	//	.Dump();

	int Weight(Row[] rows) => rows.Select((x, i) => x.Movable.Sum() * (lines.Count - i))
				.Sum();
	var count = Draw(rows)
			//.Select(x=>string.Join("", x.Select(y=>y ==1 ? 'O' : '.')))
			//.Dump();
			.Select((x, i) => x.Movable.Sum() * (lines.Count - i))
			.Sum()
			.Dump()
		;
	// part 1: 103333
	// part 2: 97241 stable from itterarion 
	IEnumerable<Row> Draw(IEnumerable<Row> rows)
	{
		rows
			.Select((x, i) => i.ToString().PadLeft(3, ' ') + ": " + string.Join("", x.Movable.Select((y, i) => y == 1 ? 'O' : x.Fixed[i] ? '#' : '.')))
			.Dump();
		return rows;
	}


	IEnumerable<Row> Rotate(Row[] rows, int count = 1)
	{
		for (var cnt = 0; cnt < count; cnt++)
		{
			var w = rows[0].Movable.Length - 1;
			rows = rows[0].Movable.Select((_, c) => new Row(rows.Reverse().Select((x, i) => x.Movable[c]).ToArray(), rows.Reverse().Select(x => x.Fixed[c]).ToArray())).ToArray();
		}
		return rows;
	}
}
record Row(int[] Movable, bool[] Fixed);
// You can define other methods, fields, classes and namespaces here