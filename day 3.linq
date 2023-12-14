<Query Kind="Statements" />

var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

var lines = input.Split("\r\n");
var symbols = lines.SelectMany((x, i) => Regex.Matches(x, @"[^\d\.]").Select(y=>new { Line = i, Match = y }))
//.Where (x=>x.Match.Success)
.Select(x => new {
	Line = x.Line,
	Value = x.Match.Value,
	Colum = x.Match.Index
})
.ToList()
//.Dump()
;

var numbers = lines.SelectMany((x, i) => Regex.Matches(x, @"[\d]+").Select(y => new { Line = i, Match = y }))
//.Where (x=>x.Match.Success)
.Select(x => new
{
	Line = x.Line,
	Value = x.Match.Value,
	Start = x.Match.Index,
	End = x.Match.Index + x.Match.Length-1,
	Number = int.Parse(x.Match.Value)
})
//.Dump()
;

var parts = numbers.Select(x=>new {
Number = x,
Symbol = symbols.FirstOrDefault(s=>s.Line >= x.Line - 1 && s.Line <= x.Line + 1 && s.Colum >= x.Start - 1 && s.Colum <= x.End + 1)
})
.Where(x=>x.Symbol != null)
.ToList()
//.Dump()
;
var partsum = parts
.Sum(x=>x.Number.Number)
.Dump();


parts.GroupBy(x=>x.Symbol)
.Where(x=>x.Count() == 2)
.Dump()
.Sum(x=>x.First().Number.Number * x.Last().Number.Number)
.Dump();
