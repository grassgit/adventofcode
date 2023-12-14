<Query Kind="Statements" />

var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;


var lines = input.Split("\r\n");
var data = lines
.Select(x => Regex.Match(Regex.Replace(x, @"\s+", " "), @"Card (?<num>\d+): ((?<w>\d+) +)+\|( (?<y>\d+))+"))
.Select(x => new
{
	Card = x.Groups["num"].Value,
	Winning = x.Groups["w"].Captures.ToArray(),
	Numbers = x.Groups["y"].Captures.ToArray(),
})
.Select(x => new
{
	x.Card,
	Winning = x.Winning.Select(x => x.Value).OrderBy(x => x).ToHashSet(),
	Numbers = x.Numbers.Select(x => x.Value).OrderBy(x => x).ToHashSet()
})
//.Dump()
.Select(x => new
{
	x.Card,
	NumberCount = x.Winning.Count,
	MatchCount = x.Numbers.Where(y => x.Winning.Contains(y)).Count()
})
.ToList()
;
var part1 = data
.Where(x => x.MatchCount > 0)
.Select(x => new
{
	x.Card,
	Value = Math.Pow(2, x.MatchCount - 1)
})
//
.Sum(x => x.Value)
.Dump()
;
//21485

data.Aggregate(
  new[] {
  new
  {
  	  Card = data[0],
	  Sum = 0,
	  Total = 0,
	  Copies = new int[data.Max(x => x.NumberCount) ]
  }
  },
 (a, c) =>
 a.Concat(new[] {
 new
 {
 	 Card = c,
	 Sum = a[^1].Sum + 1 + a[^1].Copies[0],
	 Total = a[^1].Copies[0] + 1,
	 Copies = a[^1].Copies
	 	.Skip(1)
		.Concat(new[]{0})
	 	.Select((x,i) => i< c.MatchCount ? x + (1+a[^1].Copies[0]) : x)
		.ToArray()
}
}).ToArray())
.Skip(1)
.Dump()
.Last().Sum
.Dump()
;
