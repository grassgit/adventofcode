<Query Kind="Statements" />

var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

// part 2
input = Regex.Replace(input, @"(?<=\d) +", "").Dump();


var match = Regex
	.Match(input, @"Time:(\s+(?<t>\d+))+\r\nDistance:(\s+(?<d>\d+))+$");
var data = match
	.Groups["t"].Captures
	.Select((x, i) => new { Time = long.Parse(x.Value), Distance = long.Parse(match.Groups["d"].Captures[i].Value) })
	.Dump()
	;

long Distance(long total, long hold) => hold * (total - hold);

var distance = data
	.Select(x => new
	{
		x.Time,
		x.Distance,
		Valid = new int[x.Time]
			.AsParallel()
			.Select((_, i) => new { Hold = i, Travelled = Distance(x.Time, i) })
			.SkipWhile(d => d.Travelled <= x.Distance)
			.TakeWhile(d => d.Travelled > x.Distance)
	})
	.Aggregate(1, (a,x)=>x.Valid.Count() * a)
	.Dump()
	;

// part 1 
//  781200
// part 2
//  49240091