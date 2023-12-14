<Query Kind="Statements" />

var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

var value = "A, K, Q, T, 9, 8, 7, 6, 5, 4, 3, 2, J".Split(", ")
.Select((x, i) => new { x, i })
.ToDictionary(x => x.x[0], x => x.i)
//.Dump()
;

IEnumerable<(int Key, int Count)[]> Permute((int Key, int Count)[] values)
{
	yield return values;


	if (values.Any(x => x.Key == value['J'] && x.Count > 0))
	{
		for (var c = 0; c < values.Length; c++)
		{
			foreach (var r in Permute( values.Select((x, i) => (x.Key, x.Count + (x.Key == value['J'] ? -1 : i == c ? 1 : 0))).ToArray()))
				yield return r;
		}
	}
}

var lines = input
	.Split("\r\n")
	.Select(x => x.Split(' '))
	.Select(x => new { Cards = x[0], Bid = int.Parse(x[1]) })
	.Select(x => new
	{
		x.Cards,
		x.Bid,
		Values = x.Cards
			.Select(c => value[c])
			.ToArray()
	})
	.Select(x => new
	{
		x.Cards,
		x.Bid,
		x.Values,
		Counts = x.Values.GroupBy(x => x).Select(x => (Key: x.Key, Count: x.Count())).ToArray()
	})
	.SelectMany(x => Permute(x.Counts)
	.Select(y => new
	{
		x.Cards,
		x.Bid,
		x.Values,
		Counts = y
	}))
	//.Dump()
	.Select(x => new
	{
		x.Cards,
		x.Bid,
		x.Values,
		Five = x.Counts.Any(c => c.Count == 5),
		Four = x.Counts.Any(c => c.Count == 4),
		Three = x.Counts.Any(c => c.Count == 3),
		Two = x.Counts.Count(c => c.Count == 2)
	})
	.Select((x, i) => new
	{
		Index = i,
		x.Bid,
		x.Cards,
		Rank = x.Five ? 0 : x.Four ? 1 : x.Three && x.Two == 1 ? 2 : x.Three ? 3 : x.Two == 2 ? 4 : x.Two == 1 ? 5 : 6,
		Strength = string.Join("", x.Values.Select(x => x.ToString("x")))
	})
	.OrderByDescending(x => x.Rank)
	.ThenByDescending(x => x.Strength)
	.Dump()
	.Select((x,i)=>(x,i))
	.GroupBy(x => x.x.Cards)
	//.Dump()
	.Select(x => x.OrderBy(y => y.i).Last().x)
	.OrderByDescending(x => x.Rank)
	.ThenByDescending(x => x.Strength)
	.Select((x, i) => new
	{
		x.Cards,
		x.Index,
		x.Bid,
		x.Strength,
		Rank = i,
		Value = x.Bid * (i + 1)
	})
	.Dump()
	.Sum(x => x.Value)
	.Dump()
	;

// part 1 > 253638586
// part 2 > 253253225