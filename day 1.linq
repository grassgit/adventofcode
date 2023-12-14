<Query Kind="Statements" />

var input = File.ReadAllText(Util.CurrentQueryPath + ".input");

string Reverse(string value) => new string(value.Reverse().ToArray());

var numbers = new List<string> { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
var regex = new Regex(string.Join("|", numbers.Select(x => $"({x})")));
var regexRev = new Regex(string.Join("|", numbers.Select(x => $"({Reverse(x)})")));
regexRev.Dump();
//numbers.ForEach(x=>input = input.Replace(x, (numbers.IndexOf(x)+1).ToString()));

input.ToLowerInvariant().Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
.Select(x => new
{
	Source = x,
	ValueFirst = regex.Replace(x, m => (numbers.IndexOf(m.Value) + 1).ToString()),
	ValueLast = regexRev.Replace(Reverse(x), m => (numbers.IndexOf(Reverse(m.Value)) + 1).ToString())
})
.Select(x => new
{
	x.Source,
	x.ValueFirst,
	x.ValueLast,
	First = x.ValueFirst.ToCharArray().First(Char.IsDigit),
	First2 = x.ValueFirst.ToCharArray().Last(Char.IsDigit),
	Last = x.ValueLast.ToCharArray().First(Char.IsDigit),
})
.Dump()
.Select(x => int.Parse($"{x.First}{x.Last}"))
.Sum()
.Dump();
