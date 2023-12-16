<Query Kind="Program" />

void Main()
{
	var input =
		File.ReadAllText(Util.CurrentQueryPath.Replace(".linq", ".txt"))
		//File.ReadAllText(Util.CurrentQueryPath.Replace(".linq", "-1.txt"))
		;

	Regex.Matches(input + "\r\n", @"((?<c>\d+)\r\n)+(\r\n|$)")
		.Select((x, i) => new { Index = i, Values = x.Groups["c"].Captures.Select(c => int.Parse(c.Value)).ToArray() })
		.Select(x => new { x.Index, Weight = x.Values.Sum() })
		.OrderByDescending(x => x.Weight)
		.Take(3)
		.Sum(x => x.Weight)
		.Dump()
		;
}

// You can define other methods, fields, classes and namespaces here