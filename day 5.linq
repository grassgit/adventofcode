<Query Kind="Statements" />

var source =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

var line1 = source.IndexOf("\r");
string inputSeeds = source[..line1];
string input = source[(line1+2)..];

var seeds = inputSeeds
	.Split(' ')
	.Select(long.Parse)
	// part 1
	//.Select(x=>new { Start = x, Count = 1})
	//part 2
	.Select((x, i) => new { Value = x, Set = i / 2 })
	.GroupBy(x => x.Set)
	.Select(x => new { Start = x.First().Value, Count = x.Skip(1).Single().Value });
//.Dump()
;

var maps = Regex.Matches(input, @"^(?<src>[a-z]+)-to-(?<dest>[a-z]+) map:\r\n((?<map>\d+ \d+ \d+)\r\n)+", RegexOptions.Multiline)
.Select(x => new
{
	Source = x.Groups["src"].Value,
	Destination = x.Groups["dest"].Value,
	Maps = x.Groups["map"].Captures.ToArray()
		.Select(x => x.Value.Split(' ').Select(long.Parse).ToArray())
		.Select(x => new { DestRange = x[0], SrcStart = x[1], SrcEnd = x[1] + x[2] - 1 })
		.ToArray()
})
.ToDictionary(x => x.Source)
//.Dump()
;

long Map(string type, long value) => maps[type].Maps.Where(m => value >= m.SrcStart && value <= m.SrcEnd).Select(x => (long?)value - x.SrcStart + x.DestRange).FirstOrDefault() ?? value;

long result = long.MaxValue;
foreach (var item in seeds)
{
	const int batch = 100_000_000;
	for (var cnt = 0; cnt < item.Count; cnt += batch)
	{
		var mapped = new int[Math.Min(item.Count - cnt, batch)]
			.AsParallel()
			.Select((_, i) => cnt + item.Start + i)
			.Select(s => new int[maps.Count]
				.Aggregate(
						//new[] { 
						new
						{
							Seed = s,
							Type = "seed",
							Input = -1L,
							Value = s, //Range = true ? null : maps["seed"].Maps[0] 
						}
					//}
					,
					(a, _) => //a.Concat(new[]{ 
					new
					{
						Seed = a.Seed,
						Type = maps[a.Type].Destination,
						Input = a.Value,
						Value = Map(a.Type, a.Value),
						//Range = maps[a.Type].Maps.Where(m => a.Value >= m.SrcStart && a.Value <= m.SrcEnd).SingleOrDefault()
					}
						//}).ToArray()
						)
				)
				//.Dump()
				//.Select(x => new
				//{
				//	Seed = x.Single(y => y.Type == "seed").Value,
				//	Location = x.Single(y => y.Type == "location").Value,
				//})
				.OrderBy(x => x.Value)
				.First()
				//.Dump(item.Start.ToString())
				;

		result = Math.Min(result, mapped.Value);
		new { item, result }.Dump();
	}

}

result.Dump();
