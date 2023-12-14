<Query Kind="Program" />



void Main()
{
	var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

	var positions = input
		.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
		.SelectMany((x, i) => Regex.Matches(x.Trim(), @"[#]").Select(x => new { Row = i, Column = x.Index }))
		//.Dump()
		;

	var rows = positions
		.Select(x => x.Row)
		.Distinct();
	var columns = positions
		.Select(x => x.Column)
		.Distinct()
		;

	var expandFactor = 1000000;

	var expanded = positions
	.Select((x, i) => new { Index = i, Source = x, E = x.Row - rows.Count(y => y < x.Row), Row = x.Row + (x.Row - rows.Count(y => y < x.Row)) * (expandFactor -1), Column = x.Column + (x.Column - columns.Count(y => y < x.Column)) * (expandFactor -1) })
	.ToArray()
	//.Dump()
	;

	var distance = expanded
		.SelectMany(a => expanded
			.Where(b => a.Index < b.Index)
			.Select(b => new { From = a, To = b })
		)
		.Select(x => new
		{
			f = x.From.Index,
			x.To.Index,
			Distance = (long)Math.Abs(x.From.Row - x.To.Row) + Math.Abs(x.From.Column - x.To.Column)
		})
		//.Where(x=>x.Index == 15 && x.f == 4).Dump()
		.ToArray()
		//.Dump()
		;

	distance.Sum(x => x.Distance).Dump();

	Draw(positions.Select(x => (x.Row, x.Column, (char?)'#')));
	Draw(expanded.Select(x => (x.Row, x.Column, (char?)'#')));
	Draw(expanded.Select(x => (x.Row, x.Column, (char?)x.Index.ToString("x")[^1])));
}

// You can define other methods, fields, classes and namespaces here

void Draw(IEnumerable<(int X, int Y, char? Label)> data) =>
	(data.Max(x => x.X) > 1000 ? "Too big" : string.Join("\r\n", new int[data.Max(x => x.X) + 1].Select((_, x) => string.Join("", new int[data.Max(x => x.Y) + 1].Select((_, y) => data.FirstOrDefault(z => z.X == x && z.Y == y).Label ?? '.'))))).Dump();
